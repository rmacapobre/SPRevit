using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class GetProjectElementNodesByCategoryIdsInput
    {
        public string projectId;
        public List<string> categoryIds;

        public GetProjectElementNodesByCategoryIdsInput()
        {
            categoryIds = new List<string>();
        }

        public GetProjectElementNodesByCategoryIdsInput(string projectId) : this()
        {
            this.projectId = projectId;
        }
    }
}
