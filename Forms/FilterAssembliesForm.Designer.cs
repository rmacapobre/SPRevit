namespace Specpoint.Revit2026
{
    partial class FilterAssembliesForm
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
            this.groupBoxIncludeCategories = new System.Windows.Forms.GroupBox();
            this.treeViewStructural = new CheckboxTreeView();
            this.treeViewMEP = new CheckboxTreeView();
            this.treeView = new CheckboxTreeView();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxIncludeCategories.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxIncludeCategories
            // 
            this.groupBoxIncludeCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIncludeCategories.Controls.Add(this.treeViewStructural);
            this.groupBoxIncludeCategories.Controls.Add(this.treeViewMEP);
            this.groupBoxIncludeCategories.Controls.Add(this.treeView);
            this.groupBoxIncludeCategories.Location = new System.Drawing.Point(8, 7);
            this.groupBoxIncludeCategories.Name = "groupBoxIncludeCategories";
            this.groupBoxIncludeCategories.Size = new System.Drawing.Size(798, 353);
            this.groupBoxIncludeCategories.TabIndex = 0;
            this.groupBoxIncludeCategories.TabStop = false;
            this.groupBoxIncludeCategories.Text = "Include Categories";
            // 
            // treeViewStructural
            // 
            this.treeViewStructural.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewStructural.Location = new System.Drawing.Point(522, 21);
            this.treeViewStructural.Name = "treeViewStructural";
            this.treeViewStructural.Size = new System.Drawing.Size(270, 327);
            this.treeViewStructural.TabIndex = 2;
            this.treeViewStructural.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewStructural_AfterCheck);
            // 
            // treeViewMEP
            // 
            this.treeViewMEP.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeViewMEP.Location = new System.Drawing.Point(246, 21);
            this.treeViewMEP.Name = "treeViewMEP";
            this.treeViewMEP.Size = new System.Drawing.Size(270, 327);
            this.treeViewMEP.TabIndex = 1;
            this.treeViewMEP.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeViewMEP_AfterCheck);
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.treeView.Location = new System.Drawing.Point(7, 20);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(233, 327);
            this.treeView.TabIndex = 0;
            this.treeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterCheck);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(633, 376);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(78, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(717, 376);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(83, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FilterAssembliesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 405);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxIncludeCategories);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FilterAssembliesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Filter Assemblies";
            this.Load += new System.EventHandler(this.FilterAssembliesForm_Load);
            this.groupBoxIncludeCategories.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxIncludeCategories;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private CheckboxTreeView treeView;
        private CheckboxTreeView treeViewStructural;
        private CheckboxTreeView treeViewMEP;
    }
}