using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Document = Autodesk.Revit.DB.Document;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Reads in Markups for a given assembly and allows the user to edit them.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class ValidationScheduleCommand : IExternalCommand
    {
        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="commandData">Data from the Revit app.</param>
        /// <param name="message">An outgoing message.</param>
        /// <param name="elements">Data from the Revit app.</param>
        /// <returns>Result of the command.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Globals.Log.Write("[Validation Schedule]");

                var uiapp = commandData.Application;
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;
                var app = uiapp.Application;

                // Store UIApplication for later use
                Globals.UIApp = uiapp;

                // Get the projectId of associated Specpoint project with this Revit drawing
                RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "Validation Schedule", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
                if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return Result.Cancelled;

                // Get selected project category in Revit
                Assembly selectedProjectCategory = dwg.GetSelectedAssembly();

                // Get Revit categories from document
                if (Globals.revitCategories == null)
                {
                    Globals.revitCategories = new RevitCategories(doc);
                }

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                if (Globals.modelValidationForm == null)
                {
                    // Show filter by category form
                    FilterAssembliesForm fa = new FilterAssembliesForm()
                    {
                        StartPosition = FormStartPosition.CenterParent,
                        WindowState = FormWindowState.Normal
                    };

                    DialogResult ret;

                    // Use NativeWindow only if we have a valid handle
                    if (main != IntPtr.Zero)
                    {
                        NativeWindow nativeWindow = new NativeWindow();

                        try
                        {
                            nativeWindow.AssignHandle(main);
                            ret = fa.ShowDialog(nativeWindow);
                        }
                        finally
                        {
                            if (nativeWindow.Handle != IntPtr.Zero)
                            {
                                nativeWindow.ReleaseHandle();
                            }
                        }
                    }
                    else
                    {
                        ret = fa.ShowDialog();
                    }

                    if (ret == DialogResult.OK)
                    {
                        // Show model validation form
                        ModelValidationHandler handler = new ModelValidationHandler();
                        ExternalEvent externalEvent = ExternalEvent.Create(handler);
                        Globals.modelValidationForm = new ModelValidationForm(uidoc, handler, externalEvent,
                            Globals.SpecpointProjectID, fa)
                        {
                            StartPosition = FormStartPosition.CenterParent,
                            WindowState = FormWindowState.Normal
                        };

                        if (Globals.modelValidationForm == null)
                        {
                            return Result.Cancelled;
                        }

                        // Use NativeWindow only if we have a valid handle


                        if (main != IntPtr.Zero)
                        {
                            NativeWindow nativeWindow = new NativeWindow();
                            try
                            {
                                nativeWindow.AssignHandle(main);
                                Globals.modelValidationForm.Show(nativeWindow);
                            }
                            finally
                            {
                                if (nativeWindow.Handle != IntPtr.Zero)
                                {
                                    nativeWindow.ReleaseHandle();
                                }
                            }
                        }
                        else
                        {
                            Globals.modelValidationForm.Show();
                        }
                    }
                }
                else
                {
                    Globals.modelValidationForm.Activate();
                }

                return Result.Succeeded;
            }
            catch (TokenExpiredException)
            {
                Globals.OnTokenExpired();

                return Result.Cancelled;
            }
            catch (UserCancelledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                Globals.Log.Write(ex);
                ErrorReporter.ReportError(ex);
                return Result.Failed;
            }
        }
    }

    // When using modeless dialogs use IExternalEventHandler
    public class ModelValidationHandler : IExternalEventHandler
    {
        public static ExternalEvent HandlerEvent = null;
        public static ModelValidationHandler Handler = null;
        public Dictionary<Parameter, string> parameters;

        public void Execute(UIApplication app)
        {
            Document doc = app.ActiveUIDocument.Document;
            try
            {
                try
                {
                    foreach (var pair in parameters)
                    {
                        string assemblyCode = pair.Value;
                        string description = string.Format("Set Assembly Code to {0}", assemblyCode);
                        using (Transaction tx = new Transaction(doc, description))
                        {
                            Parameter parameter = pair.Key;

                            tx.Start();
                            parameter.Set(assemblyCode);
                            tx.Commit();
                        }
                    }

                    // Prompt user to save the document manually
                    MessageBox.Show("Assembly codes have been updated.\n\nPlease save your Revit project to preserve these changes.",
                        "Save Required", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload form
                    Globals.ResetGlobals();
                    if (Globals.modelValidationForm != null)
                    {
                        Globals.modelValidationForm.Init();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Model Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Model Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string GetName()
        {
            return "Model Validation Handler";
        }

        public void GetData(Dictionary<Parameter, string> parameters)
        {
            this.parameters = parameters;
        }
    }
}
