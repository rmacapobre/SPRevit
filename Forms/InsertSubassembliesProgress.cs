using System;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public partial class InsertSubassembliesProgress : Form
    {
        public ProgressBar Percent { get { return progressBar; } }

        public InsertSubassembliesProgress()
        {
            InitializeComponent();
        }

        private async void InsertSubassembliesProgress_Load(object sender, EventArgs e)
        {
            bool ret = false;
            ModelValidationForm parent = this.Owner as ModelValidationForm;
            if (parent != null)
            {
                parent.insertingAssemblies = true;

                if (parent.IsUpdate)
                {
                    ret = parent.UpdateSyncSubassemblies();
                }
                else
                {
                    // Get assemblies added to Specpoint project
                    ret = await parent.InsertSubassemblies();
                }
            }

            this.DialogResult = ret == true ?
                DialogResult.OK : DialogResult.Cancel;

            Close();
        }

        private void InsertSubassembliesProgress_FormClosing(object sender, FormClosingEventArgs e)
        {
            ModelValidationForm parent = this.Owner as ModelValidationForm;
            if (parent != null)
            {
                parent.insertingAssemblies = false;
            }
        }
    }
}
