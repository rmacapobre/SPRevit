namespace Specpoint.Revit2026
{
    partial class AssemblyCodeEditingControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textBoxAssemblyCode = new System.Windows.Forms.TextBox();
            buttonChooseAssemblyCode = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // textBoxAssemblyCode
            // 
            textBoxAssemblyCode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxAssemblyCode.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            textBoxAssemblyCode.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            textBoxAssemblyCode.Location = new System.Drawing.Point(0, 1);
            textBoxAssemblyCode.Name = "textBoxAssemblyCode";
            textBoxAssemblyCode.Size = new System.Drawing.Size(83, 27);
            textBoxAssemblyCode.TabIndex = 0;
            // 
            // buttonChooseAssemblyCode
            // 
            buttonChooseAssemblyCode.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonChooseAssemblyCode.Image = PROPERTIES.Resources.ellipses;
            buttonChooseAssemblyCode.Location = new System.Drawing.Point(82, 0);
            buttonChooseAssemblyCode.Name = "buttonChooseAssemblyCode";
            buttonChooseAssemblyCode.Size = new System.Drawing.Size(26, 24);
            buttonChooseAssemblyCode.TabIndex = 1;
            buttonChooseAssemblyCode.TextAlign = System.Drawing.ContentAlignment.TopRight;
            buttonChooseAssemblyCode.Click += buttonChooseAssemblyCode_Click;
            // 
            // AssemblyCodeEditingControl
            // 
            BackColor = System.Drawing.SystemColors.ControlLightLight;
            Controls.Add(buttonChooseAssemblyCode);
            Controls.Add(textBoxAssemblyCode);
            Name = "AssemblyCodeEditingControl";
            Size = new System.Drawing.Size(108, 24);
            Leave += AssemblyCodeEditingControl_Leave;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxAssemblyCode;
        private System.Windows.Forms.Button buttonChooseAssemblyCode;
    }
}
