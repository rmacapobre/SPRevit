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
    public class UpdateCommand : IExternalCommand
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
                Globals.Log.Write("[Update]");

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
                    using (FilterAssembliesForm fa = new FilterAssembliesForm()
                    {
                        StartPosition = FormStartPosition.CenterParent,
                        WindowState = FormWindowState.Normal
                    })
                    {
                        DialogResult result;

                        // Use NativeWindow only if we have a valid handle
                        if (main != IntPtr.Zero)
                        {
                            NativeWindow nativeWindow = new NativeWindow();
                            try
                            {
                                nativeWindow.AssignHandle(main);
                                result = fa.ShowDialog(nativeWindow);
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
                            result = fa.ShowDialog();
                        }

                        if (result == DialogResult.OK)
                        {
                            // Show model validation form
                            ModelValidationHandler handler = new ModelValidationHandler();
                            ExternalEvent externalEvent = ExternalEvent.Create(handler);
                            using (ModelValidationForm updateForm = new ModelValidationForm(uidoc, handler, externalEvent,
                                Globals.SpecpointProjectID, fa)
                            {
                                StartPosition = FormStartPosition.CenterParent,
                                WindowState = FormWindowState.Normal,
                                IsUpdate = true
                            })
                            {
                                // Use NativeWindow only if we have a valid handle
                                if (main != IntPtr.Zero)
                                {
                                    NativeWindow nativeWindow = new NativeWindow();
                                    try
                                    {
                                        nativeWindow.AssignHandle(main);
                                        result = updateForm.ShowDialog(nativeWindow);
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
                                    result = updateForm.ShowDialog();
                                }
                            }
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
}
