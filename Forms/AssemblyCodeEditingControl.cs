using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// Control for setting an an e-SPECS Assembly Code.
    /// </summary>
    public partial class AssemblyCodeEditingControl : UserControl, IDataGridViewEditingControl
    {
        DataGridView dataGridView = null;
        int rowIndex = 0;
        bool valueChanged = false;
        bool updatingAssembly = false;
        string prevText = null;
        AutoCompleteStringCollection autoCompleteCodes = null;
        AutoCompleteStringCollection autoCompleteDescriptions = null;
        AssemblyCodeCollection assemblyCodes = null;

        // This the list of all asembly codes (added and note added)
        AssemblyCodeCollection otherAssemblyCodes = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCodeEditingControl"/> class.
        /// </summary>
        public AssemblyCodeEditingControl()
        {
            InitializeComponent();
            this.textBoxAssemblyCode.LostFocus += new EventHandler(textBoxAssemblyCode_LostFocus);
        }

        /// <summary>
        /// Handles the LostFocus event of the textBoxAssemblyCode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void textBoxAssemblyCode_LostFocus(object sender, EventArgs e)
        {
            NotifyChange();
        }

        private void PersistUpdate(Assembly assembly)
        {
            ModelValidationForm parentForm = this.ParentForm as ModelValidationForm;

            // Update internal list
            string uniqueId = currentAssembly.RevitTypeId;
            if (Globals.revitElements.ContainsKey(uniqueId))
            {
                Globals.revitElements[uniqueId].AssemblyCode = assembly.AssemblyCode;
                Globals.revitElements[uniqueId].AssemblyDescription = assembly.AssemblyDescription;

                if (!parentForm.changedRows.Contains(uniqueId))
                {
                    parentForm.changedRows.Add(uniqueId);

                    // Update list of assigned assembly codes
                    Globals.revitElements.AssignedAssemblyCodes.Add(assembly.AssemblyCode);
                }
            }
        }

        /// <summary>
        /// Returns true if the specified assembly code is assigned to a BIM element.
        /// </summary>
        /// <param name="assemblyCode"></param>
        /// <returns></returns>
        private bool AssignedToBIMElement(string assemblyCode)
        {
            // Check if the assembly code is assigned to another element
            bool result = false;
            foreach (var node in Globals.revitElements)
            {
                if (node.Value.AssemblyCode != assemblyCode) continue;

                // The assembly code is assigned to another element
                result = true;
                break;
            }

            return result;
        }

        private void buttonChooseAssemblyCode_Click(object sender, EventArgs e)
        {
            try
            {
                //this could be a project file then there is no currentAssembly
                if (this.currentAssembly == null) return;

                // Disable button when it's an unassigned Specpoint Assembly row
                if (this.currentAssembly.BIMCategory == "") return;

                valueChanged = false;

                ModelValidationForm parentForm = this.ParentForm as ModelValidationForm;
                
                SetAssemblyCodeForm dlg = new SetAssemblyCodeForm(parentForm.projectId, currentAssembly);
                dlg.RevitAssemblyCode = currentAssembly.AssemblyCode;
                dlg.StartPosition = FormStartPosition.CenterParent;
                DialogResult ret = dlg.ShowDialog(parentForm);

                Assembly ca = this.currentAssembly;
                if (ca == null)
                {
                    return;
                }

                string msg = string.Format("CurrentAssembly Before. RevitTypeId({0}) Category({1}) Family({2}) Type({3}) Assembly({4})",
                    ca.RevitTypeId, ca.BIMCategory, ca.RevitFamily, ca.RevitType, ca.AssemblyCode);
                Globals.Log.Write(msg);

                if (ret == DialogResult.OK)
                {
                    // Update internal list
                    string uniqueId = currentAssembly.RevitTypeId;
                    if (Globals.revitElements.ContainsKey(uniqueId))
                    {
                        Assembly row = Globals.revitElements[uniqueId];

                        msg = string.Format("CurrentAssembly After . RevitTypeId({0}) Category({1}) Family({2}) Type({3}) Assembly({4} -> {5})",
                            uniqueId, row.BIMCategory, row.RevitFamily, row.RevitType, row.AssemblyCode, dlg.RevitAssemblyCode);
                        Globals.Log.Write(msg);

                        row.AssemblyCode = dlg.RevitAssemblyCode;
                        row.AssemblyDescription = dlg.RevitAssemblyDescription;

                        if (!parentForm.changedRows.Contains(uniqueId))
                        {
                            parentForm.changedRows.Add(uniqueId);
                        }
                            
                        if (!string.IsNullOrEmpty(row.AssemblyCode))
                        {
                            // Update list of assigned assembly codes
                            Globals.revitElements.AssignedAssemblyCodes.Add(row.AssemblyCode);
                        }

                        // If user removed the assigned assembly code
                        bool assemblyWasRemoved =
                            !string.IsNullOrEmpty(ca.AssemblyCode) &&
                            string.IsNullOrEmpty(dlg.RevitAssemblyCode);

                        // If user changed the assigned assembly code
                        bool assemblyWasChanged =
                            !string.IsNullOrEmpty(ca.AssemblyCode) &&
                            !string.IsNullOrEmpty(dlg.RevitAssemblyCode) &&
                            ca.AssemblyCode != dlg.RevitAssemblyCode;

                        if (assemblyWasRemoved ||
                            assemblyWasChanged)
                        {
                            // Check if the assembly code is assigned to another element
                            bool assigned = AssignedToBIMElement(ca.AssemblyCode);
       
                            // If not assigned to another element
                            if (assigned == false)
                            {
                                // Remove from list of assigned assembly codes
                                Globals.revitElements.AssignedAssemblyCodes.Remove(ca.AssemblyCode);
                            }
                        }
                    }

                    if (dlg.RevitAssemblyCode == null)
                    {
                        this.textBoxAssemblyCode.Text = "";
                    }
                    else
                    {
                        if (this.currentColumn.Name == "Assembly Code")
                            this.textBoxAssemblyCode.Text = dlg.RevitAssemblyCode;
                        else
                            this.textBoxAssemblyCode.Text = dlg.RevitAssemblyDescription;
                    }

                    // Notify that cell has been modified
                    NotifyChange();
                    AfterEdit();

                    if (parentForm.lastUpdated != dlg.lastUpdated ||
                        parentForm.addedAssemblyToSP == true)
                    {
                        string projectName = SpecpointRegistry.GetValue("ProjectName");
                        string modifiedMsg = string.Format("The Specpoint Project ({0}) has been modified.\nWould you like to Refresh Data to download the latest changes.", projectName); ;
                        DialogResult willRefreshData = MessageBox.Show(modifiedMsg, parentForm.Text, 
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (willRefreshData == DialogResult.Yes)
                        {
                            parentForm.addedAssemblyToSP = false;
                            parentForm.Reload();
                        }
                    }
                }
                else
                {
                    AfterEdit();
                    this.textBoxAssemblyCode.Focus();
                    this.textBoxAssemblyCode.SelectAll();
                }
            }
            catch (TokenExpiredException)
            {
                OnTokenExpired(nameof(buttonChooseAssemblyCode_Click));
            }
            catch (TargetInvocationException)
            {
                OnTokenExpired(nameof(buttonChooseAssemblyCode_Click));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }
        }

        private bool HasSpecpointProjectChanged(
            string assemblyCode,
            Dictionary<string, ProjectElementNode> previous,
            Dictionary<string, ProjectElementNode> current)
        {
            if (string.IsNullOrEmpty(assemblyCode)) return false;   
            if (previous == null) return false;
            if (current == null) return false;

            bool removedFromProject =
                previous.ContainsKey(assemblyCode) &&
                !current.ContainsKey(assemblyCode);

            bool addedToProject =
                !previous.ContainsKey(assemblyCode) &&
                current.ContainsKey(assemblyCode);

            return removedFromProject || addedToProject;
        }

        private void NotifyChange()
        {
            if (this.textBoxAssemblyCode.Text == prevText)
                return; // no change
            if (this.UpdateAssembly())
            {
                valueChanged = true;
                dataGridView.NotifyCurrentCellDirty(true);
            }
        }

        /// <summary>
        /// Validates user's input and updates the row's Assembly if valid.
        /// </summary>
        /// <returns>true if user input was valid; otherwise, false.</returns>
        private bool UpdateAssembly()
        {
            //this could be a project file then there is no currentAssembly
            if (this.currentAssembly == null) return false;

            // If already updating assembly, do nothing
            if (updatingAssembly == true) return false;

            try
            {
                updatingAssembly = true;

                string valueEntered = this.textBoxAssemblyCode.Text.Trim();
                if (valueEntered == "")
                {
                    // User deleted Assembly Code or Description
                    this.currentAssembly.AssemblyCode = "";
                    this.currentAssembly.AssemblyDescription = "";
                    return true;
                }

                // User entered a value--validate input
                if (currentColumn.Name == "Assembly Code")
                {
                    // Validate Assembly Code
                    string code = valueEntered.ToUpper();
                    Assembly assembly = null;
                    if (this.assemblyCodes != null &&
                        !this.assemblyCodes.TryGetAssemblyCode(code, out assembly))
                    {
                        // When the user switched from AddedToProject to AllProjectElements
                        // We'll need to validate Assembly Code againt all added and not added assembly codes
                        if (this.otherAssemblyCodes != null &&
                            !this.otherAssemblyCodes.TryGetAssemblyCode(code, out assembly))
                        {
                            assembly = Globals.FindAssemblyByCode(code);
                            if (assembly == null)
                            {
                                string errMsg = String.Format("\"{0}\" is not a valid Assembly Code.", code);
                                ErrorReporter.ShowMessage(errMsg);
                                this.textBoxAssemblyCode.Text = "";
                                AfterEdit();
                                return false;
                            }
                           
                        }
                    }

                    if (assembly != null)
                    {
                        this.currentAssembly.AssemblyCode = assembly.AssemblyCode;
                        this.currentAssembly.AssemblyDescription = assembly.AssemblyDescription;
                        this.textBoxAssemblyCode.Text = assembly.AssemblyCode;

                        // Persist change
                        PersistUpdate(assembly);
                    }
                }
                else
                {
                    // Validate Assembly Description
                    string desc = valueEntered;
                    Assembly assembly = null;
                    if (this.assemblyCodes != null &&
                        !this.assemblyCodes.TryGetAssemblyCodeByDescription(desc, out assembly))
                    {
                        // Validate Assembly Description againt all added and not added assembly description
                        if (this.otherAssemblyCodes != null &&
                            !this.otherAssemblyCodes.TryGetAssemblyCodeByDescription(desc, out assembly))
                        {
                            assembly = Globals.FindAssemblyByDescription(desc);
                            if (assembly == null)
                            {
                                string errMsg = String.Format("\"{0}\" is not a valid Assembly Description.", desc);
                                ErrorReporter.ShowMessage(errMsg);
                                this.textBoxAssemblyCode.Text = "";
                                AfterEdit();
                                return false;
                            }
                        }
                    }

                    if (assembly != null)
                    {
                        this.currentAssembly.AssemblyCode = assembly.AssemblyCode;
                        this.currentAssembly.AssemblyDescription = assembly.AssemblyDescription;
                        this.textBoxAssemblyCode.Text = assembly.AssemblyDescription;

                        // Persist change
                        PersistUpdate(assembly);
                    }
                }
            }
            finally
            {
                // Reset flag
                updatingAssembly = false;
            }

            return true;
        }

        private DataGridViewColumn currentColumn
        {
            get
            {
                return this.dataGridView.Columns[this.dataGridView.CurrentCell.ColumnIndex];
            }
        }

        private Assembly currentAssembly
        {
            get
            {
                if (this.dataGridView == null) return null;
                if (this.dataGridView.CurrentRow == null) return null;
                
                return (Assembly)this.dataGridView.CurrentRow.Tag;
            }
        }

        #region IDataGridViewEditingControl Members

        /// <summary>
        /// Changes the control's user interface (UI) to be consistent with the specified cell style.
        /// </summary>
        /// <param name="dataGridViewCellStyle">The <see cref="T:System.Windows.Forms.DataGridViewCellStyle"/> to use as the model for the UI.</param>
        public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
        {
            // Do nothing
        }

        /// <summary>
        /// Gets the editing control cursor.
        /// </summary>
        /// <value>The editing control cursor.</value>
        public Cursor EditingControlCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }


        /// <summary>
        /// Gets the cursor used when the mouse pointer is over the <see cref="P:System.Windows.Forms.DataGridView.EditingPanel"/> but not over the editing control.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// A <see cref="T:System.Windows.Forms.Cursor"/> that represents the mouse pointer used for the editing panel.
        /// </returns>
        public Cursor EditingPanelCursor
        {
            get
            {
                return Cursors.IBeam;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.Forms.DataGridView"/> that contains the cell.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Windows.Forms.DataGridView"/> that contains the <see cref="T:System.Windows.Forms.DataGridViewCell"/> that is being edited; null if there is no associated <see cref="T:System.Windows.Forms.DataGridView"/>.
        /// </returns>
        public DataGridView EditingControlDataGridView
        {
            get
            {
                return dataGridView;
            }
            set
            {
                dataGridView = value;
            }
        }

        /// <summary>
        /// Gets or sets the formatted value of the cell being modified by the editor.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the formatted value of the cell.
        /// </returns>
        public object EditingControlFormattedValue
        {
            get
            {
                return this.textBoxAssemblyCode.Text;
            }
            set
            {
                this.textBoxAssemblyCode.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the index of the hosting cell's parent row.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The index of the row that contains the cell, or –1 if there is no parent row.
        /// </returns>
        public int EditingControlRowIndex
        {
            get
            {
                return rowIndex;
            }
            set
            {
                rowIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the value of the editing control differs from the value of the hosting cell.
        /// </summary>
        /// <value></value>
        /// <returns>true if the value of the control differs from the cell value; otherwise, false.
        /// </returns>
        public bool EditingControlValueChanged
        {
            get
            {
                return valueChanged;
            }
            set
            {
                valueChanged = value;
            }
        }

        /// <summary>
        /// Determines whether the specified key is a regular input key that the editing control should process or a special key that the <see cref="T:System.Windows.Forms.DataGridView"/> should process.
        /// </summary>
        /// <param name="keyData">A <see cref="T:System.Windows.Forms.Keys"/> that represents the key that was pressed.</param>
        /// <param name="dataGridViewWantsInputKey">true when the <see cref="T:System.Windows.Forms.DataGridView"/> wants to process the <see cref="T:System.Windows.Forms.Keys"/> in <paramref name="keyData"/>; otherwise, false.</param>
        /// <returns>
        /// true if the specified key is a regular input key that should be handled by the editing control; otherwise, false.
        /// </returns>
        public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
        {
            switch (keyData)
            {
                case Keys.Tab:
                    return true;
                case Keys.Home:
                case Keys.End:
                case Keys.Left:
                    if (this.textBoxAssemblyCode.SelectionLength == this.textBoxAssemblyCode.Text.Length)
                        return false;
                    else
                        return true;
                case Keys.Right:
                    return true;
                case Keys.Delete:
                    this.textBoxAssemblyCode.Text = "";
                    return true;
                case Keys.Enter:
                    NotifyChange();
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Retrieves the formatted value of the cell.
        /// </summary>
        /// <param name="context">A bitwise combination of <see cref="T:System.Windows.Forms.DataGridViewDataErrorContexts"/> values that specifies the context in which the data is needed.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that represents the formatted version of the cell contents.
        /// </returns>
        public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
        {
            return this.textBoxAssemblyCode.Text;
        }

        private int GetSetAssemblyCodeMode()
        {
            int setAssemblyCodeMode = SetAssemblyCodeMode.AddedToProject;
            string mode = SpecpointRegistry.GetValue("SetAssemblyCodeMode");
            if (!string.IsNullOrEmpty(mode))
            {
                setAssemblyCodeMode = Convert.ToInt32(mode);
            }

            return setAssemblyCodeMode;
        }

        public void PrepareAssembliesChecklist()
        {
            ModelValidationForm parent = this.ParentForm as ModelValidationForm;
            if (parent == null) return;

            // Get list of auto complete codes and descriptions 
            // from the last subassemblies added to the project
            foreach (var a in Globals.lastSubAssemblies)
            {
                assemblyCodes.Add(a.Value);

                autoCompleteCodes.Add(a.Key);
                autoCompleteDescriptions.Add(a.Value.AssemblyDescription);
            }
        }

        public void PrepareOtherAssembliesChecklist()
        {
            ModelValidationForm parent = this.ParentForm as ModelValidationForm;
            if (parent == null) return;

            // Get list of auto complete codes and descriptions 
            // from all last subassemblies
            foreach (var a in Globals.otherAssemblies)
            {
                assemblyCodes.Add(a.Value);
                otherAssemblyCodes.Add(a.Value);

                autoCompleteCodes.Add(a.Key);
                autoCompleteDescriptions.Add(a.Value.AssemblyDescription);
            }
        }

        /// <summary>
        /// Prepares the currently selected cell for editing.
        /// </summary>
        /// <param name="selectAll">true to select all of the cell's content; otherwise, false.</param>
        public async void PrepareEditingControlForEdit(bool selectAll)
        {
            ModelValidationForm parent = this.ParentForm as ModelValidationForm;
            if (parent == null) return;
            if (parent.matchingAssemblies == null) return;

            if (this.dataGridView.CurrentRow != null &&
                this.dataGridView.CurrentRow.Cells[ColumnIndex.BIMCategory].Value.ToString() == "")
            {
                // Skip edit for unassigned Specpoint assembly rows
                parent.grid_EndEdit();
                return;
            }

            if (this.dataGridView.CurrentCell == null || this.dataGridView.CurrentCell.Value == null)
                this.textBoxAssemblyCode.Text = "";
            else
                this.textBoxAssemblyCode.Text = this.dataGridView.CurrentCell.Value.ToString();
            if (selectAll)
                this.textBoxAssemblyCode.SelectAll();
            prevText = this.textBoxAssemblyCode.Text;

            if (Globals.lastSubAssemblies == null ||
                (Globals.lastSubAssemblies != null && Globals.lastSubAssemblies.Count == 0))
            {
                bool ret = await parent.GetLastSubAssemblies();
            }

            if (Globals.otherAssemblies == null ||
                (Globals.otherAssemblies != null && Globals.otherAssemblies.Count == 0))
            {
                bool ret = await parent.GetOtherAssemblies();
            }

            if (this.assemblyCodes == null)
            {
                this.assemblyCodes = new AssemblyCodeCollection();
            }

            if (this.otherAssemblyCodes == null)
            {
                this.otherAssemblyCodes = new AssemblyCodeCollection();
            }

            // Cache strings for AutoComplete
            if (this.assemblyCodes != null)
            {
                this.autoCompleteCodes = new AutoCompleteStringCollection();
                this.autoCompleteDescriptions = new AutoCompleteStringCollection();

                int setAssemblyCodeMode = GetSetAssemblyCodeMode();
                if (setAssemblyCodeMode == SetAssemblyCodeMode.AddedToProject)
                {
                    // Prepare the list of added assembly codes
                    PrepareAssembliesChecklist();

                    // Prepare the list of not added assembly codes
                    if (otherAssemblyCodes.Count == 0)
                    {
                        // Get list of auto complete codes and descriptions 
                        // from all last subassemblies
                        foreach (var a in Globals.otherAssemblies)
                        {
                            otherAssemblyCodes.Add(a.Value);
                        }
                    }
                }
                else if (setAssemblyCodeMode == SetAssemblyCodeMode.AllProjectElements)
                {
                    // Prepare the list of added and not added assembly codes
                    PrepareOtherAssembliesChecklist();
                }

            }

            // Assign AutoComplete strings appropriate to the column type
            if (this.currentColumn.Name == "Assembly Code")
            {
                this.textBoxAssemblyCode.AutoCompleteCustomSource = autoCompleteCodes;
                this.textBoxAssemblyCode.Enabled = true;
                this.buttonChooseAssemblyCode.Enabled = true;
            }
            else if (this.currentColumn.Name == "Assembly Description")
            {
                this.textBoxAssemblyCode.AutoCompleteCustomSource = autoCompleteDescriptions;
                this.textBoxAssemblyCode.Enabled = true;
                this.buttonChooseAssemblyCode.Enabled = true;
            }
        }

        private void OnTokenExpired(string msg)
        {
            ModelValidationForm parent = this.ParentForm as ModelValidationForm;
            if (parent == null) return;

            parent.OnTokenExpired(msg);
        }

        private void AfterEdit()
        {
            ModelValidationForm parent = this.ParentForm as ModelValidationForm;
            if (parent == null) return;

            parent.grid_EndEdit();
        }

        private void AssemblyCodeEditingControl_Leave(object sender, EventArgs e)
        {
            AfterEdit();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cell contents need to be repositioned whenever the value changes.
        /// </summary>
        /// <value></value>
        /// <returns>true if the contents need to be repositioned; otherwise, false.
        /// </returns>
        public bool RepositionEditingControlOnValueChange
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}
