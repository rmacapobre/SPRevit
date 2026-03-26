# Specpoint.Revit2026 - Developer Quick Reference

## Project Structure Quick Map

```
Specpoint.Revit2026/
├── Commands/              # 8 command implementations
│   ├── AccountCommand.cs           (4K)  - Login/account management
│   ├── KeynotesManagerCommand.cs   (10K) - Keynote operations
│   ├── LinkProjectCommand.cs       (9K)  - Project linking
│   ├── ProductSelectionCommand.cs  (3K)  - Product browser
│   ├── SetAssemblyCodeCommand.cs   (5K)  - Element assembly code
│   ├── UpdateCommand.cs            (6K)  - Sync to Specpoint
│   ├── ValidationScheduleCommand.cs (8K) - Bulk validation
│   └── ViewSpecificationsCommand.cs (5K) - Open web specs
│
├── Forms/                # 22 UI forms
│   ├── LoginForm.cs                (24K)  - Auth UI
│   ├── LinkProjectForm.cs          (9K)   - Project selection
│   ├── SelectProjectForm.cs        (11K)  - Project browser
│   ├── KeynotesManagerForm.cs      (112K) - Full keynote mgmt
│   ├── ModelValidationForm.cs      (139K) - Bulk grid editor
│   ├── SetAssemblyCodeForm.cs      (48K)  - Assembly picker
│   ├── ProductSelectionProgress.cs (4K)   - Product loading
│   ├── SetAssemblyCodeProgress.cs  (2K)   - Code assignment
│   └── ... (progress forms, utilities)
│
├── Queries/              # GraphQL data access
│   ├── Query.cs                    (45K)  - Main GraphQL client
│   ├── Assembly.cs                 (13K)  - Uniformat data
│   ├── ProjectElementNode.cs       (0.5K) - Element mapping
│   ├── RevitCategories.cs          (5K)   - Category mapping
│   ├── LoadProductListings.cs      (12K)  - Product queries
│   └── ... (DTOs, input types)
│
├── Utilities/            # 15 helper classes
│   ├── Availability.cs             (7K)   - Ribbon button rules
│   ├── DocumentExtensions.cs       (10K)  - Document helpers
│   ├── RevitDrawing.cs             (14K)  - Drawing utilities
│   ├── SpecpointLog.cs             (4K)   - Logger singleton
│   ├── SpecpointRegistry.cs        (3K)   - Registry access
│   ├── LoginExternalEventHandler.cs (1K)  - Async login
│   ├── DeferredSaveHandler.cs      (4K)   - Async save
│   └── ... (extensions, utilities)
│
├── PluginApp.cs          # Entry point, ribbon creation
├── Globals.cs            # Global state, environment config
└── Command.cs            # Template/example command
```

---

## Key Classes Reference

### Entry Points

#### `PluginApp.cs`
**Implements:** `IExternalApplication`
**Purpose:** Plugin initialization, ribbon creation
**Key Methods:**
- `OnStartup()` - Initialize globals, create ribbon
- `OnShutdown()` - Cleanup
- `application_Idling()` - Ribbon refresh handler

#### Command Classes (8 total)
**Pattern:** Implement `IExternalCommand`
**Transaction Mode:** `[Transaction(TransactionMode.Manual)]`
**Key Method:**
```csharp
public Result Execute(
    ExternalCommandData commandData,
    ref string message,
    ElementSet elements)
```

---

### Global State Management

#### `Globals.cs`
**Static Properties:**
```csharp
public static string Token;                    // JWT auth token
public static string SpecpointProjectID;       // Current project
public static string extension_AccountId;      // User account
public static SpecpointLog Log;                // Logger instance
public static ExternalEvent LoginExternalEvent;
public static ExternalEvent DeferredSaveEvent;
public static bool NeedsRibbonRefresh;
public static PushButtonData btnCurrentUser;
public static PushButtonData btnCurrentProject;
```

**Key Methods:**
```csharp
public static void RefreshRibbon();
public static void RefreshButtons();
```

#### `SpecpointEnvironment` (in Globals.cs)
**Environment Selection:**
- `d` = Development
- `s` = Staging
- `q` = QE
- `p` = Production (default)

**Properties:**
- `LoginPage` - OAuth URL
- `WebAppLoginPage` - Web app URL
- `Workspace` - Project workspace URL template
- `Projects` - Projects list URL
- `SpecpointAPI` - GraphQL endpoint
- `ForgotPassword` - Password reset URL

---

### Authentication & Security

#### `Query.cs`
**Constructor:**
```csharp
public Query(string title = "")
{
    // 1. Check if logged in (fail-fast)
    if (!LoginUser.IsLoggedIn())
        throw new TokenExpiredException();

    // 2. Get token from Globals or Registry
    token = Globals.Token ?? SpecpointRegistry.GetValue("Token");

    // 3. Validate token exists
    if (string.IsNullOrEmpty(token))
        throw new TokenExpiredException();

    // 4. Create GraphQL client with bearer token
    client = new GraphQLHttpClient(env.SpecpointAPI, ...);
    client.HttpClient.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
}
```

#### `LoginUser` (static class)
**Key Methods:**
```csharp
public static bool IsLoggedIn()
{
    // Check token expiration
}

public static void LogOff()
{
    // Clear token, reset state
}
```

#### `TokenExpiredException`
**Usage:** Thrown when token is expired or invalid
**Handling:** Trigger re-authentication flow via `LoginExternalEvent`

---

### Data Access Layer

#### `Query.cs` - GraphQL Client
**Key Query Methods:**

```csharp
// User & Firm
public async ValueTask<CurrentUserActiveFirm> CurrentUserActiveFirmQuery()

// Projects
public async ValueTask<GetProjects> GetProjectsQuery(...)

// Elements
public async ValueTask<ProjectElementNodesResults>
    GetProjectElementNodesQuery(...)

// Products
public async ValueTask<LoadProductListings>
    LoadProductListingsQuery(...)

// Assembly Codes
public async ValueTask<List<Assembly>>
    GetAllUniformatClassificationsQuery()
```

**Error Handling Pattern:**
```csharp
try
{
    var request = new GraphQLRequest { Query = "..." };
    var watch = Stopwatch.StartNew();
    var response = await client.SendQueryAsync<T>(request);
    watch.Stop();

    // Check for errors
    if (response.Errors != null)
    {
        OnError(request, response, watch.ElapsedMilliseconds, func);
        return null;
    }

    // Deserialize and return
    return JsonConvert.DeserializeObject<T>(response.Data.ToString());
}
catch (TokenExpiredException)
{
    throw; // Propagate
}
catch (Exception ex)
{
    Globals.Log.GraphQL(ex);
}
```

---

### Registry Management

#### `SpecpointRegistry.cs`
**Location:** `HKEY_CURRENT_USER\Software\Specpoint`

**Key Methods:**
```csharp
public static string GetValue(string key)
public static void SetValue(string key, string value)
public static void DeleteValue(string key)
```

**Common Keys:**
- `env` - Environment (d/s/q/p)
- `Token` - JWT token
- `name` - User display name
- `extension_AccountId` - Account ID
- `ProjectName` - Current project name
- `SpecpointProjectID` - Project GUID

---

### Logging

#### `SpecpointLog.cs`
**Pattern:** Singleton
**Access:** `Globals.Log` or `SpecpointLog.Instance`

**Methods:**
```csharp
public void Write(string message)
public void Write(Exception ex)
public void GraphQL(string message)
public void GraphQL(Exception ex)
```

**Usage:**
```csharp
Globals.Log.Write("User clicked button");
Globals.Log.GraphQL($"Query {queryName} took {ms}ms");
Globals.Log.Write(exception);
```

---

### Utility Extensions

#### `DocumentExtensions.cs`
**Extension Methods on `Document`:**
```csharp
public static Element GetElementById(this Document doc, ElementId id)
public static IList<Element> GetElementsByCategory(...)
public static Parameter GetParameter(this Document doc, string name)
// ... more helpers
```

#### `ElementExtensions.cs`
**Extension Methods on `Element`:**
```csharp
public static string GetParameterValueAsString(this Element elem, string paramName)
// ... more helpers
```

---

### Ribbon Availability

#### `Availability.cs`
**Classes:**

```csharp
// Always enabled
public class AlwaysAvailable : IExternalCommandAvailability
{
    public bool IsCommandAvailable(...) => true;
}

// Enabled for logged in or guest users
public class AvailableIfGuest : IExternalCommandAvailability
{
    // Check login status
}

// Enabled when project is linked
public class AvailableWhenProjectIsSet : IExternalCommandAvailability
{
    // Check if SpecpointProjectID is set
}

// Enabled when one element selected
public class AvailableWhenOneRevitElementIsSelected : IExternalCommandAvailability
{
    // Check selection count == 1
}

// Enabled during sync
public class OnUpdateSync : IExternalCommandAvailability
{
    // Check sync state
}
```

---

### External Events

#### `LoginExternalEventHandler.cs`
**Purpose:** Handle token refresh without blocking Revit
**Usage:**
```csharp
// Raise event
Globals.LoginExternalEvent.Raise();

// Handler executes on main thread
public void Execute(UIApplication app)
{
    // Show login form
    // Update token
}
```

#### `DeferredSaveHandler.cs`
**Purpose:** Save documents asynchronously
**Usage:**
```csharp
// Set document to save
Globals.DeferredSaveHandler.DocumentToSave = doc;

// Raise event
Globals.DeferredSaveEvent.Raise();
```

---

## Common Patterns

### Creating a New Command

```csharp
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Specpoint.Revit2026
{
    [Transaction(TransactionMode.Manual)]
    public class MyNewCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Check authentication
                Query query = new Query("MyNewCommand");

                // 2. Get data
                var data = await query.SomeQuery();

                // 3. Show UI
                using (var form = new MyNewForm())
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return Result.Cancelled;
                }

                // 4. Modify document
                using (Transaction trans = new Transaction(doc))
                {
                    trans.Start("My Operation");
                    // ... modifications
                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (TokenExpiredException)
            {
                MessageBox.Show("Please log in again.");
                return Result.Failed;
            }
            catch (Exception ex)
            {
                Globals.Log.Write(ex);
                ErrorReporter.Show(ex);
                return Result.Failed;
            }
        }
    }
}
```

### Adding to Ribbon

In `PluginApp.OnStartup()`:
```csharp
PushButtonData btnMyCommand = new PushButtonData(
    "My Command",               // Internal name
    "My\nCommand",             // Display name (use \n for line break)
    Assembly.GetExecutingAssembly().Location,
    typeof(MyNewCommand).FullName)
{
    Image = GetResourceBitmap("MyCommand.png"),
    LargeImage = GetResourceBitmap("MyCommand.png"),
    ToolTip = "Does something useful",
    AvailabilityClassName = typeof(AlwaysAvailable).FullName
};
ribbon.AddItem(btnMyCommand);
```

---

### Making GraphQL Query

```csharp
public async ValueTask<MyData> GetMyDataQuery(string param)
{
    string func = "getMyData";

    try
    {
        var request = new GraphQLRequest
        {
            Query = @"
                query GetMyData($param: String!) {
                    myData(param: $param) {
                        id
                        name
                        value
                    }
                }",
            Variables = new { param = param }
        };

        var watch = Stopwatch.StartNew();
        var response = await client.SendQueryAsync<object>(request);
        watch.Stop();

        // Log timing
        DebugTokenExpired(request, response, watch.ElapsedMilliseconds, func);

        // Check errors
        if (response.Errors != null && response.Errors.Length > 0)
        {
            OnError(request, response, watch.ElapsedMilliseconds, func);
            return null;
        }

        // Deserialize
        if (response.Data != null)
        {
            string result = response.Data.ToString();
            MyData value = JsonConvert.DeserializeObject<MyData>(result);
            LogQuery(func, request, watch.ElapsedMilliseconds);
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
```

---

### Showing Progress Form

```csharp
// Create progress form
var progressForm = new MyProgressForm();
progressForm.Show();

try
{
    // Long operation
    for (int i = 0; i < items.Count; i++)
    {
        // Update progress
        progressForm.UpdateProgress(i, items.Count, $"Processing {items[i].Name}");

        // Allow UI updates
        Application.DoEvents();

        // Check for cancellation
        if (progressForm.IsCancelled)
        {
            progressForm.Close();
            return Result.Cancelled;
        }

        // Do work
        ProcessItem(items[i]);
    }

    progressForm.Close();
    return Result.Succeeded;
}
catch (Exception ex)
{
    progressForm.Close();
    Globals.Log.Write(ex);
    ErrorReporter.Show(ex);
    return Result.Failed;
}
```

---

## Data Model Quick Reference

### Assembly (Uniformat Code)
```csharp
public class Assembly
{
    public string assemblyId;                    // GUID
    public string assemblyCode;                  // e.g., "B2010.10"
    public string assemblyTitle;                 // Description
    public string assemblyDescription;           // Full description
    public int? level;                           // Hierarchy level
    public string parentId;                      // Parent assembly ID
    public List<Assembly> subAssemblies;         // Children
}
```

### ProjectElementNode
```csharp
public class ProjectElementNode
{
    public string id;                            // Element ID in Specpoint
    public string name;                          // Element name
    public string assemblyCode;                  // Assigned assembly code
    public string categoryId;                    // Revit category
}
```

### CurrentUserActiveFirm
```csharp
public class CurrentUserActiveFirm
{
    public string firmId;
    public string firmName;
    public string firmRole;
    public string userId;
    public bool isActive;
    public bool isPrimary;
    public string firmAccountType;
}
```

### ProjectItem
```csharp
public class ProjectItem
{
    public string id;                            // Project GUID
    public string name;                          // Project name
    public string projectNumber;                 // Project number
    public string description;                   // Description
}
```

---

## Environment URLs Quick Reference

### Production (p)
- **API:** `https://api-specpoint.mydeltek.com/`
- **Web:** `https://ae-specpoint.mydeltek.com`
- **Login:** `https://specpointb2c.b2clogin.com/...`
- **Client ID:** `d6e622b7-993d-4a14-af96-1c33ce5f6fb9`

### Staging (s)
- **API:** `https://api-specpoint-staging.engdeltek.com/`
- **Web:** `https://ae-specpoint-staging.engdeltek.com`
- **Login:** `https://specpointdevb2c.b2clogin.com/...`
- **Client ID:** `a7486ba9-7308-4617-90aa-6cc45171ad68`

### QE (q)
- **API:** `https://api-specpoint-qe1.engdeltek.com`
- **Web:** `https://ae-specpoint-qe1.engdeltek.com`

### Dev (d)
- **API:** `https://knowledgepointapidev.azurewebsites.net/`
- **Web:** `https://knowledgepointclientdev.azureedge.net`

---

## Debugging Tips

### Enable Detailed Logging
Check log files in configured location (see `SpecpointLog.cs`)

### Inspect Token
```csharp
string token = SpecpointRegistry.GetValue("Token");
// Decode JWT at https://jwt.ms
```

### Check Authentication State
```csharp
bool loggedIn = LoginUser.IsLoggedIn();
string userName = SpecpointRegistry.GetValue("name");
string projectId = Globals.SpecpointProjectID;
```

### Test GraphQL Queries
Use GraphQL Playground at API endpoint + `/graphql`

### Registry Inspection
```bash
reg query HKCU\Software\Specpoint
```

---

## Build & Deploy Checklist

### Development Build
1. Open in Visual Studio 2022
2. Restore NuGet packages
3. Build solution (Debug configuration)
4. DLLs copied to `%AppData%\Autodesk\REVIT\Addins\2026`
5. Start Revit 2026 to test

### Release Build
1. Switch to Release configuration
2. Build solution
3. Post-build runs code signing script
4. Verify signature on DLL
5. Test in clean Revit environment
6. Package for distribution

### Deployment
1. Copy `.addin` manifest
2. Copy all DLLs (including dependencies)
3. Copy image assets
4. Set registry environment (`env` key)
5. Document installation steps

---

## Troubleshooting Guide

### Token Expired Errors
**Symptom:** `TokenExpiredException` thrown frequently
**Solution:**
1. Check token in registry
2. Verify Azure AD B2C is accessible
3. Clear token and re-login
4. Check system clock sync

### Ribbon Buttons Not Appearing
**Symptom:** Specpoint tab or buttons missing
**Solution:**
1. Check `.addin` file in correct location
2. Verify DLL path in `.addin`
3. Check Revit version matches (2026)
4. Review Revit journal file for errors

### GraphQL Query Failures
**Symptom:** Queries return errors or null
**Solution:**
1. Check internet connectivity
2. Verify API endpoint for environment
3. Test query in GraphQL Playground
4. Check token validity
5. Review logs for details

### UI Not Responsive
**Symptom:** Forms freeze during operations
**Solution:**
1. Ensure long operations use async/await
2. Call `Application.DoEvents()` in loops
3. Show progress forms
4. Use External Events for Revit API calls

---

## Performance Optimization Tips

1. **Use Async Queries:**
   ```csharp
   var data = await query.GetDataAsync();
   ```

2. **Batch Operations:**
   ```csharp
   // Good: Single transaction for multiple elements
   using (Transaction trans = new Transaction(doc))
   {
       trans.Start("Batch");
       foreach (var elem in elements)
           UpdateElement(elem);
       trans.Commit();
   }
   ```

3. **Filter Early:**
   ```csharp
   // Good: Filter at database
   var walls = new FilteredElementCollector(doc)
       .OfCategory(BuiltInCategory.OST_Walls)
       .WhereElementIsNotElementType();
   ```

4. **Cache Results:**
   ```csharp
   // Cache frequently accessed data
   private static List<Assembly> _assemblies;
   public static async Task<List<Assembly>> GetAssemblies()
   {
       if (_assemblies == null)
           _assemblies = await query.GetAllUniformatClassificationsQuery();
       return _assemblies;
   }
   ```

---

## Security Best Practices

1. **Never Log Tokens:**
   ```csharp
   // Bad: Globals.Log.Write($"Token: {token}");
   // Good: Globals.Log.Write("Token retrieved successfully");
   ```

2. **Validate All Inputs:**
   ```csharp
   if (string.IsNullOrEmpty(projectId))
       throw new ArgumentException("Project ID required");
   ```

3. **Handle Expiration:**
   ```csharp
   try
   {
       var query = new Query("MyCommand");
   }
   catch (TokenExpiredException)
   {
       // Trigger re-login
       Globals.LoginExternalEvent.Raise();
   }
   ```

4. **Use HTTPS Only:**
   All API endpoints use HTTPS.

---

## Quick Command Reference

### User Commands
- `/Account` → `AccountCommand.cs`
- `/Link Project` → `LinkProjectCommand.cs`
- `/Validation Schedule` → `ValidationScheduleCommand.cs`
- `/Set Assembly Code` → `SetAssemblyCodeCommand.cs`
- `/View Specifications` → `ViewSpecificationsCommand.cs`
- `/Product Selection` → `ProductSelectionCommand.cs`
- `/Update` → `UpdateCommand.cs`
- `/Keynotes Manager` → `KeynotesManagerCommand.cs` (disabled)

### Registry Keys
- `env` - Environment
- `Token` - JWT token
- `name` - User name
- `ProjectName` - Project name
- `SpecpointProjectID` - Project GUID
- `extension_AccountId` - Account ID

### Log Locations
See `SpecpointLog.cs` for configured path.

---

**Quick Reference Version:** 1.0
**Last Updated:** January 15, 2026
**Project Version:** 1.6.0.0
