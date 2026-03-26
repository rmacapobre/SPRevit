using System;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public partial class GetLastSubassembliesProgress : Form
    {
        public ProgressBar Percent { get { return progressBar; } }
  
        public GetLastSubassembliesProgress()
        {
            InitializeComponent();
        }

        private async void GetLastSubassembliesProgress_Load(object sender, EventArgs e)
        {
            bool ret = false;
            ModelValidationForm parent = this.Owner as ModelValidationForm;
            if (parent != null)
            {
                parent.lastUpdated = await parent.GetLastUpdated();

                if (parent.lastUpdated == null)
                {
                    this.DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }

                // Get assemblies added to Specpoint project
                ret = await parent.GetLastSubAssemblies(this);
                if (ret == true)
                {
                    // Get assemblies NOT added to Specpoint project
                    ret = await parent.GetOtherAssemblies(this);
                }
            }

            this.DialogResult = ret == true ?
                DialogResult.OK : DialogResult.Cancel;

            Close();
        }

        private void GetLastSubassembliesProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            ModelValidationForm parent = this.Owner as ModelValidationForm;
            if (parent != null)
            {
                parent.getLastAssemblies = false;
                parent.getOtherAssemblies = false;
            }
        }
    }
}
