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
    public partial class AddNewKeynoteForm : Form
    {
        public string SectionNumber { get; set; }
        public string StartingLetter { get; set; }
        public string Value { get; set; }
        public string OKButton { get; set; }
        public SortedDictionary<string, List<string>> UsedNumbers { get; set; }
        public TreeNode SelectedNode { get; set; }
        public Dictionary<string, TreeNode> Letters { get; set; }
        public Dictionary<string, TreeNode> KeynoteNodes { get; set; }
        public Dictionary<string, string> Keynotes { get; set; }
        public TreeView TreeViewKeynotes { get; set; }
        public TreeView TreeViewElements { get; set; }
        public TreeView TreeViewMaterials { get; set; }
        private SortedDictionary<string, SortedDictionary<string, string>> _added;

        public TreeNode NodeToEdit { get; set; }
        public string StartingNumber { get; set; }


        public AddNewKeynoteForm()
        {
            InitializeComponent();

            _added = new SortedDictionary<string, SortedDictionary<string, string>>();
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

        private void AddNewKeynoteForm_Load(object sender, EventArgs e)
        {
            this.MinimumSize = new Size(500, 147);
            this.MaximumSize = new Size(1000, 147);

            textBoxSection.Text = SectionNumber;
            textBoxValue.Text = Value;
            textBoxValue.SelectionStart = 0;
            textBoxValue.SelectAll();
            textBoxValue.Focus();
            
            if (!string.IsNullOrEmpty(OKButton))
            {
                buttonOK.Text = OKButton;
            }

            // Edit session
            if (NodeToEdit != null)
            {
                this.comboBoxLetter.SelectedIndexChanged -= this.comboBoxLetter_SelectedIndexChanged;

                this.Text = "Edit Keynote";
                comboBoxLetter.Enabled = false;
                comboBoxNumber.Enabled = false;

                comboBoxLetter.Items.Add(NodeToEdit.Parent.Text);
                comboBoxNumber.Items.Add(StartingNumber);

                comboBoxLetter.SelectedIndex = 0;
                comboBoxNumber.SelectedIndex = 0;

                this.comboBoxLetter.SelectedIndexChanged += new System.EventHandler(this.comboBoxLetter_SelectedIndexChanged);

                checkBoxKeepOpen.Visible = false;
            }

            // Add or Copy 
            else
            {
                // Copy session
                if (Text == "Copy Keynote")
                {
                    checkBoxKeepOpen.Visible = false;
                }

                PopulateComboBoxLetter();
            }
        }

        private void PopulateComboBoxLetter()
        {
            comboBoxLetter.Items.Clear();

            int startingLetterIndex = 0;
            for (char c = 'A'; c <= 'Z'; c++)
            {
                string letter = c.ToString();
                int index = comboBoxLetter.Items.Add(letter);

                if (!string.IsNullOrEmpty(StartingLetter) && letter == StartingLetter)
                {
                    startingLetterIndex = index;
                }
            }

            comboBoxLetter.SelectedIndex = startingLetterIndex;
        }
        private void PopulateComboBoxNumber(string letter = "")
        {
            if (UsedNumbers == null) return;

            comboBoxNumber.Items.Clear();

            for (int n = 0; n <= 99; n++)
            {
                // Filter out used numbers
                string number = n.ToString("D2");

                if (UsedNumbers.ContainsKey(letter))
                {
                    if (UsedNumbers[letter].Contains(number)) continue;
                }

                // Filter out added numbers
                if (_added.ContainsKey(letter))
                {
                    if (_added[letter].ContainsKey(number)) continue;
                }

                comboBoxNumber.Items.Add(string.Format("{0}", number));
            }

            if (comboBoxNumber.Items.Count == 0)
            {
                // When all numbers are used up
                comboBoxNumber.SelectedIndex = -1;
            }
            else
            {
                comboBoxNumber.SelectedIndex = 0;
            }

            comboBoxNumber.Enabled = comboBoxNumber.Items.Count > 0;
            buttonOK.Enabled = !string.IsNullOrEmpty(textBoxValue.Text) &&
                comboBoxNumber.Items.Count > 0;
        }

        private void EditElements(string source, string target)
        {
            if (TreeViewElements == null) return;
            if (TreeViewElements.Nodes.Count == 0) return;

            TreeNode root = TreeViewElements.Nodes[0];
            foreach (TreeNode categories in root.Nodes)
            {
                foreach (TreeNode elements in categories.Nodes)
                {
                    foreach (TreeNode keynote in elements.Nodes)
                    {
                        if (keynote.Text != source) continue;

                        keynote.Text = target;
                    }
                }
            }
        }

        private void EditMaterials(string source, string target)
        {
            if (TreeViewMaterials == null) return;
            if (TreeViewMaterials.Nodes.Count == 0) return;

            TreeNode root = TreeViewMaterials.Nodes[0];
            foreach (TreeNode materialClass in root.Nodes)
            {
                foreach (TreeNode material in materialClass.Nodes)
                {
                    foreach (TreeNode keynote in material.Nodes)
                    {
                        if (keynote.Text != source) continue;

                        keynote.Text = target;
                    }
                }
            }
        }


        private void buttonOK_Click(object sender, EventArgs e)
        {
            //
            // Edit session
            //

            string value = textBoxValue.Text.Trim();
            if (NodeToEdit != null)
            {
                string text = string.Format("{0}.{1}{2} - {3}", SectionNumber,
                    comboBoxLetter.Text, StartingNumber, value);

                EditElements(NodeToEdit.Text, text);
                EditMaterials(NodeToEdit.Text, text);

                NodeToEdit.Text = text;
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            string letter = comboBoxLetter.Text;
            string number = comboBoxNumber.Text;
            string description = value;

            if (!_added.ContainsKey(letter))
            {
                _added[letter] = new SortedDictionary<string, string>();
            }

            if (_added[letter] != null)
            {
                _added[letter][number] = description;

                PopulateComboBoxNumber(letter);
            }

            //
            // Copy
            //
            if (Text == "Copy Keynote")
            {

            }

            //
            // Add 
            //


            // keynote to tree
            AddKeynotesToTree();

            if (checkBoxKeepOpen.Checked == false)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboBoxLetter_SelectedIndexChanged(object sender, EventArgs e)
        {
            string letter = comboBoxLetter.SelectedItem as string;
            PopulateComboBoxNumber(letter);
        }

        private void textBoxValue_TextChanged(object sender, EventArgs e)
        {
            string value = textBoxValue.Text.Trim();
            buttonOK.Enabled = !string.IsNullOrEmpty(value) &&
                comboBoxNumber.Items.Count > 0 &&
                value != this.Value;
        }

        private void AddKeynotesToTree()
        {
            if (string.IsNullOrEmpty(SectionNumber)) return;

            foreach (var added in _added)
            {
                string letter = added.Key;
                foreach (var keynote in added.Value)
                {
                    string number = keynote.Key;
                    string desc = keynote.Value;
                    AddKeynoteToTree(letter, number, desc);
                }
            }

            _added.Clear();
        }

        private bool LetterExists(TreeNode section, string letter)
        {
            foreach (TreeNode letterNode in section.Nodes)
            {
                if (letterNode.Text == letter)
                    return true;
            }

            return false;
        }

        private void SetTreeNodeImage(TreeNode treeNode, int index)
        {
            treeNode.ImageIndex = index;
            treeNode.SelectedImageIndex = index;
        }

        private void AddKeynoteToTree(string letter, string number, string desc)
        {
            if (SelectedNode == null) return;
            if (Keynotes == null) return;
            if (KeynoteNodes == null) return;
            if (string.IsNullOrEmpty(SectionNumber)) return;

            // Copy session
            if (Text == "Copy Keynote")
            {
                // Get the section node
                TreeNode letterNode = SelectedNode.Parent;
                SelectedNode = letterNode.Parent;
            }

            // If section does not have specified letter
            if (!LetterExists(SelectedNode, letter))
            {
                // Create letter node under section
                TreeNode letterNode = SelectedNode.Nodes.Add(letter);
                SetTreeNodeImage(letterNode, KeynotesTreeImageIndex.Letter);
                letterNode.EnsureVisible();

                // Create letter under used numbers
                if (!UsedNumbers.ContainsKey(letter))
                {
                    UsedNumbers[letter] = new List<string>();
                }
            }

            // Traverse all letter nodes
            foreach (TreeNode letterNode in SelectedNode.Nodes)
            {
                if (letterNode.Text != letter) continue;

                string code = string.Format("{0}.{1}{2}", SectionNumber, letter, number);
                TreeNode newKeynote = letterNode.Nodes.Add(string.Format("{0} - {1}", code, desc));
                SetTreeNodeImage(newKeynote, KeynotesTreeImageIndex.Keynote);
                newKeynote.Tag = code;
                KeynoteNodes[code] = newKeynote;
                Keynotes[code] = desc;

                UsedNumbers[letter].Add(number);

                TreeViewKeynotes.SelectedNode = newKeynote;
                newKeynote.EnsureVisible();

                break;
            }

        }

        private string GetSectionNumber(string value)
        {
            char[] space = { ' ' };
            string[] words = value.Split(space);
            if (words.Length == 0) return string.Empty;

            // Traverse list of numbers under letter
            return words[1];
        }

        private void textBoxValue_MouseHover(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)sender;

            string value = textbox.Text.Trim();
            if (!string.IsNullOrEmpty(value))
            {
                // 3 secs
                int duration = 3000; 
                ToolTip tt = new ToolTip();
                tt.Show(value, textbox, 0, 22, duration);
            }
        }
    }
}
