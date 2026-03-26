using Autodesk.Revit.UI;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Specpoint.Revit2026
{
    public class Query
    {
        string token;
        string loggedinUser;
        GraphQLHttpClient client;

        string title = "Specpoint GraphQL";

        public Query(string title = "")
        {
            this.title = title;

            if (!string.IsNullOrEmpty(title))
            {
                // FAIL-FAST: Check token expiration before any operations
                if (!LoginUser.IsLoggedIn())
                {
                    Globals.Log.GraphQL($"Query ({title}) constructor: Token is expired or invalid");
                    throw new TokenExpiredException($"Query ({title}): authentication token has expired.");
                }
            }

            // Retrieve token from global state or registry
            if (!string.IsNullOrEmpty(Globals.Token))
            {
                token = Globals.Token;
            }
            else
            {
                token = SpecpointRegistry.GetValue("Token");
            }

            // Additional validation: ensure token is not empty
            if (string.IsNullOrEmpty(token))
            {
                Globals.Log.GraphQL("Query constructor: Token is empty");
                if (!string.IsNullOrEmpty(title))
                {
                    throw new TokenExpiredException(
                        $"Cannot create Query ({title}): authentication token is missing.");
                }
                else
                {
                    throw new TokenExpiredException("Cannot create Query: authentication token is missing.");
                }
            }

            loggedinUser = SpecpointRegistry.GetValue("name");

            SpecpointEnvironment env = new SpecpointEnvironment();
            client = new GraphQLHttpClient(env.SpecpointAPI, new NewtonsoftJsonSerializer());
            client.HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        public async ValueTask<CurrentUserActiveFirm> CurrentUserActiveFirmQuery()
        {
            string func = "myActiveFirm";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query CurrentUserActiveFirmQuery {
                        myActiveFirm {
                            firmId
                            firmName
                            firmRole
                            userId
                            isActive
                            isPrimary
                            firmAccountType
                            firmAccountTypeAbbreviation
                            firmAccountTypeApplicationURL
                            __typename
                        }
                    }"
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    GraphQLError e = response.Errors[0];
                    if (e != null)
                    {
                        bool isUnknownUser =
                            e.Message.StartsWith("The User with ID") &&
                            e.Message.EndsWith("was not found.");

                        if (isUnknownUser)
                        {
                            LoginUser.LogOff();

                            string prompt = "The specified user is not found.";
                            MessageBox.Show(prompt, "Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            OnError(request, response, elapsedMs, func);
                        }
                    }

                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    CurrentUserActiveFirm value = JsonConvert.DeserializeObject<CurrentUserActiveFirm>(result);

                    LogQuery(func, request, elapsedMs);

                    return value;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;


        }

        public void LogStack()
        {
            var stackTrace = new StackTrace(true); // true = capture file info
            foreach (var frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                Globals.Log.WriteError($"{method.DeclaringType}.{method.Name} (Line {frame.GetFileLineNumber()})");
            }
        }

        public void OnError(GraphQLRequest request, GraphQLResponse<object> response,
            long elapsedMs, string func)
        {
            if (response.Errors[0].Message == "The current user is not authorized to access this resource.")
            {
                LogQuery(func + " (Token Expired)", request, elapsedMs);
                LogStack();

                throw new TokenExpiredException(
                    $"OnError ({func}): authentication token is missing.");
                
            }
        }

        public async ValueTask<GetFirm> firm(string firmID)
        {
            const string func = "firm";

            try
            {
                var request = firmID == Globals.SystemFirmID ?
                new GraphQLRequest
                {
                    Query = @"
                    query GetFirm($id: String) {
                        firm(id: $id) { 
                            professionalDisciplines {
			                    id      
			                    name
			                    categories(masterId: $id) {
				                    id
                                    name
                                    revitCategory
			                    }
                            }
                        }
                    }",

                    Variables = new
                    {
                        id = firmID
                    }
                } :
                new GraphQLRequest
                {
                    Query = @"
                    query GetFirm($id: String) {
                        firm(id: $id) { 
                            projectGroups {
                                name
                                id
                                isDeleted
                            }
                            professionalDisciplines {
			                    id      
			                    name
			                    categories(masterId: $id) {
				                    id
                                    name
                                    revitCategory
			                    }
                            }
                        }
                    }",

                    Variables = new
                    {
                        id = firmID
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    GetFirm value = JsonConvert.DeserializeObject<GetFirm>(result);

                    string input = string.Format("firmId({0})", firmID);
                    LogQuery(func, request, input, elapsedMs);

                    return value;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public void DebugTokenExpired(GraphQLRequest request, GraphQLResponse<object> response,
            long elapsedMs, string func)
        {
            if (string.IsNullOrEmpty(title)) return;

            string debugTokenExpired = SpecpointRegistry.GetValue("TokenExpired");
            if (debugTokenExpired == "1")
            {
                LogQuery(func + " (Token Expired)", request, elapsedMs);
                LogStack();

                throw new TokenExpiredException(func + " (Token Expired)");
            }
        }

        public async ValueTask<GetAllUniformatClassifications> allUniformatClassifications()
        {
            const string func = "allUniformatClassifications";

            try
            {
                string query = @"
                    query {
                      allUniformatClassifications(firmId: ""4e3d44c1-ec50-40c3-8783-1983a905a8a9"") {
                            id
                            parentId
                            code
                            description
                            level
                            ppdDescription
                            associatedCategoryIds
                            associatedCategories {
                                id
                                name
                                revitCategory
                            }
                        }
                    }";

                var request = new GraphQLRequest
                {
                    Query = query
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    GetAllUniformatClassifications value = JsonConvert.DeserializeObject<GetAllUniformatClassifications>(result);

                    string input = "SystemFirmId(4e3d44c1-ec50-40c3-8783-1983a905a8a9)";
                    LogQuery(func, request, input, elapsedMs);

                    return value;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        private void LogQuery(string fx, GraphQLRequest request, long elapsedMs)
        {
            string msg = String.Format("{0} ({1}ms)\n{2}\n", fx, elapsedMs, request.Query);
            Globals.Log.GraphQL(msg);
        }

        private void LogQuery(string fx, GraphQLRequest request, string input, long elapsedMs)
        {
            string msg = String.Format("{0} ({1}ms)\n\n{2}\n{3}\n", fx, elapsedMs, input, request.Query);
            Globals.Log.GraphQL(msg);
        }

        private void LogQuery(string fx, GraphQLRequest request, string input, double elapsedMins)
        {
            string msg = String.Format("{0} ({1:F2}mins)\n\n{2}\n{3}\n", fx, elapsedMins, input, request.Query);
            Globals.Log.GraphQL(msg);
        }

        private void LogQuery(string fx, GraphQLRequest request, double elapsedMins)
        {
            string msg = String.Format("{0} ({1:F2}mins)\n{2}\n", fx, elapsedMins, request.Query);
            Globals.Log.GraphQL(msg);
        }

        private void ShowError(GraphQLResponse<object> response, long elapsedMs, string fx)
        {
            GraphQLError e = response.Errors[0];
            if (e != null)
            {
                string msg = String.Format("{0} ({1}ms) {2}", fx, elapsedMs, e.Message.Replace(';', '\n'));
                Globals.Log.GraphQLError(msg);

                string prompt = string.Format("User: {0}\nError running {1}.\n{2}", loggedinUser, fx, msg);
                MessageBox.Show(prompt, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public async ValueTask<NumberOfProjects> getNumberOfProjects()
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query {
                        getProjects(input: {
                            limit:1,
                            skip:0
                        }) {
                            projectGroupCounts {
                                groupId
                                totalCount
                            }
                        }
                    }"
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, nameof(getNumberOfProjects));

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, "getNumberOfProjects - getProjects");
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    NumberOfProjects count = JsonConvert.DeserializeObject<NumberOfProjects>(result);

                    LogQuery("getNumberOfProjects - getProjects", request, elapsedMs);

                    return count;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public async ValueTask<GetProjects> getProjects()
        {
            const string func = "getProjects";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProjects {
                        getProjects {
                            projects {           
                                id
                                name     
                                groupId
                                groupName
                            }
                            count
                          }
                    }"
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    GetProjects gp = JsonConvert.DeserializeObject<GetProjects>(result);

                    LogQuery(func, request, elapsedMs);

                    return gp;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public void ShowUserHasNoAccessToProjectError()
        {
            string msg = String.Format("The logged-in user ({0}) is not authorized to view or modify the Specpoint project this Revit Model is linked to.\n\nPlease link to another Specpoint project.", loggedinUser);
            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public async ValueTask<GetProject> getProject(string projectId)
        {
            string func = "getProject";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProject($projectId: String) {
                        getProject(projectId: $projectId) {
                            id
                            name
                            groupName
                            projectSettings {
                              familyNumberFormat
                              unitOfMeasure
                            }
                            lastUpdated 
                        }
                    }",

                    Variables = new
                    {
                        projectId = projectId
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    // Special handling of userHasNoAccess error
                    string userHasNoAccess = "You are not authorized to view or modify information on this project.";
                    string error = response.Errors[0].Message;
                    if (error == userHasNoAccess)
                    {
                        ShowUserHasNoAccessToProjectError();
                    }
                    else
                    {
                        OnError(request, response, elapsedMs, func);
                    }

                    // User has no access to the project
                    Globals.UserCanAccessProject = false;
                    
                    string input = string.Format("projectId({0})", projectId);
                    LogQuery(func, request, input, elapsedMs);
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    GetProject gp = JsonConvert.DeserializeObject<GetProject>(result);

                    LogQuery(func, request, elapsedMs);

                    // If user has access to the project
                    Globals.UserCanAccessProject = true;

                    return gp;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }
            finally
            {
                // Set config flag
                SpecpointRegistry.SetValue("UserCanAccessProject",
                    Globals.UserCanAccessProject ? "1" : "0");
            }

            return null;
        }

        public async void listingSearch()
        {
            const string func = "listingSearch";

            try
            {
                ManufacturerProductSearchInput v = new ManufacturerProductSearchInput();
                v.facets = new List<SearchFacetInput>();
                v.facets.Add(new SearchFacetInput
                {
                    name = "displayNameWithFirmName",
                    values = new List<string>()
                {
                    "'26993 test 1'%|%'26993 test 1'"
                }
                });

                var request = new GraphQLRequest
                {
                    Query = @"
                    query firmIdQuery($searchInput: ManufacturerProductSearchInput!) {
                        listingSearch(input: $searchInput) {
                        matches {  
                            firmId
                            firmName      
                        }
                        count
                        }
                    }",

                    Variables = new
                    {
                        searchInput = v
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    ListingSearch ls = JsonConvert.DeserializeObject<ListingSearch>(result);
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }
        }

        public async ValueTask<ProjectElementNodeToCategoryResults> getProjectElementNodesByMultipleCategory(
            GetProjectElementNodesByCategoryIdsInput value)
        {
            const string func = "getProjectElementNodesByMultipleCategory";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProjectElementNodesByMultipleCategory($input: GetProjectElementNodesByCategoryIdsInput!) {
                        getProjectElementNodesByMultipleCategory(input: $input) {
	                        categoryToProjectElementNodes {
                                categoryId
                                categoryName
                                revitCategory
                                projectElementNodes {
                                    id
                                    treePath
                                    isInProject
                                    text
                                    parentNodeId
                                    projectId
                                }
                                count
                            }
                            totalCount
	                    }
                    }",

                    Variables = new
                    {
                        input = value
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    ProjectElementNodeToCategoryResults results = JsonConvert.
                        DeserializeObject<ProjectElementNodeToCategoryResults>(response.Data.ToString());

                    string input = string.Format("projectId({0})\n", value.projectId);
                    LogQuery(func, request, input, elapsedMs);

                    return results;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public async ValueTask<GetProjectElementNodes> getProjectElementsNodes(
            GetAllProjectElementNodesInput value)
        {
            const string func = "getProjectElementsNodes";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProjectElementNodes($input: GetProjectElementNodesInput!) {
                        getProjectElementNodes(input: $input) {
	                        projectElementNodes {
			                    id
                                baseElementId
			                    elementId
			                    elementType
			                    text
			                    treePath
			                    parentTreePath
			                    parentNodeId
			                    isLastSubAssembly
                                isInProject
		                    }
		                    totalCount
	                    }
                    }",

                    Variables = new
                    {
                        input = value
                    }
                };

                var watch = Stopwatch.StartNew();
                client.HttpClient.Timeout = TimeSpan.FromMinutes(5);
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;
                double elapsedMin = watch.Elapsed.TotalMinutes;

                string projectId = value.projectId;
                string categoryId = "";
                if (value.GetType() == typeof(GetProjectElementNodesInput))
                {
                    categoryId = ((GetProjectElementNodesInput)value).categoryId;
                }

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    // Special handling of invalidProjectId error
                    string invalidProjectId = "Invalid Project Id;";
                    string error = response.Errors[0].Message;
                    if (error.StartsWith(invalidProjectId))
                    {
                        ShowUserHasNoAccessToProjectError();
                    }
                    else
                    {
                        OnError(request, response, elapsedMs, func);
                    }

                    // If user has access to the project
                    Globals.UserCanAccessProject = false;

                    return null;
                }

                if (response.Data != null)
                {
                    GetProjectElementNodes results = JsonConvert.
                        DeserializeObject<GetProjectElementNodes>(response.Data.ToString());

                    int totalCount = 0;
                    if (results != null)
                    {
                        totalCount = results.getProjectElementNodes.totalCount;
                    }

                    int currentCount = 0;
                    currentCount = results.getProjectElementNodes.projectElementNodes.Count;

                    // If user has access to the project
                    Globals.UserCanAccessProject = true;

                    string input = string.Format("projectId({0})\ncategoryId({1})\nCount({2})\nTotalCount({3})",
                        projectId, categoryId, currentCount, totalCount);
                    LogQuery("getProjectElementNodes", request, input, elapsedMin);

                    return results;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }
            finally
            {
                // Set config flag
                SpecpointRegistry.SetValue("UserCanAccessProject",
                    Globals.UserCanAccessProject ? "1" : "0");
            }

            return null;
        }

        private async ValueTask<GetProjectElementNodes> getDivsandSecs(string func, GetAllProjectElementNodesInput value)
        {
            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProjectElementNodes($input: GetProjectElementNodesInput!) {
                        getProjectElementNodes(input: $input) {
                            projectElementNodes { 
                                id 
                                text 
                                treePath 
                                parentTreePath 
                                parentNodeId 
                            } 
                        } 
                    }",

                    Variables = new
                    {
                        input = value
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                string projectId = value.projectId;
                string categoryId = "";
                if (value.GetType() == typeof(GetProjectElementNodesInput))
                {
                    categoryId = ((GetProjectElementNodesInput)value).categoryId;
                }

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    GetProjectElementNodes results = JsonConvert.
                        DeserializeObject<GetProjectElementNodes>(response.Data.ToString());

                    int totalCount = 0;
                    if (results != null)
                    {
                        totalCount = results.getProjectElementNodes.totalCount;
                    }

                    string input = string.Format("projectId({0})\ncategoryId({1})\nCount({2})",
                        projectId, categoryId, totalCount);
                    LogQuery(func, request, input, elapsedMs);

                    return results;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public async ValueTask<GetProjectElementNodes> getDivisions(string projId)
        {
            const string func = "getDivisions";

            GetAllProjectElementNodesInput value = new GetAllProjectElementNodesInput()
            {
                isProductFamilyGroupView = true,
                projectId = projId,
                parentNodeId = "root",
                limit = 100
            };

            return await getDivsandSecs(func, value);
        }

        public async ValueTask<GetProjectElementNodes> getSections(string projId, string divId)
        {
            const string func = "getSections";

            GetAllProjectElementNodesInput value = new GetAllProjectElementNodesInput()
            {
                isProductFamilyGroupView = true,
                projectId = projId,
                parentNodeId = divId,
                limit = 100
            };

            return await getDivsandSecs(func, value);
        }

        public async ValueTask<GetProjectElementNodes> getProductTypes(string projId, string sectionId)
        {
            const string func = "getProductTypes";

            GetAllProjectElementNodesInput value = new GetAllProjectElementNodesInput()
            {
                isProductFamilyGroupView = true,
                projectId = projId,
                parentNodeId = sectionId,
                limit = 100
            };

            value.projectElementType.Add(ProjectElementType.PRODUCTFAMILY);
            value.projectElementType.Add(ProjectElementType.PRODUCTTYPE);

            return await getDivsandSecs(func, value);
        }

        public async ValueTask<ProjectElementKeynoteNode> SetKeynoteIsHidden(string keynoteId,
            bool isHidden, string firmId, string projectId)
        {
            const string func = "setKeynoteIsHidden";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    mutation setKeynoteIsHidden($id: String, $isHidden: Boolean, $firmId: String, $projectId: String) {
                      setKeynoteIsHidden(id: $id, isHidden: $isHidden, firmId: $firmId, projectId: $projectId) {
                        text
                        isHidden
                        projectElementNodeId
                      }
                    }",

                    Variables = new
                    {
                        id = keynoteId,
                        isHidden = isHidden,
                        firmId = firmId,
                        projectId = projectId
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendMutationAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                else if (response.Data != null)
                {
                    ProjectElementKeynoteNode result = JsonConvert.
                        DeserializeObject<ProjectElementKeynoteNode>(response.Data.ToString());

                    LogQuery(func, request, elapsedMs);

                    return result;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
                return null;
            }

            return null;
        }

        public async ValueTask<bool> AddUniformatClassificationToProject(CreateElementInput value)
        {
            const string func = "addUniformatClassificationToProject";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    mutation addUniformatClassificationToProject($input: CreateElementInput!) {
                      addUniformatClassificationToProject(input: $input) {
                        projectElementNodes {
                          id
                          elementId
                          baseElementId
                          nodeOrder
                          isInProject
                          elementType
                          text
                          treePath
                          parentNodeId
                          projectId
                          isHidden
                          parentTreePath
                          isSearchResult
                          hasChildren
                          isLastSubAssembly
                          isBestPractice
                          isGlobalBestPractice
                          hasBestPractice
                          isImportedToProject
                          importedToProjectId
                          isBasedFromImportedNode
                          isProjectOnly
                          isBpPublished
                          __typename
                        }
                        __typename
                      }
                    }",

                    Variables = new
                    {
                        input = value
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendMutationAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return false;
                }

                string input = string.Format("value({0})", value.treePath);
                LogQuery(func, request, input, elapsedMs);
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
                return false;
            }

            return true;
        }

        public async Task<Firm> GetCurrentUserActiveFirm()
        {
            try
            {
                string firmID = await GetCurrentUserActiveFirmID();
                if (firmID == null) return null;

                Query query = new Query(title);
                var result = await query.firm(firmID);
                if (result == null) return null;

                return result.firm;
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public async Task<string> GetCurrentUserActiveFirmID()
        {
            if (Globals.CurrentUserFirm == null)
            {
                string func = nameof(GetCurrentUserActiveFirmID);
                Query query = new Query(func);
                Globals.CurrentUserActiveFirm = await query.CurrentUserActiveFirmQuery();
                if (Globals.CurrentUserActiveFirm == null) return null;
                if (Globals.CurrentUserActiveFirm.myActiveFirm == null) return null;
            }

            return Globals.CurrentUserActiveFirm.myActiveFirm.firmId;
        }

        public async ValueTask<GetProjectElementKeynotes> getProjectElementKeynotes(
            string projectId, string firmId)
        {
            const string func = "getProjectElementKeynotes";

            try
            {
                var request = new GraphQLRequest
                {
                    Query = @"
                    query GetProjectElementKeynotes($projectId: String, $firmId: String) {
                        getProjectElementKeynotes(projectId: $projectId, firmId: $firmId) {
                            projectElementKeynoteNodes {
                                id
                                keynoteCode
                                text
                                projectElementType
                                isHidden
                                baseElementId
                                projectElementNodeId
                            }
                            totalCount
                        } 
                    }",

                    Variables = new
                    {
                        projectId = projectId,
                        firmId = firmId
                    }
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    GetProjectElementKeynotes results = JsonConvert.
                        DeserializeObject<GetProjectElementKeynotes>(response.Data.ToString());

                    // Create list of keynotes for families and productTypes
                    results.Init();

                    int totalCount = 0;
                    if (results != null)
                    {
                        totalCount = results.getProjectElementKeynotes.totalCount;
                    }

                    string input = string.Format("projectId({0})\nfirmId({1})",
                        projectId, firmId);
                    LogQuery(func, request, input, elapsedMs);

                    return results;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }

        public async Task<User> user(string firmId, string userId)
        {
            const string func = "user";

            try
            {
                string parameters = string.Format(@"firmId: ""{0}"", id: ""{1}""", firmId, userId);

                string query = @"
                    query GetUser{
                        user(" + parameters + @") {
                            userFullName
                            role
                            status
                            email
                            firmId
                            id
                        }
                    }";

                var request = new GraphQLRequest
                {
                    Query = query
                };

                var watch = Stopwatch.StartNew();
                var response = await client.SendQueryAsync<object>(request);
                watch.Stop();
                long elapsedMs = watch.ElapsedMilliseconds;

                DebugTokenExpired(request, response, elapsedMs, func);

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    OnError(request, response, elapsedMs, func);
                    return null;
                }

                if (response.Data != null)
                {
                    string result = response.Data.ToString();
                    UserResult value = JsonConvert.DeserializeObject<UserResult>(result);

                    string input = string.Format("firm({0}) user({1})", firmId, userId);
                    LogQuery("user", request, input, elapsedMs);

                    return value.user;
                }
            }
            catch (TokenExpiredException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Globals.Log.GraphQL(ex);
            }

            return null;
        }
    }
}
