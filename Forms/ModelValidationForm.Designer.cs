using Zuby.ADGV;

namespace Specpoint.Revit2026
{
    partial class ModelValidationForm
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
            buttonOK = new System.Windows.Forms.Button();
            buttonCancel = new System.Windows.Forms.Button();
            buttonSaveReportAs = new System.Windows.Forms.Button();
            buttonHelp = new System.Windows.Forms.Button();
            dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            grid = new AdvancedDataGridView();
            groupBoxInclude = new System.Windows.Forms.GroupBox();
            groupBoxRevitElements = new System.Windows.Forms.GroupBox();
            checkBoxAssignedRevitElements = new System.Windows.Forms.CheckBox();
            checkBoxUnassignedRevitElements = new System.Windows.Forms.CheckBox();
            groupBoxSpecpointFamilies = new System.Windows.Forms.GroupBox();
            checkBoxBoundElements = new System.Windows.Forms.CheckBox();
            checkBoxUnboundElements = new System.Windows.Forms.CheckBox();
            groupBoxAssemblyCodes = new System.Windows.Forms.GroupBox();
            checkBoxAssignedAssemblies = new System.Windows.Forms.CheckBox();
            checkBoxUnassignedAssemblies = new System.Windows.Forms.CheckBox();
            labelStatus = new System.Windows.Forms.Label();
            buttonExportAssemblyCodes = new System.Windows.Forms.Button();
            statusStrip = new System.Windows.Forms.StatusStrip();
            toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            labelVerified = new System.Windows.Forms.Label();
            labelCaution = new System.Windows.Forms.Label();
            labelNeedsAttention = new System.Windows.Forms.Label();
            labelVerifiedValue = new System.Windows.Forms.Label();
            labelCautionValue = new System.Windows.Forms.Label();
            labelNeedsAttentionValue = new System.Windows.Forms.Label();
            labelCircleVerified = new System.Windows.Forms.Label();
            labelCircleCaution = new System.Windows.Forms.Label();
            labelCircleNeeds = new System.Windows.Forms.Label();
            buttonReload = new System.Windows.Forms.Button();
            buttonAdd = new System.Windows.Forms.Button();
            labelUpdateSync = new System.Windows.Forms.Label();
            progressBarUpdateSync = new System.Windows.Forms.ProgressBar();
            buttonCancelUpdate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            groupBoxInclude.SuspendLayout();
            groupBoxRevitElements.SuspendLayout();
            groupBoxSpecpointFamilies.SuspendLayout();
            groupBoxAssemblyCodes.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // buttonOK
            // 
            buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonOK.Location = new System.Drawing.Point(1482, 841);
            buttonOK.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new System.Drawing.Size(101, 36);
            buttonOK.TabIndex = 1;
            buttonOK.Text = "OK";
            buttonOK.UseVisualStyleBackColor = true;
            buttonOK.Click += buttonOK_Click;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonCancel.Location = new System.Drawing.Point(1591, 841);
            buttonCancel.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new System.Drawing.Size(101, 36);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonSaveReportAs
            // 
            buttonSaveReportAs.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            buttonSaveReportAs.Location = new System.Drawing.Point(16, 841);
            buttonSaveReportAs.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonSaveReportAs.Name = "buttonSaveReportAs";
            buttonSaveReportAs.Size = new System.Drawing.Size(144, 36);
            buttonSaveReportAs.TabIndex = 3;
            buttonSaveReportAs.Text = "Save Report As...";
            buttonSaveReportAs.UseVisualStyleBackColor = true;
            buttonSaveReportAs.Click += buttonSaveReportAs_Click;
            // 
            // buttonHelp
            // 
            buttonHelp.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            buttonHelp.Location = new System.Drawing.Point(377, 841);
            buttonHelp.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonHelp.Name = "buttonHelp";
            buttonHelp.Size = new System.Drawing.Size(101, 36);
            buttonHelp.TabIndex = 4;
            buttonHelp.Text = "Help";
            buttonHelp.UseVisualStyleBackColor = true;
            buttonHelp.Click += buttonHelp_Click;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.HeaderText = "BIM Category";
            dataGridViewTextBoxColumn1.MinimumWidth = 6;
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.Width = 125;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Revit Family";
            dataGridViewTextBoxColumn2.MinimumWidth = 6;
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.Width = 125;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Revit Type";
            dataGridViewTextBoxColumn3.MinimumWidth = 6;
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.Width = 125;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.HeaderText = "Specpoint Family Number";
            dataGridViewTextBoxColumn4.MinimumWidth = 6;
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.Width = 125;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.HeaderText = "Specpoint Family";
            dataGridViewTextBoxColumn5.MinimumWidth = 6;
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            dataGridViewTextBoxColumn5.Width = 125;
            // 
            // grid
            // 
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            grid.BackgroundColor = System.Drawing.Color.White;
            grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.FilterAndSortEnabled = true;
            grid.FilterStringChangedInvokeBeforeDatasourceUpdate = true;
            grid.Location = new System.Drawing.Point(17, 120);
            grid.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            grid.MaxFilterButtonImageHeight = 23;
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.RightToLeft = System.Windows.Forms.RightToLeft.No;
            grid.RowHeadersWidth = 51;
            grid.Size = new System.Drawing.Size(1673, 599);
            grid.SortStringChangedInvokeBeforeDatasourceUpdate = true;
            grid.TabIndex = 6;
            grid.SortStringChanged += grid_SortStringChanged;
            grid.FilterStringChanged += grid_FilterStringChanged;
            grid.CellClick += grid_CellClick;
            grid.CellDoubleClick += grid_CellDoubleClick;
            grid.CellFormatting += grid_CellFormatting;
            grid.CellValueChanged += grid_CellValueChanged;
            grid.ColumnDividerDoubleClick += grid_ColumnDividerDoubleClick;
            grid.ColumnDividerWidthChanged += grid_ColumnDividerWidthChanged;
            grid.ColumnHeaderMouseClick += grid_ColumnHeaderMouseClick;
            grid.ColumnHeaderMouseDoubleClick += grid_ColumnHeaderMouseDoubleClick;
            grid.ColumnWidthChanged += grid_ColumnWidthChanged;
            grid.DataBindingComplete += grid_DataBindingComplete;
            grid.RowPostPaint += grid_RowPostPaint;
            grid.Scroll += grid_Scroll;
            grid.SelectionChanged += grid_SelectionChanged;
            grid.Paint += grid_Paint;
            grid.Resize += grid_Resize;
            // 
            // groupBoxInclude
            // 
            groupBoxInclude.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxInclude.Controls.Add(groupBoxRevitElements);
            groupBoxInclude.Controls.Add(groupBoxSpecpointFamilies);
            groupBoxInclude.Controls.Add(groupBoxAssemblyCodes);
            groupBoxInclude.Location = new System.Drawing.Point(17, 727);
            groupBoxInclude.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            groupBoxInclude.Name = "groupBoxInclude";
            groupBoxInclude.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            groupBoxInclude.Size = new System.Drawing.Size(1673, 107);
            groupBoxInclude.TabIndex = 7;
            groupBoxInclude.TabStop = false;
            groupBoxInclude.Text = "Coordination Filters";
            // 
            // groupBoxRevitElements
            // 
            groupBoxRevitElements.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBoxRevitElements.Controls.Add(checkBoxAssignedRevitElements);
            groupBoxRevitElements.Controls.Add(checkBoxUnassignedRevitElements);
            groupBoxRevitElements.Location = new System.Drawing.Point(19, 27);
            groupBoxRevitElements.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxRevitElements.Name = "groupBoxRevitElements";
            groupBoxRevitElements.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxRevitElements.Size = new System.Drawing.Size(312, 72);
            groupBoxRevitElements.TabIndex = 5;
            groupBoxRevitElements.TabStop = false;
            groupBoxRevitElements.Text = "Revit Elements";
            // 
            // checkBoxAssignedRevitElements
            // 
            checkBoxAssignedRevitElements.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxAssignedRevitElements.AutoSize = true;
            checkBoxAssignedRevitElements.Checked = true;
            checkBoxAssignedRevitElements.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAssignedRevitElements.Location = new System.Drawing.Point(20, 33);
            checkBoxAssignedRevitElements.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxAssignedRevitElements.Name = "checkBoxAssignedRevitElements";
            checkBoxAssignedRevitElements.Size = new System.Drawing.Size(91, 24);
            checkBoxAssignedRevitElements.TabIndex = 0;
            checkBoxAssignedRevitElements.Text = "Assigned";
            checkBoxAssignedRevitElements.UseVisualStyleBackColor = true;
            checkBoxAssignedRevitElements.CheckedChanged += checkBoxAssignedRevitElements_CheckedChanged;
            // 
            // checkBoxUnassignedRevitElements
            // 
            checkBoxUnassignedRevitElements.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxUnassignedRevitElements.AutoSize = true;
            checkBoxUnassignedRevitElements.Checked = true;
            checkBoxUnassignedRevitElements.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxUnassignedRevitElements.Location = new System.Drawing.Point(114, 33);
            checkBoxUnassignedRevitElements.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxUnassignedRevitElements.Name = "checkBoxUnassignedRevitElements";
            checkBoxUnassignedRevitElements.Size = new System.Drawing.Size(107, 24);
            checkBoxUnassignedRevitElements.TabIndex = 1;
            checkBoxUnassignedRevitElements.Text = "Unassigned";
            checkBoxUnassignedRevitElements.UseVisualStyleBackColor = true;
            checkBoxUnassignedRevitElements.CheckedChanged += checkBoxUnassignedRevitElements_CheckedChanged;
            // 
            // groupBoxSpecpointFamilies
            // 
            groupBoxSpecpointFamilies.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            groupBoxSpecpointFamilies.Controls.Add(checkBoxBoundElements);
            groupBoxSpecpointFamilies.Controls.Add(checkBoxUnboundElements);
            groupBoxSpecpointFamilies.Location = new System.Drawing.Point(655, 27);
            groupBoxSpecpointFamilies.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxSpecpointFamilies.Name = "groupBoxSpecpointFamilies";
            groupBoxSpecpointFamilies.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxSpecpointFamilies.Size = new System.Drawing.Size(1010, 72);
            groupBoxSpecpointFamilies.TabIndex = 5;
            groupBoxSpecpointFamilies.TabStop = false;
            groupBoxSpecpointFamilies.Text = "Specpoint Families";
            // 
            // checkBoxBoundElements
            // 
            checkBoxBoundElements.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxBoundElements.AutoSize = true;
            checkBoxBoundElements.Checked = true;
            checkBoxBoundElements.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxBoundElements.Location = new System.Drawing.Point(25, 33);
            checkBoxBoundElements.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxBoundElements.Name = "checkBoxBoundElements";
            checkBoxBoundElements.Size = new System.Drawing.Size(76, 24);
            checkBoxBoundElements.TabIndex = 2;
            checkBoxBoundElements.Text = "Added";
            checkBoxBoundElements.UseVisualStyleBackColor = true;
            checkBoxBoundElements.CheckedChanged += checkBoxBoundElements_CheckedChanged;
            // 
            // checkBoxUnboundElements
            // 
            checkBoxUnboundElements.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxUnboundElements.AutoSize = true;
            checkBoxUnboundElements.Checked = true;
            checkBoxUnboundElements.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxUnboundElements.Location = new System.Drawing.Point(104, 33);
            checkBoxUnboundElements.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxUnboundElements.Name = "checkBoxUnboundElements";
            checkBoxUnboundElements.Size = new System.Drawing.Size(92, 24);
            checkBoxUnboundElements.TabIndex = 3;
            checkBoxUnboundElements.Text = "Unadded";
            checkBoxUnboundElements.UseVisualStyleBackColor = true;
            checkBoxUnboundElements.CheckedChanged += checkBoxUnboundElements_CheckedChanged;
            // 
            // groupBoxAssemblyCodes
            // 
            groupBoxAssemblyCodes.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            groupBoxAssemblyCodes.Controls.Add(checkBoxAssignedAssemblies);
            groupBoxAssemblyCodes.Controls.Add(checkBoxUnassignedAssemblies);
            groupBoxAssemblyCodes.Location = new System.Drawing.Point(337, 27);
            groupBoxAssemblyCodes.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxAssemblyCodes.Name = "groupBoxAssemblyCodes";
            groupBoxAssemblyCodes.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            groupBoxAssemblyCodes.Size = new System.Drawing.Size(312, 72);
            groupBoxAssemblyCodes.TabIndex = 4;
            groupBoxAssemblyCodes.TabStop = false;
            groupBoxAssemblyCodes.Text = "Assembly Codes";
            // 
            // checkBoxAssignedAssemblies
            // 
            checkBoxAssignedAssemblies.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxAssignedAssemblies.AutoSize = true;
            checkBoxAssignedAssemblies.Checked = true;
            checkBoxAssignedAssemblies.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxAssignedAssemblies.Location = new System.Drawing.Point(27, 33);
            checkBoxAssignedAssemblies.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxAssignedAssemblies.Name = "checkBoxAssignedAssemblies";
            checkBoxAssignedAssemblies.Size = new System.Drawing.Size(91, 24);
            checkBoxAssignedAssemblies.TabIndex = 0;
            checkBoxAssignedAssemblies.Text = "Assigned";
            checkBoxAssignedAssemblies.UseVisualStyleBackColor = true;
            checkBoxAssignedAssemblies.CheckedChanged += checkBoxAssignedAssemblies_CheckedChanged;
            // 
            // checkBoxUnassignedAssemblies
            // 
            checkBoxUnassignedAssemblies.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            checkBoxUnassignedAssemblies.AutoSize = true;
            checkBoxUnassignedAssemblies.Checked = true;
            checkBoxUnassignedAssemblies.CheckState = System.Windows.Forms.CheckState.Checked;
            checkBoxUnassignedAssemblies.Location = new System.Drawing.Point(121, 33);
            checkBoxUnassignedAssemblies.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            checkBoxUnassignedAssemblies.Name = "checkBoxUnassignedAssemblies";
            checkBoxUnassignedAssemblies.Size = new System.Drawing.Size(107, 24);
            checkBoxUnassignedAssemblies.TabIndex = 1;
            checkBoxUnassignedAssemblies.Text = "Unassigned";
            checkBoxUnassignedAssemblies.UseVisualStyleBackColor = true;
            checkBoxUnassignedAssemblies.CheckedChanged += checkBoxUnassignedAssemblies_CheckedChanged;
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new System.Drawing.Point(275, 636);
            labelStatus.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new System.Drawing.Size(0, 20);
            labelStatus.TabIndex = 8;
            // 
            // buttonExportAssemblyCodes
            // 
            buttonExportAssemblyCodes.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            buttonExportAssemblyCodes.Location = new System.Drawing.Point(168, 841);
            buttonExportAssemblyCodes.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonExportAssemblyCodes.Name = "buttonExportAssemblyCodes";
            buttonExportAssemblyCodes.Size = new System.Drawing.Size(201, 36);
            buttonExportAssemblyCodes.TabIndex = 9;
            buttonExportAssemblyCodes.Text = "Export Assembly Codes";
            buttonExportAssemblyCodes.UseVisualStyleBackColor = true;
            buttonExportAssemblyCodes.Click += buttonExportAssemblyCodes_Click;
            // 
            // statusStrip
            // 
            statusStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripStatusLabel });
            statusStrip.Location = new System.Drawing.Point(0, 903);
            statusStrip.Name = "statusStrip";
            statusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 18, 0);
            statusStrip.Size = new System.Drawing.Size(1710, 26);
            statusStrip.TabIndex = 10;
            statusStrip.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            toolStripStatusLabel.Name = "toolStripStatusLabel";
            toolStripStatusLabel.Size = new System.Drawing.Size(72, 20);
            toolStripStatusLabel.Text = "Loading...";
            // 
            // labelVerified
            // 
            labelVerified.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelVerified.Location = new System.Drawing.Point(63, 36);
            labelVerified.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelVerified.Name = "labelVerified";
            labelVerified.Size = new System.Drawing.Size(74, 27);
            labelVerified.TabIndex = 11;
            labelVerified.Text = "Verified";
            labelVerified.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCaution
            // 
            labelCaution.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelCaution.Location = new System.Drawing.Point(199, 36);
            labelCaution.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCaution.Name = "labelCaution";
            labelCaution.Size = new System.Drawing.Size(74, 27);
            labelCaution.TabIndex = 12;
            labelCaution.Text = "Caution";
            labelCaution.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelNeedsAttention
            // 
            labelNeedsAttention.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            labelNeedsAttention.Location = new System.Drawing.Point(334, 36);
            labelNeedsAttention.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelNeedsAttention.Name = "labelNeedsAttention";
            labelNeedsAttention.Size = new System.Drawing.Size(145, 27);
            labelNeedsAttention.TabIndex = 13;
            labelNeedsAttention.Text = "Needs Attention";
            labelNeedsAttention.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelVerifiedValue
            // 
            labelVerifiedValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelVerifiedValue.ForeColor = System.Drawing.Color.Green;
            labelVerifiedValue.Location = new System.Drawing.Point(63, 61);
            labelVerifiedValue.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelVerifiedValue.Name = "labelVerifiedValue";
            labelVerifiedValue.Size = new System.Drawing.Size(71, 36);
            labelVerifiedValue.TabIndex = 14;
            labelVerifiedValue.Text = "100";
            labelVerifiedValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCautionValue
            // 
            labelCautionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelCautionValue.ForeColor = System.Drawing.Color.Gold;
            labelCautionValue.Location = new System.Drawing.Point(199, 61);
            labelCautionValue.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCautionValue.Name = "labelCautionValue";
            labelCautionValue.Size = new System.Drawing.Size(71, 36);
            labelCautionValue.TabIndex = 15;
            labelCautionValue.Text = "100";
            labelCautionValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelNeedsAttentionValue
            // 
            labelNeedsAttentionValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            labelNeedsAttentionValue.ForeColor = System.Drawing.Color.Red;
            labelNeedsAttentionValue.Location = new System.Drawing.Point(334, 61);
            labelNeedsAttentionValue.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelNeedsAttentionValue.Name = "labelNeedsAttentionValue";
            labelNeedsAttentionValue.Size = new System.Drawing.Size(144, 36);
            labelNeedsAttentionValue.TabIndex = 16;
            labelNeedsAttentionValue.Text = "100";
            labelNeedsAttentionValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelCircleVerified
            // 
            labelCircleVerified.AutoSize = true;
            labelCircleVerified.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            labelCircleVerified.Location = new System.Drawing.Point(41, 41);
            labelCircleVerified.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCircleVerified.Name = "labelCircleVerified";
            labelCircleVerified.Size = new System.Drawing.Size(17, 20);
            labelCircleVerified.TabIndex = 17;
            labelCircleVerified.Text = "0";
            labelCircleVerified.Paint += labelCircleVerified_Paint;
            // 
            // labelCircleCaution
            // 
            labelCircleCaution.AutoSize = true;
            labelCircleCaution.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            labelCircleCaution.Location = new System.Drawing.Point(177, 41);
            labelCircleCaution.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCircleCaution.Name = "labelCircleCaution";
            labelCircleCaution.Size = new System.Drawing.Size(17, 20);
            labelCircleCaution.TabIndex = 18;
            labelCircleCaution.Text = "0";
            labelCircleCaution.Paint += labelCircleCaution_Paint;
            // 
            // labelCircleNeeds
            // 
            labelCircleNeeds.AutoSize = true;
            labelCircleNeeds.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            labelCircleNeeds.Location = new System.Drawing.Point(309, 41);
            labelCircleNeeds.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            labelCircleNeeds.Name = "labelCircleNeeds";
            labelCircleNeeds.Size = new System.Drawing.Size(17, 20);
            labelCircleNeeds.TabIndex = 19;
            labelCircleNeeds.Text = "0";
            labelCircleNeeds.Paint += labelCircleNeeds_Paint;
            // 
            // buttonReload
            // 
            buttonReload.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonReload.Location = new System.Drawing.Point(1360, 841);
            buttonReload.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonReload.Name = "buttonReload";
            buttonReload.Size = new System.Drawing.Size(114, 36);
            buttonReload.TabIndex = 20;
            buttonReload.Text = "Refresh Data";
            buttonReload.UseVisualStyleBackColor = true;
            buttonReload.Click += buttonReload_Click;
            // 
            // buttonAdd
            // 
            buttonAdd.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            buttonAdd.Location = new System.Drawing.Point(1087, 841);
            buttonAdd.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new System.Drawing.Size(265, 36);
            buttonAdd.TabIndex = 21;
            buttonAdd.Text = "Add Assembly to Specpoint Project";
            buttonAdd.UseVisualStyleBackColor = true;
            buttonAdd.Click += buttonAdd_Click;
            // 
            // labelUpdateSync
            // 
            labelUpdateSync.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            labelUpdateSync.AutoSize = true;
            labelUpdateSync.Location = new System.Drawing.Point(1324, 45);
            labelUpdateSync.Name = "labelUpdateSync";
            labelUpdateSync.Size = new System.Drawing.Size(142, 20);
            labelUpdateSync.TabIndex = 22;
            labelUpdateSync.Text = "Updating Specpoint";
            // 
            // progressBarUpdateSync
            // 
            progressBarUpdateSync.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            progressBarUpdateSync.Location = new System.Drawing.Point(1324, 68);
            progressBarUpdateSync.Name = "progressBarUpdateSync";
            progressBarUpdateSync.Size = new System.Drawing.Size(266, 29);
            progressBarUpdateSync.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBarUpdateSync.TabIndex = 23;
            // 
            // buttonCancelUpdate
            // 
            buttonCancelUpdate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            buttonCancelUpdate.Location = new System.Drawing.Point(1596, 68);
            buttonCancelUpdate.Name = "buttonCancelUpdate";
            buttonCancelUpdate.Size = new System.Drawing.Size(94, 29);
            buttonCancelUpdate.TabIndex = 24;
            buttonCancelUpdate.Text = "Cancel";
            buttonCancelUpdate.UseVisualStyleBackColor = true;
            buttonCancelUpdate.Click += buttonCancelUpdate_Click;
            // 
            // ModelValidationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1710, 929);
            Controls.Add(buttonCancelUpdate);
            Controls.Add(progressBarUpdateSync);
            Controls.Add(labelUpdateSync);
            Controls.Add(buttonAdd);
            Controls.Add(buttonReload);
            Controls.Add(labelCircleNeeds);
            Controls.Add(labelCircleCaution);
            Controls.Add(labelCircleVerified);
            Controls.Add(labelNeedsAttentionValue);
            Controls.Add(labelCautionValue);
            Controls.Add(labelVerifiedValue);
            Controls.Add(labelNeedsAttention);
            Controls.Add(labelCaution);
            Controls.Add(labelVerified);
            Controls.Add(statusStrip);
            Controls.Add(buttonExportAssemblyCodes);
            Controls.Add(labelStatus);
            Controls.Add(groupBoxInclude);
            Controls.Add(grid);
            Controls.Add(buttonHelp);
            Controls.Add(buttonSaveReportAs);
            Controls.Add(buttonCancel);
            Controls.Add(buttonOK);
            Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            MinimumSize = new System.Drawing.Size(1140, 518);
            Name = "ModelValidationForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Specpoint Model Validation";
            FormClosing += ModelValidationForm_FormClosing;
            FormClosed += ModelValidationForm_FormClosed;
            Load += ModelValidationForm_Load;
            Paint += ModelValidationForm_Paint;
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            groupBoxInclude.ResumeLayout(false);
            groupBoxRevitElements.ResumeLayout(false);
            groupBoxRevitElements.PerformLayout();
            groupBoxSpecpointFamilies.ResumeLayout(false);
            groupBoxSpecpointFamilies.PerformLayout();
            groupBoxAssemblyCodes.ResumeLayout(false);
            groupBoxAssemblyCodes.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSaveReportAs;
        private System.Windows.Forms.Button buttonHelp;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private AdvancedDataGridView grid;
        private System.Windows.Forms.GroupBox groupBoxInclude;
        private System.Windows.Forms.CheckBox checkBoxUnboundElements;
        private System.Windows.Forms.CheckBox checkBoxBoundElements;
        private System.Windows.Forms.CheckBox checkBoxUnassignedAssemblies;
        private System.Windows.Forms.CheckBox checkBoxAssignedAssemblies;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonExportAssemblyCodes;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label labelVerified;
        private System.Windows.Forms.Label labelCaution;
        private System.Windows.Forms.Label labelNeedsAttention;
        private System.Windows.Forms.Label labelVerifiedValue;
        private System.Windows.Forms.Label labelCautionValue;
        private System.Windows.Forms.Label labelNeedsAttentionValue;
        private System.Windows.Forms.Label labelCircleVerified;
        private System.Windows.Forms.Label labelCircleCaution;
        private System.Windows.Forms.Label labelCircleNeeds;
        private System.Windows.Forms.Button buttonReload;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.GroupBox groupBoxAssemblyCodes;
        private System.Windows.Forms.GroupBox groupBoxSpecpointFamilies;
        private System.Windows.Forms.Label labelUpdateSync;
        private System.Windows.Forms.ProgressBar progressBarUpdateSync;
        private System.Windows.Forms.GroupBox groupBoxRevitElements;
        private System.Windows.Forms.CheckBox checkBoxAssignedRevitElements;
        private System.Windows.Forms.CheckBox checkBoxUnassignedRevitElements;
        private System.Windows.Forms.Button buttonCancelUpdate;
    }
}