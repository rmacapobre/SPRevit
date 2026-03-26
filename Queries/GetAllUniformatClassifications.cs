using System.Collections.Generic;

namespace Specpoint.Revit2026
{
    public class UniformatClassification
    {
        public string id;
        public string parentId;
        public string code;
        public string description;
        public string level;
        public string ppdDescription;
        public List<string> associatedCategoryIds;
        public List<ProductFamilyCategory> associatedCategories;

        public SortedDictionary<string, UniformatClassification> childItems;

        public UniformatClassification()
        {
            childItems = new SortedDictionary<string, UniformatClassification>();
        }
    }

    public class GetAllUniformatClassifications
    {
        public List<UniformatClassification> allUniformatClassifications;

        public SortedDictionary<string, UniformatClassification> childItems;

        public GetAllUniformatClassifications()
        {
            this.allUniformatClassifications = new List<UniformatClassification>();            
        }
    }
}
