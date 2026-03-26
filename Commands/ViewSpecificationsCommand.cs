using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    /// <remarks>
    /// View Specifications
    /// </remarks>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class ViewSpecificationsCommand : IExternalCommand
    {
        /// <summary>
        /// The Revit window handle.
        /// </summary>
        //private WindowWrapper _hWndRevit = null;

        /// <summary>
        /// Execute command.  See IExternalCommand.
        /// </summary>
        /// <param name="commandData">See IExternalCommand.</param>
        /// <param name="message">See IExternalCommand.</param>
        /// <param name="elements">See IExternalCommand.</param>
        /// <returns>See IExternalCommand.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Globals.Log.Write("[View Specifications]");

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

                // Get Specpoint projectId 
                RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "View Specification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
                if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return Result.Cancelled;

                // Get selected Revit element
                ElementId selElementId = uidoc.Selection.GetElementIds().FirstOrDefault();
                if (selElementId == null)
                {
                    string msg = "View Specifications requires a selected BIM element in the drawing.";
                    ErrorReporter.ReportError(msg);
                    return Result.Failed;
                }

                // Get selected Revit assembly code
                Element selElement = doc.GetElement(selElementId);
                ElementType elemType = doc.GetElement(selElement.GetTypeId()) as ElementType;
                string famName = elemType.FamilyName;
                string assemblyCode = elemType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE).AsString();

                // Get selected project category in Revit
                Assembly selectedProjectCategory = dwg.GetSelectedAssembly();

                ViewSpecificationsForm dlg = new ViewSpecificationsForm(Globals.SpecpointProjectID, selectedProjectCategory)
                {
                    RevitAssemblyCode = assemblyCode,
                    StartPosition = FormStartPosition.CenterParent,
                    WindowState = FormWindowState.Normal
                };

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

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
