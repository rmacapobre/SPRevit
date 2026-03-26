using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class Assembly
    {
        public string BIMCategory { get; set; }
        public long BIMCategoryId { get; set; }
        public string RevitFamily { get; set; }
        public string RevitType { get; set; }
        public string RevitTypeId { get; set; }
        public string AssemblyCode { get; set; }
        public string AssemblyDescription { get; set; }
        public string SpecpointFamilyNumber { get; set; }
        public string SpecpointFamily { get; set; }
        public bool Changed { get; set; }
        public bool IsLastSubassembly { get; set; }
        public bool IsInProject { get; set; }

        /// <summary>
        /// Gets or sets the key for looking up the assembly's keynote.
        /// </summary>
        public string Keynote { get; set; }

        public Assembly()
        {
            Changed = false;
        }

        // Copy constructor.
        public Assembly(Assembly value) : this()
        {
            BIMCategory = value.BIMCategory;
            BIMCategoryId = value.BIMCategoryId;
            RevitFamily = value.RevitFamily;
            RevitType = value.RevitType;
            RevitTypeId = value.RevitTypeId;
            AssemblyCode = value.AssemblyCode;
            AssemblyDescription = value.AssemblyDescription;
            SpecpointFamilyNumber = value.SpecpointFamilyNumber;
            SpecpointFamily = value.SpecpointFamily;
            Changed = value.Changed;
            IsLastSubassembly = value.IsLastSubassembly;
            IsInProject = value.IsInProject;
    }

        public override string ToString()
        {
            return string.Format("{0}|{1}|{2}|{3}|{4}",
                BIMCategory, RevitFamily, RevitType, AssemblyCode, AssemblyDescription);
        }

    }

    /// <summary>
    /// Data grid view cell for specifying an assembly code or an assembly
    /// parameter value, depending on whether the row contains an assembly
    /// or an assembly parameter.
    /// </summary>
    public class AssemblyValueCell : DataGridViewTextBoxCell
    {
        private const int MaxItemsToShowInListBox = 20;
        static AssemblyCodeEditingControl editingControl = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyValueCell"/> class.
        /// </summary>
        public AssemblyValueCell()
        {
            editingControl = new AssemblyCodeEditingControl();
        }

        /// <summary>
        /// Gets the type of the cell's hosted editing control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Type"/> representing the <see cref="T:System.Windows.Forms.DataGridViewTextBoxEditingControl"/> type.
        /// </returns>
        public override Type EditType
        {
            get
            {
                // Use our custom assembly code control for specifying assembly descriptions.
                return typeof(AssemblyCodeEditingControl);
            }
        }

        /// <summary>
        /// Attaches and initializes the hosted editing control.
        /// </summary>
        /// <param name="rowIndex">The index of the row being edited.</param>
        /// <param name="initialFormattedValue">The initial value to be displayed in the control.</param>
        /// <param name="dataGridViewCellStyle">A cell style that is used to determine the appearance of the hosted control.</param>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
        /// 	<IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
        /// </PermissionSet>
        public override void InitializeEditingControl(int rowIndex,
            object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Set the value of the editing control to the current cell value.
            base.InitializeEditingControl(rowIndex, initialFormattedValue,
                dataGridViewCellStyle);

            editingControl.Enabled = true;
            ReadOnly = false;
        }

        /// <summary>
        /// Handles the SelectionChangeCommitted event of the comboBoxControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void comboBoxControl_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (null != this.DataGridView)
            {
                DataGridViewComboBoxEditingControl comboBox = (DataGridViewComboBoxEditingControl)sender;
                comboBox.EditingControlValueChanged = true;
                comboBox.EditingControlDataGridView.NotifyCurrentCellDirty(true);
                comboBox.EditingControlDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);

                // Unbind the combo box cell's data source so that if the row gets
                // removed as a result of changing the cell value (e.g., gets
                // filtered out) an exception won't be thrown when the DataGridView
                // looks for the no-longer-existent cell.
                comboBox.DataSource = null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="graphics">The <see cref="T:System.Drawing.Graphics"/> used to draw the cell.</param>
        /// <param name="cellStyle">A <see cref="T:System.Windows.Forms.DataGridViewCellStyle"/> that represents the style of the cell.</param>
        /// <param name="rowIndex">The zero-based row index of the cell.</param>
        /// <param name="constraintSize">The cell's maximum allowable size.</param>
        /// <returns>
        /// A <see cref="T:System.Drawing.Size"/> that represents the preferred size, in pixels, of the cell.
        /// </returns>
        protected override Size GetPreferredSize(Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
        {
            try
            {
                if (this.IsInEditMode)
                    return editingControl.Size;
                else
                    return base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);
            }
            catch
            {
                return base.GetPreferredSize(graphics, cellStyle, rowIndex, constraintSize);
            }
        }

    }

    public class AssemblyCodeCollection : IEnumerable<Assembly>
    {
        /// <summary>
        /// Minimum assembly code level for which to include Revit Category ID
        /// when exporting to a text file.
        /// </summary>
        private const int MinimumLevelToIncludeCategory = 4;

        private SortedList<string, Assembly> assemblyCodes;

        private int maxLevel;

        /// <summary>
        /// Constructs a new AssemblyCodeCollection.
        /// </summary>
        public AssemblyCodeCollection()
        {
            this.assemblyCodes = new SortedList<string, Assembly>();
            this.maxLevel = -1;
        }

        /// <summary>
        /// The number of Assembly Codes in this collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this.assemblyCodes.Count;
            }
        }

        /// <summary>
        /// The deepest level among Assembly Codes in this collection.
        /// </summary>
        public int MaxLevel
        {
            get
            {
                return this.maxLevel;
            }
        }

        /// <summary>
        /// Gets or sets an Assembly by its Code.
        /// </summary>
        /// <param name="code">Identifies the Assembly Code to get or set.</param>
        public Assembly this[string code]
        {
            get
            {
                return this.assemblyCodes[code];
            }
            set
            {
                this.assemblyCodes[code] = value;
            }
        }

        /// <summary>
        /// Adds an Assembly to the collection.
        /// </summary>
        /// <param name="a">Assembly to add.</param>
        public void Add(Assembly a)
        {
            if (!this.ContainsAssemblyCode(a.AssemblyCode))
            {
                this.assemblyCodes.Add(a.AssemblyCode, a);
            }
        }

        /// <summary>
        /// Removes all Assembly Codes from this AssemblyCodeCollection.
        /// </summary>
        public void Clear()
        {
            this.assemblyCodes.Clear();
        }

        /// <summary>
        /// Determines whether this AssemblyCodeCollection contains an Assembly Code having
        /// the specified Code.
        /// </summary>
        /// <param name="code">Code of Assembly Code to check for.</param>
        /// <returns>true if this AssemblyCodeCollection contains a Assembly having the given Code; otherwise, false.</returns>
        public bool ContainsAssemblyCode(string code)
        {
            return this.assemblyCodes.ContainsKey(code);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Assembly> GetEnumerator()
        {
            return this.assemblyCodes.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }


        /// <summary>
        /// Removes the Assembly Code having the given Code.
        /// </summary>
        /// <param name="code">Assembly Code to remove.</param>
        /// <returns>true if Assembly Code was found and removed; otherwise, false.</returns>
        public bool Remove(string code)
        {
            return this.assemblyCodes.Remove(code);
        }

        /// <summary>
        /// Gets the Assembly Code having the given Code.
        /// </summary>
        /// <param name="code">Code of Assembly Code to get.</param>
        /// <param name="assemblyCode">Receives Assembly Code having given Code.</param>
        /// <returns>true if Assembly was found; otherwise, false.</returns>
        public bool TryGetAssemblyCode(string code, out Assembly assemblyCode)
        {
            return this.assemblyCodes.TryGetValue(code, out assemblyCode);
        }

        /// <summary>
        /// Gets the Assembly Code having the given Description.
        /// </summary>
        /// <param name="description">Description of Assembly Code to get.</param>
        /// <param name="assemblyCode">Receives Assembly Code having given Description.</param>
        /// <returns>true if Assembly was found; otherwise, false.</returns>
        public bool TryGetAssemblyCodeByDescription(string description, out Assembly assemblyCode)
        {
            assemblyCode = default(Assembly);
            foreach (Assembly candidateCode in this)
            {
                if (candidateCode.AssemblyDescription.ToUpper() == description.ToUpper())
                {
                    assemblyCode = candidateCode;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the parent of the given assembly code.
        /// </summary>
        /// <param name="code">Assembly code whose parent to look up.</param>
        /// <param name="parentCode">Receives the parent assembly code or <c>null</c> if not foun.</param>
        /// <returns><c>true</c> if parent was found; otherwise, <c>false</c>.</returns>
        public bool TryGetParentAssemblyCode(Assembly code, out Assembly parentCode)
        {
            parentCode = this.assemblyCodes.Values.FirstOrDefault(a =>
                code.AssemblyCode.StartsWith(a.AssemblyCode, StringComparison.OrdinalIgnoreCase));
            return parentCode != null;
        }


        private TreeNode AddAssemblyNode(TreeNode root, Assembly assemblyCode)
        {
            string nodeText = assemblyCode.ToString();
            TreeNode node = root.Nodes.Add(nodeText);
            if (assemblyCode.BIMCategory != string.Empty)
            {
                node.ToolTipText = "Revit Category: " + assemblyCode.BIMCategory;
            }
            return node;
        }
    }
}
