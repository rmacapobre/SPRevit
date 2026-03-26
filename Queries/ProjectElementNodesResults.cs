using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class ProjectElementNodeResults
    {
        public List<ProjectElementNode> projectElementNodes;
        public int totalCount;

        public ProjectElementNodeResults()
        {
            this.projectElementNodes = new List<ProjectElementNode>();
            this.totalCount = 0;
        }

        public void Add(List<ProjectElementNode> value)
        {
            foreach (var node in value)
            {
                this.projectElementNodes.Add(node);
            }
            totalCount = projectElementNodes.Count;
        }
    }

    public class GetProjectElementNodes
    {
        public ProjectElementNodeResults getProjectElementNodes;

        public List<ProjectElementNode> ListNodes
        {
            get
            {
                return getProjectElementNodes.projectElementNodes;
            }
        }

        public GetProjectElementNodes()
        {
            this.getProjectElementNodes = new ProjectElementNodeResults();
        }

        public GetProjectElementNodes(List<ProjectElementNode> projectElementNodes) : this()
        {
            this.getProjectElementNodes.projectElementNodes.Clear();
            foreach (var node in projectElementNodes)
            {
                this.getProjectElementNodes.projectElementNodes.Add(node);
            }
        }

        // Add a list of nodes to the list
        public void Add(List<ProjectElementNode> value)
        {
            this.getProjectElementNodes.Add(value);
        }
    }
}
