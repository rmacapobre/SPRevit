using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Account command
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)] // We might generate an ID for the drawing.
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class AccountCommand : IExternalCommand
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
                Globals.Log.Write("[Account]");

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                var uiapp = commandData.Application;
                var uidoc = commandData.Application.ActiveUIDocument;
                var doc = uidoc.Document;

                // Store UIApplication for later use
                Globals.UIApp = uiapp;

                // Get the projectId of associated Specpoint project with this Revit drawing
                RevitDrawing dwg = new RevitDrawing(uidoc, uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "Account", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);

                if (Globals.modelValidationForm != null)
                {
                    Globals.modelValidationForm.Close();
                }

                LoginForm dlg = new LoginForm()
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    WindowState = FormWindowState.Normal,
                    Doc = doc,
                    Dwg = dwg
                };

                if (main != IntPtr.Zero)
                {
                    NativeWindow nativeWindow = new NativeWindow();
                    try
                    {
                        nativeWindow.AssignHandle(main);
                        dlg.ShowDialog(nativeWindow);
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
                   dlg.ShowDialog();
                }

                if (dlg.LoginSuccessful == false &&
                    dlg.IsBPMUser)
                {
                    ErrorReporter.ReportError("Failed to login.\n\nConfirm that you're using a valid Specpoint AE account to proceed.");
                }

                if (Globals.SpecpointProjectID != "")
                {
                    Globals.Log.Write("[Account] Specpoint Project ID: " + Globals.SpecpointProjectID);

                    if (LoginUser.IsLoggedIn())
                    {
                        // Initialise Specpoint project attached to the model if possible
                        Globals.SetProject();
                    }
                }

                return Result.Succeeded;
            }
            catch (UserCancelledException)
            {
                Globals.Log.Write("[Account] User cancelled.");
                return Result.Cancelled;
            }            
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
                return Result.Failed;
            }
        }
    }
}
