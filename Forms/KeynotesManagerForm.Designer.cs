namespace Specpoint.Revit2026
{
    partial class KeynotesManagerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelElementList = new System.Windows.Forms.Label();
            this.treeViewElements = new CheckboxTreeView();
            this.menuElements = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.applyElementsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeElementsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelMaterialList = new System.Windows.Forms.Label();
            this.treeViewMaterials = new CheckboxTreeView();
            this.menuMaterials = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.applyMaterialsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeMaterialsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeViewKeynotes = new CheckboxTreeView();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelFind = new System.Windows.Forms.Label();
            this.textBoxFind = new System.Windows.Forms.TextBox();
            this.buttonPrevious = new System.Windows.Forms.Button();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonShowHide = new System.Windows.Forms.Button();
            this.buttonEdit = new System.Windows.Forms.Button();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.buttonNew = new System.Windows.Forms.Button();
            this.groupBoxKeynotes = new System.Windows.Forms.GroupBox();
            this.comboBoxFilter = new System.Windows.Forms.ComboBox();
            this.comboBoxView = new System.Windows.Forms.ComboBox();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.buttonExport = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.buttonReload = new System.Windows.Forms.Button();
            this.menuElements.SuspendLayout();
            this.menuMaterials.SuspendLayout();
            this.groupBoxKeynotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelElementList
            // 
            this.labelElementList.AutoSize = true;
            this.labelElementList.Location = new System.Drawing.Point(2, 0);
            this.labelElementList.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelElementList.Name = "labelElementList";
            this.labelElementList.Size = new System.Drawing.Size(64, 13);
            this.labelElementList.TabIndex = 1;
            this.labelElementList.Text = "Element List";
            // 
            // treeViewElements
            // 
            this.treeViewElements.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewElements.ContextMenuStrip = this.menuElements;
            this.treeViewElements.HideSelection = false;
            this.treeViewElements.Location = new System.Drawing.Point(0, 25);
            this.treeViewElements.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewElements.Name = "treeViewElements";
            this.treeViewElements.Size = new System.Drawing.Size(414, 319);
            this.treeViewElements.TabIndex = 0;
            this.treeViewElements.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewElements_BeforeCheck);
            this.treeViewElements.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewElements_AfterSelect);
            this.treeViewElements.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewElements_NodeMouseClick);
            this.treeViewElements.Click += new System.EventHandler(this.treeViewElements_Click);
            // 
            // menuElements
            // 
            this.menuElements.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyElementsMenuItem,
            this.removeElementsMenuItem});
            this.menuElements.Name = "contextMenuStripElements";
            this.menuElements.Size = new System.Drawing.Size(211, 48);
            this.menuElements.Opening += new System.ComponentModel.CancelEventHandler(this.menuElements_Opening);
            this.menuElements.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.contextMenuStripElements_ItemClicked);
            // 
            // applyElementsMenuItem
            // 
            this.applyElementsMenuItem.Name = "applyElementsMenuItem";
            this.applyElementsMenuItem.Size = new System.Drawing.Size(210, 22);
            this.applyElementsMenuItem.Text = "Apply Selected Keynote";
            this.applyElementsMenuItem.Click += new System.EventHandler(this.applyKeynoteMenuItem_Click);
            // 
            // removeElementsMenuItem
            // 
            this.removeElementsMenuItem.Name = "removeElementsMenuItem";
            this.removeElementsMenuItem.Size = new System.Drawing.Size(210, 22);
            this.removeElementsMenuItem.Text = "Remove Selected Keynote";
            this.removeElementsMenuItem.Click += new System.EventHandler(this.removeElementsMenuItem_Click);
            // 
            // labelMaterialList
            // 
            this.labelMaterialList.AutoSize = true;
            this.labelMaterialList.Location = new System.Drawing.Point(2, 0);
            this.labelMaterialList.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelMaterialList.Name = "labelMaterialList";
            this.labelMaterialList.Size = new System.Drawing.Size(63, 13);
            this.labelMaterialList.TabIndex = 1;
            this.labelMaterialList.Text = "Material List";
            // 
            // treeViewMaterials
            // 
            this.treeViewMaterials.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewMaterials.ContextMenuStrip = this.menuMaterials;
            this.treeViewMaterials.HideSelection = false;
            this.treeViewMaterials.Location = new System.Drawing.Point(5, 25);
            this.treeViewMaterials.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewMaterials.Name = "treeViewMaterials";
            this.treeViewMaterials.Size = new System.Drawing.Size(456, 319);
            this.treeViewMaterials.TabIndex = 0;
            this.treeViewMaterials.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewMaterials_BeforeCheck);
            this.treeViewMaterials.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMaterials_AfterSelect);
            this.treeViewMaterials.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewMaterials_NodeMouseClick);
            this.treeViewMaterials.Click += new System.EventHandler(this.treeViewMaterials_Click);
            // 
            // menuMaterials
            // 
            this.menuMaterials.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyMaterialsMenuItem,
            this.removeMaterialsMenuItem});
            this.menuMaterials.Name = "menuMaterials";
            this.menuMaterials.Size = new System.Drawing.Size(211, 48);
            this.menuMaterials.Opening += new System.ComponentModel.CancelEventHandler(this.menuMaterials_Opening);
            // 
            // applyMaterialsMenuItem
            // 
            this.applyMaterialsMenuItem.Name = "applyMaterialsMenuItem";
            this.applyMaterialsMenuItem.Size = new System.Drawing.Size(210, 22);
            this.applyMaterialsMenuItem.Text = "Apply Selected Keynote";
            this.applyMaterialsMenuItem.Click += new System.EventHandler(this.applyMaterialsMenuItem_Click);
            // 
            // removeMaterialsMenuItem
            // 
            this.removeMaterialsMenuItem.Name = "removeMaterialsMenuItem";
            this.removeMaterialsMenuItem.Size = new System.Drawing.Size(210, 22);
            this.removeMaterialsMenuItem.Text = "Remove Selected Keynote";
            this.removeMaterialsMenuItem.Click += new System.EventHandler(this.removeMaterialsMenuItem_Click);
            // 
            // treeViewKeynotes
            // 
            this.treeViewKeynotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewKeynotes.HideSelection = false;
            this.treeViewKeynotes.Location = new System.Drawing.Point(15, 45);
            this.treeViewKeynotes.Margin = new System.Windows.Forms.Padding(2);
            this.treeViewKeynotes.Name = "treeViewKeynotes";
            this.treeViewKeynotes.Size = new System.Drawing.Size(852, 206);
            this.treeViewKeynotes.TabIndex = 0;
            this.treeViewKeynotes.BeforeCheck += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewKeynotes_BeforeCheck);
            this.treeViewKeynotes.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeViewKeynotes_BeforeSelect);
            this.treeViewKeynotes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewKeynotes_AfterSelect);
            this.treeViewKeynotes.DoubleClick += new System.EventHandler(this.treeViewKeynotes_DoubleClick);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(841, 655);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(56, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(781, 655);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(56, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "Apply";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelFind
            // 
            this.labelFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelFind.AutoSize = true;
            this.labelFind.Location = new System.Drawing.Point(458, 22);
            this.labelFind.Name = "labelFind";
            this.labelFind.Size = new System.Drawing.Size(27, 13);
            this.labelFind.TabIndex = 9;
            this.labelFind.Text = "Find";
            // 
            // textBoxFind
            // 
            this.textBoxFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFind.Location = new System.Drawing.Point(491, 19);
            this.textBoxFind.Name = "textBoxFind";
            this.textBoxFind.Size = new System.Drawing.Size(214, 20);
            this.textBoxFind.TabIndex = 10;
            // 
            // buttonPrevious
            // 
            this.buttonPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPrevious.Location = new System.Drawing.Point(711, 17);
            this.buttonPrevious.Name = "buttonPrevious";
            this.buttonPrevious.Size = new System.Drawing.Size(75, 23);
            this.buttonPrevious.TabIndex = 11;
            this.buttonPrevious.Text = "Previous";
            this.buttonPrevious.UseVisualStyleBackColor = true;
            this.buttonPrevious.Click += new System.EventHandler(this.buttonPrevious_Click);
            // 
            // buttonNext
            // 
            this.buttonNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonNext.Location = new System.Drawing.Point(792, 17);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 12;
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonShowHide
            // 
            this.buttonShowHide.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonShowHide.Location = new System.Drawing.Point(170, 256);
            this.buttonShowHide.Name = "buttonShowHide";
            this.buttonShowHide.Size = new System.Drawing.Size(55, 23);
            this.buttonShowHide.TabIndex = 13;
            this.buttonShowHide.Text = "Hide";
            this.buttonShowHide.UseVisualStyleBackColor = true;
            this.buttonShowHide.Click += new System.EventHandler(this.buttonShowHide_Click);
            // 
            // buttonEdit
            // 
            this.buttonEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEdit.Location = new System.Drawing.Point(119, 256);
            this.buttonEdit.Name = "buttonEdit";
            this.buttonEdit.Size = new System.Drawing.Size(45, 23);
            this.buttonEdit.TabIndex = 14;
            this.buttonEdit.Text = "Edit";
            this.buttonEdit.UseVisualStyleBackColor = true;
            this.buttonEdit.Click += new System.EventHandler(this.buttonEdit_Click);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCopy.Location = new System.Drawing.Point(68, 256);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(45, 23);
            this.buttonCopy.TabIndex = 15;
            this.buttonCopy.Text = "Copy";
            this.buttonCopy.UseVisualStyleBackColor = true;
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // buttonNew
            // 
            this.buttonNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNew.Location = new System.Drawing.Point(15, 256);
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(47, 23);
            this.buttonNew.TabIndex = 16;
            this.buttonNew.Text = "New";
            this.buttonNew.UseVisualStyleBackColor = true;
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // groupBoxKeynotes
            // 
            this.groupBoxKeynotes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxKeynotes.Controls.Add(this.comboBoxFilter);
            this.groupBoxKeynotes.Controls.Add(this.comboBoxView);
            this.groupBoxKeynotes.Controls.Add(this.treeViewKeynotes);
            this.groupBoxKeynotes.Controls.Add(this.buttonNew);
            this.groupBoxKeynotes.Controls.Add(this.buttonCopy);
            this.groupBoxKeynotes.Controls.Add(this.buttonEdit);
            this.groupBoxKeynotes.Controls.Add(this.buttonNext);
            this.groupBoxKeynotes.Controls.Add(this.buttonShowHide);
            this.groupBoxKeynotes.Controls.Add(this.buttonPrevious);
            this.groupBoxKeynotes.Controls.Add(this.labelFind);
            this.groupBoxKeynotes.Controls.Add(this.textBoxFind);
            this.groupBoxKeynotes.Location = new System.Drawing.Point(12, 364);
            this.groupBoxKeynotes.Name = "groupBoxKeynotes";
            this.groupBoxKeynotes.Size = new System.Drawing.Size(883, 285);
            this.groupBoxKeynotes.TabIndex = 18;
            this.groupBoxKeynotes.TabStop = false;
            this.groupBoxKeynotes.Text = "Project Keynotes";
            // 
            // comboBoxFilter
            // 
            this.comboBoxFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFilter.FormattingEnabled = true;
            this.comboBoxFilter.Location = new System.Drawing.Point(15, 19);
            this.comboBoxFilter.Name = "comboBoxFilter";
            this.comboBoxFilter.Size = new System.Drawing.Size(149, 21);
            this.comboBoxFilter.TabIndex = 18;
            this.comboBoxFilter.SelectedIndexChanged += new System.EventHandler(this.comboBoxFilter_SelectedIndexChanged);
            // 
            // comboBoxView
            // 
            this.comboBoxView.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxView.FormattingEnabled = true;
            this.comboBoxView.Location = new System.Drawing.Point(170, 19);
            this.comboBoxView.Name = "comboBoxView";
            this.comboBoxView.Size = new System.Drawing.Size(118, 21);
            this.comboBoxView.TabIndex = 17;
            this.comboBoxView.SelectedIndexChanged += new System.EventHandler(this.comboBoxView_SelectedIndexChanged);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 12);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.labelElementList);
            this.splitContainer.Panel1.Controls.Add(this.treeViewElements);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.labelMaterialList);
            this.splitContainer.Panel2.Controls.Add(this.treeViewMaterials);
            this.splitContainer.Size = new System.Drawing.Size(883, 346);
            this.splitContainer.SplitterDistance = 416;
            this.splitContainer.TabIndex = 19;
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(701, 655);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(75, 23);
            this.buttonExport.TabIndex = 19;
            this.buttonExport.Text = "Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
            this.statusStrip.Location = new System.Drawing.Point(0, 684);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(909, 22);
            this.statusStrip.TabIndex = 20;
            this.statusStrip.Text = "statusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(112, 17);
            this.labelStatus.Text = "toolStripStatusLabel";
            // 
            // buttonReload
            // 
            this.buttonReload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReload.Location = new System.Drawing.Point(620, 655);
            this.buttonReload.Name = "buttonReload";
            this.buttonReload.Size = new System.Drawing.Size(75, 23);
            this.buttonReload.TabIndex = 21;
            this.buttonReload.Text = "Reload";
            this.buttonReload.UseVisualStyleBackColor = true;
            this.buttonReload.Click += new System.EventHandler(this.buttonReload_Click);
            // 
            // KeynotesManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(909, 706);
            this.Controls.Add(this.buttonReload);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.buttonExport);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.groupBoxKeynotes);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "KeynotesManagerForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Specpoint Keynotes Manager";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.KeynotesManagerForm_FormClosed);
            this.Load += new System.EventHandler(this.KeynotesManagerForm_Load);
            this.menuElements.ResumeLayout(false);
            this.menuMaterials.ResumeLayout(false);
            this.groupBoxKeynotes.ResumeLayout(false);
            this.groupBoxKeynotes.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private CheckboxTreeView treeViewElements;
        private CheckboxTreeView treeViewMaterials;
        private CheckboxTreeView treeViewKeynotes;
        // private System.Windows.Forms.TreeView treeViewElements;
        // private System.Windows.Forms.TreeView treeViewMaterials;
        // private System.Windows.Forms.TreeView treeViewKeynotes;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelElementList;
        private System.Windows.Forms.Label labelMaterialList;
        private System.Windows.Forms.ContextMenuStrip menuElements;
        private System.Windows.Forms.ToolStripMenuItem applyElementsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeElementsMenuItem;
        private System.Windows.Forms.ContextMenuStrip menuMaterials;
        private System.Windows.Forms.ToolStripMenuItem applyMaterialsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeMaterialsMenuItem;
        private System.Windows.Forms.Label labelFind;
        private System.Windows.Forms.TextBox textBoxFind;
        private System.Windows.Forms.Button buttonPrevious;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonShowHide;
        private System.Windows.Forms.Button buttonEdit;
        private System.Windows.Forms.Button buttonCopy;
        private System.Windows.Forms.Button buttonNew;
        private System.Windows.Forms.GroupBox groupBoxKeynotes;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ComboBox comboBoxView;
        private System.Windows.Forms.ComboBox comboBoxFilter;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.Button buttonReload;
    }
}