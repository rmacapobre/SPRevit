using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class SpecpointCategories : SortedDictionary<string, ProductFamilyCategory>
    {
        public SortedDictionary<string, ProductFamilyCategory> noMatchInRevit;

        public SpecpointCategories()
        {
            noMatchInRevit = null;
        }

        private async Task<Firm> GetSystemFirm()
        {
            string func = nameof(GetSystemFirm);
            Query query = new Query(func);
            var result = await query.firm(Globals.SystemFirmID);
            if (result == null) return null;

            return result.firm;
        }

        public async Task<bool> Init()
        {
            // Get list of Specpoint project categories
            if (Globals.SystemFirm == null)
            {
                Globals.SystemFirm = await GetSystemFirm();

                if (Globals.SystemFirm == null) return false;
            }

            foreach (var pd in Globals.SystemFirm.professionalDisciplines)
            {
                if (pd.name != "All Disciplines") continue;

                foreach (var c in pd.categories)
                {
                    if (c.revitCategory == null)
                    {
                        string msg = String.Format("{0} has null revitCategory", c.name);
                        Globals.Log.Write(String.Format("{0} has null revitCategory", c.name));
                        continue;
                    }
                    this[c.name] = c;
                }
            }

            return true;
        }

        public void GetNoMatchInRevit()
        {
            if (Globals.revitCategories == null) return;

            noMatchInRevit = new SortedDictionary<string, ProductFamilyCategory>();
            foreach (var c in this)
            {
                string name = c.Value.name;
                string id = c.Value.revitCategory;

                if (!Globals.revitCategories.ContainsKey(name))
                {
                    noMatchInRevit[name] = c.Value;
                }
            }
        }
    }
}
