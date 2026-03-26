using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Runs the Table of Contents Report.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class LinkProjectCommand : IExternalCommand
    {
        /// <summary>
        /// Runs command.
        /// </summary>
        /// <param name="commandData">See Revit Documentation.</param>
        /// <param name="message">See Revit Documentation.</param>
        /// <param name="elements">See Revit Documentation.</param>
        /// <returns>See Revit Documentation.</returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Globals.Log.Write("[Link Project]");

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
                        "Link Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                if (Globals.modelValidationForm != null)
                {
                    Globals.modelValidationForm.Close();
                }

                using (LinkProjectForm dlg = new LinkProjectForm(doc.Title, Globals.SpecpointProjectID)
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

                    if (result == DialogResult.Cancel)
                    {
                        if (dlg.RebasedProject)
                        {
                            dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, "");
                            Globals.SpecpointProjectID = "";

                            SpecpointRegistry.SetValue("ProjectId", "");
                            SpecpointRegistry.SetValue("ProjectName", "");
                            Globals.SetButtonText("Current Project", "No current project");

                            // Prompt user to save the document manually
                            MessageBox.Show("The project link has been removed.\n\nPlease save your Revit project to preserve these changes.",
                                "Save Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        return Result.Cancelled;
                    }

                    // Set the projectId of associated Specpoint project with this Revit drawing
                    ProjectItem selectedProject = dlg.selectedProject;
                    if (selectedProject != null)
                    {
                        dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, selectedProject.id);
                        Globals.SpecpointProjectID = selectedProject.id;

                        SpecpointRegistry.SetValue("ProjectId", Globals.SpecpointProjectID);
                        SpecpointRegistry.SetValue("ProjectName", selectedProject.name);
                        Globals.SetButtonText("Current Project", selectedProject.name);
                    }
                    else
                    {
                        dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, "");
                        Globals.SpecpointProjectID = "";

                        SpecpointRegistry.SetValue("ProjectId", "");
                        SpecpointRegistry.SetValue("ProjectName", "");
                        Globals.SetButtonText("Current Project", "No current project");
                    }

                    // Check if specpoint id was saved
                    string specpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
                    if (selectedProject != null &&
                        !string.IsNullOrEmpty(selectedProject.id) &&
                        string.IsNullOrEmpty(specpointProjectID))
                    {
                        MessageBox.Show("Unable to link this Revit Project (RVT) to Specpoint.\n\nMissing project parameter (SpecpointProjectGuid) in the Revit Project Template (RTE) file.",
                            "Link Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Prompt user to save the document manually
                        MessageBox.Show("The project has been linked successfully.\n\nPlease save your Revit project to preserve these changes.",
                            "Save Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                } // End of using (LinkProjectForm dlg)

                // Reset
                Globals.ResetGlobals();
                Globals.SpecpointKeynotes = null;
                Globals.KeynoteDivisions = null;
                Globals.KeynoteAssemblies = null;
                Globals.KeynoteAssembliesByCategory = null;

                return Result.Succeeded;
            }
            catch (System.Runtime.InteropServices.SEHException sehEx)
            {
                // 0xC0000374 is STATUS_HEAP_CORRUPTION
                int errorCode = sehEx.ErrorCode;
                Globals.Log.Write($"[SEH EXCEPTION in LinkProjectCommand] Code: 0x{errorCode:X8}, Message: {sehEx.Message}");
                Globals.Log.Write($"Stack: {sehEx.StackTrace}");

                if (errorCode == unchecked((int)0xC0000374))
                {
                    Globals.Log.Write("[HEAP CORRUPTION DETECTED] Error code 0xC0000374 in LinkProjectCommand");
                    MessageBox.Show("A critical heap corruption error occurred. Please restart Revit and report this issue.\n\nThe application may become unstable.",
                        "Heap Corruption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"A system error occurred (Code: 0x{errorCode:X8}). Please try again or restart Revit.",
                        "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return Result.Failed;
            }
            catch (System.AccessViolationException avEx)
            {
                Globals.Log.Write($"[ACCESS VIOLATION in LinkProjectCommand] {avEx.Message}");
                Globals.Log.Write($"Stack: {avEx.StackTrace}");
                MessageBox.Show("A critical memory access violation occurred. Please save your work and restart Revit.",
                    "Access Violation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return Result.Failed;
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

                if (ex.Message == "File path must be already set to be able to save the document.It needs to be first saved using the SaveAs method instead.")
                {
                    MessageBox.Show("Please save the Revit project before linking it to a Specpoint project.",
                        "Link Project", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ErrorReporter.ReportError(ex);
                }

                return Result.Failed;
            }
        }

    }
}
