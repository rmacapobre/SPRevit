using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using FontStyle = System.Drawing.FontStyle;
using Form = System.Windows.Forms.Form;
using MessageBox = System.Windows.Forms.MessageBox;
using Size = System.Drawing.Size;

namespace Specpoint.Revit2026
{
    public partial class KeynotesManagerForm : Form
    {
        private string title = "Keynotes Manager";

        private Document _doc;
        private string _masterKeynoteFile;

        // Elements
        private TreeNode _rootElements;

        private Dictionary<string, TreeNode> _categories;
        private Dictionary<string, TreeNode> _elements;
        private Dictionary<string, TreeNode> _elementsKeynoteNodes;
        private List<TreeNode> _elementsBoldNodes;

        // Keynotes
        private TreeNode _rootKeynotes;
        private Dictionary<string, TreeNode> _nodesByID;
        private Dictionary<string, string> _keynoteNamesById;
        private Dictionary<string, TreeNode> _keynoteNodes;
        private Dictionary<string, string> _keynoteBaseElementIds;
        private List<TreeNode> _keynotesBoldNodes;
        private string _familyNumberFormat;

        // Material
        private TreeNode _rootMaterials;
        private List<string> _revitMaterialClasses;
        private Dictionary<string, AssemblyMaterial> _revitMaterials;
        private Dictionary<Assembly, TreeNode> _materialNodes;
        private Dictionary<string, TreeNode> _materialKeynoteNodes;
        private Dictionary<string, TreeNode> _materialClassNodes;
        private List<TreeNode> _materialsBoldNodes;

        // Find
        private string previousFind;
        private int findIndex;
        private List<TreeNode> findResults;
        TreeNode lastMarkedNode;

        public List<string> IncludedCategories { get; set; }

        public string RevitModel { get; set; }

        private ImageList imageList;

        // This is used to allow form to go back to the previous category filter
        private string previousFilter;

        public KeynotesManagerForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            // Elements
            _categories = new Dictionary<string, TreeNode>();
            _elements = new Dictionary<string, TreeNode>();
            _elementsKeynoteNodes = new Dictionary<string, TreeNode>();
            _elementsBoldNodes = new List<TreeNode>();

            // Keynotes
            _nodesByID = new Dictionary<string, TreeNode>();
            _keynoteNamesById = new Dictionary<string, string>();
            _keynoteNodes = new Dictionary<string, TreeNode>();
            _keynotesBoldNodes = new List<TreeNode>();
            _keynoteBaseElementIds = new Dictionary<string, string>();

            // Materials
            _materialNodes = new Dictionary<Assembly, TreeNode>();
            _materialKeynoteNodes = new Dictionary<string, TreeNode>();
            _materialsBoldNodes = new List<TreeNode>();
            _revitMaterialClasses = new List<string>();
            _materialClassNodes = new Dictionary<string, TreeNode>();
        }

        public KeynotesManagerForm(Document doc,
                Dictionary<string, AssemblyMaterial> revitMaterials,
                List<string> revitMaterialClasses, string masterKeynoteFile) : this()
        {
            _doc = doc;
            _revitMaterials = revitMaterials;
            _revitMaterialClasses = revitMaterialClasses;
            _masterKeynoteFile = masterKeynoteFile;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                Save();

                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private async void KeynotesManagerForm_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(770, 700);

            // Set filter to All Categories until persistedElement issue is fixed
            previousFilter = SpecpointRegistry.GetValue("FilterAssemblies");
            string filter = SpecpointRegistry.GetValue("FilterAssemblies");

            PopulateImageList();

            using (new WaitCursor())
            {
                if (Globals.specpointCategories == null)
                {
                    // Get list of Specpoint project categories
                    Globals.specpointCategories = new SpecpointCategories();
                    bool ret = await Globals.specpointCategories.Init();
                    if (ret == false) return;
                }

                // Create a map between Revit and Specpoint categories
                CreateCategoryMap();

                PopulateFilter();

                PopulateViews();
            }

            PopulateTrees(filter, IncludedCategories);
        }

        private void PopulateImageList()
        {
            Image divisionImage = GetResourceBitmap("Division.bmp");
            Image sectionImage = GetResourceBitmap("Section.bmp");
            Image letterImage = GetResourceBitmap("Organizer.bmp");
            Image keynoteImage = GetResourceBitmap("Keynote.bmp");
            Image assemblyImage = GetResourceBitmap("assembly.png");
            Image categoryImage = GetResourceBitmap("category.png");
            Image productImage = GetResourceBitmap("product.png");

            imageList = new ImageList();
            imageList.Images.Add(divisionImage);
            imageList.Images.Add(sectionImage);
            imageList.Images.Add(letterImage);
            imageList.Images.Add(keynoteImage);
            imageList.Images.Add(assemblyImage);
            imageList.Images.Add(categoryImage);
            imageList.Images.Add(productImage);
        }

        private async void PopulateTrees(string filter, List<string> includedCategories = null)
        {
            string view = comboBoxView.Text;

            using (new WaitCursor())
            {
                // Divisions
                if (view == KeynotesView.Divisions)
                {
                    // Get assemblies for Specpoint project
                    KeynotesManagerProgress dlg = new KeynotesManagerProgress()
                    {
                        Owner = this,
                        Title = KeynotesProgressTitle.Divisions,
                        View = view,
                        StartPosition = FormStartPosition.CenterParent
                    };
                    DialogResult dr = dlg.ShowDialog(this);
                    if (dr == DialogResult.Cancel)
                    {
                        Close();
                        return;
                    }

                    if (filter == Globals.AllCategoriesFilter)
                    {
                        PopulateKeynotesFromDivisions();
                    }
                    else
                    {
                        PopulateKeynotesFromDivisionsByCategory();
                    }
                }

                // Assemblies
                else
                {
                    if (filter == Globals.AllCategoriesFilter)
                    {
                        // Get assemblies for Specpoint project
                        KeynotesManagerProgress dlg = new KeynotesManagerProgress()
                        {
                            Owner = this,
                            Title = KeynotesProgressTitle.AllAssemblies,
                            View = view,
                            Filter = filter,
                            StartPosition = FormStartPosition.CenterParent
                        };
                        DialogResult dr = dlg.ShowDialog(this);
                        if (dr == DialogResult.Cancel)
                        {
                            Close();
                            return;
                        }

                        PopulateKeynotesFromAssemblies();
                    }
                    else if (includedCategories != null)
                    {
                        // Get assemblies for Specpoint project
                        KeynotesManagerProgress dlg = new KeynotesManagerProgress()
                        {
                            Owner = this,
                            Title = KeynotesProgressTitle.Assemblies,
                            IncludedCategories = includedCategories,
                            View = view,
                            StartPosition = FormStartPosition.CenterParent
                        };
                        DialogResult dr = dlg.ShowDialog(this);
                        if (dr == DialogResult.Cancel)
                        {
                            Close();
                            return;
                        }

                        PopulateKeynotesFromAssembliesByCategory(includedCategories);
                    }
                }

                _familyNumberFormat = await GetFamilyNumberFormat();

                PopulateTreeElements();
                PopulateTreeMaterials();

                CheckElementNodes();
                CheckMaterialNodes();

                EnableKeynoteButtons();
            }
        }

        private void PopulateFilter()
        {
            comboBoxFilter.SelectedIndexChanged -= comboBoxFilter_SelectedIndexChanged;

            comboBoxFilter.Items.Clear();
            comboBoxFilter.Items.Add(KeynotesFilter.ByFilter);
            comboBoxFilter.Items.Add(KeynotesFilter.AllCategories);

            foreach (var c in Globals.specpointCategories)
            {
                if (!Globals.mapCategoryIDs.ContainsKey(c.Value.revitCategory)) continue;
                if (!Globals.revitCategories.ContainsKey(c.Key)) continue;

                comboBoxFilter.Items.Add(c.Key);
            }

            string filter = SpecpointRegistry.GetValue("FilterAssemblies");

            // Disable filter for now while persisted element issue is fixed
            // comboBoxFilter.Enabled = false;

            comboBoxFilter.SelectedIndex = (filter == Globals.AllCategoriesFilter) ? 1 : 0;

            comboBoxFilter.SelectedIndexChanged += new EventHandler(comboBoxFilter_SelectedIndexChanged);

        }

        private void CreateCategoryMap()
        {
            // If map already exists, exit
            if (Globals.mapCategoryIDs != null) return;

            // Create map
            Globals.mapCategoryIDs = new RevitSpecpointCategoryMap();
            Globals.mapCategoryIDs.Init();
        }

        private void PopulateViews()
        {
            comboBoxView.SelectedIndexChanged -= this.comboBoxView_SelectedIndexChanged;

            comboBoxView.Items.Clear();
            comboBoxView.Items.Add(KeynotesView.Assemblies);
            comboBoxView.Items.Add(KeynotesView.Divisions);
            // comboBoxView.Items.Add(KeynotesView.AssembliesFile);
            // comboBoxView.Items.Add(KeynotesView.DivisionsFile);
            comboBoxView.SelectedIndex = 0;

            comboBoxView.SelectedIndexChanged += new System.EventHandler(this.comboBoxView_SelectedIndexChanged);
        }

        private bool IsAKeynoteCode(string line)
        {
            char[] dot = { '.' };
            string[] words = line.Split(dot);
            if (words.Length == 0) return false;

            // Handle 411213.A00
            if (words.Length == 2)
            {
                return Char.IsLetter(words[1], 0);
            }

            //  Handle format 411213.19.A00
            else if (words.Length == 3)
            {
                return Char.IsLetter(words[2], 0);
            }

            return false;
        }

        private TreeNode GetKeynoteLetterNode(TreeNode node, string letter)
        {
            // If there are no letter nodes yet
            if (node.Nodes.Count == 0)
            {
                // Create a letter node 
                TreeNode letterNode = node.Nodes.Add(letter);
                letterNode.Tag = node.Tag;
                return letterNode;
            }

            // Find if letter node already exists
            foreach (TreeNode child in node.Nodes)
            {
                if (child.Text == letter)
                    return child;
            }

            // Otherwise, create a new letter node
            TreeNode newLetterNode = node.Nodes.Add(letter);
            newLetterNode.Tag = node.Tag;
            return newLetterNode;
        }

        private void SetTreeNodeImage(TreeNode treeNode, int index)
        {
            treeNode.ImageIndex = index;
            treeNode.SelectedImageIndex = index;
        }

        private void SetTreeNodeImage(ProjectElementNode node, TreeNode treeNode)
        {
            if (node.elementType == ProjectElementType.ASSEMBLY)
            {
                treeNode.ImageIndex = KeynotesTreeImageIndex.Assembly;
                treeNode.SelectedImageIndex = KeynotesTreeImageIndex.Assembly;
            }
            else if (node.elementType == ProjectElementType.PRODUCTFAMILY)
            {
                treeNode.ImageIndex = KeynotesTreeImageIndex.Section;
                treeNode.SelectedImageIndex = KeynotesTreeImageIndex.Section;
            }
            else if (node.elementType == ProjectElementType.PRODUCTTYPE)
            {
                treeNode.ImageIndex = KeynotesTreeImageIndex.ProductType;
                treeNode.SelectedImageIndex = KeynotesTreeImageIndex.ProductType;
            }
        }

        /// <summary>
        /// Populate tree from Specpoint Assemblies
        /// </summary>
        private void PopulateKeynotesFromAssemblies()
        {
            try
            {
                treeViewKeynotes.CheckBoxes = true;
                treeViewKeynotes.Scrollable = true;
                treeViewKeynotes.Nodes.Clear();

                treeViewKeynotes.ImageList = imageList;
                treeViewKeynotes.ImageIndex = KeynotesTreeImageIndex.Category;
                treeViewKeynotes.SelectedImageIndex = KeynotesTreeImageIndex.Category;

                _rootKeynotes = null;
                _rootKeynotes = treeViewKeynotes.Nodes.Add(KeynotesFilter.AllCategories);

                _keynoteNodes = new Dictionary<string, TreeNode>();
                _keynoteNamesById = new Dictionary<string, string>();
                _nodesByID = new Dictionary<string, TreeNode>();
                _keynoteBaseElementIds = new Dictionary<string, string>();
                _keynotesBoldNodes = new List<TreeNode>();

                // Used to locate tree nodes
                string code = "";
                string desc = "";
                Dictionary<string, TreeNode> mapNodes = new Dictionary<string, TreeNode>();

                if (Globals.KeynoteAssemblies == null) return;

                foreach (var node in Globals.KeynoteAssemblies.getProjectElementNodes.projectElementNodes)
                {
                    // Ignore FAMILY GROUPS
                    if (node.text.EndsWith("FAMILY GROUPS")) continue;

                    ExtractCodeDesc(node, ref code, ref desc);

                    if (node.parentNodeId == null ||
                        (node.parentNodeId != null && node.parentNodeId == ""))
                    {
                        HiddenCheckBoxTreeNode newNode = new HiddenCheckBoxTreeNode();
                        newNode.Name = node.id;
                        newNode.Text = string.Format("{0} - {1}", code, desc);
                        SetTreeNodeImage(node, newNode);

                        _rootKeynotes.Nodes.Add(newNode);
                        mapNodes[node.id] = newNode;
                        _nodesByID[code] = newNode;
                    }
                    else
                    {
                        if (mapNodes.ContainsKey(node.parentNodeId))
                        {
                            TreeNode newNode = new TreeNode();
                            newNode.Name = node.id;
                            newNode.Text = string.Format("{0} - {1}", code, desc);
                            SetTreeNodeImage(node, newNode);

                            // If product family
                            if (node.elementType == ProjectElementType.PRODUCTFAMILY &&
                                Globals.SpecpointKeynotes.familyKeynotes.ContainsKey(node.baseElementId))
                            {
                                var keynoteNode = Globals.SpecpointKeynotes.familyKeynotes[node.baseElementId];
                                AddTreeKeynoteNode(node, keynoteNode, newNode, code, desc);
                            }

                            // If product type
                            else if (node.elementType == ProjectElementType.PRODUCTTYPE &&
                                Globals.SpecpointKeynotes.productTypeKeynotes.ContainsKey(node.baseElementId))
                            {
                                var keynoteNode = Globals.SpecpointKeynotes.productTypeKeynotes[node.baseElementId];
                                TreeNode familyNode = mapNodes[node.parentNodeId];
                                AddTreeKeynoteNode(node, keynoteNode, familyNode, code, desc);
                                mapNodes[node.id] = newNode;
                                continue;
                            }

                            mapNodes[node.parentNodeId].Nodes.Add(newNode);
                            mapNodes[node.id] = newNode;
                        }
                    }
                }

                treeViewKeynotes.CollapseAll();

                if (_rootKeynotes != null)
                {
                    _rootKeynotes.EnsureVisible();
                    _rootKeynotes.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private string ExtractLetterFromKeynoteCode(string keynoteCode)
        {
            int dot = keynoteCode.LastIndexOf('.');
            if (dot == -1) return string.Empty;

            string letter = keynoteCode[dot + 1].ToString();
            return letter;
        }

        private TreeNode GetLetterNode(TreeNode node, string letter)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Text != letter) continue;
                return childNode;
            }

            return null;
        }

        private void ExtractCodeDesc(ProjectElementNode node, ref string code, ref string desc)
        {
            char[] pipe = { '|' };
            string[] words = node.text.Split(pipe);
            if (words.Length == 1)
            {
                // Product Type
                desc = words[0];
            }
            else if (words.Length == 2)
            {
                // Assembly or Family 
                code = words[0];
                desc = words[1];
            }
        }

        private void AddTreeKeynoteNode(ProjectElementNode node, ProjectElementKeynoteNode keynoteNode,
            TreeNode newNode, string code, string desc)
        {
            string keynoteCode = keynoteNode.keynoteCode;
            string letter = ExtractLetterFromKeynoteCode(keynoteCode);

            // Add letter node 
            TreeNode letterNode = GetLetterNode(newNode, letter);
            if (letterNode == null)
            {
                // Add letter node A 
                letterNode = new TreeNode();
                letterNode.Name = node.id;
                letterNode.Text = letter;
                letterNode.Tag = code;
                SetTreeNodeImage(letterNode, KeynotesTreeImageIndex.Letter);

                newNode.Nodes.Add(letterNode);
            }

            // Add keynote for product family (A00) under letter node
            string familyId = keynoteCode;
            TreeNode familyNode = new TreeNode();
            familyNode.Name = keynoteNode.id;
            familyNode.Text = familyId + " - " + desc;
            familyNode.Tag = familyId;
            SetTreeNodeImage(familyNode, KeynotesTreeImageIndex.Keynote);
            letterNode.Nodes.Add(familyNode);

            _keynoteBaseElementIds[keynoteNode.id] = keynoteNode.baseElementId;

            if (keynoteNode.isHidden)
            {
                familyNode.ForeColor = Color.Gray;
                familyNode.NodeFont = new Font(treeViewKeynotes.Font, FontStyle.Italic);
            }
            else
            {
                familyNode.ForeColor = Color.Black;
                familyNode.NodeFont = null;
            }

            _keynoteNodes[familyId] = familyNode;
            _keynoteNamesById[familyId] = desc;
            _nodesByID[familyId] = familyNode;
        }

        /// <summary>
        /// Populate tree from Specpoint Assemblies
        /// </summary>
        private void PopulateKeynotesFromAssembliesByCategory(List<string> includedCategories)
        {
            try
            {
                treeViewKeynotes.CheckBoxes = true;
                treeViewKeynotes.Scrollable = true;
                treeViewKeynotes.Nodes.Clear();

                treeViewKeynotes.ImageList = imageList;
                treeViewKeynotes.ImageIndex = KeynotesTreeImageIndex.Category;
                treeViewKeynotes.SelectedImageIndex = KeynotesTreeImageIndex.Category;

                _rootKeynotes = null;
                _rootKeynotes = treeViewKeynotes.Nodes.Add(KeynotesFilter.ByFilter);

                _keynoteNodes = new Dictionary<string, TreeNode>();
                _keynoteNamesById = new Dictionary<string, string>();
                _nodesByID = new Dictionary<string, TreeNode>();
                _keynoteBaseElementIds = new Dictionary<string, string>();
                _keynotesBoldNodes = new List<TreeNode>();

                if (Globals.KeynoteAssembliesByCategory == null) return;

                foreach (var c in Globals.KeynoteAssembliesByCategory)
                {
                    string categoryName = c.Key;

                    // Filter out excluded categories
                    if (!includedCategories.Contains(categoryName)) continue;

                    TreeNode categoryRoot = _rootKeynotes.Nodes.Add(categoryName);
                    SetTreeNodeImage(categoryRoot, KeynotesTreeImageIndex.Category);

                    // Used to locate tree nodes
                    string code = "";
                    string desc = "";
                    Dictionary<string, TreeNode> mapNodes = new Dictionary<string, TreeNode>();
                    GetProjectElementNodes assemblies = c.Value;
                    if (assemblies == null) continue;
                    if (assemblies.getProjectElementNodes == null) continue;
                    if (assemblies.getProjectElementNodes.projectElementNodes == null) continue;
                    if (assemblies.getProjectElementNodes.projectElementNodes.Count == 0) continue;

                    foreach (var node in assemblies.getProjectElementNodes.projectElementNodes)
                    {
                        // Ignore FAMILY GROUPS
                        if (node.text.EndsWith("FAMILY GROUPS")) continue;

                        ExtractCodeDesc(node, ref code, ref desc);

                        if (node.parentNodeId == null ||
                            (node.parentNodeId != null && node.parentNodeId == ""))
                        {
                            HiddenCheckBoxTreeNode newNode = new HiddenCheckBoxTreeNode();
                            newNode.Name = node.id;
                            newNode.Text = string.Format("{0} - {1}", code, desc);
                            SetTreeNodeImage(node, newNode);

                            categoryRoot.Nodes.Add(newNode);
                            mapNodes[node.id] = newNode;
                            _nodesByID[code] = newNode;
                        }
                        else if (mapNodes.ContainsKey(node.parentNodeId))
                        {
                            TreeNode newNode = new TreeNode();
                            newNode.Name = node.id;
                            newNode.Text = string.Format("{0} - {1}", code, desc);
                            SetTreeNodeImage(node, newNode);

                            // If product family
                            if (node.elementType == ProjectElementType.PRODUCTFAMILY &&
                                Globals.SpecpointKeynotes.familyKeynotes.ContainsKey(node.baseElementId))
                            {
                                var keynoteNode = Globals.SpecpointKeynotes.familyKeynotes[node.baseElementId];
                                AddTreeKeynoteNode(node, keynoteNode, newNode, code, desc);
                            }

                            // If product type
                            else if (node.elementType == ProjectElementType.PRODUCTTYPE &&
                                Globals.SpecpointKeynotes.productTypeKeynotes.ContainsKey(node.baseElementId))
                            {
                                var keynoteNode = Globals.SpecpointKeynotes.productTypeKeynotes[node.baseElementId];
                                TreeNode familyNode = mapNodes[node.parentNodeId];
                                AddTreeKeynoteNode(node, keynoteNode, familyNode, code, desc);
                                mapNodes[node.id] = newNode;
                                continue;
                            }

                            mapNodes[node.parentNodeId].Nodes.Add(newNode);
                            mapNodes[node.id] = newNode;

                        }
                    }
                }

                treeViewKeynotes.CollapseAll();

                if (_rootKeynotes != null)
                {
                    _rootKeynotes.EnsureVisible();
                    _rootKeynotes.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void PopulateKeynotesFromDivisionsByCategory()
        {
            try
            {
                treeViewKeynotes.CheckBoxes = true;
                treeViewKeynotes.Scrollable = true;
                treeViewKeynotes.Nodes.Clear();
                _rootKeynotes.Nodes.Clear();

                _rootKeynotes = treeViewKeynotes.Nodes.Add("Specpoint");
                _keynoteNodes = new Dictionary<string, TreeNode>();
                _keynoteNamesById = new Dictionary<string, string>();
                _nodesByID = new Dictionary<string, TreeNode>();
                _keynoteBaseElementIds = new Dictionary<string, string>();
                _keynotesBoldNodes = new List<TreeNode>();

                if (Globals.KeynoteDivisions == null)
                {
                    // Loading Divisions
                    KeynotesManagerProgress form = new KeynotesManagerProgress()
                    {
                        Owner = this,
                        Title = KeynotesProgressTitle.Divisions
                    };
                    form.ShowDialog(this);
                }

                // Used to locate tree nodes
                Dictionary<string, TreeNode> mapNodes = new Dictionary<string, TreeNode>();

                if (Globals.KeynoteDivisions == null) return;

                foreach (var division in Globals.KeynoteDivisions.Nodes)
                {
                    string code = "";
                    string desc = "";
                    ExtractCodeDesc(division, ref code, ref desc);

                    HiddenCheckBoxTreeNode newNode = new HiddenCheckBoxTreeNode();
                    newNode.Name = division.id;
                    newNode.Text = string.Format("DIVISION {0} - {1}", code, desc);
                    SetTreeNodeImage(newNode, KeynotesTreeImageIndex.Division);

                    _rootKeynotes.Nodes.Add(newNode);
                    mapNodes[division.id] = newNode;
                    _nodesByID[code] = newNode;
                }

                foreach (var item in Globals.KeynoteDivisions.SectionList)
                {
                    string divisionId = item.Key;
                    List<ProjectElementNode> sections = item.Value as List<ProjectElementNode>;

                    foreach (var section in sections)
                    {
                        if (!mapNodes.ContainsKey(divisionId)) continue;

                        string code = "";
                        string desc = "";
                        ExtractCodeDesc(section, ref code, ref desc);

                        TreeNode newNode = new TreeNode();
                        newNode.Name = section.id;
                        newNode.Text = string.Format("SECTION {0} - {1}", code, desc);
                        newNode.Tag = code;
                        SetTreeNodeImage(newNode, KeynotesTreeImageIndex.Section);

                        mapNodes[divisionId].Nodes.Add(newNode);
                        mapNodes[section.id] = newNode;
                        _nodesByID[code] = newNode;

                        // Add letter node A 
                        TreeNode a = new TreeNode();
                        a.Name = section.id;
                        a.Text = "A";
                        a.Tag = code;
                        SetTreeNodeImage(a, KeynotesTreeImageIndex.Letter);
                        newNode.Nodes.Add(a);

                        // Add keynote for product family (A00)
                        string familyId = code + ".A00";
                        TreeNode familyNode = new TreeNode();
                        familyNode.Name = section.id;
                        familyNode.Text = familyId + " - " + desc;
                        familyNode.Tag = familyId;
                        SetTreeNodeImage(familyNode, KeynotesTreeImageIndex.Keynote);
                        a.Nodes.Add(familyNode);

                        _keynoteNodes[familyId] = familyNode;
                        _keynoteNamesById[familyId] = desc;
                        _nodesByID[familyId] = familyNode;

                        if (!Globals.KeynoteDivisions.ProductTypeList.ContainsKey(section.id)) continue;

                        int keynoteCodeIndex = 1;
                        var productTypes = Globals.KeynoteDivisions.ProductTypeList[section.id];
                        foreach (var productType in productTypes)
                        {
                            string trailingIndex = string.Format(".A{0}", keynoteCodeIndex.ToString("D2"));

                            // Add keynote for product type (A01 and up)
                            string productTypeId = code + trailingIndex;
                            TreeNode productTypeNode = new TreeNode();

                            productTypeNode.Name = productType.id;
                            productTypeNode.Text = productTypeId + " - " + productType.text;
                            productTypeNode.Tag = productTypeId;
                            SetTreeNodeImage(productTypeNode, KeynotesTreeImageIndex.Keynote);
                            a.Nodes.Add(productTypeNode);

                            _keynoteNodes[productTypeId] = productTypeNode;
                            _keynoteNamesById[productTypeId] = productType.text;
                            _nodesByID[productTypeId] = productTypeNode;

                            keynoteCodeIndex++;
                        }

                    }
                }

                treeViewKeynotes.CollapseAll();

                if (_rootKeynotes != null)
                {
                    _rootKeynotes.EnsureVisible();
                    _rootKeynotes.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void PopulateKeynotesFromDivisions()
        {
            try
            {
                treeViewKeynotes.CheckBoxes = true;
                treeViewKeynotes.Scrollable = true;
                treeViewKeynotes.Nodes.Clear();
                _rootKeynotes.Nodes.Clear();

                _rootKeynotes = treeViewKeynotes.Nodes.Add("Specpoint");
                _keynoteNodes = new Dictionary<string, TreeNode>();
                _keynoteNamesById = new Dictionary<string, string>();
                _nodesByID = new Dictionary<string, TreeNode>();
                _keynoteBaseElementIds = new Dictionary<string, string>();
                _keynotesBoldNodes = new List<TreeNode>();

                if (Globals.KeynoteDivisions == null)
                {
                    // Loading Divisions
                    KeynotesManagerProgress form = new KeynotesManagerProgress()
                    {
                        Owner = this,
                        Title = KeynotesProgressTitle.Divisions
                    };
                    form.ShowDialog(this);
                }

                // Used to locate tree nodes
                Dictionary<string, TreeNode> mapNodes = new Dictionary<string, TreeNode>();

                if (Globals.KeynoteDivisions == null) return;

                foreach (var division in Globals.KeynoteDivisions.Nodes)
                {
                    string code = "";
                    string desc = "";
                    ExtractCodeDesc(division, ref code, ref desc);

                    HiddenCheckBoxTreeNode newNode = new HiddenCheckBoxTreeNode();
                    newNode.Name = division.id;
                    newNode.Text = string.Format("DIVISION {0} - {1}", code, desc);
                    SetTreeNodeImage(newNode, KeynotesTreeImageIndex.Division);

                    _rootKeynotes.Nodes.Add(newNode);
                    mapNodes[division.id] = newNode;
                    _nodesByID[code] = newNode;
                }

                foreach (var item in Globals.KeynoteDivisions.SectionList)
                {
                    string divisionId = item.Key;
                    List<ProjectElementNode> sections = item.Value as List<ProjectElementNode>;

                    foreach (var section in sections)
                    {
                        if (!mapNodes.ContainsKey(divisionId)) continue;

                        string code = "";
                        string desc = "";
                        ExtractCodeDesc(section, ref code, ref desc);

                        TreeNode newNode = new TreeNode();
                        newNode.Name = section.id;
                        newNode.Text = string.Format("SECTION {0} - {1}", code, desc);
                        newNode.Tag = code;
                        SetTreeNodeImage(newNode, KeynotesTreeImageIndex.Section);

                        mapNodes[divisionId].Nodes.Add(newNode);
                        mapNodes[section.id] = newNode;
                        _nodesByID[code] = newNode;

                        // Add letter node A 
                        TreeNode a = new TreeNode();
                        a.Name = section.id;
                        a.Text = "A";
                        a.Tag = code;
                        SetTreeNodeImage(a, KeynotesTreeImageIndex.Letter);
                        newNode.Nodes.Add(a);

                        // Add keynote for product family (A00)
                        string familyId = code + ".A00";
                        TreeNode familyNode = new TreeNode();
                        familyNode.Name = section.id;
                        familyNode.Text = familyId + " - " + desc;
                        familyNode.Tag = familyId;
                        SetTreeNodeImage(familyNode, KeynotesTreeImageIndex.Keynote);
                        a.Nodes.Add(familyNode);

                        _keynoteNodes[familyId] = familyNode;
                        _keynoteNamesById[familyId] = desc;
                        _nodesByID[familyId] = familyNode;

                        if (!Globals.KeynoteDivisions.ProductTypeList.ContainsKey(section.id)) continue;

                        int keynoteCodeIndex = 1;
                        var productTypes = Globals.KeynoteDivisions.ProductTypeList[section.id];
                        foreach (var productType in productTypes)
                        {
                            string trailingIndex = string.Format(".A{0}", keynoteCodeIndex.ToString("D2"));

                            // Add keynote for product type (A01 and up)
                            string productTypeId = code + trailingIndex;
                            TreeNode productTypeNode = new TreeNode();

                            productTypeNode.Name = productType.id;
                            productTypeNode.Text = productTypeId + " - " + productType.text;
                            productTypeNode.Tag = productTypeId;
                            SetTreeNodeImage(productTypeNode, KeynotesTreeImageIndex.Keynote);
                            a.Nodes.Add(productTypeNode);

                            _keynoteNodes[productTypeId] = productTypeNode;
                            _keynoteNamesById[productTypeId] = productType.text;
                            _nodesByID[productTypeId] = productTypeNode;

                            keynoteCodeIndex++;
                        }

                    }
                }

                treeViewKeynotes.CollapseAll();

                if (_rootKeynotes != null)
                {
                    _rootKeynotes.EnsureVisible();
                    _rootKeynotes.Expand();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }



        private void CheckKeynoteNodeAncestors(TreeNode keynoteNode)
        {
            TreeNode parent = keynoteNode.Parent;
            while (parent != null)
            {
                // If tree root, exit
                TreeView tree = parent.TreeView;
                if (parent == tree.Nodes[0])
                    break;

                parent.Checked = HasCheckedNodes(parent);
                parent = parent.Parent;
            }
        }

        // Check keynote nodes that were assigned to elements
        private void CheckElementNodes()
        {
            AllowCheck();
            foreach (var element in Globals.revitElements)
            {
                Assembly assembly = element.Value as Assembly;
                if (string.IsNullOrEmpty(assembly.Keynote)) continue;

                if (_keynoteNodes != null && _keynoteNodes.ContainsKey(assembly.Keynote))
                {
                    _keynoteNodes[assembly.Keynote].Checked = true;

                    // Check keynote nodes
                    CheckKeynoteNodeAncestors(_keynoteNodes[assembly.Keynote]);
                }
            }
            AllowCheck(false);
        }

        // Check keynote nodes that were assigned to materials
        private void CheckMaterialNodes()
        {
            AllowCheck();
            foreach (var material in _materialKeynoteNodes)
            {
                TreeNode node = material.Value as TreeNode;
                string keynote = node.Tag as string;
                if (string.IsNullOrEmpty(keynote)) continue;

                if (_keynoteNodes != null && _keynoteNodes.ContainsKey(keynote))
                {
                    _keynoteNodes[keynote].Checked = true;

                    // Check keynote nodes
                    CheckKeynoteNodeAncestors(_keynoteNodes[keynote]);
                }
            }
            AllowCheck(false);
        }

        private void CheckNodeAndItsAncestors(TreeNode node)
        {
            AllowCheck();
            node.Checked = true;
            TreeNode parent = node.Parent;
            while (parent != null)
            {
                parent.Checked = true;
                parent = parent.Parent;
            }
            AllowCheck(false);
        }

        private void PopulateTreeMaterials()
        {
            treeViewMaterials.CheckBoxes = true;
            treeViewMaterials.Scrollable = true;
            treeViewMaterials.Nodes.Clear();

            // Material type
            _rootMaterials = treeViewMaterials.Nodes.Add("Material Types:");

            // Create material class treenodes
            foreach (string materialClassName in _revitMaterialClasses)
            {
                _materialClassNodes[materialClassName] = _rootMaterials.Nodes.Add(materialClassName);
            }

            // Add materials under their appropriate types.
            foreach (var m in _revitMaterials)
            {
                AssemblyMaterial material = m.Value;

                TreeNode materialNode = null;
                if (_materialClassNodes.ContainsKey(material.Class))
                {
                    // Add material under material class
                    materialNode = _materialClassNodes[material.Class].Nodes.Add(material.Name);
                }

                // If material has a keynote
                materialNode.Tag = material;
                string keynote = material.Keynote;
                if (!string.IsNullOrEmpty(material.Keynote))
                {
                    string description = "";
                    if (_keynoteNodes.ContainsKey(keynote))
                    {
                        description = _keynoteNodes[keynote].Text;
                        _keynoteNodes[keynote].Checked = true;
                    }

                    // If keynote description is in keynotes tree
                    if (!string.IsNullOrEmpty(description))
                    {
                        // Display the keynote name and description
                        TreeNode keynoteNode = materialNode.Nodes.Add(description);
                        keynoteNode.Tag = keynote;

                        _materialKeynoteNodes[material.Name] = keynoteNode;

                        CheckNodeAndItsAncestors(keynoteNode);
                    }
                    else
                    {
                        // If keynote is not in the keynotes file
                        TreeNode keynoteNode = materialNode.Nodes.Add(
                            string.Format("{0} - ?", keynote));
                        keynoteNode.Parent.ForeColor = Color.Red;
                    }
                }

                materialNode.Tag = material;
                if (!_materialNodes.ContainsKey(material))
                {
                    _materialNodes.Add(material, materialNode);
                }
                else
                {
                    _materialNodes[material] = materialNode;
                }

            }

            treeViewMaterials.CollapseAll();
            _rootMaterials.EnsureVisible();
            _rootMaterials.Expand();
        }

        private void AllowCheck(bool allow = true)
        {
            if (allow == true)
            {
                this.treeViewElements.BeforeCheck -= this.treeViewElements_BeforeCheck;
                this.treeViewMaterials.BeforeCheck -= this.treeViewMaterials_BeforeCheck;
                this.treeViewKeynotes.BeforeCheck -= this.treeViewKeynotes_BeforeCheck;
            }
            else
            {
                this.treeViewElements.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewElements_BeforeCheck);
                this.treeViewMaterials.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewMaterials_BeforeCheck);
                this.treeViewKeynotes.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewKeynotes_BeforeCheck);
            }
        }

        private void PopulateTreeElements()
        {
            treeViewElements.CheckBoxes = true;
            treeViewElements.Scrollable = true;
            treeViewElements.Nodes.Clear();
            _rootElements = treeViewElements.Nodes.Add("Elements");

            _categories = new Dictionary<string, TreeNode>();
            foreach (var element in Globals.revitElements)
            {
                Assembly assembly = element.Value;
                TreeNode categoryNode = null;
                string category = assembly.BIMCategory;
                if (_categories.ContainsKey(category))
                {
                    categoryNode = _categories[category];
                }
                else
                {
                    categoryNode = _rootElements.Nodes.Add(category);
                    _categories[category] = categoryNode;
                }

                string elementName = string.Format("{0} - {1}",
                    assembly.RevitFamily, assembly.RevitType);

                TreeNode elementNode = categoryNode.Nodes.Add(elementName);
                elementNode.Tag = assembly;
                _elements[elementName] = elementNode;

                string keynote = assembly.Keynote;
                if (string.IsNullOrEmpty(assembly.Keynote)) continue;

                string description = "";
                if (_keynoteNodes.ContainsKey(keynote))
                {
                    description = _keynoteNodes[keynote].Text;
                }

                // If code is in the keynotes tree
                if (!string.IsNullOrEmpty(description))
                {
                    // display the keynote name and description
                    TreeNode keynoteNode = elementNode.Nodes.Add(description);
                    keynoteNode.Tag = keynote;

                    _elementsKeynoteNodes[elementName] = keynoteNode;

                    CheckNodeAndItsAncestors(keynoteNode);
                }
                else
                {
                    // If keynote is not in the keynotes file
                    TreeNode keynoteNode = elementNode.Nodes.Add(
                        string.Format("{0} - ?", keynote));
                    keynoteNode.Parent.ForeColor = Color.Red;
                    keynoteNode.Parent.Expand();
                }
            }

            _rootElements.EnsureVisible();
            _rootElements.Expand();
        }

        private void DoubleClickToAssignElement()
        {
            try
            {
                // If tree is empty, do nothing
                if (treeViewKeynotes.Nodes.Count == 0) return;

                // Get keynote node to copy from
                TreeNode keynoteNode = treeViewKeynotes.SelectedNode;
                if (keynoteNode == null) return;
                if (keynoteNode.Tag == null) return;

                // A keynote node is always under a letter
                if (keynoteNode.Parent.Text.Length != 1) return;

                // Get element node 
                TreeNode elementNode = treeViewElements.SelectedNode;
                if (elementNode == null) return;
                if (elementNode.Tag == null) return;

                // If element is red from a questionable keynote code
                if (elementNode.ForeColor == Color.Red)
                {
                    elementNode.ForeColor = Color.Black;
                }

                // Add the new keynote node under element
                elementNode.Nodes.Clear();

                // Create assigned keynote node to element
                TreeNode newKeynote = elementNode.Nodes.Add(keynoteNode.Text);
                Assembly assembly = elementNode.Tag as Assembly;
                if (assembly == null) return;

                assembly.Keynote = keynoteNode.Tag as string;
                assembly.Changed = true;

                // Check relevant nodes
                AllowCheck();
                newKeynote.Checked = true;
                keynoteNode.Checked = true;
                elementNode.Checked = true;
                elementNode.Parent.Checked = true;
                CheckKeynoteNodeAncestors(keynoteNode);
                AllowCheck(false);

                // Bold relevant nodes
                BoldTreeNode(treeViewElements, newKeynote, ref _elementsBoldNodes);
                BoldTreeNode(treeViewElements, elementNode, ref _elementsBoldNodes);
                BoldTreeNode(treeViewKeynotes, keynoteNode, ref _keynotesBoldNodes);

                elementNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void DoubleClickToAssignMaterial()
        {
            // If tree is empty, do nothing
            if (treeViewKeynotes.Nodes.Count == 0) return;

            TreeNode keynoteNode = treeViewKeynotes.SelectedNode;
            if (keynoteNode == null) return;
            if (keynoteNode.Tag == null) return;

            // A keynote node is always under a letter
            if (keynoteNode.Parent.Text.Length != 1) return;

            // Add key note to an material node
            TreeNode materialNode = treeViewMaterials.SelectedNode;
            if (materialNode == null) return;

            // Add the keynote node under material
            string materialName = materialNode.Text;
            AssemblyMaterial material = materialNode.Tag as AssemblyMaterial;
            if (material == null) return;

            // If there was a previous assigned keynote
            /*
            if (!string.IsNullOrEmpty(material.Keynote) &&
                _keynoteNodes.ContainsKey(material.Keynote))
            {
                // Uncheck previous assigned keynote from Keynotes tree
                TreeNode previousKeynote = _keynoteNodes[material.Keynote];
                previousKeynote.Checked = 
                    IsAssignedToElement(material.Keynote) ||
                    IsAassignedToMaterial(material.Keynote);
            }
            */

            materialNode.Nodes.Clear();

            // If material is red from a questionable keynote code
            if (materialNode.ForeColor == Color.Red)
            {
                materialNode.ForeColor = Color.Black;
            }

            TreeNode newKeynote = materialNode.Nodes.Add(keynoteNode.Text);
            newKeynote.Tag = keynoteNode.Tag;

            material.Keynote = keynoteNode.Tag as string;
            material.Changed = true;
            _materialKeynoteNodes[material.Name] = newKeynote;

            // Check relevant nodes
            AllowCheck();
            newKeynote.Checked = true;
            keynoteNode.Checked = true;

            // Check keynote nodes
            CheckKeynoteNodeAncestors(keynoteNode);

            TreeNode materialType = materialNode.Parent;
            materialNode.Checked = true;
            materialType.Checked = true;
            AllowCheck(false);

            // Bold relevant nodes
            BoldTreeNode(treeViewMaterials, newKeynote, ref _materialsBoldNodes);
            BoldTreeNode(treeViewMaterials, materialNode, ref _materialsBoldNodes);
            BoldTreeNode(treeViewKeynotes, keynoteNode, ref _keynotesBoldNodes);

            materialNode.Expand();
        }

        private void treeViewKeynotes_DoubleClick(object sender, EventArgs e)
        {
            DoubleClickToAssignElement();
            DoubleClickToAssignMaterial();
        }

        private void BoldTreeNode(TreeView treeview, TreeNode treeNode, ref List<TreeNode> boldNodes)
        {
            Font bold = new Font(treeview.Font, FontStyle.Bold);
            treeNode.NodeFont = bold;
            treeNode.Text = treeNode.Text;

            boldNodes.Add(treeNode);
        }

        private void RegularTreeNodes()
        {
            // Remove bold nodes
            RegularTreeNode(treeViewElements, _elementsBoldNodes);
            RegularTreeNode(treeViewKeynotes, _keynotesBoldNodes);
            RegularTreeNode(treeViewMaterials, _materialsBoldNodes);
        }

        private void RegularTreeNode(TreeView treeview, List<TreeNode> boldNodes)
        {
            Font regular = new Font(treeview.Font, FontStyle.Regular);
            foreach (var node in boldNodes)
            {
                node.NodeFont = regular;
            }

            boldNodes.Clear();
        }

        private void SaveElements()
        {
            try
            {
                // Traverse the list of modified assemblies
                foreach (var e in _elements)
                {
                    TreeNode elementNode = e.Value;
                    Assembly assembly = elementNode.Tag as Assembly;
                    string revitTypeId = assembly.RevitTypeId;
                    string newKeynote = assembly.Keynote;
                    if (assembly.Changed == false) continue;

                    ElementType elemType = _doc.GetElement(revitTypeId) as ElementType;

                    Parameter p = elemType.get_Parameter(BuiltInParameter.KEYNOTE_PARAM);
                    using (Transaction t = new Transaction(_doc, "Set Element Keynote"))
                    {
                        t.Start();
                        p.Set(newKeynote);
                        t.Commit();
                    }

                    assembly.Changed = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void SaveMaterials()
        {
            try
            {
                // Traverse the list of modified materials
                foreach (var revitMaterial in _revitMaterials)
                {
                    AssemblyMaterial material = revitMaterial.Value;
                    string newKeynote = material.Keynote;
                    if (material.Changed == false) continue;

                    string name = material.Name;
                    Material m = _doc.GetMaterial(name);
                    if (m == null) continue;

                    Parameter p = m.LookupParameter("Keynote");
                    using (Transaction t = new Transaction(_doc, "Set Material Keynote"))
                    {
                        t.Start();
                        p.Set(newKeynote);
                        t.Commit();
                    }

                    material.Changed = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void Save()
        {
            // Save to Revit model
            SaveElements();
            SaveMaterials();
        }

        private string GetAssemblyNumber(string value)
        {
            char[] dash = { '-' };
            string[] words = value.Split(dash);

            return words[0].Trim();
        }

        private string GetDivisionNumber(string value)
        {
            char[] space = { ' ' };
            string[] words = value.Split(space);
            if (words.Length < 2) return string.Empty;

            return words[1];
        }

        private string GetSectionNumber(string value)
        {
            char[] space = { ' ' };
            string[] words = value.Split(space);
            if (words.Length < 2) return string.Empty;

            return words[1];
        }

        private void SaveDivisionsKeynotes(FileInfo fi)
        {
            // Create a new file
            using (StreamWriter sw = fi.CreateText())
            {
                foreach (TreeNode division in _rootKeynotes.Nodes)
                {
                    string divisionNumber = GetDivisionNumber(division.Text);
                    string divisionLine = string.Format("{0}\t{1}", divisionNumber, division.Text);
                    sw.WriteLine(divisionLine);

                    foreach (TreeNode section in division.Nodes)
                    {
                        string sectionNumber = GetSectionNumber(section.Text);
                        string sectionLine = string.Format("{0}\t{1}\t{2}", sectionNumber, section.Text, divisionNumber);
                        sw.WriteLine(sectionLine);

                        foreach (TreeNode letter in section.Nodes)
                        {
                            string letterName = letter.Text;
                            foreach (TreeNode keynote in letter.Nodes)
                            {
                                // Get keynote description
                                int dash = keynote.Text.IndexOf('-');
                                if (dash == -1) continue;
                                string code = keynote.Text.Substring(0, dash).Trim();
                                string desc = keynote.Text.Substring(dash + 1).Trim();

                                string keynoteLine = string.Format("{0}\t{1}\t{2}", code, desc, sectionNumber);
                                sw.WriteLine(keynoteLine);
                            }
                        }
                    }
                }
            }
        }

        private void SaveChildAssemblies(StreamWriter sw, TreeNode node, string parentId)
        {
            foreach (TreeNode child in node.Nodes)
            {
                string assemblyNumber = GetAssemblyNumber(child.Text);

                if (assemblyNumber.Length > 1)
                {
                    string assemblyLine = string.Format("{0}\t{1}\t{2}", assemblyNumber, child.Text, parentId);
                    sw.WriteLine(assemblyLine);

                    SaveChildAssemblies(sw, child, assemblyNumber);
                }

                // If letter node
                else if (assemblyNumber.Length == 1)
                {
                    foreach (TreeNode keynote in child.Nodes)
                    {
                        string keynoteNumber = GetAssemblyNumber(keynote.Text);
                        string keynoteLine = string.Format("{0}\t{1}\t{2}", keynoteNumber, keynote.Text, parentId);
                        sw.WriteLine(keynoteLine);
                    }
                }
            }
        }

        private void SaveAssembliesKeynotes(FileInfo fi)
        {
            // Create a new file
            using (StreamWriter sw = fi.CreateText())
            {
                foreach (TreeNode assembly in _rootKeynotes.Nodes)
                {
                    string assemblyNumber = GetAssemblyNumber(assembly.Text);
                    string assemblyLine = string.Format("{0}\t{1}", assemblyNumber, assembly.Text);
                    sw.WriteLine(assemblyLine);

                    SaveChildAssemblies(sw, assembly, assemblyNumber);
                }
            }
        }

        private void SaveKeynotes()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.AddExtension = true;
                dlg.OverwritePrompt = true;
                if (!string.IsNullOrEmpty(_masterKeynoteFile))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(_masterKeynoteFile);
                    dlg.FileName = Path.GetFileName(_masterKeynoteFile);
                }
                dlg.Filter = "Keynotes File (Text file (*.txt)|*.txt|All files (*.*)|*.*";
                dlg.Title = "Export Keynotes";
                var result = dlg.ShowDialog();
                if (result != DialogResult.OK) return;

                string fileNameOnly = Path.GetFileName(dlg.FileName);
                if (string.IsNullOrEmpty(fileNameOnly)) return;

                if (dlg.FileName != _masterKeynoteFile)
                {
                    string message = string.Format("Use Annotate > Keynote Settings to link the Revit Model to this keynotes file ({0}).", fileNameOnly);
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                FileInfo fiNewFile = new FileInfo(dlg.FileName);
                if (fiNewFile.Exists)
                {
                    fiNewFile.Delete();
                }

                if (comboBoxView.Text == KeynotesView.Assemblies)
                {
                    SaveAssembliesKeynotes(fiNewFile);
                }
                else if (comboBoxView.Text == KeynotesView.Divisions)
                {
                    SaveDivisionsKeynotes(fiNewFile);
                }

                labelStatus.Text = "Done.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void treeViewElements_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AfterSelectElements();
        }

        private void HighlightElements(TreeNode root, TreeNode elementNode, TreeNode keynoteNode, string codeToHighlight)
        {
            foreach (TreeNode node in root.Nodes)
            {
                if (node.Tag != null)
                {
                    string code = node.Tag as string;
                    if (!string.IsNullOrEmpty(code) &&
                        IsAKeynoteCode(code) &&
                        code == codeToHighlight)
                    {
                        node.Parent.Expand();
                        node.EnsureVisible();

                        // treeViewKeynotes.SelectedNode = node;

                        // Bold relevant nodes
                        BoldTreeNode(treeViewElements, elementNode, ref _elementsBoldNodes);
                        BoldTreeNode(treeViewElements, keynoteNode, ref _elementsBoldNodes);
                        BoldTreeNode(treeViewKeynotes, node, ref _keynotesBoldNodes);
                    }
                }

                HighlightElements(node, elementNode, keynoteNode, codeToHighlight);
            }
        }

        private void HighlightMaterials(TreeNode root, TreeNode materialNode, TreeNode keynoteNode, string codeToHighlight)
        {
            foreach (TreeNode node in root.Nodes)
            {
                if (node.Tag != null)
                {
                    string code = node.Tag as string;
                    if (!string.IsNullOrEmpty(code) &&
                        IsAKeynoteCode(code) &&
                        code == codeToHighlight)
                    {
                        node.Parent.Expand();
                        node.EnsureVisible();

                        // treeViewKeynotes.SelectedNode = node;

                        // Bold relevant nodes
                        BoldTreeNode(treeViewMaterials, materialNode, ref _materialsBoldNodes);
                        BoldTreeNode(treeViewMaterials, keynoteNode, ref _materialsBoldNodes);
                        BoldTreeNode(treeViewKeynotes, node, ref _keynotesBoldNodes);
                    }
                }

                HighlightElements(node, materialNode, keynoteNode, codeToHighlight);
            }
        }

        private void GetDescription(TreeNode root, string codeToSearch, ref string description)
        {
            if (!string.IsNullOrEmpty(description)) return;

            foreach (TreeNode node in root.Nodes)
            {
                if (node.Tag != null)
                {
                    string code = node.Tag as string;
                    if (!string.IsNullOrEmpty(code) &&
                        IsAKeynoteCode(code) &&
                        code == codeToSearch)
                    {
                        description = node.Text;
                        return;
                    }
                }

                GetDescription(node, codeToSearch, ref description);
            }
        }

        private void AfterSelectElements()
        {
            // Remove bold nodes
            RegularTreeNodes();

            // If tree is empty, do nothing
            if (treeViewElements.Nodes.Count == 0) return;

            TreeNode keynoteNode = treeViewElements.SelectedNode;
            if (keynoteNode == null) return;

            TreeNode elementNode = keynoteNode.Parent;
            if (elementNode == null) return;

            Assembly assembly = elementNode.Tag as Assembly;
            if (assembly == null) return;

            string keynote = assembly.Keynote;
            if (string.IsNullOrEmpty(keynote)) return;

            foreach (TreeNode node in _rootKeynotes.Nodes)
            {
                HighlightElements(node, elementNode, keynoteNode, keynote);
            }
        }

        private bool IsAssignedToElement(string keynote)
        {
            foreach (var element in _elements)
            {
                TreeNode elementNode = element.Value;
                if (elementNode.Tag == null) continue;

                Assembly assembly = elementNode.Tag as Assembly;
                if (assembly.Keynote != keynote) continue;

                return true;
            }

            return false;
        }
        private bool IsAassignedToMaterial(string keynote)
        {
            foreach (var m in _materialNodes)
            {
                Assembly assembly = m.Key;
                if (string.IsNullOrEmpty(assembly.Keynote)) continue;
                if (assembly.Keynote != keynote) continue;

                return true;
            }

            return false;
        }

        private void treeViewKeynotes_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            // If tree is empty, do nothing
            if (treeViewKeynotes.Nodes.Count == 0) return;

            TreeNode keynoteNode = treeViewKeynotes.SelectedNode;
            if (keynoteNode == null) return;
            if (keynoteNode.Tag == null) return;

            string keynote = keynoteNode.Tag as string;
            if (string.IsNullOrEmpty(keynote)) return;

            keynoteNode.Checked =
                IsAssignedToElement(keynote) ||
                IsAassignedToMaterial(keynote);
        }

        private void EnableKeynoteButtons()
        {
            labelStatus.Text = "";
            buttonNew.Enabled = false;
            buttonCopy.Enabled = false;
            buttonEdit.Enabled = false;
            buttonShowHide.Enabled = false;

            // If tree is empty, do nothing
            if (treeViewKeynotes.Nodes.Count == 0) return;

            // Disable New if no selected node, root, child of the root, or not under a letter, 
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;
            if (node == _rootKeynotes) return;
            if (node.Parent == _rootKeynotes) return;

            bool isLetter = node.Text.Length == 1;
            bool isAKeynote = false;
            string id = node.Tag as string;
            if (!string.IsNullOrEmpty(id))
            {
                isAKeynote = IsAKeynoteCode(id);
            }

            bool isParentToALetter = false;
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Text.Length == 1)
                {
                    isParentToALetter = true;
                    break;
                }
            }

            buttonNew.Enabled = isLetter || isParentToALetter;
            buttonCopy.Enabled = isAKeynote;
            buttonEdit.Enabled = isAKeynote;
            buttonShowHide.Enabled = isAKeynote;

        }

        private void RemoveElementKeynotes(string keynote)
        {
            AllowCheck();
            foreach (var element in _elements)
            {
                TreeNode elementNode = element.Value;
                Assembly assembly = elementNode.Tag as Assembly;
                if (assembly.Keynote != keynote) continue;

                // Get keynote from Keynotes
                TreeNode keynoteNode = null;
                if (_keynoteNodes != null && _keynoteNodes.ContainsKey(keynote))
                {
                    keynoteNode = _keynoteNodes[keynote];
                    keynoteNode.Checked =
                        IsAssignedToElement(keynote) ||
                        IsAassignedToMaterial(keynote);
                }

                assembly.Keynote = "";
                assembly.Changed = true;
                elementNode.Nodes.Clear();
                elementNode.Checked = false;

                TreeNode category = elementNode.Parent;
                category.Checked = HasCheckedNodes(category);
            }

            AllowCheck(false);
        }

        private void RemoveMaterialKeynotes(string keynote)
        {
            AllowCheck();
            Dictionary<string, TreeNode> nodesToRemove = new Dictionary<string, TreeNode>();
            foreach (var material in _materialKeynoteNodes)
            {
                TreeNode materialKeynoteNode = material.Value;
                string keynoteCode = materialKeynoteNode.Tag as string;
                if (string.IsNullOrEmpty(keynoteCode)) continue;
                if (keynoteCode != keynote) continue;

                nodesToRemove[material.Key] = materialKeynoteNode;
            }

            foreach (var node in nodesToRemove)
            {
                string materialName = node.Key;
                TreeNode materialKeynoteNode = node.Value;
                TreeNode material = materialKeynoteNode.Parent;

                AssemblyMaterial am = material.Tag as AssemblyMaterial;
                am.Keynote = "";
                am.Changed = true;

                material.Nodes.Clear();
                material.Checked = false;

                TreeNode materialType = material.Parent;
                materialType.Checked = HasCheckedNodes(materialType);

                _materialKeynoteNodes.Remove(materialName);
            }
            AllowCheck(false);
        }

        private void BoldElementTreeNodes(string code)
        {
            foreach (var element in _elements)
            {
                TreeNode elementNode = element.Value;
                Assembly assembly = elementNode.Tag as Assembly;
                if (assembly.Keynote != code) continue;

                elementNode.EnsureVisible();
                elementNode.Expand();

                // Bold relevant nodes
                foreach (TreeNode child in elementNode.Nodes)
                {
                    BoldTreeNode(treeViewElements, child, ref _elementsBoldNodes);
                    child.EnsureVisible();
                }
            }
        }

        private void BoldMaterialTreeNodes(string code)
        {
            foreach (var material in _materialKeynoteNodes)
            {
                TreeNode materialKeynoteNode = material.Value;
                string keynoteCode = materialKeynoteNode.Tag as string;
                if (string.IsNullOrEmpty(keynoteCode)) continue;
                if (keynoteCode != code) continue;

                materialKeynoteNode.EnsureVisible();
                materialKeynoteNode.Expand();

                // Bold relevant nodes
                BoldTreeNode(treeViewMaterials, materialKeynoteNode, ref _materialsBoldNodes);
            }
        }


        private void treeViewKeynotes_AfterSelect(object sender, TreeViewEventArgs e)
        {
            EnableKeynoteButtons();

            // Remove bold nodes
            RegularTreeNodes();

            // If tree is empty, do nothing
            if (treeViewKeynotes.Nodes.Count == 0) return;

            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;
            if (node.Tag == null) return;

            buttonShowHide.Text = (node.ForeColor == Color.Gray) ? 
                ShowHide.Show : ShowHide.Hide;

            string code = node.Tag as string;
            if (string.IsNullOrEmpty(code)) return;
            if (!IsAKeynoteCode(code)) return;

            bool assignedToElement = IsAssignedToElement(code);
            bool assignedToMaterial = IsAassignedToMaterial(code);
            node.Checked = assignedToElement || assignedToMaterial;
            if (node.Checked == false) return;

            // If node was checked, check corresponding keynotes assigned to element and material 
            BoldElementTreeNodes(code);
            BoldMaterialTreeNodes(code);
        }

        private void AfterSelectMaterials()
        {
            // Remove bold nodes
            RegularTreeNodes();

            // If tree is empty, do nothing
            if (treeViewMaterials.Nodes.Count == 0) return;

            TreeNode keynoteNode = treeViewMaterials.SelectedNode;
            if (keynoteNode == null) return;

            string keynote = keynoteNode.Tag as string;
            if (string.IsNullOrEmpty(keynote)) return;

            TreeNode materialNode = keynoteNode.Parent;

            foreach (TreeNode node in _rootKeynotes.Nodes)
            {
                HighlightMaterials(node, materialNode, keynoteNode, keynote);
            }
        }

        private void treeViewMaterials_AfterSelect(object sender, TreeViewEventArgs e)
        {
            AfterSelectMaterials();
        }

        private void contextMenuStripElements_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null) return;
            if (e.ClickedItem.Text != "Delete") return;

            // Remove bold nodes
            RegularTreeNodes();

            // If tree is empty, do nothing
            if (treeViewElements.Nodes.Count == 0) return;

            TreeNode keynoteNode = treeViewElements.SelectedNode;
            if (keynoteNode == null) return;

            TreeNode elementNode = keynoteNode.Parent;
            if (elementNode == null) return;

            Assembly assembly = elementNode.Tag as Assembly;
            if (assembly == null) return;

            string keynote = assembly.Keynote;
            if (string.IsNullOrEmpty(keynote)) return;

            // Remove keynote
            assembly.Keynote = "";
            assembly.Changed = true;
            elementNode.Nodes.Clear();
            elementNode.Checked = false;

            TreeNode category = elementNode.Parent;
            if (elementNode == null) return;

            category.Checked = HasCheckedNodes(category);
        }

        private bool HasCheckedNodes(TreeNode node)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Checked == true)
                    return true;

                if (HasCheckedNodes(childNode) == true)
                    return true;
            }

            return false;
        }

        private bool IsKeynoteSelectedFromKeynotes()
        {
            // If tree is empty
            if (treeViewKeynotes.Nodes.Count == 0) return false;

            TreeNode selectedNode = treeViewKeynotes.SelectedNode;
            if (selectedNode == null) return false;
            if (selectedNode.Tag == null) return false;

            // If root, div, sec, letter 
            string code = selectedNode.Tag as string;
            if (!IsAKeynoteCode(code)) return false;

            return true;
        }

        private void menuElements_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Hide menu and disable by default
            e.Cancel = true;
            applyElementsMenuItem.Enabled = false;
            removeElementsMenuItem.Enabled = false;

            // If no selected node
            TreeNode node = treeViewElements.SelectedNode;
            if (node == null) return;

            // Remove bold nodes
            RegularTreeNodes();

            // If keynote is not selected from Keynotes
            bool isKeynoteSelectedFromKeynotes = IsKeynoteSelectedFromKeynotes();

            // If tree empty
            if (treeViewElements.Nodes.Count == 0) return;

            // If root, category, not an element
            if (node == treeViewElements.Nodes[0]) return;
            if (_categories.ContainsKey(node.Text)) return;

            // If an element is selected
            if (_elements.ContainsKey(node.Text))
            {
                // No elements 
                if (node.Nodes.Count == 0)
                {
                    e.Cancel = false;
                    applyElementsMenuItem.Enabled = isKeynoteSelectedFromKeynotes;
                }

                // If there a keynote, user can remove or replace
                else
                {
                    e.Cancel = false;
                    applyElementsMenuItem.Enabled = isKeynoteSelectedFromKeynotes;
                    removeElementsMenuItem.Enabled = true;
                }
            }

            // Refocus to selected node
            treeViewElements.SelectedNode = node;
        }

        private void removeElementsMenuItem_Click(object sender, EventArgs e)
        {
            AllowCheck();
            try
            {
                // If tree is empty, do nothing
                if (treeViewKeynotes.Nodes.Count == 0) return;

                // An element node must have been selected
                TreeNode elementNode = treeViewElements.SelectedNode;
                if (elementNode == null) return;

                Assembly assembly = elementNode.Tag as Assembly;
                if (assembly == null) return;

                // If element is red from a questionable keynote code
                if (elementNode.ForeColor == Color.Red)
                {
                    elementNode.ForeColor = Color.Black;
                }

                // Add the keynote node under element
                elementNode.Nodes.Clear();
                string keynodeCode = assembly.Keynote;
                assembly.Keynote = "";
                assembly.Changed = true;

                bool assignedToElement = IsAssignedToElement(keynodeCode);
                bool assignedToMaterial = IsAassignedToMaterial(keynodeCode);
                TreeNode nodeFromKeyNotes = _keynoteNodes[keynodeCode];
                nodeFromKeyNotes.Checked = assignedToElement || assignedToMaterial;

                // Check relevant nodes
                CheckKeynoteNodeAncestors(nodeFromKeyNotes);
                CheckKeynoteNodeAncestors(elementNode);
                elementNode.Checked = false;

                // Remove bold nodes
                RegularTreeNodes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }
            AllowCheck(false);

        }

        private void applyKeynoteMenuItem_Click(object sender, EventArgs e)
        {
            DoubleClickToAssignElement();
        }

        private void treeViewElements_Click(object sender, EventArgs e)
        {
            treeViewMaterials.SelectedNode = null;
            AfterSelectElements();
        }

        private void treeViewMaterials_Click(object sender, EventArgs e)
        {
            treeViewElements.SelectedNode = null;
            AfterSelectMaterials();
        }

        private void applyMaterialsMenuItem_Click(object sender, EventArgs e)
        {
            DoubleClickToAssignMaterial();
        }

        private void removeMaterialsMenuItem_Click(object sender, EventArgs e)
        {
            AllowCheck();

            // If tree is empty, do nothing
            if (treeViewKeynotes.Nodes.Count == 0) return;

            // Add key note to an material node
            TreeNode materialNode = treeViewMaterials.SelectedNode;
            if (materialNode == null) return;

            // If root
            string materialName = materialNode.Text;
            if (materialName == _rootMaterials.Text) return;

            // Add the keynote node under material
            AssemblyMaterial material = materialNode.Tag as AssemblyMaterial;

            materialNode.Nodes.Clear();

            // If material is red from a questionable keynote code
            if (materialNode.ForeColor == Color.Red)
            {
                materialNode.ForeColor = Color.Black;
            }

            string keynote = material.Keynote;
            material.Keynote = "";
            material.Changed = true;

            _materialKeynoteNodes.Remove(material.Name);

            // Check relevant nodes
            materialNode.Checked = false;
            TreeNode materialType = materialNode.Parent;
            materialType.Checked = HasCheckedNodes(materialType);

            // If there was a previous assigned keynote
            if (!string.IsNullOrEmpty(keynote) &&
                _keynoteNodes.ContainsKey(keynote))
            {
                // Uncheck previous assigned keynote from Keynotes tree
                TreeNode previousKeynote = _keynoteNodes[keynote];
                previousKeynote.Checked =
                    IsAssignedToElement(keynote) ||
                    IsAassignedToMaterial(keynote);

                // Check keynote nodes
                CheckKeynoteNodeAncestors(previousKeynote);
            }

            // Remove bold nodes
            RegularTreeNodes();
            AllowCheck(false);
        }

        private void menuMaterials_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Hide menu and disable by default
            e.Cancel = true;
            applyMaterialsMenuItem.Enabled = false;
            removeMaterialsMenuItem.Enabled = false;

            // Remove bold nodes
            RegularTreeNodes();

            // If no keynote is selected from Keynotes
            bool isKeynoteSelectedFromKeynotes = IsKeynoteSelectedFromKeynotes();

            // If tree empty
            if (treeViewMaterials.Nodes.Count == 0) return;

            // If no selected node, root or material class
            TreeNode node = treeViewMaterials.SelectedNode;
            if (node == null) return;
            if (node == _rootMaterials) return;
            if (_materialClassNodes.ContainsKey(node.Text))
            {
                // Double check because there is the case where Wood is a material and a material class
                if (!_materialClassNodes.ContainsKey(node.Parent.Text)) return;
            }

            // If material 
            if (_revitMaterials.ContainsKey(node.Text))
            {
                // If material has a keynote
                if (node.Nodes.Count > 0)
                {
                    // Allow user to replace or remove keynote
                    e.Cancel = false;
                    applyMaterialsMenuItem.Enabled = isKeynoteSelectedFromKeynotes;
                    removeMaterialsMenuItem.Enabled = true;
                }
                else
                {
                    // Allow user to replace or remove keynote
                    e.Cancel = false;
                    applyMaterialsMenuItem.Enabled = isKeynoteSelectedFromKeynotes;
                }

                return;
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxFind.Text)) return;

            if (previousFind != textBoxFind.Text)
            {
                previousFind = textBoxFind.Text;
                findIndex = 0;
                ClearSearch(treeViewKeynotes.Nodes);
                findResults = new List<TreeNode>();
            }

            SearchRecursive(treeViewKeynotes.Nodes,
                textBoxFind.Text);

            if (findResults.Count > 0)
            {
                if (lastMarkedNode != null)
                {
                    lastMarkedNode.BackColor = Color.White;
                }

                lastMarkedNode = findResults[findIndex];
                treeViewKeynotes.SelectedNode = lastMarkedNode;

                findIndex++;
            }

            RevolveFindIndex();
            treeViewKeynotes.Focus();
        }

        private void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxFind.Text)) return;

            if (previousFind != textBoxFind.Text)
            {
                previousFind = textBoxFind.Text;
                findIndex = 0;
                ClearSearch(treeViewKeynotes.Nodes);
                findResults = new List<TreeNode>();
            }

            SearchRecursive(treeViewKeynotes.Nodes,
                textBoxFind.Text);

            if (findResults.Count > 0)
            {
                if (lastMarkedNode != null)
                {
                    lastMarkedNode.BackColor = Color.White;
                }

                lastMarkedNode = findResults[findIndex];
                treeViewKeynotes.SelectedNode = lastMarkedNode;


                findIndex--;
            }

            RevolveFindIndex();
            treeViewKeynotes.Focus();
        }

        private void ClearSearch(IEnumerable nodes)
        {
            if (findResults == null) return;
            if (findResults.Count == 0) return;

            foreach (TreeNode node in nodes)
            {
                node.BackColor = Color.White;
                ClearSearch(node.Nodes);
            }
        }

        private void SearchRecursive(IEnumerable nodes, string searchFor)
        {
            searchFor = searchFor.ToUpper();
            foreach (TreeNode node in nodes)
            {
                if (node.Text.ToUpper().Contains(searchFor))
                {
                    findResults.Add(node);
                }

                SearchRecursive(node.Nodes, searchFor);
            }
        }

        private void RevolveFindIndex()
        {
            if (findResults == null) return;

            if (findIndex < 0)
            {
                findIndex = findResults.Count - 1;
            }

            if (findIndex >= findResults.Count)
            {
                findIndex = 0;
            }
        }

        private void buttonNew_Click_Divisions(object sender, EventArgs e)
        {
            // Get selection node
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;

            string id = node.Tag as string;
            string name = node.Text;

            //. If a letter node
            string startingLetter = "";
            if (name.Length == 1)
            {
                // Get section node
                startingLetter = name;
                node = node.Parent;
            }

            // TODO 
            // Traverse list of letters under section
            string sectionNumber = node.Tag as string;
            if (sectionNumber == null) return;

            SortedDictionary<string, List<string>> usedNumbers = GetUsedNumbers(node);
            AddNewKeynoteForm form = new AddNewKeynoteForm()
            {
                SelectedNode = node,
                KeynoteNodes = _keynoteNodes,
                Keynotes = _keynoteNamesById,
                TreeViewKeynotes = treeViewKeynotes,
                UsedNumbers = usedNumbers,
                SectionNumber = sectionNumber,
                StartingLetter = startingLetter

            };
            form.ShowDialog();
        }

        private void buttonNew_Click_Assemblies(object sender, EventArgs e)
        {
            // Get selection node
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;

            string id = node.Tag as string;
            string name = node.Text;

            //. If a letter node
            string startingLetter = "";
            if (name.Length == 1)
            {
                // Get section node
                startingLetter = name;
                node = node.Parent;
            }

            // Get family code
            char[] dash = { '-' };
            string[] words = node.Text.Split(dash);
            string code = words[0].Trim();

            SortedDictionary<string, List<string>> usedNumbers = GetUsedNumbers(node);
            AddNewKeynoteForm form = new AddNewKeynoteForm()
            {
                SelectedNode = node,
                KeynoteNodes = _keynoteNodes,
                Keynotes = _keynoteNamesById,
                TreeViewKeynotes = treeViewKeynotes,
                UsedNumbers = usedNumbers,
                SectionNumber = code,
                StartingLetter = startingLetter

            };
            form.ShowDialog();
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            if (comboBoxView.Text == KeynotesView.Assemblies)
            {
                buttonNew_Click_Assemblies(sender, e);
            }
            else if (comboBoxView.Text == KeynotesView.Divisions)
            {
                buttonNew_Click_Divisions(sender, e);
            }

            treeViewKeynotes.Focus();
        }

        private string GetKeynoteDescription(string line)
        {
            int dash = line.IndexOf('-');
            if (dash == -1) return string.Empty;

            return line.Substring(dash + 1).Trim();
        }

        private string GetKeynoteIndex(string line)
        {
            char[] dot = { '.' };
            string[] words = line.Split(dot);
            if (words.Length == 0) return string.Empty;

            // Handle 411213.A00
            if (words.Length == 2)
            {
                return words[1].Substring(1);
            }

            //  Handle format 411213.19.A00
            else if (words.Length == 3)
            {
                return words[2].Substring(1);
            }

            return string.Empty;
        }

        private string GetKeynoteSectionNumber(string line)
        {
            char[] dot = { '.' };
            string[] words = line.Split(dot);

            if (words.Length == 2)
            {
                // Handle 123456
                return words[0];
            }
            else if (words.Length == 3)
            {
                // Handle 123456.12
                return string.Format("{0}.{1}", words[0], words[1]);
            }

            return string.Empty;
        }

        private string GetKeynoteLetter(string line)
        {
            char[] dot = { '.' };
            string[] words = line.Split(dot);
            if (words.Length == 0) return string.Empty;

            // Handle 411213.A00
            if (words.Length == 2)
            {
                return words[1].Substring(0, 1);
            }

            //  Handle format 411213.19.A00
            else if (words.Length == 3)
            {
                return words[2].Substring(0, 1);
            }

            return string.Empty;
        }

        private SortedDictionary<string, List<string>> GetUsedNumbers(TreeNode section)
        {
            SortedDictionary<string, List<string>> usedNumbers = new SortedDictionary<string, List<string>>();
            foreach (TreeNode letterNode in section.Nodes)
            {
                usedNumbers[letterNode.Text] = new List<string>();

                foreach (TreeNode number in letterNode.Nodes)
                {
                    string code = number.Tag as string;
                    if (code == null) continue;

                    // Handle 411213.A00
                    string value = GetKeynoteIndex(code);
                    if (string.IsNullOrEmpty(value)) continue;

                    usedNumbers[letterNode.Text].Add(value);
                }
            }

            return usedNumbers;
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            // Get selection node
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;

            // Verify if keynote node
            string keynoteCode = node.Tag as string;
            if (string.IsNullOrEmpty(keynoteCode)) return;
            if (!_keynoteNamesById.ContainsKey(keynoteCode)) return;

            // Get keynote description
            int dash = node.Text.IndexOf('-');
            if (dash == -1) return;
            string keynoteDesc = node.Text.Substring(dash + 1).Trim();
            if (string.IsNullOrEmpty(keynoteDesc)) return;

            // Get letter
            TreeNode letter = node.Parent;
            if (letter == null) return;
            string startingLetter = letter.Text;

            // Get section
            TreeNode section = letter.Parent;
            if (section == null) return;

            // Get section number
            int dot = node.Text.Substring(0, dash).LastIndexOf('.');
            if (dot == -1) return;
            string sectionNumber = node.Text.Substring(0, dot).Trim();

            // Traverse list of letters under section
            SortedDictionary<string, List<string>> usedNumbers = GetUsedNumbers(section);
            AddNewKeynoteForm form = new AddNewKeynoteForm()
            {
                SelectedNode = node,
                KeynoteNodes = _keynoteNodes,
                Keynotes = _keynoteNamesById,
                TreeViewKeynotes = treeViewKeynotes,
                UsedNumbers = usedNumbers,
                SectionNumber = sectionNumber,
                StartingLetter = startingLetter,
                Value = keynoteDesc,
                OKButton = "Add",
                Text = "Copy Keynote"

            };
            form.ShowDialog();

            treeViewKeynotes.Focus();
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            // Get selection node
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;

            // Verify if keynote node
            string keynoteCode = node.Tag as string;
            if (string.IsNullOrEmpty(keynoteCode)) return;
            if (!_keynoteNamesById.ContainsKey(keynoteCode)) return;

            // Get the keynote number and description
            string keynoteLetter = GetKeynoteLetter(keynoteCode);
            if (string.IsNullOrEmpty(keynoteLetter)) return;

            string startingNumber = GetKeynoteIndex(keynoteCode);
            if (string.IsNullOrEmpty(startingNumber)) return;

            string keynoteDesc = GetKeynoteDescription(node.Text);
            if (string.IsNullOrEmpty(keynoteDesc)) return;

            // Get letter
            TreeNode letter = node.Parent;
            if (letter == null) return;
            string startingLetter = letter.Text;

            // Get section
            TreeNode section = letter.Parent;
            if (section == null) return;

            // Get section number
            int dash = node.Text.IndexOf('-');
            if (dash == -1) return;
            int dot = node.Text.Substring(0, dash).LastIndexOf('.');
            if (dot == -1) return;
            string sectionNumber = node.Text.Substring(0, dot).Trim();

            // Traverse list of letters under section            
            SortedDictionary<string, List<string>> usedNumbers = GetUsedNumbers(section);
            AddNewKeynoteForm form = new AddNewKeynoteForm()
            {
                KeynoteNodes = _keynoteNodes,
                Keynotes = _keynoteNamesById,
                TreeViewElements = treeViewElements,
                TreeViewMaterials = treeViewMaterials,
                TreeViewKeynotes = treeViewKeynotes,
                UsedNumbers = usedNumbers,
                SectionNumber = sectionNumber,
                StartingLetter = startingLetter,
                StartingNumber = startingNumber,
                Value = keynoteDesc,
                NodeToEdit = node,
                OKButton = "Edit",
                Text = "Edit Keynote"

            };
            form.ShowDialog();

            treeViewKeynotes.Focus();
        }

        private async Task<ProjectElementKeynoteNode> SetKeynoteIsHidden(string keynoteId, bool isHidden)
        {
            try
            {
                string firmId = Globals.CurrentUserActiveFirm.myActiveFirm.firmId;
                string projectId = Globals.SpecpointProjectID;

                // Get the Specpoint project name 
                Query query = new Query(nameof(SetKeynoteIsHidden));
                ProjectElementKeynoteNode result = await query.SetKeynoteIsHidden(keynoteId, isHidden, firmId, projectId);
                if (result == null) return null;

                return result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }

            return null;
        }

        private async void buttonShowHide_Click(object sender, EventArgs e)
        {
            // Get selection node
            TreeNode node = treeViewKeynotes.SelectedNode;
            if (node == null) return;

            // Verify if keynote node
            string keynoteCode = node.Tag as string;
            if (string.IsNullOrEmpty(keynoteCode)) return;
            if (!_keynoteNamesById.ContainsKey(keynoteCode)) return;

            string title = (node.ForeColor == Color.Gray) ? "Show Keynote" : "Hide Keynote";
            string question = (node.ForeColor == Color.Gray) ? 
                string.Format("Are you sure you want to show {0}?", node.Text) :
                string.Format("Are you sure you want to hide {0}?", node.Text);
            DialogResult ret = MessageBox.Show(question, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ret == DialogResult.Yes)
            {
                bool isHidden = false;
                if (node.ForeColor == Color.Gray)
                {
                    node.ForeColor = Color.Black;
                    node.NodeFont = null;
                    buttonShowHide.Text = ShowHide.Hide;
                }
                else
                {
                    node.ForeColor = Color.Gray;
                    node.NodeFont = new Font(treeViewKeynotes.Font, FontStyle.Italic);
                    buttonShowHide.Text = ShowHide.Show;
                    isHidden = true;

                    RemoveElementKeynotes(keynoteCode);
                    RemoveMaterialKeynotes(keynoteCode);

                    // Check keynote nodes
                    CheckKeynoteNodeAncestors(node);
                }

                // Get keynote id
                string keynoteId = node.Name;

                // Get baseElementId
                string baseElementId = "";
                if (_keynoteBaseElementIds.ContainsKey(keynoteId))
                {
                    baseElementId = _keynoteBaseElementIds[keynoteId];
                }

                if (keynoteId != "" && baseElementId != "")
                {
                    var keynoteNode = await SetKeynoteIsHidden(keynoteId, isHidden);
                    if (keynoteNode != null)
                    {
                        // Update in memory keynotes hidden flag
                        if (Globals.SpecpointKeynotes.familyKeynotes.ContainsKey(baseElementId))
                        {
                            Globals.SpecpointKeynotes.familyKeynotes[baseElementId].isHidden = isHidden;
                        }
                        else if (Globals.SpecpointKeynotes.productTypeKeynotes.ContainsKey(baseElementId))
                        {
                            Globals.SpecpointKeynotes.productTypeKeynotes[baseElementId].isHidden = isHidden;
                        }
                    }
                }
                
                // _keynoteNamesById.Remove(keynoteCode);
                // treeViewKeynotes.Nodes.Remove(node);
            }

            treeViewKeynotes.Focus();
        }

        private void treeViewKeynotes_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;

        }

        private void treeViewElements_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void treeViewMaterials_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private async Task<string> GetFamilyNumberFormat()
        {
            try
            {
                // Get the Specpoint project name 
                Query query = new Query(nameof(GetFamilyNumberFormat));
                GetProject result = await query.getProject(Globals.SpecpointProjectID);

                return result.getProject.projectSettings.familyNumberFormat;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
            }

            return String.Empty;
        }

        private void treeViewElements_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Handle focus jumps to assigned keynote when right clicking on an Assembly
            ((TreeView)sender).SelectedNode = e.Node;
        }

        private void treeViewMaterials_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // Handle focus jumps to assigned keynote when right clicking on a Material Class
            ((TreeView)sender).SelectedNode = e.Node;
        }

        private void PopulateTrees()
        {
            string filter = comboBoxFilter.Text;

            if (filter == KeynotesFilter.ByFilter)
            {
                // From Filter Assemblies
                PopulateTrees(filter, IncludedCategories);

            }
            else if (filter == KeynotesFilter.AllCategories)
            {
                filter = Globals.AllCategoriesFilter;
                PopulateTrees(filter);
            }
            else
            {
                // Selected 
                List<string> includedCategories = new List<string>();
                includedCategories.Add(filter);
                PopulateTrees(filter, includedCategories);
            }
        }

        private void comboBoxFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateTrees();
        }

        private void comboBoxView_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopulateTrees();
        }

        public async Task<bool> GetKeynotes()
        {
            if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return false;

            try
            {
                if (Globals.SpecpointKeynotes == null)
                {
                    Globals.SpecpointKeynotes = await GetKeynotes(Globals.SpecpointProjectID);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MethodBase.GetCurrentMethod().Name);
                return false;
            }

            return true;
        }

        public async Task<GetProjectElementNodes> GetAllSpecpointAssemblies()
        {
            if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return null;

            // Get all project elements 
            GetAllProjectElementNodesInput all = new GetAllProjectElementNodesInput()
            {
                projectId = Globals.SpecpointProjectID
            };

            all.projectElementType = new List<ProjectElementType>()
            {
                ProjectElementType.ASSEMBLY,
                ProjectElementType.PRODUCTFAMILY,
                ProjectElementType.PRODUCTTYPE
            };

            Query query = new Query(nameof(GetAllSpecpointAssemblies));
            GetProjectElementNodes results = await query.getProjectElementsNodes(all);
            if (results == null)
            {
                // Exit the form
                Close();
            }

            return results;
        }

        public async Task<Dictionary<string, GetProjectElementNodes>> GetSpecpointAssembliesByCategory()
        {
            if (Globals.mapCategoryIDs == null) return null;

            Dictionary<string, GetProjectElementNodes> results = new Dictionary<string, GetProjectElementNodes>();

            // Get all project elements 
            foreach (string categoryName in IncludedCategories)
            {
                // Get node from Revit Category
                Category c = Globals.revitCategories[categoryName];

                GetProjectElementNodes nodes = await GetSpecpointAssemblies(c);
                results[categoryName] = nodes;
            }

            return results;
        }

        public async Task<GetProjectElementNodes> GetSpecpointAssembliesByCategory(string specificCategory)
        {
            if (Globals.mapCategoryIDs == null) return null;

            if (string.IsNullOrEmpty(specificCategory)) return null;
            if (!Globals.revitCategories.ContainsKey(specificCategory)) return null;

            // Get node from Revit Category
            Category c = Globals.revitCategories[specificCategory];

            GetProjectElementNodes results = await GetSpecpointAssemblies(c);
            return results;
        }

        public async Task<GetProjectElementNodes> GetSpecpointAssemblies(Category revitCategory)
        {
            if (revitCategory == null) return null;
            if (Globals.mapCategoryIDs == null) return null;
            if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return null;

            // Get specific project elements given the Specpoint category ID
            GetProjectElementNodesInput specific = null;
            if (Globals.mapCategoryIDs.ContainsKey(revitCategory.Id.ToString()))
            {
                specific = new GetProjectElementNodesInput()
                {
                    projectId = Globals.SpecpointProjectID,
                    categoryId = Globals.mapCategoryIDs[revitCategory.Id.ToString()]
                };

                specific.projectElementType = new List<ProjectElementType>()
                {
                    ProjectElementType.ASSEMBLY,
                    ProjectElementType.PRODUCTFAMILY,
                    ProjectElementType.PRODUCTTYPE
                };
            }

            if (specific == null) return null;

            string queryTitle = string.Format("{0} - {1}", 
                nameof(GetSpecpointAssemblies), revitCategory.Name);
            Query query = new Query(queryTitle);
            GetProjectElementNodes results = await query.getProjectElementsNodes(specific);
            if (results == null)
            {
                // Exit the form
                Close();
            }

            return results;
        }

        private void buttonExport_Click(object sender, EventArgs e)
        {
            // Save to file
            SaveKeynotes();
        }

        public async Task<GetProjectElementKeynotes> GetKeynotes(string ProjectId)
        {
            string func = nameof(GetKeynotes);
            Query query = new(func);
            string firmId = await query.GetCurrentUserActiveFirmID();
            GetProjectElementKeynotes results = await query.getProjectElementKeynotes(ProjectId, firmId);
            return results;
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            Globals.SpecpointKeynotes = null;
            Globals.KeynoteDivisions = null;
            Globals.KeynoteAssemblies = null;
            Globals.KeynoteAssembliesByCategory = null;

            PopulateTrees();
        }

        /// <summary>
        /// Gets the resource bitmap.
        /// </summary>
        /// <param name="name">The name of the bitmap file.</param>
        /// <returns>Resource bitmap from file.</returns>
        private static Image GetResourceBitmap(String name)
        {
            string currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string path = currentDir + "\\" + name;

            Image bitmap = Bitmap.FromFile(path);

            return bitmap;
        }

        private void KeynotesManagerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Reset to previous filter
            SpecpointRegistry.SetValue("FilterAssemblies", previousFilter);
        }
    }

    public static class KeynotesView
    {
        public const string Assemblies = "ASSEMBLIES";
        public const string Divisions = "DIVISIONS";
    }

    public static class KeynotesFilter
    {
        public const string ByFilter = "By Filter";
        public const string AllCategories = "All Categories";
    }

    public static class KeynotesProgressTitle
    {
        public const string AllAssemblies = "Loading All Assemblies";
        public const string Assemblies = "Loading Assemblies";
        public const string Divisions = "Loading Divisions";
    }

    public static class KeynotesTreeImageIndex
    {
        public const int Division = 0;
        public const int Section = 1;
        public const int Letter = 2;
        public const int Keynote = 3;
        public const int Assembly = 4;
        public const int Category = 5;
        public const int ProductType = 6;
    }

    public static class ShowHide
    {
        public const string Show = "Show";
        public const string Hide = "Hide";
    }
}
