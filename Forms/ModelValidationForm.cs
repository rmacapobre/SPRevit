using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Color = System.Drawing.Color;
using Control = System.Windows.Forms.Control;
using FontStyle = System.Drawing.FontStyle;
using Form = System.Windows.Forms.Form;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;


namespace Specpoint.Revit2026
{
    public partial class ModelValidationForm : Form
    {
        // List of matching Revit elements and Specpoint families
        public List<Assembly> matchingAssemblies;

        // List of modified rows identified by RevitTypeId
        public List<string> changedRows;

        public string projectId = "";

        // Form title
        private string title = "Model Validation";

        // Values before edit 
        public string statusBeforeEdit;
        public string assemblyBeforeEdit;


        // true if we are in the middle of loading the table.
        private bool tableIsLoading;

        // true if we are in the middle of loading the form.
        private bool formIsLoading;

        // true if we are in the middle of after edit.
        public bool afterEdit;

        // Specpoint Project Last modified date
        public DateTime? lastUpdated;

        // Flag that an assembly has been added to the project from BIM
        public bool addedAssemblyToSP;

        // Table holding Assembly report data.
        private DataTable reportDataTable;

        // Name of data table (not shown to the user)
        private const string DataTableName = "Assembly Report";

        /// Text to show for cells having no value.
        public const string NotSetCellText = "(Not Set)";

        // List of specific controls we want to enable/disable
        private List<Control> controls;

        private string loadingMsg = "";

        private int redCount;
        private int yellowCount;
        private int greenCount;

        // Grid
        private Color pastelWhite = Color.FromArgb(250, 250, 250);
        private Color pastelRed = Color.FromArgb(250, 160, 160);
        private Color pastelYellow = Color.FromArgb(250, 250, 160);
        private Color pastelGreen = Color.FromArgb(193, 225, 193);

        private Color labelRed = Color.FromArgb(255, 211, 71, 93);
        private Color labelGreen = Color.FromArgb(255, 41, 179, 165);
        private Color labelYellow = Color.FromArgb(255, 247, 198, 50);

        public List<ModelValidationFilter> filters;

        // Row tag vs visible row index
        private Dictionary<string, int> visibleRowIndexes;

        // Modeless
        private UIDocument uidocument;
        private Document document;
        private ModelValidationHandler handler;
        private ExternalEvent externalEvent;
        private FilterAssembliesForm fa;

        public bool insertingAssemblies;
        public bool getLastAssemblies;
        public bool getOtherAssemblies;

        // True when user clicks on Update Sync button
        public bool IsUpdate { get; set; }

        public ModelValidationForm()
        {
            InitializeComponent();

            changedRows = null;

            redCount = 0;
            yellowCount = 0;
            greenCount = 0;

            // Modeless
            document = null;
            handler = null;
            externalEvent = null;

            insertingAssemblies = false;
            getLastAssemblies = false;
            getOtherAssemblies = false;

            lastUpdated = DateTime.Now;
            addedAssemblyToSP = false;

            filters = new List<ModelValidationFilter>();
        }

        public ModelValidationForm(UIDocument uidocument,
            ModelValidationHandler handler, ExternalEvent externalEvent,
            string projectId, FilterAssembliesForm fa) : this()
        {
            this.uidocument = uidocument;
            this.document = uidocument.Document;
            this.handler = handler;
            this.externalEvent = externalEvent;
            this.fa = fa;

            this.projectId = projectId;

            changedRows = new List<string>();

            controls = new List<Control>();
            controls.Add(buttonOK);
            controls.Add(buttonSaveReportAs);
            controls.Add(buttonHelp);
            controls.Add(buttonReload);
            controls.Add(buttonExportAssemblyCodes);
            controls.Add(buttonAdd);
            controls.Add(checkBoxAssignedAssemblies);
            controls.Add(checkBoxUnassignedAssemblies);
            controls.Add(checkBoxAssignedRevitElements);
            controls.Add(checkBoxUnassignedRevitElements);
            controls.Add(checkBoxBoundElements);
            controls.Add(checkBoxUnboundElements);
        }

        private void ModelValidationForm_Load(object sender, EventArgs e)
        {
            try
            {
                ShowUpdateSyncControls(false);
                string updateSync = SpecpointRegistry.GetValue("UpdateSync");
                if (updateSync == "1")
                {
                    ShowUpdateSyncControls();
                }
                else if (updateSync == "2")
                {
                    Globals.ResetGlobals();
                    SpecpointRegistry.SetValue("UpdateSync", "0");
                }

                Init();

                if (Globals.TokenExpired)
                {
                    Close();
                    return;
                }

                if (IsUpdate)
                {
                    // Hide the form
                    this.Hide();

                    // Insert all assemblies added to Specpoint project
                    Globals.nodesToAdd = new List<ProjectElementNode>();
                    Add();

                }
                else if (this != null  && !this.IsDisposed)
                {
                    StartPosition = FormStartPosition.Manual;
                    Rectangle screen = Screen.FromPoint(Cursor.Position).WorkingArea;
                    int w = (int)Math.Round(screen.Width * 0.8);
                    Size = new Size(w, Height);
                    CenterToScreen();
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ModelValidationForm_Load));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public void ShowUpdateSyncControls(bool show = true)
        {
            try
            {
                labelUpdateSync.Visible = show;
                progressBarUpdateSync.Visible = show;
                buttonCancelUpdate.Visible = show;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ShowUpdateSyncControls));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public async Task<DateTime?> GetLastUpdated()
        {
            try
            {
                string projectId = Globals.SpecpointProjectID;
                DateTime lastUpdated = DateTime.Now;

                // Get the Specpoint project name
                Query query = new Query(nameof(GetLastUpdated));
                var result = await query.getProject(projectId);

                if (result != null &&
                    result.getProject != null)
                {
                    lastUpdated = result.getProject.lastUpdated;

                    Globals.Log.Write(string.Format("ModelValidationForm Last updated (local): {0}", lastUpdated.ToLocalTime()));
                }

                return lastUpdated;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetLastUpdated));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return null;
        }

        public void Reload()
        {
            try
            {
                // If there are changes to the Revit Model
                if (changedRows.Count > 0)
                {
                    AskSaveChangesToRevitModel(true);
                }
                else
                {
                    // Reload form
                    Globals.ResetGlobals();
                    if (Globals.modelValidationForm != null)
                    {
                        Globals.modelValidationForm.Init();
                    }
                }

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(Reload));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            try
            {
                Reload();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonReload_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public void Init()
        {
            try
            {
                formIsLoading = true;

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
                Globals.revitElements.Init(document);

                if (Globals.revitCategories == null)
                {
                    Globals.revitCategories = new RevitCategories(document);
                }

                toolStripStatusLabel.Text = "Loading...";

                labelVerified.SendToBack();
                labelVerifiedValue.Text = greenCount.ToString();
                labelVerifiedValue.ForeColor = labelGreen;
                labelCircleVerified.Text = "*";

                labelCaution.SendToBack();
                labelCautionValue.Text = yellowCount.ToString();
                labelCautionValue.ForeColor = labelYellow;
                labelCircleCaution.Text = "*";

                labelNeedsAttention.SendToBack();
                labelNeedsAttentionValue.Text = redCount.ToString();
                labelNeedsAttentionValue.ForeColor = labelRed;
                labelCircleNeeds.Text = "*";

                using (new WaitCursor(controls))
                {
                    // Create a map between Revit and Specpoint categories
                    CreateCategoryMap();

                    // Show progress bar
                    GetLastSubassembliesProgress dlg = new GetLastSubassembliesProgress()
                    {
                        Owner = this
                    };
                    DialogResult ret = dlg.ShowDialog();
                    if (ret == DialogResult.OK)
                    {
                        LoadTable();
                    }
                    else if (ret == DialogResult.Cancel)
                    {
                        if (IsUpdate)
                        {
                            // Reset flag
                            IsUpdate = false;
                            SpecpointRegistry.SetValue("UpdateSync", "0");
                        }

                        Close();
                        return;
                    }
                }

                EnableButtonAdd();
                toolStripStatusLabel.Text = "";
                formIsLoading = false;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(Init));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private bool HasSpecpointFamily(Assembly revitRow,
            ref Dictionary<string, Assembly> specpointFamilies)
        {
            try
            {
                if (revitRow == null) return false;
                if (specpointFamilies == null) return false;

                bool match = false;
                foreach (var f in specpointFamilies)
                {
                    Assembly spRow = f.Value;
                    if (spRow == null) continue;

                    match = (revitRow.AssemblyCode == spRow.AssemblyCode);
                    if (match) return true;
                }

                if (!match)
                {
                    foreach (var category in Globals.specpointOnlyFamilies.Keys)
                    {
                        foreach (var f in Globals.specpointOnlyFamilies[category])
                        {
                            Assembly spRow = f.Value;
                            if (spRow == null) continue;

                            match = (revitRow.AssemblyCode == spRow.AssemblyCode);
                            if (match) return true;
                        }
                    }
                }

                if (!match)
                {
                    foreach (var category in Globals.otherSpecpointOnlyFamilies.Keys)
                    {
                        foreach (var f in Globals.otherSpecpointOnlyFamilies[category])
                        {
                            Assembly spRow = f.Value;
                            if (spRow == null) continue;

                            match = (revitRow.AssemblyCode == spRow.AssemblyCode);
                            if (match) return true;
                        }
                    }
                }

                return match;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(HasSpecpointFamily));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return false;
        }

        public async Task<GetProjectElementNodes> GetOtherNodes(string projectId, List<ProjectElementType> types)
        {
            try
            {
                int currentCount = 0;
                Dictionary<int, GetProjectElementNodes> finalResults = new Dictionary<int, GetProjectElementNodes>();

                int totalCount = 0;
                GetProjectElementNodes results = null;
                do
                {
                    // Get specific project elements given the Specpoint category ID
                    GetAllProjectElementNodesInput all = new GetAllProjectElementNodesInput()
                    {
                        projectId = projectId,
                        projectElementType = types,
                        onlyElementsAssignedToProject = false,
                        skip = currentCount
                    };

                    string queryTitle = string.Format("{0} - GetOtherNodes", nameof(GetOtherNodes));
                    Query query = new Query(queryTitle);
                    results = await query.getProjectElementsNodes(all);

                    if (results != null)
                    {
                        finalResults[currentCount] = results;

                        totalCount = results.getProjectElementNodes.totalCount;

                        // Next batch
                        currentCount += results.ListNodes.Count;
                    }
                }
                while (currentCount < totalCount);

                // Consolidate results
                results = Globals.ConsolidateResults(finalResults);

                return results;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetOtherNodes));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return null;
        }


        public async Task<bool> GetOtherAssemblies(GetLastSubassembliesProgress progress = null)
        {
            try
            {
                getOtherAssemblies = true;

                // Instantiate once
                if (Globals.otherNodes == null)
                {
                    // Get all assembly NOT added to the project
                    Globals.otherNodes = await GetOtherNodes(projectId, new List<ProjectElementType>()
                    {
                        ProjectElementType.ASSEMBLY
                    });

                    if (Globals.otherAssemblies == null)
                    {
                        Globals.otherAssemblies = new SortedDictionary<string, Assembly>();
                        Globals.otherAssembliesByNode = new SortedDictionary<string, ProjectElementNode>();
                    }

                    // Make certain that otherNodes exist
                    if (Globals.otherNodes != null)
                    {
                        foreach (var node in Globals.otherNodes.ListNodes)
                        {
                            if (getOtherAssemblies == false) break;

                            char[] pipe = { '|' };
                            string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                            try
                            {
                                if (list.Length == 6)
                                {
                                    Assembly row = new Assembly()
                                    {
                                        RevitFamily = list[1],
                                        RevitType = list[3],
                                        AssemblyCode = list[4],
                                        AssemblyDescription = list[5],
                                        IsLastSubassembly = node.isLastSubAssembly,
                                        IsInProject = node.isInProject

                                    };

                                    Globals.otherAssemblies[list[4]] = row;
                                    Globals.otherAssembliesByNode[list[4]] = node;
                                }
                                else if (list.Length == 4)
                                {
                                    Assembly row = new Assembly()
                                    {
                                        RevitFamily = list[1],
                                        AssemblyCode = list[2],
                                        AssemblyDescription = list[3],
                                        IsLastSubassembly = node.isLastSubAssembly,
                                        IsInProject = node.isInProject
                                    };

                                    Globals.otherAssemblies[list[2]] = row;
                                    Globals.otherAssembliesByNode[list[2]] = node;
                                }
                                else if (list.Length == 2)
                                {
                                    Assembly row = new Assembly()
                                    {
                                        AssemblyCode = list[0],
                                        AssemblyDescription = list[1],
                                        IsLastSubassembly = node.isLastSubAssembly,
                                        IsInProject = node.isInProject
                                    };

                                    Globals.otherAssemblies[list[0]] = row;
                                    Globals.otherAssembliesByNode[list[0]] = node;
                                }
                            }
                            catch (TokenExpiredException)
                            {
                                throw;
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw new TokenExpiredException(ex.Message);
                            }
                            catch (Exception ex)
                            {
                                ErrorReporter.ReportError(ex);
                            }
                        }
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            getOtherAssemblies = false;

            return true;
        }

        public async Task<bool> GetLastSubAssemblies(GetLastSubassembliesProgress progress = null)
        {
            try
            {
                getLastAssemblies = true;

                // These include the missing F and G assemblies
                await Globals.LoadAssemblies(projectId, new List<string> {
                    "Administration",
                    "Specialty Equipment"
                });

                // Instantiate once
                if (Globals.nodes == null)
                {
                    // Get all assembly and family nodes added to the project
                    Globals.nodes = await GetNodes(projectId, new List<ProjectElementType>()
                    {
                        ProjectElementType.ASSEMBLY,
                        ProjectElementType.PRODUCTFAMILY
                    });

                    if (Globals.nodes == null) return false;

                    Globals.mapAddedToProject = new Dictionary<string, ProjectElementNode>();
                    foreach (var node in Globals.nodes.ListNodes.Where(x =>
                        (x.elementType == ProjectElementType.ASSEMBLY ||
                        x.elementType == ProjectElementType.PRODUCTFAMILY) &&
                        x.isInProject))
                    {
                        // Ignore FAMILY GROUPS
                        if (node.text.EndsWith("FAMILY GROUPS")) continue;
                        if (!string.IsNullOrEmpty(node.parentTreePath) &&
                            node.parentTreePath.StartsWith("Z4010|FAMILY GROUPS")) continue;

                        if (node.elementType == ProjectElementType.ASSEMBLY)
                        {
                            // Extract Specpoint Assembly Code
                            string assemblyCode = Globals.ExtractAssemblyCode(node.text);

                            Globals.mapAddedToProject[assemblyCode] = node;
                        }
                        else if (node.elementType == ProjectElementType.PRODUCTFAMILY)
                        {
                            string assemblyCode = "", familyId = "";
                            ExtractAssemblyCodeFamilyId(node, ref assemblyCode, ref familyId);
                            Globals.mapAddedToProject[assemblyCode + "." + familyId] = node;
                        }
                    }
                }

                // Instantiate once
                if (Globals.nodesByCategory == null ||
                    (Globals.nodesByCategory != null && Globals.nodesByCategory.Categories == null))
                {
                    // Get all assembly and family nodes by category
                    Globals.nodesByCategory = new NodesByMultipleCategory(projectId);
                    bool ret = await Globals.nodesByCategory.Init();

                    if (ret == false) return false;
                }

                if (Globals.lastSubAssemblies == null)
                {
                    Globals.lastSubAssemblies = new SortedDictionary<string, Assembly>();
                }

                int total = Globals.nodesByCategory.Categories.Count;
                foreach (var categoryNode in Globals.nodesByCategory.Categories)
                {
                    if (getLastAssemblies == false) break;

                    string categoryName = categoryNode.categoryName;

                    if (progress != null)
                    {
                        progress.Text = "Loading Assemblies";
                    }

                    // Filter out categories that do not exist in Revit
                    if (!Globals.revitCategories.ContainsKey(categoryName))
                    {
                        Globals.Log.Write(String.Format("Specpoint category {0} does not exist in Revit", categoryName));
                        continue;
                    }

                    // Load specpoint elements
                    Category c = Globals.revitCategories[categoryName];

                    SortedDictionary<string, Assembly> lastSubAssembliesByCategory = new SortedDictionary<string, Assembly>();
                    foreach (var node in categoryNode.projectElementNodes)
                    {
                        char[] pipe = { '|' };
                        string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            if (list.Length == 6)
                            {
                                Assembly row = new Assembly()
                                {
                                    BIMCategory = categoryName,
                                    BIMCategoryId = c.Id.Value,
                                    RevitFamily = list[1],
                                    RevitType = list[3],
                                    AssemblyCode = list[4],
                                    AssemblyDescription = list[5],
                                    IsLastSubassembly = node.isLastSubAssembly,
                                    IsInProject = node.isInProject
                                };

                                lastSubAssembliesByCategory[list[4]] = row;
                            }
                            else if (list.Length == 4)
                            {
                                Assembly row = new Assembly()
                                {
                                    BIMCategory = categoryName,
                                    BIMCategoryId = c.Id.Value,
                                    RevitFamily = list[1],
                                    AssemblyCode = list[2],
                                    AssemblyDescription = list[3],
                                    IsLastSubassembly = node.isLastSubAssembly,
                                    IsInProject = node.isInProject
                                };

                                lastSubAssembliesByCategory[list[2]] = row;
                            }
                        }
                        catch (TokenExpiredException)
                        {
                            throw;
                        }
                        catch (TargetInvocationException ex)
                        {
                            string msdg = ex.Message;
                            throw new TokenExpiredException(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.ReportError(ex);
                        }
                    }

                    // Save the list by category to the final list 
                    foreach (var lsa in lastSubAssembliesByCategory)
                    {
                        try
                        {
                            Globals.lastSubAssemblies[lsa.Key] = lsa.Value;
                        }
                        catch (TokenExpiredException)
                        {
                            throw;
                        }
                        catch (TargetInvocationException ex)
                        {
                            string msdg = ex.Message;
                            throw new TokenExpiredException(ex.Message);
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.ReportError(ex);
                        }
                    }
                }

                return true;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                Globals.Log.Write(msg);
            }

            getLastAssemblies = false;

            return false;
        }

        private void ExtractAssemblyCodeFamilyId(ProjectElementNode familyNode,
            ref string assemblyCode, ref string familyId)
        {
            try
            {
                if (familyNode.elementType != ProjectElementType.PRODUCTFAMILY) return;

                familyId = Globals.ExtractAssemblyCode(familyNode.text);

                string parentTreePath = familyNode.parentTreePath;
                string doublePipe = "||";
                string[] list = familyNode.treePath.Split(doublePipe, StringSplitOptions.RemoveEmptyEntries);
                int count = list.Count();

                string parentAssembly = list[count - 2];
                string pipe = "|";
                string[] listParentAssembly = parentAssembly.Split(pipe, StringSplitOptions.RemoveEmptyEntries);
                assemblyCode = listParentAssembly[0];
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ExtractAssemblyCodeFamilyId));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        

        private List<Assembly> GetMatchingFamilies()
        {
            try
            {
                // Unique list of assemblies by Revit TypeIds
                Dictionary<string, Assembly> rows = new Dictionary<string, Assembly>();

                // Show progress bar 
                // Get specpoint families(sections) assigned to the specpoint project
                GetSpecpointFamiliesProgress dlg = new GetSpecpointFamiliesProgress()
                {
                    Owner = this
                };
                DialogResult result = dlg.ShowDialog(this);
                if (result == DialogResult.Cancel)
                {
                    if (IsUpdate)
                    {
                        // Reset flag
                        IsUpdate = false;
                        SpecpointRegistry.SetValue("UpdateSync", "0");
                    }

                    Close();
                    return null;
                }

                Dictionary<string, Assembly> specpointFamilies = dlg.specpointFamilies;
                if (specpointFamilies == null) return null;

                // Populate the grid with Revit elements
                foreach (var row in Globals.revitElements)
                {
                    Assembly revitRow = row.Value;

                    // Has Specpoint Family
                    bool hasSpecpointFamily = HasSpecpointFamily(revitRow, ref specpointFamilies);
                    if (hasSpecpointFamily)
                    {
                        Dictionary<string, Assembly> families = GetSpecpointFamilies(revitRow, specpointFamilies);

                        if (families.Count == 0)
                        {
                            foreach (var category in Globals.specpointOnlyFamilies.Keys)
                            {
                                families = GetSpecpointFamilies(revitRow, Globals.specpointOnlyFamilies[category]);
                                if (families.Count > 0) break;
                            }
                        }

                        if (families.Count == 0)
                        {
                            foreach (var category in Globals.otherSpecpointOnlyFamilies.Keys)
                            {
                                families = GetSpecpointFamilies(revitRow, Globals.otherSpecpointOnlyFamilies[category]);
                                if (families.Count > 0) break;
                            }
                        }

                        foreach (var f in families)
                        {
                            Assembly spRow = f.Value;
                            if (revitRow.AssemblyCode != spRow.AssemblyCode) continue;

                            // If a revit and specpoint assembly code matches
                            revitRow.AssemblyDescription = spRow.AssemblyDescription;
                            revitRow.SpecpointFamilyNumber = spRow.SpecpointFamilyNumber;
                            revitRow.SpecpointFamily = spRow.SpecpointFamily;

                            string keyName = string.Format("{0}|{1}",
                                revitRow.RevitTypeId,
                                spRow.SpecpointFamily);
                            rows[keyName] = new Assembly(revitRow);
                        }
                    }

                    // Has no Specpoint Family
                    else
                    {
                        revitRow.SpecpointFamilyNumber = NotSetCellText;
                        revitRow.SpecpointFamily = NotSetCellText;

                        rows[revitRow.RevitTypeId] = revitRow;
                    }
                }

                List<Assembly> list = new List<Assembly>();
                foreach (var row in rows)
                {
                    list.Add(row.Value);
                }

                return list;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        /// <summary>
        /// Create a map between Revit and Specpoint categories
        /// </summary>
        private void CreateCategoryMap()
        {
            try
            {
                // If map already exists, exit
                if (Globals.mapCategoryIDs != null) return;

                Globals.mapCategoryIDs = new RevitSpecpointCategoryMap();
                Globals.mapCategoryIDs.Init();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(CreateCategoryMap));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        // Get Specpoint families from Specpoint
        public Dictionary<string, Assembly> GetSpecpointFamilies(GetSpecpointFamiliesProgress progress)
        {
            try
            {
                if (Globals.mapCategoryIDs == null) return null;
                if (Globals.nodes == null) return null;
                if (Globals.nodes.getProjectElementNodes == null) return null;
                if (Globals.nodes.getProjectElementNodes.projectElementNodes == null) return null;

                Dictionary<string, Assembly> result = new Dictionary<string, Assembly>();

                // Get all project elements 
                int i = 0;
                int total = Globals.nodes.getProjectElementNodes.projectElementNodes.Count;
                foreach (ProjectElementNode node in Globals.nodes.getProjectElementNodes.projectElementNodes)
                {
                    string categoryName = Globals.nodesByCategory.GetCategory(node.parentNodeId);
                    if (categoryName == "") continue;

                    i++;
                    if (progress != null)
                    {
                        int percent = Convert.ToInt32((double)i / total * 100);
                        progress.Text = String.Format("Loading {0} ({1}%)", node.text, percent);
                        progress.Percent.Value = percent;
                    }

                    // Filter out none families
                    if (node.elementType != ProjectElementType.PRODUCTFAMILY) continue;
                    if (!node.treePath.Contains("|")) continue;

                    // Filter out categories that do not exist in Revit
                    if (!Globals.revitCategories.ContainsKey(categoryName))
                    {
                        Globals.Log.Write(String.Format("Specpoint category {0} does not exist in Revit", categoryName));
                        continue;
                    }

                    // Get node of all element types (assembly, family,.product type) from Revit Category
                    Category c = Globals.revitCategories[categoryName];

                    char[] pipe = { '|' };
                    string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                    try
                    {
                        if (list.Length == 8)
                        {
                            Assembly row = new Assembly()
                            {
                                BIMCategory = categoryName,
                                BIMCategoryId = c.Id.Value,
                                RevitFamily = list[1],
                                RevitType = list[3],
                                AssemblyCode = list[4],
                                AssemblyDescription = list[5],
                                SpecpointFamilyNumber = list[6],
                                SpecpointFamily = list[7],
                                IsLastSubassembly = node.isLastSubAssembly,
                                IsInProject = node.isInProject

                            };

                            result[node.treePath] = row;
                        }
                        else if (list.Length == 6)
                        {
                            Assembly row = new Assembly()
                            {
                                BIMCategory = categoryName,
                                BIMCategoryId = c.Id.Value,
                                RevitFamily = list[1],
                                AssemblyCode = list[2],
                                AssemblyDescription = list[3],
                                SpecpointFamilyNumber = list[4],
                                SpecpointFamily = list[5],
                                IsLastSubassembly = node.isLastSubAssembly,
                                IsInProject = node.isInProject
                            };

                            result[node.treePath] = row;
                        }
                    }
                    catch (TokenExpiredException)
                    {
                        throw;
                    }
                    catch (TargetInvocationException ex)
                    {
                        string msdg = ex.Message;
                        throw new TokenExpiredException(ex.Message);
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.ReportError(ex);
                    }
                }

                return result;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Get Specpoint families that match Revit row
        private Dictionary<string, Assembly> GetSpecpointFamilies(Assembly revitRow,
            Dictionary<string, Assembly> source)
        {
            try
            {
                Dictionary<string, Assembly> target = new Dictionary<string, Assembly>();

                if (revitRow == null) return null;
                if (source == null) return null;

                bool match = false;
                foreach (var f in source)
                {
                    Assembly spRow = f.Value;
                    if (spRow == null) continue;

                    match = (revitRow.AssemblyCode == spRow.AssemblyCode);
                    if (match)
                    {
                        target[f.Key] = spRow;
                    }
                }

                return target;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Get all nodes 
        public async Task<GetProjectElementNodes> GetNodes(string projectId, List<ProjectElementType> types)
        {
            try
            {
                int currentCount = 0;
                Dictionary<int, GetProjectElementNodes> finalResults = new Dictionary<int, GetProjectElementNodes>();

                int totalCount = 0;
                GetProjectElementNodes results = null;

                do
                {
                    // Get specific project elements given the Specpoint category ID
                    GetAllProjectElementNodesInput all = new GetAllProjectElementNodesInput()
                    {
                        projectId = projectId,
                        projectElementType = types,
                        skip = currentCount
                    };

                    string queryTitle = string.Format("{0} - GetNodes", title);
                    Query query = new Query(queryTitle);
                    results = await query.getProjectElementsNodes(all);

                    if (results != null)
                    {
                        finalResults[currentCount] = results;

                        totalCount = results.getProjectElementNodes.totalCount;

                        // Next batch
                        currentCount += results.ListNodes.Count;
                    }
                }
                while (currentCount < totalCount);

                // Consolidate results
                results = Globals.ConsolidateResults(finalResults);

                return results;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Get node of all element types (assembly, family,.product type) from Revit Category
        public async Task<GetProjectElementNodes> GetSpecpointAssemblies(string projectId, Category revitCategory)
        {
            try
            {
                if (revitCategory == null) return null;
                if (Globals.mapCategoryIDs == null) return null;

                // Get specific project elements given the Specpoint category ID
                GetProjectElementNodesInput specific = null;
                if (Globals.mapCategoryIDs.ContainsKey(revitCategory.Id.ToString()))
                {
                    specific = new GetProjectElementNodesInput()
                    {
                        projectId = projectId,
                        categoryId = Globals.mapCategoryIDs[revitCategory.Id.ToString()],
                    };

                    specific.projectElementType = new List<ProjectElementType>()
                {
                    ProjectElementType.ASSEMBLY,
                    ProjectElementType.PRODUCTFAMILY
                };
                }

                if (specific == null) return null;

                string queryTitle = string.Format("{0} - {1}", title, revitCategory.Name);
                Query query = new Query(queryTitle);
                GetProjectElementNodes results = await query.getProjectElementsNodes(specific);

                return results;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private bool AskSaveChangesToRevitModel(bool reloadForm = false)
        {
            try
            {
                // If there are changes to the Revit Model
                if (changedRows.Count > 0)
                {
                    string question = "Save current changes to the Revit model?";
                    DialogResult ret = MessageBox.Show(question, title,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (ret == DialogResult.Cancel) return false;

                    SaveChangesToRevitModel(reloadForm);
                }

                return true;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(AskSaveChangesToRevitModel));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return false;
        }

        private void SaveChangesToRevitModel(bool reloadForm = false)
        {
            try
            {
                using (new WaitCursor(controls))
                {
                    // Traverse the list of modified assemblies
                    Dictionary<Parameter, string> parameters = new Dictionary<Parameter, string>();
                    foreach (string revitTypeId in changedRows)
                    {
                        string newAssemblyCode = Globals.revitElements[revitTypeId].AssemblyCode;
                        ElementType elemType = document.GetElement(revitTypeId) as ElementType;
                        Parameter p = elemType.get_Parameter(BuiltInParameter.ASSEMBLY_CODE);

                        if (string.IsNullOrEmpty(newAssemblyCode))
                        {
                            toolStripStatusLabel.Text = string.Format("Unassigning Assembly({0}) from Element({1})", newAssemblyCode, elemType.Name);
                        }
                        else
                        {
                            toolStripStatusLabel.Text = string.Format("Assigning Assembly({0}) to Element({1})", newAssemblyCode, elemType.Name);
                        }

                        parameters[p] = newAssemblyCode;
                    }

                    if (parameters.Count > 0)
                    {
                        // Save row changes
                        handler.GetData(parameters);
                        externalEvent.Raise();
                    }

                    toolStripStatusLabel.Text = "Done";
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(SaveChangesToRevitModel));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.OK;

                // Save changes to the Revit Model if any
                SaveChangesToRevitModel();

                Close();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonOK_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonCancel_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonSaveReportAs_Click(object sender, EventArgs e)
        {
            try
            {
                GridReportExporter gr = new GridReportExporter(
                    "Specpoint Model Validation Report",
                    "Specpoint Model Validation");
                gr.ExportGrid(grid);

                toolStripStatusLabel.Text = "Saved successfully.";
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonSaveReportAs_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            try
            {
                // Launch browser
                Browser b = new Browser();
                SpecpointEnvironment env = new SpecpointEnvironment();
                b.OpenUrl(env.HelpURL);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonHelp_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // Format not-set assembly code cells making them easier to see
                if ((e.ColumnIndex == grid.Columns[ColumnName.AssemblyCode].Index ||
                    e.ColumnIndex == grid.Columns[ColumnName.AssemblyDescription].Index)
                    && e.Value as string == NotSetCellText)
                {
                    Font cellFont = new Font(e.CellStyle.Font, FontStyle.Italic);
                    e.CellStyle.Font = cellFont;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Only respond to cell value changes resulting from user input
                if (this.tableIsLoading)
                    return;

                grid_EndEdit();

                // Reload table to account for modified assemblies
                this.ReloadTable();

                this.grid.TriggerFilterStringChanged();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void ReloadTable()
        {
            try
            {
                this.grid.CellClick -= this.grid_CellClick;
                this.grid.CellDoubleClick -= this.grid_CellDoubleClick;

                using (new WaitCursor(controls))
                {
                    // Remember sort column and direction so we can restore it after reloading the table.
                    this.UpdateSortSettings();

                    // Remember top row
                    int topRowIndex = grid.FirstDisplayedScrollingRowIndex;

                    // Remember row/column index of current selection.
                    DataGridViewCell currentCell = this.grid.CurrentCell;
                    int columnIndex = 0;
                    int rowIndex = 0;
                    if (currentCell != null)
                    {
                        columnIndex = currentCell.ColumnIndex;
                        rowIndex = currentCell.RowIndex;
                    }

                    // Recreate the table (restores the sort column and direction).
                    bool ret = this.LoadTable();
                    if (ret == false)
                    {
                        Close();
                        return;
                    }

                    // Restore the top row
                    if (-1 < topRowIndex && topRowIndex < this.grid.Rows.Count)
                    {
                        this.grid.FirstDisplayedScrollingRowIndex = topRowIndex;
                    }

                    // Adjust selection row and column if they are now out of range.
                    if (columnIndex >= grid.Columns.Count)
                    {
                        columnIndex = grid.Columns.Count - 1;
                    }
                    if (rowIndex >= grid.Rows.Count)
                    {
                        rowIndex = grid.Rows.Count - 1;
                    }

                    // Restore previously-selected row/column index.
                    if (-1 < columnIndex && -1 < rowIndex)
                    {
                        grid.CurrentCell = grid[columnIndex, rowIndex];
                    }
                }

                this.grid.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
                this.grid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellDoubleClick);

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ReloadTable));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public void EnableButtonAdd()
        {
            try
            {
                if (grid.IsCurrentCellInEditMode)
                {
                    buttonAdd.Enabled = false;
                }
                else
                {
                    buttonAdd.Enabled = grid.SelectedRows.Count > 0;
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(EnableButtonAdd));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        /// <summary>
        /// Update the sort order settings saved in the report options according to the data grid view.
        /// </summary>
        private void UpdateSortSettings()
        {
            try
            {
                string sortColumnName = "";
                DataGridViewColumn sortColumn = grid.SortedColumn;
                if (sortColumn != null)
                {
                    sortColumnName = sortColumn.Name;
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(UpdateSortSettings));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private int GetMinimumWidth(string colName)
        {
            try
            {
                switch (colName)
                {
                    case ColumnName.BIMCategory:
                    case ColumnName.RevitFamily:
                    case ColumnName.RevitType:
                    case ColumnName.AssemblyCode:
                    case ColumnName.AssemblyDescription:
                    case ColumnName.SpecpointFamilyNumber:
                        return 100;
                    case ColumnName.SpecpointFamily:
                        return 250;
                    case ColumnName.Status:
                        return 300;
                }

                return 100;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetMinimumWidth));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return 100;
        }

        private float GetFillWeight(string colName)
        {
            try
            {
                switch (colName)
                {
                    case ColumnName.BIMCategory:
                    case ColumnName.RevitFamily:
                    case ColumnName.RevitType:
                        return 150;
                    case ColumnName.AssemblyCode:
                        return 50;
                    case ColumnName.AssemblyDescription:
                        return 250;
                    case ColumnName.SpecpointFamilyNumber:
                        return 50;
                    case ColumnName.SpecpointFamily:
                        return 200;
                    case ColumnName.Status:
                        return 300;
                }

                return 100;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetFillWeight));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return 100;
        }

        public void AddUnlinkedSpecpointAssemblies(List<Assembly> assemblies)
        {
            try
            {
                string filter = "";
                string projectName = SpecpointRegistry.GetValue("ProjectName");
                if (string.IsNullOrEmpty(projectName))
                {
                    filter = SpecpointRegistry.GetValue("FilterAssemblies");
                }
                else
                {
                    filter = SpecpointRegistry.GetValue("FilterAssemblies|" + projectName);
                }

                foreach (ProjectElementNode node in Globals.nodes.ListNodes)
                {
                    Assembly revitRow = new Assembly();
                    if (node.elementType != ProjectElementType.ASSEMBLY) continue;
                    if (node.isInProject == false) continue;
                    if (node.isLastSubAssembly == false) continue;

                    // Ignore FAMILY GROUPS
                    if (node.text.EndsWith("FAMILY GROUPS")) continue;
                    if (!string.IsNullOrEmpty(node.parentTreePath) &&
                        node.parentTreePath.StartsWith("Z4010|FAMILY GROUPS")) continue;

                    string assemblyCode = Globals.ExtractAssemblyCode(node.text);
                    if (Globals.revitElements.AssignedAssemblyCodes.Contains(assemblyCode))
                    {
                        // Skip assembly codes already assigned to Revit elements
                        continue;
                    }

                    revitRow.BIMCategory = "";
                    revitRow.RevitFamily = "";
                    revitRow.RevitType = "";
                    revitRow.AssemblyCode = assemblyCode;
                    revitRow.AssemblyDescription = Globals.ExtractAssemblyDescription(node.text);
                    revitRow.SpecpointFamily = "";
                    revitRow.SpecpointFamilyNumber = "";
                    revitRow.RevitTypeId = "";

                    // if Architecture|MEP|Structure
                    if (filter == Globals.AllCategoriesFilter)
                    {
                        assemblies.Add(revitRow);
                        continue;
                    }

                    bool add = Globals.includedCategories.
                        SelectMany(category => Globals.nodesByCategory.Categories.
                        Where(c => c.categoryName == category).
                        SelectMany(c => c.projectElementNodes)).
                        Any(n => n.text == node.text);

                    if (add)
                    {
                        assemblies.Add(revitRow);
                    }
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(AddUnlinkedSpecpointAssemblies));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        /// <summary>
        /// Loads the Assemblies into the DataGridView.
        /// </summary>
        /// <param name="progressForm">Form to update with progress.</param>
        public bool LoadTable()
        {
            try
            {
                this.tableIsLoading = true;

                matchingAssemblies = GetMatchingFamilies();
                if (matchingAssemblies == null)
                {
                    this.tableIsLoading = false;
                    Close();
                    return false;
                }

                AddUnlinkedSpecpointAssemblies(matchingAssemblies);

                // Make the BindingSet available to the AssemblyValueCell 
                // so it can provide drop-down values for assembly parameters.
                this.grid.Tag = matchingAssemblies;

                //this.grid.SetDoubleBuffered();

                // Add report columns
                this.reportDataTable = new DataTable(DataTableName);
                DataColumn categoryColumn = reportDataTable.Columns.Add(ColumnName.BIMCategory);
                DataColumn revitFamilyColumn = reportDataTable.Columns.Add(ColumnName.RevitFamily);
                DataColumn revitTypeColumn = reportDataTable.Columns.Add(ColumnName.RevitType);
                DataColumn codeColumn = reportDataTable.Columns.Add(ColumnName.AssemblyCode);
                DataColumn descriptionColumn = reportDataTable.Columns.Add(ColumnName.AssemblyDescription);
                DataColumn familyNumberColumn = reportDataTable.Columns.Add(ColumnName.SpecpointFamilyNumber);
                DataColumn familyColumn = reportDataTable.Columns.Add(ColumnName.SpecpointFamily);
                DataColumn statusColumn = reportDataTable.Columns.Add(ColumnName.Status);
                DataColumn revitTypeIdColumn = reportDataTable.Columns.Add(ColumnName.RevitTypeId);

                // Add report rows
                int assembliesProcessed = 0;
                redCount = 0;
                yellowCount = 0;
                greenCount = 0;
                foreach (Assembly assembly in matchingAssemblies)
                {
                    // Add rows for this assembly
                    if (false == AddAssemblyRow(assembly))
                    {
                        this.tableIsLoading = false;
                        Close();
                        return false;
                    }

                    // Update progress bar
                    assembliesProcessed++;
                }

                // Set data source for view
                this.grid.DataSource = this.reportDataTable;

                // Set cell templates and writeability
                foreach (DataGridViewColumn col in this.grid.Columns)
                {
                    col.ReadOnly = true;

                    if (col.Name == ColumnName.AssemblyCode ||
                        col.Name == ColumnName.AssemblyDescription)
                    {
                        col.CellTemplate = new AssemblyValueCell();
                        col.ReadOnly = false;
                    }

                    col.FillWeight = GetFillWeight(col.Name);
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    col.MinimumWidth = GetMinimumWidth(col.Name);
                }

                // Sort by Category
                DataGridViewColumn sortColumn = grid.Columns[ColumnIndex.BIMCategory];
                if (sortColumn != null)
                {
                    ListSortDirection sortDirection = ListSortDirection.Ascending;
                    grid.Sort(sortColumn, sortDirection);
                }

                // Set last column's sizing mode to Fill to avoid blank space between
                // the right edge of the grid and the right edge of the control.
                grid.Columns[grid.Columns.Count - 2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                grid.Columns[grid.Columns.Count - 2].FillWeight = 500;
                grid.Columns[ColumnName.RevitTypeId].Visible = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.AllowUserToResizeColumns = true;

                labelVerifiedValue.Text = greenCount.ToString();
                labelCautionValue.Text = yellowCount.ToString();
                labelNeedsAttentionValue.Text = redCount.ToString();

                EnableButtonAdd();

                // Set flags
                this.tableIsLoading = false;
                return true;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(LoadTable));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return false;
        }

        private bool AddAssemblyRow(Assembly assembly)
        {
            try
            {
                DataRow row = this.reportDataTable.NewRow();
                row[ColumnName.BIMCategory] = assembly.BIMCategory;
                row[ColumnName.RevitFamily] = assembly.RevitFamily;
                row[ColumnName.RevitType] = assembly.RevitType;
                row[ColumnName.AssemblyCode] = assembly.AssemblyCode;
                row[ColumnName.AssemblyDescription] = assembly.AssemblyDescription;
                row[ColumnName.SpecpointFamilyNumber] = assembly.SpecpointFamilyNumber;
                row[ColumnName.SpecpointFamily] = assembly.SpecpointFamily;
                row[ColumnName.Status] = "";
                row[ColumnName.RevitTypeId] = assembly.RevitTypeId;

                bool foundUnderSpecpointOnlyCategory = 
                        Globals.FindAssemblyByCode(Globals.specpointOnlyCategories, assembly.AssemblyCode) != null;
                bool foundUnderOtherSpecpointOnlyCategory =
                        Globals.FindAssemblyByCode(Globals.otherSpecpointOnlyCategories, assembly.AssemblyCode) != null;

                bool addedToProject =
                    Globals.lastSubAssemblies.ContainsKey(assembly.AssemblyCode) ||
                    Globals.mapAddedToProject.ContainsKey(assembly.AssemblyCode) ||
                    foundUnderSpecpointOnlyCategory;
                bool notAddedToProject = 
                    Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) ||
                    foundUnderOtherSpecpointOnlyCategory;
                bool doesnotExistInSpecpoint = 
                    !Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                    !foundUnderSpecpointOnlyCategory &&
                    !foundUnderOtherSpecpointOnlyCategory;

                if (string.IsNullOrEmpty(assembly.AssemblyCode))
                {
                    // No Assembly Code Assigned to Model Element
                    row[ColumnName.Status] = CoordinationStatus.Red1;
                }
                else if (string.IsNullOrEmpty(assembly.BIMCategory))
                {
                    // Unassigned Specpoint Assembly: Link a Revit Model Element.
                    row[ColumnName.Status] = CoordinationStatus.Red4;
                }
                else
                {
                    if (doesnotExistInSpecpoint)
                    {
                        // Assembly code does not exist in Specpoint
                        row[ColumnName.Status] = CoordinationStatus.Red2;
                    }
                    else if (addedToProject)
                    {
                        // The last subassembly flag is valid in otherAssemblies ***
                        if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                            Globals.otherAssemblies[assembly.AssemblyCode].IsLastSubassembly ||
                            Globals.IsLastSubassembly(Globals.specpointOnlyCategories, assembly.AssemblyCode))
                        {
                            // Assembly is added to Specpoint Project and Model Element but no Families exist
                            // To be determined but default to Yellow1 
                            row[ColumnName.Status] = CoordinationStatus.Yellow1;
                        }
                        else
                        {
                            // Assembly code is valid, but not the lowest level of the tree 
                            row[ColumnName.Status] = CoordinationStatus.Red3;
                        }
                    }
                    else if (notAddedToProject)
                    {
                        if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                            Globals.otherAssemblies[assembly.AssemblyCode].IsLastSubassembly ||
                            Globals.IsLastSubassembly(Globals.otherSpecpointOnlyCategories, assembly.AssemblyCode))
                        {
                            // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
                            row[ColumnName.Status] = CoordinationStatus.Yellow2;
                        }
                        else
                        {
                            // Assembly code is valid, but not the lowest level of the tree 
                            row[ColumnName.Status] = CoordinationStatus.Red3;
                        }
                    }
                }

                // Has Family
                bool hasFamily = !string.IsNullOrEmpty(assembly.SpecpointFamily) &&
                    assembly.SpecpointFamily != NotSetCellText;
                if (assembly.BIMCategory != "" &&
                    assembly.AssemblyCode != "" &&
                    assembly.AssemblyDescription != "")
                {
                    if (hasFamily)
                    {
                        // Verified
                        row[ColumnName.Status] = CoordinationStatus.Green;
                    }
                    else if (addedToProject)
                    {
                        // The last subassembly flag is valid in otherAssemblies ***
                        if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                            Globals.otherAssemblies[assembly.AssemblyCode].IsLastSubassembly  ||
                            Globals.IsLastSubassembly(Globals.specpointOnlyCategories, assembly.AssemblyCode))
                        {
                            // Assembly is added to Specpoint Project and Model Element but no Families exist
                            row[ColumnName.Status] = CoordinationStatus.Yellow1;
                        }
                        else
                        {
                            // Assembly code is valid, but not the lowest level of the tree 
                            row[ColumnName.Status] = CoordinationStatus.Red3;
                        }
                    }
                    else if (notAddedToProject)
                    {
                        if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                            Globals.otherAssemblies[assembly.AssemblyCode].IsLastSubassembly ||
                            Globals.IsLastSubassembly(Globals.specpointOnlyCategories, assembly.AssemblyCode))
                        {
                            // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
                            row[ColumnName.Status] = CoordinationStatus.Yellow2;
                        }
                        else
                        {
                            // Assembly code is valid, but not the lowest level of the tree 
                            row[ColumnName.Status] = CoordinationStatus.Red3;
                        }
                    }
                }

                switch (row[ColumnName.Status])
                {
                    case CoordinationStatus.Red1:
                    case CoordinationStatus.Red2:
                    case CoordinationStatus.Red3:
                        redCount++;
                        break;
                    case CoordinationStatus.Red4:
                        break;
                    case CoordinationStatus.Yellow1:
                    case CoordinationStatus.Yellow2:
                        yellowCount++;
                        break;
                    case CoordinationStatus.Green:
                        greenCount++;
                        break;
                }


                bool visible = false;
                string bimCategory = row[ColumnName.BIMCategory].ToString();
                string revitFamily = row[ColumnName.RevitFamily].ToString();
                string revitType = row[ColumnName.RevitType].ToString();
                string assemblyCode = row[ColumnName.AssemblyCode].ToString();
                string specpointFamily = row[ColumnName.SpecpointFamily].ToString();

                if (string.IsNullOrEmpty(bimCategory) &&
                    string.IsNullOrEmpty(revitFamily) &&
                    string.IsNullOrEmpty(revitType) &&
                    !string.IsNullOrEmpty(assemblyCode))
                {
                    visible = (checkBoxUnassignedRevitElements.Checked == true);
                }
                else
                {
                    visible = visible || (checkBoxAssignedRevitElements.Checked == true);
                }

                if (string.IsNullOrEmpty(assemblyCode))
                {
                    visible = visible || (checkBoxUnassignedAssemblies.Checked == true);
                }
                else
                {
                    visible = visible || (checkBoxAssignedAssemblies.Checked == true);
                }

                if (specpointFamily != NotSetCellText)
                {
                    visible = visible || (checkBoxBoundElements.Checked == true);
                }
                else
                {
                    visible = visible || (checkBoxUnboundElements.Checked == true);
                }

                // Add row
                if (visible)
                {
                    reportDataTable.Rows.Add(row);
                }

                return true;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}\n\nPlease try to refresh the data.", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        private void grid_UnassignedSpecpointAssembly(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Assembly assembly = GetAssemblyFromRow(grid.CurrentRow.Index);

                SetBIMElementForm dlg = new SetBIMElementForm()
                {
                    StartPosition = FormStartPosition.CenterParent,
                    WindowState = FormWindowState.Normal
                };
                DialogResult ret = dlg.ShowDialog();
                if (ret == DialogResult.OK &&
                    dlg.selectedBIMElements.Count > 0)
                {
                    foreach (var uniqueId in dlg.selectedBIMElements)
                    {
                        if (Globals.revitElements.ContainsKey(uniqueId))
                        {
                            Globals.revitElements[uniqueId].AssemblyCode = assembly.AssemblyCode;
                            Globals.revitElements[uniqueId].AssemblyDescription = assembly.AssemblyDescription;

                            if (!changedRows.Contains(uniqueId))
                            {
                                changedRows.Add(uniqueId);

                                // Update list of assigned assembly codes
                                Globals.revitElements.AssignedAssemblyCodes.Add(assembly.AssemblyCode);
                            }
                        }
                    }

                    Reload();
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_SpecpointFamilyClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string value = grid.CurrentCell.Value as string;
                if (string.IsNullOrEmpty(value)) return;
                if (value == NotSetCellText) return;

                Assembly assembly = GetAssemblyFromRow(grid.CurrentRow.Index);

                ViewSpecificationsForm.Mode mode = ViewSpecificationsForm.Mode.ModelValidation;
                ViewSpecificationsForm dlg = new ViewSpecificationsForm(Globals.SpecpointProjectID, assembly, mode)
                {
                    RevitAssemblyCode = assembly.AssemblyCode,
                    SpecpointFamilyNumber = assembly.SpecpointFamilyNumber,
                    SpecpointFamily = assembly.SpecpointFamily,
                    StartPosition = FormStartPosition.CenterParent,
                    WindowState = FormWindowState.Normal
                };
                DialogResult ret = dlg.ShowDialog();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_AssemblyCodeDescClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (grid.CurrentRow.Cells[ColumnIndex.BIMCategory].Value.ToString() == "")
                {
                    grid_EndEdit();
                    return;
                }

                using (new WaitCursor(controls))
                {
                    assemblyBeforeEdit = grid.CurrentRow.Cells[ColumnIndex.AssemblyCode].Value.ToString();

                    // Let user edit cell value.    
                    this.grid.BeginEdit(true);

                    // If cell hosts a combo box, drop the list down so user doesn't
                    // have to click a second time for it.
                    DataGridViewComboBoxEditingControl comboBox =
                        this.grid.EditingControl as DataGridViewComboBoxEditingControl;
                    if (null != comboBox)
                    {
                        // Remove italics that might have been added to indicate no
                        // value was set.
                        comboBox.Font = new Font(comboBox.Font, FontStyle.Regular);
                        comboBox.DroppedDown = true;
                    }
                }

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void OnTokenExpired(string func)
        {
            this.Close();
            Globals.OnTokenExpired();
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (null == grid.CurrentCell) return;
                if (grid.IsCurrentCellInEditMode) return;

                string bimCategory = grid.CurrentRow.Cells[ColumnIndex.BIMCategory].Value.ToString();
                if (string.IsNullOrEmpty(bimCategory) &&
                    (grid.CurrentCell.ColumnIndex == ColumnIndex.BIMCategory ||
                    grid.CurrentCell.ColumnIndex == ColumnIndex.RevitFamily ||
                    grid.CurrentCell.ColumnIndex == ColumnIndex.RevitType))
                {
                    if (e.RowIndex == -1) return; // Header row was clicked

                    grid_UnassignedSpecpointAssembly(sender, e);
                }

                // Specpoint Family column
                else if (grid.CurrentCell.ReadOnly &&
                    grid.CurrentCell.ColumnIndex == ColumnIndex.SpecpointFamily)
                {
                    if (e.RowIndex == -1) return; // Header row was clicked

                    grid_SpecpointFamilyClick(sender, e);
                }

                // Assembly Code and Description columns
                else if (grid.CurrentCell.ColumnIndex == ColumnIndex.AssemblyCode ||
                    grid.CurrentCell.ColumnIndex == ColumnIndex.AssemblyDescription)
                {
                    if (e.RowIndex == -1) return; // Header row was clicked

                    grid_AssemblyCodeDescClick(sender, e);
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            try
            {
                // Tag Assembly rows with their assembly and Paramter rows with their parameter
                foreach (DataGridViewRow row in grid.Rows)
                {
                    // Assign Assembly ElementId to the row
                    Assembly assembly = GetAssemblyFromRow(row.Index);
                    row.Tag = assembly;

                    string bimCategory = row.Cells[ColumnName.BIMCategory].Value.ToString();
                    string assemblyCode = row.Cells[ColumnName.AssemblyCode].Value.ToString();
                    string assemblyDescription = row.Cells[ColumnName.AssemblyDescription].Value.ToString();
                    string family = row.Cells[ColumnName.SpecpointFamily].Value.ToString();

                    // Check if code is added  to the project
                    bool foundUnderSpecpointOnlyCategory = 
                        Globals.FindAssemblyByCode(Globals.specpointOnlyCategories, assemblyCode) != null;

                    // Check if code is NOT added  to the project
                    bool foundUnderOtherSpecpointOnlyCategory =
                        Globals.FindAssemblyByCode(Globals.otherSpecpointOnlyCategories, assemblyCode) != null;

                    bool addedToProject =
                        Globals.lastSubAssemblies.ContainsKey(assembly.AssemblyCode) ||
                        Globals.mapAddedToProject.ContainsKey(assembly.AssemblyCode) ||
                        foundUnderSpecpointOnlyCategory;
                    bool notAddedToProject =
                        Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) ||
                        foundUnderOtherSpecpointOnlyCategory;
                    bool doesnotExistInSpecpoint =
                        !Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                        !foundUnderSpecpointOnlyCategory &&
                        !foundUnderOtherSpecpointOnlyCategory;

                    if (assemblyCode == "")
                    {
                        // Pastel red
                        // If No Assembly Code Assigned
                        row.DefaultCellStyle.BackColor = pastelRed;
                    }
                    else if (bimCategory == "")
                    {
                        row.DefaultCellStyle.BackColor = pastelWhite;
                    }
                    else
                    {
                        if (doesnotExistInSpecpoint)
                        {
                            // Assembly code does not exist in Specpoint
                            row.DefaultCellStyle.BackColor = pastelRed;
                        }
                        else if (addedToProject)
                        {
                            // The last subassembly flag is valid in otherAssemblies ***
                            if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                                Globals.otherAssemblies[assemblyCode].IsLastSubassembly ||
                                Globals.IsLastSubassembly(Globals.otherSpecpointOnlyCategories, assemblyCode))
                            {
                                // Assembly is added to Specpoint Project and Model Element but no Families exist
                                // To be determined but default to Yellow1 
                                row.DefaultCellStyle.BackColor = pastelYellow;
                            }
                            else
                            {
                                // Assembly code is valid, but not the lowest level of the tree 
                                row.DefaultCellStyle.BackColor = pastelRed;
                            }
                        }
                        else if (notAddedToProject)
                        {
                            if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                                Globals.otherAssemblies[assemblyCode].IsLastSubassembly ||
                                Globals.IsLastSubassembly(Globals.otherSpecpointOnlyCategories, assemblyCode))
                            {
                                // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
                                row.DefaultCellStyle.BackColor = pastelYellow;
                            }
                            else
                            {
                                // Assembly code is valid, but not the lowest level of the tree 
                                row.DefaultCellStyle.BackColor = pastelRed;
                            }
                        }
                    }

                    // Has Family
                    bool hasFamily = !string.IsNullOrEmpty(family) &&
                        family != NotSetCellText;
                    if (bimCategory != "" &&
                        assemblyCode != "" &&
                        assemblyDescription != "")
                    {
                        if (hasFamily)
                        {
                            // Assembly is added to the model element
                            // Exists in the Specpoint project, 
                            // Has Families attached to the Specpoint project
                            row.DefaultCellStyle.BackColor = pastelGreen;
                        }
                        else if (addedToProject)
                        {
                            // The last subassembly flag is valid in otherAssemblies ***
                            if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                                Globals.otherAssemblies[assemblyCode].IsLastSubassembly ||
                                Globals.IsLastSubassembly(Globals.otherSpecpointOnlyCategories, assemblyCode))
                            {
                                // Assembly is added to Specpoint Project and Model Element but no Families exist
                                row.DefaultCellStyle.BackColor = pastelYellow;
                            }
                            else
                            {
                                // Assembly code is valid, but not the lowest level of the tree 
                                row.DefaultCellStyle.BackColor = pastelRed;
                            }
                        }
                        else if (notAddedToProject)
                        {
                            if (Globals.otherAssemblies.ContainsKey(assembly.AssemblyCode) &&
                                Globals.otherAssemblies[assemblyCode].IsLastSubassembly ||
                                Globals.IsLastSubassembly(Globals.otherSpecpointOnlyCategories, assemblyCode))

                            {
                                // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
                                row.DefaultCellStyle.BackColor = pastelYellow;
                            }
                            else
                            {
                                // Assembly code is valid, but not the lowest level of the tree 
                                row.DefaultCellStyle.BackColor = pastelRed;
                            }
                        }
                    }

                    if (hasFamily)
                    {
                        // Make family hyperlink blue
                        row.Cells[ColumnName.SpecpointFamily] = new DataGridViewLinkCell();
                        DataGridViewLinkCell cell = row.Cells[ColumnName.SpecpointFamily] as DataGridViewLinkCell;
                        cell.LinkColor = Color.Blue;
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Assembly GetAssemblyFromRow(int rowIndex)
        {
            try
            {
                if (grid.Rows.Count == 0) return null;

                Assembly assembly = new Assembly();

                DataGridViewRow row = grid.Rows[rowIndex];


                assembly.BIMCategory = row.Cells[ColumnName.BIMCategory].Value.ToString();
                assembly.RevitFamily = row.Cells[ColumnName.RevitFamily].Value.ToString();
                assembly.RevitType = row.Cells[ColumnName.RevitType].Value.ToString();
                assembly.RevitTypeId = row.Cells[ColumnName.RevitTypeId].Value.ToString();
                assembly.AssemblyCode = row.Cells[ColumnName.AssemblyCode].Value.ToString();
                assembly.AssemblyDescription = row.Cells[ColumnName.AssemblyDescription].Value.ToString();
                assembly.SpecpointFamilyNumber = row.Cells[ColumnName.SpecpointFamilyNumber].Value.ToString();
                assembly.SpecpointFamily = row.Cells[ColumnName.SpecpointFamily].Value.ToString();

                if (NotSetCellText == assembly.BIMCategory) assembly.BIMCategory = "";
                if (NotSetCellText == assembly.RevitFamily) assembly.RevitFamily = "";
                if (NotSetCellText == assembly.RevitType) assembly.RevitType = "";
                if (NotSetCellText == assembly.AssemblyCode) assembly.AssemblyCode = "";
                if (NotSetCellText == assembly.AssemblyDescription) assembly.AssemblyDescription = "";
                if (NotSetCellText == assembly.SpecpointFamilyNumber) assembly.SpecpointFamilyNumber = "";
                if (NotSetCellText == assembly.SpecpointFamily) assembly.SpecpointFamily = "";

                return assembly;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetAssemblyFromRow));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return null;
        }

        private void grid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Make sure a cell was actually double-clicked.
                if (e.ColumnIndex < 0 || e.ColumnIndex >= grid.Columns.Count ||
                    e.RowIndex < 0 || e.RowIndex >= grid.Rows.Count)
                {
                    return;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxUnboundElements_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxUnboundElements_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxBoundElements_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxBoundElements_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxUnassignedAssemblies_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxUnassignedAssemblies_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxAssignedAssemblies_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxAssignedAssemblies_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxUnlinked_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxUnlinked_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                DataGridView grid = (DataGridView)sender;

                if (this.formIsLoading == true)
                {
                    using (Graphics g = e.Graphics)
                    {
                        using (Font font2 = new Font("Arial", 12, FontStyle.Regular, GraphicsUnit.Point))
                        {
                            Rectangle rect = new Rectangle(0, 0, grid.Width, grid.Height);

                            // Create a TextFormatFlags with word wrapping, horizontal center and
                            // vertical center specified.
                            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                                TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

                            // Draw the text and the surrounding rectangle.
                            TextRenderer.DrawText(e.Graphics, loadingMsg, font2, rect, Color.Black, flags);
                            g.DrawRectangle(Pens.Transparent, rect);
                        }
                    }
                }
                else
                {
                    bool hasVisibleRow = false;
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (row.Visible == true)
                        {
                            hasVisibleRow = true;
                            break;
                        }
                    }

                    if (hasVisibleRow == false)
                    {
                        using (Graphics g = e.Graphics)
                        {
                            g.DrawString("No data to display", new Font("Arial", 12), Brushes.Black,
                                new PointF(grid.Width / 2 - 50, grid.Height / 2));
                        }
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_Resize(object sender, EventArgs e)
        {
            try
            {
                DataGridView grid = (DataGridView)sender;
                grid.Invalidate();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private string GetRevitCategoryFromAssociatedCategory(UniformatClassification value)
        {
            try
            {
                if (value.associatedCategories.Count == 0) return string.Empty;

                // Return the first one
                return value.associatedCategories[0].revitCategory;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetRevitCategoryFromAssociatedCategory));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return string.Empty;
        }

        private string GetRevitCategoryFromAssociatedCategoryId(UniformatClassification value)
        {
            try
            {
                if (value.associatedCategoryIds.Count == 0) return string.Empty;

                // Return the first one
                string id = value.associatedCategoryIds[0];

                foreach (var c in Globals.specpointCategories)
                {
                    if (c.Value.id == id)
                    {
                        // return string.Format("{0}\t{1}", c.Value.revitCategory, c.Value.name);
                        return c.Value.revitCategory;
                    }
                }

                return string.Empty;
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(GetRevitCategoryFromAssociatedCategoryId));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return string.Empty;
        }

        private async void buttonExportAssemblyCodes_Click(object sender, EventArgs e)
        {
            try
            {
                using (new WaitCursor(controls))
                {
                    toolStripStatusLabel.Text = "Loading..";

                    Query query = new Query(title);
                    GetAllUniformatClassifications results = await query.allUniformatClassifications();

                    if (results == null) return;

                    SortedDictionary<string, UniformatClassification> level1 = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level2 = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level3 = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level4 = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level5 = new SortedDictionary<string, UniformatClassification>();

                    SortedDictionary<string, UniformatClassification> level2NoParent = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level3NoParent = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level4NoParent = new SortedDictionary<string, UniformatClassification>();
                    SortedDictionary<string, UniformatClassification> level5NoParent = new SortedDictionary<string, UniformatClassification>();
                    foreach (UniformatClassification item in results.allUniformatClassifications)
                    {
                        if (item.level == "1")
                        {
                            level1[item.id] = item;
                        }
                        else if (item.level == "2")
                        {
                            if (level1.ContainsKey(item.parentId))
                                level2[item.id] = item;
                            else
                                level2NoParent[item.id] = item;
                        }
                        else if (item.level == "3")
                        {
                            if (level2.ContainsKey(item.parentId))
                                level3[item.id] = item;
                            else
                                level3NoParent[item.id] = item;
                        }
                        else if (item.level == "4")
                        {
                            if (level3.ContainsKey(item.parentId))
                                level4[item.id] = item;
                            else
                                level4NoParent[item.id] = item;
                        }
                        else if (item.level == "5")
                        {
                            if (level4.ContainsKey(item.parentId))
                                level5[item.id] = item;
                            else
                                level5NoParent[item.id] = item;
                        }
                    }

                    // Compose the lines to write
                    SortedDictionary<string, string> sortedLines = new SortedDictionary<string, string>();
                    foreach (var line1 in level1)
                    {
                        string revitCategory1 = GetRevitCategoryFromAssociatedCategoryId(line1.Value);
                        sortedLines[line1.Value.code] = string.Format("{0}\t{1}\t{2}\t{3}",
                            line1.Value.code,
                            line1.Value.description,
                            line1.Value.level,
                            revitCategory1);

                        foreach (var line2 in level2)
                        {
                            if (line2.Value.parentId != line1.Value.id) continue;

                            string revitCategory2 = GetRevitCategoryFromAssociatedCategoryId(line2.Value);
                            sortedLines[line2.Value.code] = string.Format("{0}\t{1}\t{2}\t{3}",
                                line2.Value.code,
                                line2.Value.description,
                                line2.Value.level,
                                revitCategory2);

                            foreach (var line3 in level3)
                            {
                                if (line3.Value.parentId != line2.Value.id) continue;

                                string revitCategory3 = GetRevitCategoryFromAssociatedCategoryId(line3.Value);
                                // if (revitCategory3 == "")
                                //    revitCategory3 = revitCategory2;

                                sortedLines[line3.Value.code] = string.Format("{0}\t{1}\t{2}\t{3}",
                                    line3.Value.code,
                                    line3.Value.description,
                                    line3.Value.level,
                                    revitCategory3);

                                foreach (var line4 in level4)
                                {
                                    if (line4.Value.parentId != line3.Value.id) continue;

                                    string revitCategory4 = GetRevitCategoryFromAssociatedCategoryId(line4.Value);
                                    // if (revitCategory4 == "")
                                    //    revitCategory4 = revitCategory3;

                                    sortedLines[line4.Value.code] = string.Format("{0}\t{1}\t{2}\t{3}",
                                        line4.Value.code,
                                        line4.Value.description,
                                        line4.Value.level,
                                        revitCategory4);

                                    foreach (var line5 in level5)
                                    {
                                        if (line5.Value.parentId != line4.Value.id) continue;

                                        string revitCategory5 = GetRevitCategoryFromAssociatedCategoryId(line5.Value);
                                        // if (revitCategory5 == "")
                                        //    revitCategory5 = revitCategory4;

                                        sortedLines[line5.Value.code] = string.Format("{0}\t{1}\t{2}\t{3}",
                                            line5.Value.code,
                                            line5.Value.description,
                                            line5.Value.level,
                                            revitCategory5);
                                    }
                                }
                            }
                        }
                    }

                    // Dictionary to List
                    List<string> sortedList = new List<string>();
                    foreach (var line in sortedLines)
                    {
                        sortedList.Add(line.Value);
                    }

                    SaveFileDialog dlg = new SaveFileDialog();
                    dlg.AddExtension = true;
                    dlg.InitialDirectory = Directory.GetCurrentDirectory();
                    dlg.FileName = "Specpoint_Assembly_Codes.txt";
                    dlg.Filter = "Comma-separated Values (Text file (*.txt)|*.txt|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        string output = string.Format("{0}", dlg.FileName);
                        File.WriteAllText(output, string.Join("\n", sortedList));

                        toolStripStatusLabel.Text = "Saved successfully.";
                    }
                    else
                    {
                        toolStripStatusLabel.Text = "";
                    }
                }

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonExportAssemblyCodes_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void ModelValidationForm_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Pen pen = new Pen(Color.Gray);
                e.Graphics.DrawLine(pen, 0, grid.Location.Y - 10, this.Width, grid.Location.Y - 10);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ModelValidationForm_Paint));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            try
            {
                // [BIM Category] ASC
                // [BIM Category] DESC
                string sort = grid.SortString;

                // Extract column and direction
                int closeBracket = sort.IndexOf(']');
                if (closeBracket == -1) return;
                string columnName = sort.Substring(1, closeBracket - 1).Trim();
                string direction = sort.Substring(closeBracket + 1).Trim();

                // Sort by Category
                DataGridViewColumn sortColumn = null;
                switch (columnName)
                {
                    case ColumnName.BIMCategory: sortColumn = grid.Columns[ColumnIndex.BIMCategory]; break;
                    case ColumnName.RevitFamily: sortColumn = grid.Columns[ColumnIndex.RevitFamily]; break;
                    case ColumnName.RevitType: sortColumn = grid.Columns[ColumnIndex.RevitType]; break;
                    case ColumnName.AssemblyCode: sortColumn = grid.Columns[ColumnIndex.AssemblyCode]; break;
                    case ColumnName.AssemblyDescription: sortColumn = grid.Columns[ColumnIndex.AssemblyDescription]; break;
                    case ColumnName.SpecpointFamilyNumber: sortColumn = grid.Columns[ColumnIndex.SpecpointFamilyNumber]; break;
                    case ColumnName.SpecpointFamily: sortColumn = grid.Columns[ColumnIndex.SpecpointFamily]; break;
                    case ColumnName.Status: sortColumn = grid.Columns[ColumnIndex.Status]; break;
                    default: return;
                }

                if (sortColumn != null)
                {
                    ListSortDirection sortDirection = direction == "ASC" ?
                        ListSortDirection.Ascending :
                        ListSortDirection.Descending;
                    grid.Sort(sortColumn, sortDirection);
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void ShowAllRows()
        {
            try
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    row.Visible = true;
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ShowAllRows));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            try
            {
                string filter = grid.FilterString;

                // Reset filters
                filters = new List<ModelValidationFilter>();

                // If no filters were set
                grid.CurrentCell = null;
                if (filter == string.Empty)
                {
                    // Show all rows
                    ShowAllRows();
                    return;
                }

                // Extract filters
                string[] and = { "AND" };
                string[] filterByColumns = filter.Split(and, StringSplitOptions.RemoveEmptyEntries);
                foreach (string filterByColumn in filterByColumns)
                {
                    ModelValidationFilter mvf = new ModelValidationFilter(filterByColumn);
                    filters.Add(mvf);
                }

                // Initialize rows indexes of visible rows
                int i = 1;
                visibleRowIndexes = new Dictionary<string, int>();

                // Traverse each row in the grid
                foreach (DataGridViewRow row in grid.Rows)
                {
                    row.Visible = true;

                    // Traverse each filter
                    bool displayByFilter = true;
                    foreach (ModelValidationFilter f in filters)
                    {
                        int index = f.GetColumnIndex();
                        string value = row.Cells[index].Value.ToString();
                        if (displayByFilter == true)
                        {
                            // If user filter by cell value
                            if (f.included != null)
                            {
                                displayByFilter = f.included.Contains(value);
                            }

                            // If text filter equal
                            else if (!string.IsNullOrEmpty(f.equal))
                            {
                                displayByFilter = value == f.equal;
                            }

                            // If text filter starts with
                            else if (!string.IsNullOrEmpty(f.contains))
                            {
                                displayByFilter = value.Contains(f.contains);
                            }

                            // If text filter starts with
                            else if (!string.IsNullOrEmpty(f.startsWith))
                            {
                                displayByFilter = value.StartsWith(f.startsWith);
                            }

                            // If text filter ends with
                            else if (!string.IsNullOrEmpty(f.endsWith))
                            {
                                displayByFilter = value.EndsWith(f.endsWith);
                            }

                            // If text filter unequal
                            else if (!string.IsNullOrEmpty(f.unequal))
                            {
                                displayByFilter = value != f.unequal;
                            }

                            // If text filter does not contain
                            else if (!string.IsNullOrEmpty(f.doesNotContain))
                            {
                                displayByFilter = !value.Contains(f.doesNotContain);
                            }

                            // If text filter does not start with
                            else if (!string.IsNullOrEmpty(f.doesNotStartWith))
                            {
                                displayByFilter = !value.StartsWith(f.doesNotStartWith);
                            }

                            // If text filter does not end with
                            else if (!string.IsNullOrEmpty(f.doesNotEndWith))
                            {
                                displayByFilter = !value.EndsWith(f.doesNotEndWith);
                            }
                        }
                    }

                    row.Visible = displayByFilter;
                    if (row.Visible)
                    {
                        // Save row index of visible row
                        string familyNumber = row.Cells[ColumnName.SpecpointFamilyNumber].Value.ToString();
                        string key = string.Format("{0}|{1}", row.Tag.ToString(), familyNumber);
                        visibleRowIndexes[key] = i++;
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void labelCircleVerified_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Rectangle rect = ((Control)sender).ClientRectangle;
                rect.Height = rect.Width;
                var brush = new SolidBrush(labelGreen);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, rect);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(labelCircleVerified_Paint));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void labelCircleCaution_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Rectangle rect = ((Control)sender).ClientRectangle;
                rect.Height = rect.Width;
                var brush = new SolidBrush(labelYellow);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, rect);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(labelCircleCaution_Paint));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void labelCircleNeeds_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Rectangle rect = ((Control)sender).ClientRectangle;
                rect.Height = rect.Width;
                var brush = new SolidBrush(labelRed);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(brush, rect);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(labelCircleNeeds_Paint));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        // Draw rox index in row header
        private void grid_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                var rowIdx = (e.RowIndex + 1).ToString();

                // If filters were specified
                if (filters != null && filters.Count > 0)
                {
                    var row = grid.Rows[e.RowIndex];

                    // Save row index of visible row
                    string familyNumber = row.Cells[ColumnName.SpecpointFamilyNumber].Value.ToString();
                    string key = string.Format("{0}|{1}", row.Tag.ToString(), familyNumber);

                    // If row is visible after filter
                    if (visibleRowIndexes.ContainsKey(key))
                    {
                        // Display the visible row index instead
                        rowIdx = visibleRowIndexes[key].ToString();
                    }
                }

                var centerFormat = new StringFormat()
                {
                    // right alignment might actually make more sense for numbers
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
                e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private void ModelValidationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    // Force it to close
                    e.Cancel = false;
                }

                // Known Revit Issue
                // Set focus on Revit App because it loses it when modeless dialog closes
                Process revitApp = Process.GetCurrentProcess();
                SetForegroundWindow(revitApp.MainWindowHandle);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ModelValidationForm_FormClosing));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void ModelValidationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                // Allow Revit Model Validation button to be enabled
                Globals.modelValidationForm = null;

                this.uidocument.RefreshActiveView();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(ModelValidationForm_FormClosed));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public bool UpdateSyncSubassemblies()
        {
            try
            {
                using (new WaitCursor(controls))
                {
                    // Get sorted all rows
                    List<DataGridViewRow> rows =
                        grid.Rows.Cast<DataGridViewRow>().OrderBy(row => row.Index).ToList();

                    List<string> uniqueCodes = new List<string>();
                    foreach (DataGridViewRow row in rows)
                    {
                        if (insertingAssemblies == false) break;

                        string bimCategory = row.Cells[ColumnName.BIMCategory].Value.ToString();
                        if (bimCategory == "") continue;

                        string coordinationStatus = row.Cells[ColumnName.Status].Value.ToString();
                        if (coordinationStatus == CoordinationStatus.Red4) continue;

                        string assemblyCode = row.Cells[ColumnName.AssemblyCode].Value.ToString();

                        // Add Assembly in the Specpoint Project
                        if (coordinationStatus == CoordinationStatus.Yellow2 ||
                            // The last Sub-Assembly must be associated to the Revit Family
                            coordinationStatus == CoordinationStatus.Red3)
                        {
                            // Filter out duplicate adds
                            if (uniqueCodes.Contains(assemblyCode)) continue;

                            uniqueCodes.Add(assemblyCode);
                            toolStripStatusLabel.Text = string.Format("Adding Assembly({0}) into Specpoint Project", assemblyCode);
                            
                            Globals.Log.Write(toolStripStatusLabel.Text);

                            ProjectElementNode nodeToAdd = Globals.otherAssembliesByNode[assemblyCode];

                            Globals.nodesToAdd.Add(nodeToAdd);

                        }
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        public async ValueTask<bool> InsertSubassemblies()
        {
            try
            {
                using (new WaitCursor(controls))
                {
                    // During update sync get sorted all rows
                    // Otherwise get sorted selected rows
                    List<DataGridViewRow> rows = IsUpdate ?
                        grid.Rows.Cast<DataGridViewRow>().OrderBy(row => row.Index).ToList() :
                        (from DataGridViewRow row in grid.SelectedRows
                         where !row.IsNewRow
                         orderby row.Index
                         select row).ToList<DataGridViewRow>();

                    List<string> uniqueCodes = new List<string>();
                    foreach (DataGridViewRow row in rows)
                    {
                        if (insertingAssemblies == false) break;

                        string assemblyCode = row.Cells[ColumnName.AssemblyCode].Value.ToString();
                        if (Globals.otherAssembliesByNode.ContainsKey(assemblyCode))
                        {
                            // Filter out duplicate adds
                            if (uniqueCodes.Contains(assemblyCode)) continue;

                            uniqueCodes.Add(assemblyCode);
                            toolStripStatusLabel.Text = string.Format("Adding Assembly({0}) into Specpoint Project", assemblyCode);

                            ProjectElementNode nodeToAdd = Globals.otherAssembliesByNode[assemblyCode];

                            // If updating, just add to the list of nodes to add
                            if (IsUpdate)
                            {
                                Globals.nodesToAdd.Add(nodeToAdd);
                            }
                            else
                            {
                                addedAssemblyToSP = await Globals.AddAssemblyToProject(projectId, nodeToAdd);
                            }

                            if (addedAssemblyToSP == false)
                            {
                                toolStripStatusLabel.Text = string.Format("Unable to insert Row({0}) Assembly({1})", row.Index + 1, assemblyCode);
                                break;
                            }
                        }
                    }
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (TargetInvocationException ex)
            {
                string msdg = ex.Message;
                throw new TokenExpiredException(ex.Message);
            }
            catch (Exception ex)
            {
                string func = MethodBase.GetCurrentMethod().Name;
                string msg = string.Format("{0} {1}", func, ex.Message);
                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private void Add()
        {
            try
            {
                buttonAdd.Enabled = false;
                toolStripStatusLabel.Text = "Inserting Last Subassemblies ... ";
                Globals.insertSubAssembliesProgress = new InsertSubassembliesProgress()
                {
                    Owner = this
                };
                DialogResult result = Globals.insertSubAssembliesProgress.ShowDialog();
                if (result == DialogResult.Cancel)
                {
                    if (IsUpdate)
                    {
                        // Reset flag
                        IsUpdate = false;
                        SpecpointRegistry.SetValue("UpdateSync", "0");
                    }

                    Close();
                    return;
                }

                if (IsUpdate)
                {
                    // Reset flag
                    IsUpdate = false;

                    Globals.ProcessNodesToAddInThread(projectId);

                    this.Close();

                }
                else if (DialogResult.OK == result)
                {
                    // If there were no changes to the Revit Model, reload form
                    if (changedRows.Count == 0)
                    {
                        // Reload page
                        Globals.ResetGlobals();
                        Init();
                    }

                    // If there were changes to the Revit Model, Save document and then reload form.
                    else
                    {
                        // Reload form happens after Save doc.
                        // See ModelValidationHandler
                        changedRows.Clear();
                    }

                    toolStripStatusLabel.Text = "Done.";
                }
                else
                {
                    toolStripStatusLabel.Text = string.Format("{0}. User Cancelled", toolStripStatusLabel.Text);
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(Add));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bool saveChanges = AskSaveChangesToRevitModel();

                if (saveChanges == true)
                {
                    Add();
                }

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonAdd_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in grid.SelectedRows)
                {
                    // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
                    string status = row.Cells[ColumnName.Status].Value.ToString();
                    if (status != CoordinationStatus.Yellow2)
                    {
                        row.Selected = false;
                    }
                }

                EnableButtonAdd();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        public void grid_EndEdit()
        {
            // Prevent re-entrancy
            if (afterEdit == true) return;

            try
            {
                afterEdit = true;

                if (grid.IsCurrentCellInEditMode)
                {
                    grid.EndEdit();
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                afterEdit = false;
            }
        }

        private void grid_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_ColumnDividerWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void grid_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                grid_EndEdit();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(grid_Scroll));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private static uint WM_CLOSE = 0x10;
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            try
            {
                if (m.Msg == WM_CLOSE)
                {
                    grid_EndEdit();
                }

                // If this is WM_QUERYENDSESSION, the closing event should be
                // raised in the base WndProc.
                base.WndProc(ref m);
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(WndProc));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxUnassignedRevitElements_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxUnassignedRevitElements_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void checkBoxAssignedRevitElements_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                LoadTable();
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(checkBoxAssignedRevitElements_CheckedChanged));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private void buttonCancelUpdate_Click(object sender, EventArgs e)
        {
            SpecpointRegistry.SetValue("UpdateSync", "0");
        }
    }

    public class ModelValidationFilter
    {
        public string columnName;
        public string command;
        public List<string> included;

        public string equal;
        public string contains;
        public string startsWith;
        public string endsWith;

        public string unequal;
        public string doesNotContain;
        public string doesNotStartWith;
        public string doesNotEndWith;

        public ModelValidationFilter()
        {
            included = null;
        }

        public ModelValidationFilter(string filter)
        {
            // Extract column and command
            // ([BIM Category] IN('Curtain Panels', 'Detail Items', 'Doors', 'Floors', 'Lighting Fixtures'))
            int openBracket = filter.IndexOf('[');
            int closeBracket = filter.IndexOf(']');
            if (openBracket == -1 || closeBracket == -1) return;
            columnName = filter.Substring(openBracket + 1, closeBracket - openBracket - 1).Trim();
            command = filter.Substring(closeBracket + 1).Trim();

            // IN command
            bool inCommand = command.StartsWith("IN");
            if (inCommand)
            {
                included = new List<string>();
                int open = command.IndexOf('(');
                int close = command.IndexOf(')');
                string v = command.Substring(open + 1, close - open - 1).Replace("\'", "");
                string[] values = v.Split(',');
                foreach (var item in values)
                {
                    included.Add(item.Trim());
                }
            }

            // equal
            bool equalCommand = command.StartsWith("LIKE");
            if (equalCommand)
            {
                int open = command.IndexOf("'");
                int close = command.LastIndexOf("'");
                equal = command.Substring(open + 1, close - open - 1);

                // contains
                if (equal.StartsWith("%") &&
                    equal.EndsWith("%"))
                {
                    contains = equal.Replace("%", "");
                    equal = null;
                }

                // start with
                else if (equal.EndsWith("%"))
                {
                    startsWith = equal.Substring(0, equal.Length - 1);
                    equal = null;
                }

                // ends with
                else if (equal.StartsWith("%"))
                {
                    endsWith = equal.Substring(1, equal.Length - 1);
                    equal = null;
                }
            }

            bool unequalCommand = command.StartsWith("NOT LIKE");
            if (unequalCommand)
            {
                int open = command.IndexOf("'");
                int close = command.LastIndexOf("'");
                unequal = command.Substring(open + 1, close - open - 1);

                // does not contain
                if (unequal.StartsWith("%") &&
                    unequal.EndsWith("%"))
                {
                    doesNotContain = unequal.Replace("%", "");
                    unequal = null;
                }

                // does not start with
                else if (unequal.EndsWith("%"))
                {
                    doesNotStartWith = unequal.Substring(0, unequal.Length - 1);
                    unequal = null;
                }

                // does not end with
                else if (unequal.StartsWith("%"))
                {
                    doesNotEndWith = unequal.Substring(1, unequal.Length - 1);
                    unequal = null;
                }
            }
        }

        public int GetColumnIndex()
        {
            // Get column index
            int index = ColumnIndex.BIMCategory;
            switch (columnName)
            {
                case ColumnName.BIMCategory: index = ColumnIndex.BIMCategory; break;
                case ColumnName.RevitFamily: index = ColumnIndex.RevitFamily; break;
                case ColumnName.RevitType: index = ColumnIndex.RevitType; break;
                case ColumnName.AssemblyCode: index = ColumnIndex.AssemblyCode; break;
                case ColumnName.AssemblyDescription: index = ColumnIndex.AssemblyDescription; break;
                case ColumnName.SpecpointFamilyNumber: index = ColumnIndex.SpecpointFamilyNumber; break;
                case ColumnName.SpecpointFamily: index = ColumnIndex.SpecpointFamily; break;
                case ColumnName.Status: index = ColumnIndex.Status; break;
            }

            return index;
        }
    }
}
