using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Reads in Markups for a given assembly and allows the user to edit them.
    /// </summary>
    [Autodesk.Revit.Attributes.Transaction(TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(RegenerationOption.Manual)]
    public class KeynotesManagerCommand : IExternalCommand
    {
        private UIApplication _uiapp;
        private UIDocument _uidoc;
        private Document _doc;
        private Autodesk.Revit.ApplicationServices.Application _app;
        
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
                Globals.Log.Write("[Keynotes Manager]");

                _uiapp = commandData.Application;
                _uidoc = commandData.Application.ActiveUIDocument;
                _doc = _uidoc.Document;
                _app = _uiapp.Application;

                string masterKeynoteFile = GetKeynotesFile(_doc);

                // Get the projectId of associated Specpoint project with this Revit drawing
                RevitDrawing dwg = new RevitDrawing(_uidoc, _uiapp);

                if (dwg.ProjectInfoProjectGroupExists() == false)
                {
                    MessageBox.Show("Missing parameter group (Project Info) in the shared parameter file.",
                        "Keynotes Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return Result.Cancelled;
                }

                Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
                if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return Result.Cancelled;

                // Get selected project category in Revit
                Assembly selectedProjectCategory = dwg.GetSelectedAssembly();

                // Get Revit categories from document
                if (Globals.revitCategories == null)
                {
                    Globals.revitCategories = new RevitCategories(_doc);
                }

                // Get Revit Apps main window handle
                Process revitApp = Process.GetCurrentProcess();
                IntPtr main = revitApp.MainWindowHandle;

                // Show filter by category form
                using (FilterAssembliesForm fa = new FilterAssembliesForm()
                {
                    RevitModel = _doc.Title
                })
                {
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
                        // Get included categories
                        List<string> includedCategories = new List<string>(fa.checkedItems);

                    if (Globals.includedCategories == null)
                    {
                        Globals.includedCategories = includedCategories;
                    }
                    else if (!Globals.includedCategories.SequenceEqual(includedCategories))
                    {
                        Globals.ResetGlobals();

                        Globals.includedCategories = includedCategories;
                    }

                    // Get Revit elements
                    Globals.revitElements = new RevitElements();
                    Globals.revitElements.Init(_doc);
                    
                    // Get Revit categories from document
                    if (Globals.revitCategories == null)
                    {
                        Globals.revitCategories = new RevitCategories(_doc);
                    }

                    // Get Revit materials
                    List<string> materialClasses = new List<string>();
                    Dictionary<string, AssemblyMaterial> materials = new Dictionary<string, AssemblyMaterial>();
                    AddMaterialAssembliesFromDocument(_doc, ref materialClasses, ref materials);

                    KeynotesManagerForm form = new KeynotesManagerForm(_doc, materials,
                        materialClasses, masterKeynoteFile)
                    {
                        RevitModel = _doc.Title,
                        IncludedCategories = includedCategories,
                        StartPosition = FormStartPosition.CenterParent,
                        WindowState = FormWindowState.Normal
                    };

                    // Use NativeWindow only if we have a valid handle
                    if (main != IntPtr.Zero)
                    {
                        NativeWindow nativeWindow = new NativeWindow();
                        try
                        {
                            nativeWindow.AssignHandle(main);
                            ret = form.ShowDialog(nativeWindow);
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
                        ret = form.ShowDialog();
                    }
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

        private async Task<bool> GetSpecpointCategories()
        {
            if (Globals.specpointCategories == null)
            {
                // Get list of Specpoint project categories
                Globals.specpointCategories = new SpecpointCategories();
                bool ret = await Globals.specpointCategories.Init();

                return ret;
            }

            return false;
        }

        private string GetKeynotesFile(Document doc)
        {
            string result = "";
            KeynoteTable keynoteTable = KeynoteTable.GetKeynoteTable(doc);
            IDictionary<ExternalResourceType, ExternalResourceReference> references = keynoteTable.GetExternalResourceReferences();
            if (references != null && references.Count > 0)
            {
                var first = references.First();
                result = first.Value.InSessionPath;
            }

            return result;
        }

        private void AddMaterialAssembliesFromDocument(Document doc,
            ref List<string> materialClasses,
            ref Dictionary<string, AssemblyMaterial> materials)
        {
            // Create assemblies from drawing elements and add to collection.
            List<Element> elements = new List<Element>();
            foreach (Material materialElement in doc.GetMaterials())
            {
                try
                {
                    string name = materialElement.Name;
                    AssemblyMaterial mat = new AssemblyMaterial(name);
                    Parameter knparam = materialElement.get_Parameter(BuiltInParameter.KEYNOTE_PARAM);
                    if (knparam != null && knparam.AsString() != null)
                    {
                        String key = knparam.AsString();
                        mat.Keynote = key;
                    }

                    mat.Class = materialElement.MaterialClass;
                    if (!materialClasses.Contains(mat.Class))
                    {
                        // Save material class
                        materialClasses.Add(mat.Class);
                    }

                    // Add assembly to list or increment its count if it is a duplicate.
                    materials[mat.Name] = mat;
                }
                catch (UserCancelledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    // Proceed to next drawing element
                    continue;
                }
            }

            materialClasses.Sort();
        }
    }
}
