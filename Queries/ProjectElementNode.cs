using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Specpoint.Revit2026
{
    public class ProjectElementNode
    {
        public string id;
        public string baseElementId;
        public string elementId;
        public ProjectElementType elementType;
        public string text;
        public string treePath;
        public string parentTreePath;
        public string parentNodeId;
        public bool isLastSubAssembly;
        public bool isInProject;
    }
}
