using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public partial class FilterAssembliesForm : Form
    {
        private List<TreeNode> checkedNodes;

        public List<string> checkedItems;

        public string RevitModel { get; set; }
        public bool AllCategories { get; set; }

        // List of specific controls we want to enable/disable
        private List<Control> controls;

        TreeNode architecture;
        TreeNode mep;
        TreeNode structure;

        public FilterAssembliesForm()
        {
            AllCategories = false;
            InitializeComponent();

            checkedNodes = new List<TreeNode>();
            checkedItems = new List<string>();

            // List of controls we want to enable/disable
            controls = new List<Control>();
            controls.Add(buttonOK);
            controls.Add(treeView);
            controls.Add(treeViewMEP);
            controls.Add(treeViewStructural);

        }

        private void FilterAssembliesForm_Load(object sender, EventArgs e)
        {
            try
            {
                PopulateCategories();
            }
            catch (TokenExpiredException)
            {
                throw;
            }
        }

        private async void PopulateCategories()
        {
            using (new WaitCursor(controls))
            {
                treeView.CheckBoxes = true;
                treeView.Nodes.Clear();
                architecture = treeView.Nodes.Add("Architecture");

                treeViewMEP.CheckBoxes = true;
                treeViewMEP.Nodes.Clear();
                mep = treeViewMEP.Nodes.Add("MEP");

                treeViewStructural.CheckBoxes = true;
                treeViewStructural.Nodes.Clear();
                structure = treeViewStructural.Nodes.Add("Structure");

                List<string> meps = new List<string>
                {
                    "Air Terminals",
                    "Cable Trays",
                    "Communication Devices",
                    "Conduits",
                    "Data Devices",
                    "Duct Accessories",
                    "Duct Fittings",
                    "Ducts",
                    "Electrical Equipment",
                    "Electrical Fixtures",
                    "File Alarm Devices",
                    "Flex Ducts",
                    "Flex Pipes",
                    "Lighting Devices",
                    "Lighting Fixtures",
                    "Mechanical Equipment",
                    "Nurse Call Devices",
                    "Pipe Accessories",
                    "Pipe Fittings",
                    "Pipes",
                    "Plumbing Fixtures",
                    "Security Devices",
                    "Sprinkles",
                    "Telephone Devices",
                    "Wire"
                };

                // Get list of Specpoint project categories
                if (Globals.specpointCategories == null ||
                    (Globals.specpointCategories != null && Globals.specpointCategories.Count == 0))
                {
                    Globals.specpointCategories = new SpecpointCategories();
                    bool ret = await Globals.specpointCategories.Init();
                    if (ret == false)
                    {
                        DialogResult = DialogResult.Cancel;
                        Close();
                        return;
                    }

                    Globals.specpointCategories.GetNoMatchInRevit();
                }

                foreach (string category in Globals.specpointCategories.noMatchInRevit.Keys)
                {
                    architecture.Nodes.Add(category);
                }

                foreach (string category in Globals.revitCategories.Keys)
                {
                    if (category.StartsWith("Structural"))
                    {
                        if (Globals.specpointCategories.ContainsKey(category))
                        {
                            structure.Nodes.Add(category);
                        }

                        continue;
                    }

                    if (meps.Contains(category))
                    {
                        if (Globals.specpointCategories.ContainsKey(category))
                        {
                            mep.Nodes.Add(category);
                        }

                        continue;
                    }

                    if (Globals.specpointCategories.ContainsKey(category))
                    {
                        architecture.Nodes.Add(category);
                    }
                }

                if (AllCategories)
                {
                    architecture.Checked = true;
                    structure.Checked = true;
                    mep.Checked = true;
                }

                treeView.Sort();
                treeView.ExpandAll();
                treeViewMEP.ExpandAll();
                treeViewStructural.ExpandAll();

                SetCheckedItems();

            }
        }

        private void treeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            using (new WaitCursor(controls))
            {
                treeView.AfterCheck -= treeView_AfterCheck;

                CheckChildren(e.Node, e.Node.Checked);
                architecture.Checked = CheckParent(architecture);

                treeView.AfterCheck += new TreeViewEventHandler(this.treeView_AfterCheck);
            }
        }
        private void treeViewMEP_AfterCheck(object sender, TreeViewEventArgs e)
        {
            using (new WaitCursor(controls))
            {
                treeViewMEP.AfterCheck -= treeViewMEP_AfterCheck;

                CheckChildren(e.Node, e.Node.Checked);
                mep.Checked = CheckParent(mep);

                treeViewMEP.AfterCheck += new TreeViewEventHandler(treeViewMEP_AfterCheck);
            }
        }

        private void treeViewStructural_AfterCheck(object sender, TreeViewEventArgs e)
        {
            using (new WaitCursor(controls))
            {
                treeViewStructural.AfterCheck -= treeViewStructural_AfterCheck;

                CheckChildren(e.Node, e.Node.Checked);
                structure.Checked = CheckParent(structure);

                treeViewStructural.AfterCheck += new TreeViewEventHandler(treeViewStructural_AfterCheck);
            }
        }

        private void CheckChildren(TreeNode treeNode, bool checkedState)
        {
            foreach (TreeNode item in treeNode.Nodes)
            {
                if (item.Checked != checkedState)
                {
                    item.Checked = checkedState;
                }

                CheckChildren(item, item.Checked);
            }
        }

        private bool CheckParent(TreeNode treeNode)
        {
            foreach (TreeNode item in treeNode.Nodes)
            {
                if (item.Checked == false)
                    return false;
            }

            return true;
        }

        private void GetCheckedNodes(TreeNode parent)
        {
            foreach (TreeNode node in parent.Nodes)
            {
                if (node.Checked)
                {
                    checkedNodes.Add(node);
                }

                foreach (TreeNode grandchild in node.Nodes)
                {
                    GetCheckedNodes(grandchild);
                }
            }
        }
        private void GetCheckedNodes(bool includeChildren = false)
        {
            checkedNodes = new List<TreeNode>();

            if (architecture.Checked)
            {
                if (includeChildren)
                {
                    GetCheckedNodes(architecture);
                }
                else
                {
                    checkedNodes.Add(architecture);
                }
            }
            else
            {
                GetCheckedNodes(architecture);
            }

            if (mep.Checked)
            {
                if (includeChildren)
                {
                    GetCheckedNodes(mep);
                }
                else
                {
                    checkedNodes.Add(mep);
                }
            }
            else
            {
                GetCheckedNodes(mep);
            }

            if (structure.Checked)
            {
                if (includeChildren)
                {
                    GetCheckedNodes(structure);
                }
                else
                {
                    checkedNodes.Add(structure);
                }
            }
            else
            {
                GetCheckedNodes(structure);
            }
        }

        private void SetCheckedItems(TreeNode parent, List<string> filters)
        {
            if (filters.Contains(parent.Text))
            {
                foreach (TreeNode node in parent.Nodes)
                {
                    node.Checked = true;
                }
            }
            else
            {
                foreach (TreeNode node in parent.Nodes)
                {
                    node.Checked = filters.Contains(node.Text);
                }
            }
        }

        private void SetCheckedItems()
        {
            string projectName = SpecpointRegistry.GetValue("ProjectName");
            if (string.IsNullOrEmpty(projectName))
            {
                List<string> filters = SpecpointRegistry.GetValue("FilterAssemblies").Split('|').ToList();

                SetCheckedItems(architecture, filters);
                SetCheckedItems(mep, filters);
                SetCheckedItems(structure, filters);
            }
            else 
            {
                List<string> filters = SpecpointRegistry.GetValue("FilterAssemblies|" + projectName).Split('|').ToList();

                SetCheckedItems(architecture, filters);
                SetCheckedItems(mep, filters);
                SetCheckedItems(structure, filters);
            }
                
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            using (new WaitCursor(controls))
            {
                GetCheckedNodes();
                if (checkedNodes.Count == 0)
                {
                    MessageBox.Show("Please select at least one filter.", "Filter Assemblies",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool isArchitecture = false;
                bool isMEP = false;
                bool isStructure = false;
                checkedItems = new List<string>();
                foreach (TreeNode node in checkedNodes)
                {
                    checkedItems.Add(node.Text);

                    if (node.Text == "Architecture") isArchitecture = true;
                    if (node.Text == "MEP") isMEP = true;
                    if (node.Text == "Structure") isStructure = true;
                }

                // Save checked items to the registry
                // If a parent node was checked, we only save the parent to the registry
                string value = String.Join("|", checkedItems);

                string projectName = SpecpointRegistry.GetValue("ProjectName");
                if (string.IsNullOrEmpty(projectName))
                {
                    SpecpointRegistry.SetValue("FilterAssemblies", value);
                }
                else
                {
                    SpecpointRegistry.SetValue("FilterAssemblies|" + projectName, value);
                }

                // If a parent node was checked
                if (isArchitecture || isMEP || isStructure)
                {
                    // Get all children under the parent
                    bool includeChildren = true;
                    GetCheckedNodes(includeChildren);

                    checkedItems = new List<string>();
                    foreach (TreeNode node in checkedNodes)
                    {
                        checkedItems.Add(node.Text);
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
