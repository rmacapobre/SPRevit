using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class ProjectSettings
    {
        public string familyNumberFormat;
        public string unitOfMeasure;
    }

    public class ProjectItemDetail
    {
        public string id;
        public string name;
        public string groupName;
        public ProjectSettings projectSettings;
        public DateTime lastUpdated;
    }

    public class GetProject
    {
        public ProjectItemDetail getProject;
    }

    public class GetProjects
    {
        public ProjectResults getProjects;
    }
}
