using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public enum ProjectElementType
    {
        ASSEMBLY,
        PRODUCTFAMILY,
        PRODUCTTYPE
    }

    public class GetAllProjectElementNodesInput
    {
        public string projectId;
        public int limit;
        public bool onlyElementsAssignedToProject;
        public bool isHideFamilyGroups;
        public bool isProductFamilyGroupView;
        public List<ProjectElementType> projectElementType;
        public string parentNodeId;
        public int skip;

        public GetAllProjectElementNodesInput()
        {
            projectId = "";
            limit = 1000;
            onlyElementsAssignedToProject = true;
            projectElementType = new List<ProjectElementType>();
            projectElementType.Add(ProjectElementType.ASSEMBLY);
            skip = 0;
            isHideFamilyGroups = true;
            isProductFamilyGroupView = false;
            parentNodeId = "";
        }
    }

    public class GetProjectElementNodesInput : GetAllProjectElementNodesInput
    {
        public string categoryId;

        public GetProjectElementNodesInput() : base()
        {
            categoryId = "";
        }
    }
}
