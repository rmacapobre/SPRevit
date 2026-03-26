using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class NodesByMultipleCategory
    {
        private string projectId;
        private ProjectElementNodeToCategoryResults Results;

        public List<CategoryToProjectElementNode> Categories
        {
            get
            {
                if (Results == null) return null;
                if (Results.getProjectElementNodesByMultipleCategory == null) return null;
                return Results.getProjectElementNodesByMultipleCategory.categoryToProjectElementNodes;
            }
        }

        public NodesByMultipleCategory()
        {
            projectId = "";
            Results = null;
        }

        public NodesByMultipleCategory(string projectId) : this()
        {
            this.projectId = projectId;
        }

        public async Task<bool> Init()
        {
            // Get all nodes by category
            string func = nameof(Init);
            Query query = new Query(func);
            var input = new GetProjectElementNodesByCategoryIdsInput(projectId);

            if (Globals.includedCategories != null)
            {
                foreach (var c in Globals.includedCategories)
                {
                    if (!Globals.specpointCategories.ContainsKey(c)) continue;

                    input.categoryIds.Add(Globals.specpointCategories[c].id);
                }
            }
            Results = await query.getProjectElementNodesByMultipleCategory(input);

            return Results != null;

        }

        // Get category of a given node
        public string GetCategory(string nodeId)
        {
            if (Results == null) return string.Empty;

            GetProjectElementNodesByMultipleCategory g = Results.getProjectElementNodesByMultipleCategory;
            foreach (CategoryToProjectElementNode c in g.categoryToProjectElementNodes)
            {
                foreach (ProjectElementNode n in c.projectElementNodes)
                {
                    if (n.id == nodeId)
                    {
                        return c.categoryName;
                    }
                }
            }

            return string.Empty;

        }
    }

    public class RevitSpecpointCategoryMap : SortedDictionary<string, string>
    {
        public void Init()
        {
            if (Globals.revitCategories == null) return;
            if (Globals.specpointCategories == null) return;

            List<string> noMatchInSpecpoint = new List<string>();
            foreach (var rc in Globals.revitCategories)
            {
                string revitCategoryName = rc.Value.Name;
                string revitCategoryId = rc.Value.Id.ToString();

                if (Globals.specpointCategories.ContainsKey(revitCategoryName))
                {
                    string specpointCategoryId = Globals.specpointCategories[revitCategoryName].revitCategory;
                    if (revitCategoryId == specpointCategoryId)
                    {
                        this[revitCategoryId] = Globals.specpointCategories[revitCategoryName].id;
                    }
                }
                else
                {
                    noMatchInSpecpoint.Add($"{revitCategoryName} = {revitCategoryId}");
                }
            }

            // Get Specpoint categories that don't exist in Revit
            Globals.specpointCategories.GetNoMatchInRevit();

            Globals.Log.Write("=== Specpoint categories that don't exist in Revit ===");
            foreach (var sc in Globals.specpointCategories.noMatchInRevit)
            {
                string spCategoryName = sc.Value.name;
                string spCategoryId = sc.Value.revitCategory;

                Globals.Log.Write ($"{spCategoryName} = {spCategoryId}");
                this[spCategoryId] = Globals.specpointCategories[spCategoryName].id;
            }

            Globals.Log.Write("=== Revit categories that don't exist in Specpoint ===");
            foreach (var c in noMatchInSpecpoint)
            {
                Globals.Log.Write(c);
            }
        }
    }

    public class RevitCategories : SortedDictionary<string, Category>
    {
        public RevitCategories(Document doc)
        {
            if (doc == null) return;

            foreach (Category c in doc.Settings.Categories)
            {
                if (c.IsTagCategory) continue;
                if (c.CategoryType != CategoryType.Model) continue;
                if (c.IsVisibleInUI == false) continue;
                if (c.Parent != null) continue;

                this[c.Name] = c;
            }
        }

        public bool IsMEP(Category c)
        {
            List<string> meps = new List<string>
            {
                "Air Terminals",
                "Cable Trays",
                "Communication Devices",
                "Conduits",
                "Data Devices",
                "Duct Accessories",
                "Duct Fittings",
                "Ducts",
                "Electrical Equipment",
                "Electrical Fixtures",
                "File Alarm Devices",
                "Flex Ducts",
                "Flex Pipes",
                "Lighting Devices",
                "Lighting Fixtures",
                "Mechanical Equipment",
                "Nurse Call Devices",
                "Pipe Accessories",
                "Pipe Fittings",
                "Pipes",
                "Plumbing Fixtures",
                "Security Devices",
                "Sprinkles",
                "Telephone Devices",
                "Wire"
            };

            return meps.Contains(c.Name);
        }

        public bool IsStructural(Category c)
        {
            return c.Name.StartsWith("Structural");
        }

        public bool IsArchitecture(Category c)
        {
            return IsMEP(c) == false && IsStructural(c) == false;
        }
    }
}
