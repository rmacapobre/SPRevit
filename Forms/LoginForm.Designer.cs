namespace Specpoint.Revit2026
{
    partial class LoginForm
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
            webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webView2).BeginInit();
            SuspendLayout();
            // 
            // webView2
            // 
            webView2.AllowExternalDrop = true;
            webView2.CreationProperties = null;
            webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            webView2.Dock = System.Windows.Forms.DockStyle.Fill;
            webView2.Location = new System.Drawing.Point(0, 0);
            webView2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            webView2.Name = "webView2";
            webView2.Size = new System.Drawing.Size(701, 763);
            webView2.TabIndex = 0;
            webView2.ZoomFactor = 1D;
            webView2.NavigationStarting += webView2_NavigationStarting;
            webView2.NavigationCompleted += webView2_NavigationCompleted;
            webView2.ContentLoading += webView2_ContentLoading;
            // 
            // LoginForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(701, 763);
            Controls.Add(webView2);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimizeBox = false;
            Name = "LoginForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Log In";
            FormClosing += LoginForm_FormClosing;
            Load += Login_Load;
            ((System.ComponentModel.ISupportInitialize)webView2).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
    }
}