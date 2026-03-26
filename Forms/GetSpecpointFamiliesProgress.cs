using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public partial class GetSpecpointFamiliesProgress : Form
    {
        public ProgressBar Percent { get { return progressBar; } }
        public Dictionary<string, Assembly> specpointFamilies;
        
        public GetSpecpointFamiliesProgress()
        {
            InitializeComponent();
        }

        private void GetSpecpointFamiliesProgress_Load(object sender, EventArgs e)
        {
            ModelValidationForm parent = this.Owner as ModelValidationForm;
            if (parent != null)
            {
                specpointFamilies = parent.GetSpecpointFamilies(this);
            }

            DialogResult = DialogResult.OK;

            Close();
        }
    }
}
