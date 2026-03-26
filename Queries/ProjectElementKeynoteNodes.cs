using System.Collections.Generic;

namespace Specpoint.Revit2026
{
    public class GetProjectElementKeynotes
    {
        public ProjectElementKeynoteNodeResults getProjectElementKeynotes;

        // Keynotes by baseElementId
        public Dictionary<string, ProjectElementKeynoteNode> familyKeynotes;
        public Dictionary<string, ProjectElementKeynoteNode> productTypeKeynotes;

        public GetProjectElementKeynotes()
        {
            this.getProjectElementKeynotes = new ProjectElementKeynoteNodeResults();

            familyKeynotes = new Dictionary<string, ProjectElementKeynoteNode>();
            productTypeKeynotes = new Dictionary<string, ProjectElementKeynoteNode> ();
        }

        public void Init()
        {
            if (getProjectElementKeynotes == null) return;
            if (getProjectElementKeynotes.projectElementKeynoteNodes == null) return;
            if (getProjectElementKeynotes.projectElementKeynoteNodes.Count == 0) return;

            foreach (var node in getProjectElementKeynotes.projectElementKeynoteNodes)
            {
                if (node.projectElementType == ProjectElementType.PRODUCTFAMILY)
                {
                    familyKeynotes[node.baseElementId] = node;
                }
                else if (node.projectElementType == ProjectElementType.PRODUCTTYPE)
                {
                    productTypeKeynotes[node.baseElementId] = node;
                }
            }
        }
    }

    public class ProjectElementKeynoteNodeResults
    {
        public List<ProjectElementKeynoteNode> projectElementKeynoteNodes;
        public int totalCount;

        public ProjectElementKeynoteNodeResults()
        {
            this.projectElementKeynoteNodes = new List<ProjectElementKeynoteNode>();
            this.totalCount = 0;
        }
    }
}
