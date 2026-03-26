using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026.Queries
{
    public class ProjectInput
    {
        public string id; 
        public string firmId;
        public Metadata metadata;
    }

    public class  Metadata
    {
        public DateTime updated;
        public string updaterId;
    }
}
