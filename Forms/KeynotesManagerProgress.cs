using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public partial class KeynotesManagerProgress : Form
    {
        public string Title { get; set; }
        public string View { get; set; }

        public string Filter { get; set; }
        public List<string> IncludedCategories { get; set; }

        public KeynotesManagerProgress()
        {
            InitializeComponent();
        }

        private async void KeynotesManagerProgress_Load(object sender, EventArgs e)
        {
            Text = Title;

            KeynotesManagerForm parent = this.Owner as KeynotesManagerForm;
            if (parent == null)
            {
                Close();
                return;
            }
            
            bool ret = await parent.GetKeynotes();
            if (ret == false)
            {
                Close();
                return;
            }

            string projectId = Globals.SpecpointProjectID;
            if (string.IsNullOrEmpty(projectId))
            {
                Close();
                return;
            }

            if (View == KeynotesView.Divisions &&
                Globals.KeynoteDivisions == null)
            {
                Globals.KeynoteDivisions = new Divisions();
                ret = await Globals.KeynoteDivisions.Init();
            }
            else if (View == KeynotesView.Assemblies)
            {
                // All categories
                if (Filter == Globals.AllCategoriesFilter)
                {
                    if (Globals.KeynoteAssemblies == null)
                    {
                        // Get assemblies for Specpoint project
                        Text = KeynotesProgressTitle.AllAssemblies;
                        Globals.KeynoteAssemblies = await parent.GetAllSpecpointAssemblies();
                    }
                }

                // Specific categories
                else 
                {
                    if (Globals.KeynoteAssembliesByCategory == null)
                    {
                        if (IncludedCategories != null)
                        {
                            // Create assemblies by category for the first time 
                            Globals.KeynoteAssembliesByCategory = new Dictionary<string, GetProjectElementNodes>();
                            ret = await GetAssembliesByCategories(parent);
                        }
                        else
                        {
                            // From Filter Assemblies 
                            Text = KeynotesProgressTitle.Assemblies;
                            Globals.KeynoteAssembliesByCategory = await parent.GetSpecpointAssembliesByCategory();
                        }
                    }

                    else 
                    {
                        // Assemblies by category already available
                        ret = await GetAssembliesByCategories(parent);
                    }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private async Task<bool> GetAssembliesByCategories(KeynotesManagerForm parent)
        {
            if (parent == null) return false;

            foreach (string c in IncludedCategories)
            {
                if (Globals.KeynoteAssembliesByCategory.ContainsKey(c)) continue;

                Text = string.Format("Loading {0} Assemblies", c);
                Globals.KeynoteAssembliesByCategory[c] =
                    await parent.GetSpecpointAssembliesByCategory(c);
            }

            return true;
        }

    }
}
