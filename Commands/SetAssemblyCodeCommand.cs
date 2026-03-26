using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Runs the Checklist command.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class SetAssemblyCodeCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Globals.Log.Write("[Set Assembly Code]");

                var uiapp = commandData.Application;
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;
                var app = uiapp.Application;

                // Store UIApplication for later use
                Globals.UIApp = uiapp;

                // Get Revit categories from document
                if (Globals.revitCategories == null)
                {
                    Globals.revitCategories = new RevitCategories(doc);
                }

                // Get the projectId of associated Specpoint project with this Revit drawing
                RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "Set Assembly Code", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
                if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return Result.Cancelled;

                // Get selected Revit element
                ElementId selElementId = uidoc.Selection.GetElementIds().FirstOrDefault();
                if (selElementId == null)
                {
                    string msg = "Set Assembly Code requires a selected BIM element in the drawing.";
                    ErrorReporter.ReportError(msg);
                    return Result.Failed;
                }

                Element selElement = doc.GetElement(selElementId);
                ElementType elemType = doc.GetElement(selElement.GetTypeId()) as ElementType;
                if (elemType == null)
                {
                    string msg = string.Format("Unable to get the element type of {0}.", selElement.Name);
                    ErrorReporter.ReportError(msg);
                    return Result.Failed;
                }

                string famName = elemType.FamilyName;
                string elementId = selElementId.ToString();
                string assemblyCode = elemType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE).AsString();

                // Get selected project category in Revit
                Assembly selectedProjectCategory = dwg.GetSelectedAssembly();

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                SetAssemblyCodeForm dlg = new SetAssemblyCodeForm(Globals.SpecpointProjectID, selectedProjectCategory);
                dlg.RevitAssemblyCode = assemblyCode;
                dlg.StartPosition = FormStartPosition.CenterParent;

                DialogResult result;

                // Use NativeWindow only if we have a valid handle
                if (main != IntPtr.Zero)
                {
                    NativeWindow nativeWindow = new NativeWindow();
                    try
                    {
                        nativeWindow.AssignHandle(main);
                        result = dlg.ShowDialog(nativeWindow);
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
                    result = dlg.ShowDialog();
                }

                if (result == DialogResult.OK)
                {
                    // Set Assembly Code
                    Parameter p = elemType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE);
                    using (Transaction t = new Transaction(doc, "Set Assembly Code"))
                    {
                        t.Start();
                        p.Set(dlg.RevitAssemblyCode);
                        t.Commit();
                    }
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
