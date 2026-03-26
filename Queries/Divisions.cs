using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class Divisions
    {
        public List<ProjectElementNode> Nodes { get; set; }
        public Dictionary<string, List<ProjectElementNode>> SectionList { get; set; }
        public Dictionary<string, List<ProjectElementNode>> ProductTypeList { get; set; }

        public Divisions()
        {
            Nodes = null;
            SectionList = new Dictionary<string, List<ProjectElementNode>>();
            ProductTypeList = new Dictionary<string, List<ProjectElementNode>>();
        }

        public async ValueTask<bool> Init()
        {
            string projectId = Globals.SpecpointProjectID;
            if (string.IsNullOrEmpty(projectId)) return false;

            // Query divisions for specified project
            string func = nameof(Init);
            Query queryDivs = new Query(func);
            var resultDivs = await queryDivs.getDivisions(projectId);

            Nodes = resultDivs.ListNodes;

            foreach (var node in Nodes)
            {
                func = nameof(Init);
                Query querySecs = new Query(func);
                var resultSecs = await querySecs.getSections(projectId, node.id);

                string div = node.text;
                SectionList[node.id] = resultSecs.ListNodes;

                foreach (ProjectElementNode section in SectionList[node.id])
                {
                    func = nameof (Init);
                    Query queryProdTypes = new Query(func);
                    var productTypes = await queryProdTypes.getProductTypes(projectId, section.id);

                    ProductTypeList[section.id] = productTypes.ListNodes;
                }
            }

            return true;
        }
    }
}
