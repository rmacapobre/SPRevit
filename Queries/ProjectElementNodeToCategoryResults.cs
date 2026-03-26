using System.Collections.Generic;

namespace Specpoint.Revit2026
{
    public class CategoryToProjectElementNode
    {
        public string categoryId;
        public string categoryName;
        public int count;
        public List<ProjectElementNode> projectElementNodes;
        public string revitCategory;
    }

    public class GetProjectElementNodesByMultipleCategory
    {
        public List<CategoryToProjectElementNode> categoryToProjectElementNodes;
        public int totalCount;
    }

    public class ProjectElementNodeToCategoryResults
    {
        public GetProjectElementNodesByMultipleCategory getProjectElementNodesByMultipleCategory;

    }
}
