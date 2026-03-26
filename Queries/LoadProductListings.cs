using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class LoadProductListings
    {
        private const string title = "Product Selection";

        // Filter by category
        public List<string> includedCategories;

        public string projectId = "";

        // Specpoint Assembly Code associated with Revit BIM Element
        public string RevitAssemblyCode { get; set; }

        private ProductSelectionProgress parent;

        public LoadProductListings()
        {
            parent = null;
            projectId = "";
            includedCategories = null;
            RevitAssemblyCode = "";
        }

        public LoadProductListings(
            ProductSelectionProgress parent,
            string projectId,
            string assemblyCode,
            List<string> includedCategories) : this()
        {
            this.parent = parent;
            this.projectId = projectId;
            this.includedCategories = includedCategories;
            this.RevitAssemblyCode = assemblyCode;
        }

        public async ValueTask<bool> AddAssemblyToProject(string projectId, ProjectElementNode node)
        {
            Query query = new Query(title);
            CreateElementInput value = new CreateElementInput()
            {
                projectId = projectId,
                baseElementId = node.baseElementId,
                nodeId = node.id,
                treePath = node.treePath,
                isProductFamilyGroupView = false
            };
            bool result = await query.AddUniformatClassificationToProject(value);
            return result;
        }

        public async Task<bool> Init()
        {
            if (includedCategories == null) return false;
            if (includedCategories.Count < 1) return false;

            // Get list of Specpoint project categories
            if (Globals.specpointCategories == null)
            {
                Globals.specpointCategories = new SpecpointCategories();
                bool ret = await Globals.specpointCategories.Init();

                if (ret == false) return false;

                Globals.specpointCategories.GetNoMatchInRevit();
            }

            // Create a map between Revit and Specpoint categories
            CreateCategoryMap();

            // Get specpoint family assigned to the specpoint project
            ProjectElementNode node = await GetSpecpointFamily();

            if (parent != null)
            {
                parent.Visible = false;
            }

            if (node == null)
            {
                string prompt = string.Format("There are no Product Listings for {0} for {1}", includedCategories[0], RevitAssemblyCode);
                parent.ShowInformation(prompt);
                return false;
            }

            // If assembly is not added to the project, prompt error
            if (node.elementId == null)
            {
                string msg = String.Format("The assigned Assembly Code ({0}) is not added to the project.\n\nDo you want to add it to the Specpoint project?", RevitAssemblyCode);
                bool yes = parent.ShowQuestion(msg);
                if (yes == true)
                {
                    parent.Visible = true;
                    bool addAssemblySuccess = await AddAssemblyToProject(projectId, node);

                    if (addAssemblySuccess == true)
                    {
                        // Try again (Should have been already added and elementId should be available)
                        node = await GetSpecpointFamily();

                        // Unable to add assembly to the Specpoint project, just exit
                        if (node == null) return false;
                        if (node.elementId == null) return false;
                    }
                    else
                    {
                        // Unable to add assembly to the Specpoint project, just exit
                        return false;
                    }
                }
                else
                {
                    // User clicked No, just exit
                    return false;
                }
            }

            string category = includedCategories[0];
            string treePath = ExtractAssemblyPath(node);

            // Compose URL
            SpecpointEnvironment env = new SpecpointEnvironment();
            string url = "";

            if (Globals.specpointCategories != null)
            {
                if (Globals.specpointCategories.ContainsKey(category))
                {
                    url = string.Format("{0}/{1}/dialog?source=plugin&category={2}&isLastSubAssembly=true&assemblyPath={3}&nodeId={4}&elementId={5}&elementType=ASSEMBLY",
                        env.Projects, Globals.SpecpointProjectID, category, treePath, node.id, node.elementId);

                }
                else
                {
                    url = string.Format("{0}/{1}/dialog?source=plugin&category=All%7C%7CCategories&isLastSubAssembly=true&assemblyPath={2}&nodeId={3}&elementId={4}&elementType=ASSEMBLY",
                        env.Projects, Globals.SpecpointProjectID, treePath, node.id, node.elementId);

                }
            }

            Globals.Log.Write(url);

            // Launch browser
            Browser b = new Browser();
            b.OpenUrl(url);

            return true;

        }

        private string ExtractAssemblyPath(ProjectElementNode node)
        {
            string pipe = "%7C";
            string space = "%23";

            string assemblyPath = node.treePath.
                Replace("|", pipe).
                Replace(" ", space);

            return assemblyPath;
        }

        private string ExtractFamily(ProjectElementNode node)
        {
            // Compose family component (Note space == ||)
            // Ex. 055000%7C%7C-%7C%7CMETAL%7C%7CFABRICATIONS
            string pipe = "%7C";
            char[] pipeSep = { '|' };
            string[] listFamily = node.text.Split(pipeSep, 2, StringSplitOptions.RemoveEmptyEntries);
            string family = string.Format("{0} - {1}", listFamily[0], listFamily[1]);
            return family.Replace(" ", pipe + pipe);
        }

        /// <summary>
        /// Create a map between Revit and Specpoint categories
        /// </summary>
        private void CreateCategoryMap()
        {
            // If map already exists, exit
            if (Globals.mapCategoryIDs != null) return;

            Globals.mapCategoryIDs = new RevitSpecpointCategoryMap();
            Globals.mapCategoryIDs.Init();
        }

        private async Task<ProjectElementNode> GetSpecpointFamily()
        {
            if (Globals.mapCategoryIDs == null) return null;

            // Get all project elements 
            foreach (string categoryName in includedCategories)
            {
                // Get node from Revit Category
                Category c = Globals.revitCategories[categoryName];
                GetProjectElementNodes nodes = null;

                int modeValue = SetAssemblyCodeMode.AddedToProject;
                string mode = SpecpointRegistry.GetValue("SetAssemblyCodeMode");
                if (!string.IsNullOrEmpty(mode))
                {
                    modeValue = Convert.ToInt32(mode);
                }

                if (modeValue == SetAssemblyCodeMode.AddedToProject)
                {
                    // Load added to project assemblies in the project
                    nodes = await GetSpecpointAssemblies(projectId, c);

                    if (nodes == null) return null;
                }
                else if (modeValue == SetAssemblyCodeMode.AllProjectElements)
                {
                    // Get all assembly NOT added to the project
                    nodes = await GetOtherNodes(projectId, new List<ProjectElementType>()
                    {
                        ProjectElementType.ASSEMBLY
                    });

                    if (nodes == null) return null;
                }

                foreach (ProjectElementNode node in nodes.getProjectElementNodes.projectElementNodes)
                {
                    if (node.elementType == ProjectElementType.ASSEMBLY &&
                        node.treePath.Contains("|"))
                    {
                        char[] pipe = { '|' };
                        string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                        try
                        {
                            if (list.Length == 4)
                            {
                                if (RevitAssemblyCode == list[2])
                                    return node;
                            }
                            else if (list.Length == 6)
                            {
                                if (RevitAssemblyCode == list[4])
                                    return node;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.ReportError(ex);
                        }
                    }
                }
            }

            return null;
        }

        public async Task<GetProjectElementNodes> GetOtherNodes(string projectId, List<ProjectElementType> types)
        {
            int currentCount = 0;
            Dictionary<int, GetProjectElementNodes> finalResults = new Dictionary<int, GetProjectElementNodes>();

            int totalCount = 0;
            GetProjectElementNodes results = null;

            do
            {
                // Get specific project elements given the Specpoint category ID
                GetAllProjectElementNodesInput all = new GetAllProjectElementNodesInput()
                {
                    projectId = projectId,
                    projectElementType = types,
                    onlyElementsAssignedToProject = false,
                    skip = currentCount,
                };

                string queryTitle = string.Format("{0} - GetOtherNodes", title);
                Query query = new Query(queryTitle);
                results = await query.getProjectElementsNodes(all);

                if (results != null)
                {
                    finalResults[currentCount] = results;

                    totalCount = results.getProjectElementNodes.totalCount;

                    // Next batch
                    currentCount += results.ListNodes.Count;
                }
            }
            while (currentCount < totalCount);

            // Consolidate results
            results = Globals.ConsolidateResults(finalResults);

            return results;
        }

        public async Task<GetProjectElementNodes> GetSpecpointAssemblies(string projectId, Category revitCategory)
        {
            if (revitCategory == null) return null;
            if (Globals.mapCategoryIDs == null) return null;

            // Get specific project elements given the Specpoint category ID
            GetProjectElementNodesInput specific = null;
            if (Globals.mapCategoryIDs.ContainsKey(revitCategory.Id.ToString()))
            {
                specific = new GetProjectElementNodesInput()
                {
                    projectId = projectId,
                    categoryId = Globals.mapCategoryIDs[revitCategory.Id.ToString()],
                };

                specific.projectElementType = new List<ProjectElementType>();
                specific.projectElementType.Add(ProjectElementType.ASSEMBLY);
            }

            if (specific == null) return null;

            string func = nameof(GetSpecpointAssemblies);
            Query query = new Query(func);
            GetProjectElementNodes results = await query.getProjectElementsNodes(specific);

            return results;
        }
    }
}
