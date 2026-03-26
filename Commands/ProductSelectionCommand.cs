using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class ProductSelectionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Globals.Log.Write("[Product Selection]");

                var uiapp = commandData.Application;
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;
                var app = uiapp.Application;

                // Store UIApplication for later use
                Globals.UIApp = uiapp;

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                // Get Specpoint projectId 
                RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "Product Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                ProductSelectionProgress dlg = new ProductSelectionProgress(commandData)
                {
                    StartPosition = FormStartPosition.CenterParent,
                    WindowState = FormWindowState.Normal
                };

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
