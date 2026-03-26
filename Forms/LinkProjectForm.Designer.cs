namespace Specpoint.Revit2026
{
    partial class LinkProjectForm
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
            this.textBoxSpecpointProject = new System.Windows.Forms.TextBox();
            this.buttonSet = new System.Windows.Forms.Button();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.labelUser = new System.Windows.Forms.Label();
            this.labelUserValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxSpecpointProject
            // 
            this.textBoxSpecpointProject.BackColor = System.Drawing.Color.LightGray;
            this.textBoxSpecpointProject.Enabled = false;
            this.textBoxSpecpointProject.Location = new System.Drawing.Point(15, 32);
            this.textBoxSpecpointProject.Name = "textBoxSpecpointProject";
            this.textBoxSpecpointProject.Size = new System.Drawing.Size(350, 20);
            this.textBoxSpecpointProject.TabIndex = 0;
            this.textBoxSpecpointProject.Text = "Specpoint Project Not Selected";
            this.textBoxSpecpointProject.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // buttonSet
            // 
            this.buttonSet.Location = new System.Drawing.Point(371, 29);
            this.buttonSet.Name = "buttonSet";
            this.buttonSet.Size = new System.Drawing.Size(75, 23);
            this.buttonSet.TabIndex = 1;
            this.buttonSet.Text = "Set...";
            this.buttonSet.UseVisualStyleBackColor = true;
            this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Location = new System.Drawing.Point(15, 58);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(433, 23);
            this.buttonRemove.TabIndex = 2;
            this.buttonRemove.Text = "Remove Linked Project";
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(292, 87);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 3;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(373, 87);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelUser
            // 
            this.labelUser.AutoSize = true;
            this.labelUser.Location = new System.Drawing.Point(12, 9);
            this.labelUser.Name = "labelUser";
            this.labelUser.Size = new System.Drawing.Size(32, 13);
            this.labelUser.TabIndex = 5;
            this.labelUser.Text = "User:";
            // 
            // labelUserValue
            // 
            this.labelUserValue.AutoSize = true;
            this.labelUserValue.Location = new System.Drawing.Point(50, 9);
            this.labelUserValue.Name = "labelUserValue";
            this.labelUserValue.Size = new System.Drawing.Size(11, 13);
            this.labelUserValue.TabIndex = 6;
            this.labelUserValue.Text = "*";
            // 
            // LinkProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 117);
            this.Controls.Add(this.labelUserValue);
            this.Controls.Add(this.labelUser);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonRemove);
            this.Controls.Add(this.buttonSet);
            this.Controls.Add(this.textBoxSpecpointProject);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LinkProjectForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Link Specpoint Project";
            this.Load += new System.EventHandler(this.LinkProjectForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSpecpointProject;
        private System.Windows.Forms.Button buttonSet;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label labelUser;
        private System.Windows.Forms.Label labelUserValue;
    }
}