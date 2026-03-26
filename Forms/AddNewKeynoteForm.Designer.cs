namespace Specpoint.Revit2026
{
    partial class AddNewKeynoteForm
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
            this.groupBoxKey = new System.Windows.Forms.GroupBox();
            this.comboBoxNumber = new System.Windows.Forms.ComboBox();
            this.comboBoxLetter = new System.Windows.Forms.ComboBox();
            this.textBoxSection = new System.Windows.Forms.TextBox();
            this.groupBoxValue = new System.Windows.Forms.GroupBox();
            this.textBoxValue = new System.Windows.Forms.TextBox();
            this.checkBoxKeepOpen = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupBoxKey.SuspendLayout();
            this.groupBoxValue.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxKey
            // 
            this.groupBoxKey.Controls.Add(this.comboBoxNumber);
            this.groupBoxKey.Controls.Add(this.comboBoxLetter);
            this.groupBoxKey.Controls.Add(this.textBoxSection);
            this.groupBoxKey.Location = new System.Drawing.Point(12, 12);
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.Size = new System.Drawing.Size(250, 60);
            this.groupBoxKey.TabIndex = 0;
            this.groupBoxKey.TabStop = false;
            this.groupBoxKey.Text = "Key";
            // 
            // comboBoxNumber
            // 
            this.comboBoxNumber.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxNumber.FormattingEnabled = true;
            this.comboBoxNumber.Location = new System.Drawing.Point(189, 19);
            this.comboBoxNumber.Name = "comboBoxNumber";
            this.comboBoxNumber.Size = new System.Drawing.Size(50, 21);
            this.comboBoxNumber.TabIndex = 2;
            // 
            // comboBoxLetter
            // 
            this.comboBoxLetter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLetter.FormattingEnabled = true;
            this.comboBoxLetter.Location = new System.Drawing.Point(132, 19);
            this.comboBoxLetter.Name = "comboBoxLetter";
            this.comboBoxLetter.Size = new System.Drawing.Size(50, 21);
            this.comboBoxLetter.TabIndex = 1;
            this.comboBoxLetter.SelectedIndexChanged += new System.EventHandler(this.comboBoxLetter_SelectedIndexChanged);
            // 
            // textBoxSection
            // 
            this.textBoxSection.Enabled = false;
            this.textBoxSection.Location = new System.Drawing.Point(10, 19);
            this.textBoxSection.Name = "textBoxSection";
            this.textBoxSection.Size = new System.Drawing.Size(100, 20);
            this.textBoxSection.TabIndex = 0;
            // 
            // groupBoxValue
            // 
            this.groupBoxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxValue.Controls.Add(this.textBoxValue);
            this.groupBoxValue.Location = new System.Drawing.Point(268, 12);
            this.groupBoxValue.Name = "groupBoxValue";
            this.groupBoxValue.Size = new System.Drawing.Size(252, 60);
            this.groupBoxValue.TabIndex = 1;
            this.groupBoxValue.TabStop = false;
            this.groupBoxValue.Text = "Value";
            // 
            // textBoxValue
            // 
            this.textBoxValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxValue.Location = new System.Drawing.Point(7, 19);
            this.textBoxValue.MaxLength = 250;
            this.textBoxValue.Name = "textBoxValue";
            this.textBoxValue.Size = new System.Drawing.Size(231, 20);
            this.textBoxValue.TabIndex = 0;
            this.textBoxValue.TextChanged += new System.EventHandler(this.textBoxValue_TextChanged);
            this.textBoxValue.MouseHover += new System.EventHandler(this.textBoxValue_MouseHover);
            // 
            // checkBoxKeepOpen
            // 
            this.checkBoxKeepOpen.AutoSize = true;
            this.checkBoxKeepOpen.Location = new System.Drawing.Point(12, 79);
            this.checkBoxKeepOpen.Name = "checkBoxKeepOpen";
            this.checkBoxKeepOpen.Size = new System.Drawing.Size(120, 17);
            this.checkBoxKeepOpen.TabIndex = 2;
            this.checkBoxKeepOpen.Text = "Keep this form open";
            this.checkBoxKeepOpen.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(445, 78);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.Location = new System.Drawing.Point(364, 78);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "Add";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // AddNewKeynoteForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 108);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.checkBoxKeepOpen);
            this.Controls.Add(this.groupBoxValue);
            this.Controls.Add(this.groupBoxKey);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddNewKeynoteForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add New Keynote";
            this.Load += new System.EventHandler(this.AddNewKeynoteForm_Load);
            this.groupBoxKey.ResumeLayout(false);
            this.groupBoxKey.PerformLayout();
            this.groupBoxValue.ResumeLayout(false);
            this.groupBoxValue.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxKey;
        private System.Windows.Forms.ComboBox comboBoxNumber;
        private System.Windows.Forms.ComboBox comboBoxLetter;
        private System.Windows.Forms.TextBox textBoxSection;
        private System.Windows.Forms.GroupBox groupBoxValue;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.CheckBox checkBoxKeepOpen;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}