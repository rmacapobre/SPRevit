using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
// AdWindows.dll
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MessageBox = System.Windows.MessageBox;
using RibbonButton = Autodesk.Windows.RibbonButton;
using RibbonPanel = Autodesk.Windows.RibbonPanel;

namespace Specpoint.Revit2026
{
    public class SpecpointEnvironment
    {
        private const string devLoginPage = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_SignIn_PrePrd_TOTP&client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&nonce=defaultNonce&redirect_uri=https%3A%2F%2Fjwt.ms&scope=openid&response_type=id_token&prompt=login&source=plugin";
        private const string devWorkspace = @"https://knowledgepointclientdev.azureedge.net/projects/{0}/workspace/{1}";
        private const string devProjects = @"https://knowledgepointclientdev.azureedge.net/projects";
        private const string devSpecpointAPI = @"https://knowledgepointapidev.azurewebsites.net/";
        private const string devForgotPassword = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/B2C_1A_PasswordReset_PrePrd/oauth2/v2.0/authorize?client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&response_type=id_token&scope=openid&redirect_uri=https%3a%2f%2fspecpoint-staging.engdeltek.com%2fbounce&response_mode=form_post&state=%7C1";
        private const string devWebAppLoginPage = @"https://www.google.com";

        private const string stgLoginPage = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_SignIn_PrePrd_TOTP&client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&nonce=defaultNonce&redirect_uri=https%3A%2F%2Fjwt.ms&scope=openid&response_type=id_token&prompt=login&source=plugin";
        private const string stgWorkspace = @"https://ae-specpoint-staging.engdeltek.com/projects/{0}/workspace/{1}";
        private const string stgProjects = @"https://ae-specpoint-staging.engdeltek.com/projects";
        private const string stgSpecpointAPI = "https://api-specpoint-staging.engdeltek.com/";
        private const string stgForgotPassword = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/B2C_1A_PasswordReset_PrePrd/oauth2/v2.0/authorize?client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&response_type=id_token&scope=openid&redirect_uri=https%3a%2f%2fspecpoint-staging.engdeltek.com%2fbounce&response_mode=form_post&state=%7C1";
        private const string stgWebAppLoginPage = @"https://ae-specpoint-staging.engdeltek.com";

        private const string prodLoginPage = @"https://specpointb2c.b2clogin.com/specpointb2c.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_SignIn_Prd_TOTP&client_id=d6e622b7-993d-4a14-af96-1c33ce5f6fb9&nonce=defaultNonce&redirect_uri=https%3A%2F%2Fjwt.ms&scope=openid&response_type=id_token&prompt=login&source=plugin";
        private const string prodWorkspace = @"https://ae-specpoint.mydeltek.com/projects/{0}/workspace/{1}";
        private const string prodProjects = @"https://ae-specpoint.mydeltek.com/projects";
        private const string prodSpecpointAPI = "https://api-specpoint.mydeltek.com/";
        private const string prodForgotPassword = @"https://specpointb2c.b2clogin.com/specpointb2c.onmicrosoft.com/B2C_1A_PasswordReset_Prd/oauth2/v2.0/authorize?client_id=d6e622b7-993d-4a14-af96-1c33ce5f6fb9&response_type=id_token&scope=openid&redirect_uri=https%3a%2f%2fspecpoint.mydeltek.com%2fbounce&response_mode=form_post&state=%7C1";
        private const string prodWebAppLoginPage = @"https://ae-specpoint.mydeltek.com";

        private const string qeLoginPage = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/oauth2/v2.0/authorize?p=B2C_1A_SignIn_PrePrd_TOTP&client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&nonce=defaultNonce&redirect_uri=https%3A%2F%2Fjwt.ms&scope=openid&response_type=id_token&prompt=login&source=plugin";
        private const string qeWorkspace = @"https://ae-specpoint-qe1.engdeltek.com/projects/{0}/workspace/{1}";
        private const string qeProjects = @"https://ae-specpoint-qe1.engdeltek.com/projects";
        private const string qeSpecpointAPI = @"https://api-specpoint-qe1.engdeltek.com";
        private const string qeForgotPassword = @"https://specpointdevb2c.b2clogin.com/specpointdevb2c.onmicrosoft.com/B2C_1A_PasswordReset_PrePrd/oauth2/v2.0/authorize?client_id=a7486ba9-7308-4617-90aa-6cc45171ad68&response_type=id_token&scope=openid&redirect_uri=https%3a%2f%2fspecpoint-qe1.engdeltek.com%2fbounce&response_mode=form_post&state=%7C1";
        private const string qeWebAppLoginPage = @"https://specpoint-qe1.engdeltek.com/login";

        private string env;

        public SpecpointEnvironment()
        {
            env = SpecpointRegistry.GetValue("env");
            if (env == "")
            {
                env = "p";
            }
        }

        // This is the login page used in the Plugin
        public string LoginPage
        {
            get
            {
                if (env == "d") return devLoginPage;
                if (env == "s") return stgLoginPage;
                if (env == "p") return prodLoginPage;
                if (env == "q") return qeLoginPage;

                return prodLoginPage;
            }
        }

        // This is the login page used in the Web App
        public string WebAppLoginPage
        {
            get
            {
                if (env == "d") return devWebAppLoginPage;
                if (env == "s") return stgWebAppLoginPage;
                if (env == "p") return prodWebAppLoginPage;
                if (env == "q") return qeWebAppLoginPage;

                return prodWebAppLoginPage;
            }
        }

        public string Workspace
        {
            get
            {
                if (env == "d") return devWorkspace;
                if (env == "s") return stgWorkspace;
                if (env == "p") return prodWorkspace;
                if (env == "q") return qeWorkspace;

                return prodWorkspace;
            }
        }

        public string Projects
        {
            get
            {
                if (env == "d") return devProjects;
                if (env == "s") return stgProjects;
                if (env == "p") return prodProjects;
                if (env == "q") return qeProjects;

                return prodProjects;
            }
        }

        public string SpecpointAPI
        {
            get
            {
                if (env == "d") return devSpecpointAPI;
                if (env == "s") return stgSpecpointAPI;
                if (env == "p") return prodSpecpointAPI;
                if (env == "q") return qeSpecpointAPI;

                return prodSpecpointAPI;
            }
        }

        public string ForgotPassword
        {
            get
            {
                if (env == "d") return devForgotPassword;
                if (env == "s") return stgForgotPassword;
                if (env == "p") return prodForgotPassword;
                if (env == "q") return qeForgotPassword;

                return prodForgotPassword;
            }
        }

        public string HelpURL
        {
            get
            {
                return @"https://help.deltek.com/products/Specpoint/AE/Integrating_with_Revit.html";
            }
        }
    }


    /// <summary>
    /// Provides static objects that exist for the lifetime of the CAD plug-in.
    /// </summary>
    public static class Globals
    {
        public static UIApplication UIApp { get; set; }
        public static ExternalEvent LoginExternalEvent { get; set; }
        public static ExternalEvent DeferredSaveEvent { get; set; }
        public static DeferredSaveHandler DeferredSaveHandler { get; set; }
        public static string Token { get; set; }
        public static bool TokenExpired { get; set; }

        public static bool NeedsRibbonRefresh { get; set; }

        public static string LoginUser { get; set; }
        public static string extension_AccountId { get; set; }
        public static string SpecpointKey { get { return "SOFTWARE\\Specpoint"; } }
        public static string SpecpointProjectID { get; set; }
        public static SpecpointLog Log { get; set; }
        public static string RevitVersion = typeof(Globals).Namespace.Replace("Specpoint.", "");
        public static Firm SystemFirm { get; set; }
        public static Firm CurrentUserFirm { get; set; }
        public static CurrentUserActiveFirm CurrentUserActiveFirm { get; set; }
        public static GetProjectElementKeynotes SpecpointKeynotes { get; set; }
        public static bool UserCanAccessProject { get; set; }
        public static bool LoginNavigating { get; set; }

        // System Firm
        public const string SystemFirmID = "4e3d44c1-ec50-40c3-8783-1983a905a8a9";

        // Single instance ModelValidation form
        public static ModelValidationForm modelValidationForm;

        public static InsertSubassembliesProgress insertSubAssembliesProgress;

        // Revit elements (that needs to be updated after selecting new codes and desc)
        public static RevitElements revitElements;

        // Specpoint node that are added to the Specpoint project
        public static Dictionary<string, ProjectElementNode> mapAddedToProject;

        // Get all assembly and family nodes added to the project
        public static GetProjectElementNodes nodes;

        // Get all assembly and family nodes NOT added to the project
        public static GetProjectElementNodes otherNodes;

        // Get all assembly and family nodes by category added to the project
        public static NodesByMultipleCategory nodesByCategory;

        // Specpoint categories 
        public static SpecpointCategories specpointCategories;

        // Revit categories
        public static RevitCategories revitCategories;

        // Specpoint only categories and assemblies (Added/Not Added to Project)
        public static Dictionary<string, SortedDictionary<string, Assembly>> specpointOnlyCategories;
        public static Dictionary<string, SortedDictionary<string, Assembly>> otherSpecpointOnlyCategories;
        public static Dictionary<string, SortedDictionary<string, ProjectElementNode>> specpointOnlyCategoriesByNode;
        public static Dictionary<string, SortedDictionary<string, ProjectElementNode>> otherSpecpointOnlyCategoriesByNode;

        // Families under specpoint only categories and assemblies (Added/Not Added to Project)
        public static Dictionary<string, Dictionary<string, Assembly>> specpointOnlyFamilies;
        public static Dictionary<string, Dictionary<string, Assembly>> otherSpecpointOnlyFamilies;

        // List of last SubAssemblies
        public static SortedDictionary<string, Assembly> lastSubAssemblies;

        // Specpoint divisions. Used in Keynotes Manager
        public static Divisions KeynoteDivisions;
        public static Dictionary<string, GetProjectElementNodes> KeynoteDivisionsByCategory;

        // Specpoint assemblies. Used in Keynotes Manager
        public static GetProjectElementNodes KeynoteAssemblies;
        public static Dictionary<string, GetProjectElementNodes> KeynoteAssembliesByCategory;

        // List of assemblies to add to Specpoint Project during Update Sync
        public static List<ProjectElementNode> nodesToAdd;

        // List of assemblies that are not last subassemblies
        public static SortedDictionary<string, Assembly> otherAssemblies;
        public static SortedDictionary<string, ProjectElementNode> otherAssembliesByNode;

        // Filter by category
        public static List<string> includedCategories;

        // Filter All categories
        public const string AllCategoriesFilter = "Architecture|MEP|Structure";

        // Map between Revit and Specpoint category IDs
        public static RevitSpecpointCategoryMap mapCategoryIDs;

        public static PushButtonData btnCurrentUser;
        public static PushButtonData btnCurrentProject;

        public static void ResetGlobals()
        {
            Globals.revitElements = null;
            Globals.nodes = null;
            Globals.nodesByCategory = null;
            Globals.otherNodes = null;
            Globals.lastSubAssemblies = null;
            Globals.otherAssemblies = null;
            Globals.includedCategories = null;
            Globals.mapCategoryIDs = null;
        }

        public static void EnablePanelButton(string name, bool enable = true)
        {
            // Get the Revit main window to access its dispatcher
            var revitWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var window = HwndSource.FromHwnd(revitWindow)?.RootVisual as Window;

            // Ensure we're on the UI thread
            if (window != null && !window.Dispatcher.CheckAccess())
            {
                window.Dispatcher.Invoke(() => EnablePanelButton(name, enable));
                return;
            }

            string tabName = "Specpoint";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            string panelName = $"Specpoint {version.ToString()}";

            RibbonControl ribbon = ComponentManager.Ribbon;
            RibbonTab specpointTab = null;
            RibbonPanel specpointPanel = null;
            RibbonButton targetButton = null;
            foreach (var tab in ribbon.Tabs)
            {
                if (tab.Id == tabName)
                {
                    specpointTab = tab;
                    break;
                }
            }

            if (specpointTab != null)
            {
                foreach (var panel in specpointTab.Panels)
                {
                    if (panel.Source.Id.StartsWith("CustomCtrl_%Specpoint%" + panelName))
                    {
                        specpointPanel = panel;
                        break;
                    }
                }
            }

            if (specpointPanel != null)
            {
                foreach (var item in specpointPanel.Source.Items)
                {
                    string button = string.Format("CustomCtrl_%CustomCtrl_%Specpoint%{0}%{1}", panelName, name);

                    RibbonRowPanel rowPanel = item as RibbonRowPanel;
                    if (rowPanel == null) continue;

                    foreach (var rowPanelItem in rowPanel.Items)
                    {
                        if (!rowPanelItem.Id.Contains(button)) continue;

                        targetButton = rowPanelItem as RibbonButton;
                        if (targetButton == null) continue;

                        targetButton.IsEnabled = enable;
                        break;

                    }
                }
            }
        }

        /// <summary>
        /// Initializes the Specpoint project settings asynchronously.
        /// Uses fire-and-forget pattern to avoid blocking callers.
        /// Safe to call from any context (commands, forms, initialization).
        /// </summary>
        public static void SetProject()
        {
            if (!Specpoint.Revit2026.LoginUser.IsLoggedIn()) return;
            if (string.IsNullOrEmpty(Globals.SpecpointProjectID)) return;

            // Use fire-and-forget pattern with proper error handling
            Task.Run(async () =>
            {
                try
                {
                    // Get the Specpoint project name
                    Query query = new Query("Globals.SetProject");
                    var result = await query.getProject(Globals.SpecpointProjectID);
                    if (result == null) return;
                    if (result.getProject == null) return;
                    if (!Specpoint.Revit2026.LoginUser.IsLoggedIn()) return;

                    // Verify if project exists
                    bool projectExists = false;
                    var gp = await query.getProjects();
                    if (gp?.getProjects?.projects == null) return;

                    foreach (var project in gp.getProjects.projects)
                    {
                        if (project.name == result.getProject.name &&
                            project.groupName == result.getProject.groupName)
                        {
                            projectExists = true;
                            break;
                        }
                    }

                    if (projectExists == true)
                    {
                        ProjectItem selectedProject = new ProjectItem()
                        {
                            id = result.getProject.id,
                            name = result.getProject.name,
                            groupName = result.getProject.groupName
                        };

                        SpecpointRegistry.SetValue("ProjectId", selectedProject.id);
                        SpecpointRegistry.SetValue("ProjectName", selectedProject.name);
                        SpecpointRegistry.SetValue("UserCanAccessProject", "1");

                        // Use async-safe button update
                        SetButtonTextAsync("Current Project", selectedProject.name);
                        Globals.NeedsRibbonRefresh = true;
                    }
                    else
                    {
                        SpecpointRegistry.SetValue("ProjectId", "");
                        SpecpointRegistry.SetValue("ProjectName", "");
                        SpecpointRegistry.SetValue("UserCanAccessProject", "0");

                        // Use async-safe button update
                        SetButtonTextAsync("Current Project", "No current project");
                    }
                }
                catch (TokenExpiredException)
                {
                    // Silent handling - just clear the project link
                    // This runs during plugin initialization, so no user notification needed
                    Globals.SpecpointProjectID = null;
                    SpecpointRegistry.SetValue("ProjectId", "");
                    SpecpointRegistry.SetValue("ProjectName", "");
                    SpecpointRegistry.SetValue("UserCanAccessProject", "0");

                    // Use async-safe button update
                    SetButtonTextAsync("Current Project", "No current project");
                }
                catch (Exception ex)
                {
                    // Catch any other exceptions to prevent unobserved task exceptions
                    Log.Write($"ERROR in SetProject: {ex.GetType().Name} - {ex.Message}");
                    Log.Write($"Stack: {ex.StackTrace}");
                }
            });
        }

        public static void OnTokenExpired()
        {
            Globals.TokenExpired = true;
            MessageBox.Show("The Specpoint token has expired.\nPlease relogin.", "Specpoint",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            // Use ExternalEvent to trigger login outside of command execution context
            if (Globals.LoginExternalEvent == null)
            {
                Globals.Log.Write("WARNING: LoginExternalEvent is not initialized. Cannot trigger re-login.");
                return;
            }

            try
            {
                // Raise the external event to trigger login
                // This will execute outside the current command context, avoiding the PostCommand error
                Globals.LoginExternalEvent.Raise();
            }
            catch (Exception ex)
            {
                Globals.Log.Write($"ERROR in OnTokenExpired: {ex.Message}");
                Globals.Log.GraphQL(ex);
            }
        }

        public static void RefreshButtons()
        {
            var revitWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var window = HwndSource.FromHwnd(revitWindow)?.RootVisual as Window;

            if (window != null && !window.Dispatcher.CheckAccess())
            {
                window.Dispatcher.Invoke(() => RefreshButtons());
                return;
            }

            try
            {
                string tabName = "Specpoint";
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                string panelName = $"Specpoint {version.ToString()}";

                RibbonControl ribbon = ComponentManager.Ribbon;
                RibbonTab specpointTab = null;
                RibbonPanel specpointPanel = null;

                foreach (var tab in ribbon.Tabs)
                {
                    if (tab.Id == tabName)
                    {
                        specpointTab = tab;
                        break;
                    }
                }

                if (specpointTab != null)
                {
                    foreach (var panel in specpointTab.Panels)
                    {
                        if (panel.Source.Id.StartsWith("CustomCtrl_%Specpoint%" + panelName))
                        {
                            specpointPanel = panel;
                            break;
                        }
                    }
                }

                if (specpointPanel != null)
                {
                    // Toggle visibility to force refresh
                    bool currentVisible = specpointPanel.IsVisible;
                    specpointPanel.IsVisible = false;

                    // Force dispatcher to process the change
                    System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Render,
                        new Action(() => { }));

                    specpointPanel.IsVisible = currentVisible;

                    // Refresh buttons within the panel
                    foreach (var item in specpointPanel.Source.Items)
                    {
                        RibbonRowPanel rowPanel = item as RibbonRowPanel;
                        if (rowPanel != null)
                        {
                            foreach (var button in rowPanel.Items)
                            {
                                RibbonButton ribbonButton = button as RibbonButton;
                                if (ribbonButton != null)
                                {
                                    // Toggle enabled state to force refresh
                                    bool wasEnabled = ribbonButton.IsEnabled;
                                    ribbonButton.IsEnabled = !wasEnabled;
                                    ribbonButton.IsEnabled = wasEnabled;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Log?.Write($"ERROR in RefreshButtons: {ex.Message}");
            }
        }

        /// <summary>
        /// Refreshes the Revit ribbon to update button enabled/disabled states.
        /// This forces Revit to re-evaluate IExternalCommandAvailability for all buttons.
        /// Call this after logging in/out, linking projects, or when buttons appear stuck.
        /// </summary>
        /// <param name="uiApp">The UIApplication instance</param>
        public static void RefreshRibbon()
        {
            try
            {
                if (Globals.UIApp?.ActiveUIDocument?.Document == null)
                    return;

                // Toggle selection to force ribbon update
                // Revit evaluates button availability when selection changes
                var selection = Globals.UIApp.ActiveUIDocument.Selection;
                var currentSelection = selection.GetElementIds();

                // Clear and restore selection to trigger ribbon refresh
                selection.SetElementIds(new List<Autodesk.Revit.DB.ElementId>());
                selection.SetElementIds(currentSelection);
            }
            catch (Exception ex)
            {
                Globals.Log.Write($"Error refreshing ribbon: {ex.Message}");
            }
        }

        public static void SetButtonText(string name, string text)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(name) || text == null)
                {
                    Log.Write("WARNING: SetButtonText called with null/empty parameters");
                    return;
                }

                // Check if UIApp is available - critical for Revit API context
                if (UIApp == null)
                {
                    Log.Write("WARNING: UIApp is null in SetButtonText, cannot update ribbon");
                    return;
                }

                // Get the Revit main window to access its dispatcher
                var revitWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                // Check if we have a valid window handle
                if (revitWindow == IntPtr.Zero)
                {
                    Log.Write("WARNING: Could not get Revit main window handle");
                    return;
                }

                var window = HwndSource.FromHwnd(revitWindow)?.RootVisual as Window;

                // Ensure we're on the UI thread - only if we have a valid WPF window
                if (window != null && !window.Dispatcher.CheckAccess())
                {
                    try
                    {
                        window.Dispatcher.Invoke(() => SetButtonText(name, text));
                        return;
                    }
                    catch (Exception dispatcherEx)
                    {
                        Log.Write($"ERROR: Dispatcher.Invoke failed in SetButtonText: {dispatcherEx.Message}");
                        // Continue with direct execution as fallback
                    }
                }

                string tabName = "Specpoint";
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;

                if (version == null)
                {
                    Log.Write("WARNING: Could not get assembly version");
                    return;
                }

                string panelName = $"Specpoint {version.ToString()}";

                // Get ribbon with null check
                RibbonControl ribbon = ComponentManager.Ribbon;
                if (ribbon == null)
                {
                    Log.Write("WARNING: ComponentManager.Ribbon is null in SetButtonText");
                    return;
                }

                // Find Specpoint tab
                RibbonTab specpointTab = null;
                if (ribbon.Tabs == null)
                {
                    Log.Write("WARNING: Ribbon.Tabs is null");
                    return;
                }

                foreach (var tab in ribbon.Tabs)
                {
                    if (tab != null && tab.Id == tabName)
                    {
                        specpointTab = tab;
                        break;
                    }
                }

                if (specpointTab == null)
                {
                    Log.Write($"WARNING: Could not find Specpoint tab in ribbon");
                    return;
                }

                // Find Specpoint panel
                RibbonPanel specpointPanel = null;
                if (specpointTab.Panels == null)
                {
                    Log.Write("WARNING: SpecpointTab.Panels is null");
                    return;
                }

                foreach (var panel in specpointTab.Panels)
                {
                    if (panel?.Source?.Id != null &&
                        panel.Source.Id.StartsWith("CustomCtrl_%Specpoint%" + panelName))
                    {
                        specpointPanel = panel;
                        break;
                    }
                }

                if (specpointPanel == null)
                {
                    Log.Write($"WARNING: Could not find Specpoint panel '{panelName}' in ribbon");
                    return;
                }

                // Find and update target button
                if (specpointPanel.Source?.Items == null)
                {
                    Log.Write("WARNING: SpecpointPanel.Source.Items is null");
                    return;
                }

                RibbonButton targetButton = null;
                foreach (var item in specpointPanel.Source.Items)
                {
                    if (item == null) continue;

                    string button = string.Format("CustomCtrl_%CustomCtrl_%Specpoint%{0}%{1}", panelName, name);

                    RibbonRowPanel rowPanel = item as RibbonRowPanel;
                    if (rowPanel?.Items == null) continue;

                    foreach (var rowPanelItem in rowPanel.Items)
                    {
                        if (rowPanelItem?.Id == null) continue;
                        if (!rowPanelItem.Id.Contains(button)) continue;

                        targetButton = rowPanelItem as RibbonButton;
                        if (targetButton == null) continue;

                        // Update button properties safely
                        try
                        {
                            targetButton.IsEnabled = true;
                            targetButton.IsActive = true;
                            targetButton.IsActive = false;
                            targetButton.Text = text;
                        }
                        catch (Exception buttonEx)
                        {
                            Log.Write($"ERROR: Failed to update button properties: {buttonEx.Message}");
                        }
                        break;
                    }

                    if (targetButton != null)
                        break;
                }

                if (targetButton == null)
                {
                    Log.Write($"WARNING: Could not find button '{name}' in panel");
                }
            }
            catch (System.Runtime.InteropServices.SEHException sehEx)
            {
                Log.Write($"[SEH EXCEPTION in SetButtonText] Code: 0x{sehEx.ErrorCode:X8}, Message: {sehEx.Message}");
                Log.Write($"Stack: {sehEx.StackTrace}");
                // Don't rethrow - this is a UI update, not critical to functionality
            }
            catch (System.AccessViolationException avEx)
            {
                Log.Write($"[ACCESS VIOLATION in SetButtonText] {avEx.Message}");
                Log.Write($"Stack: {avEx.StackTrace}");
                // Don't rethrow - this is a UI update, not critical to functionality
            }
            catch (Exception ex)
            {
                Log.Write($"ERROR in SetButtonText: {ex.GetType().Name} - {ex.Message}");
                Log.Write($"Stack: {ex.StackTrace}");
                // Don't rethrow - button text update failures should not crash the application
            }
        }

        /// <summary>
        /// Safely updates button text from async context using fire-and-forget pattern.
        /// This method is safe to call from async methods, background threads, or form load events.
        /// </summary>
        /// <param name="name">Button name to update</param>
        /// <param name="text">New text for the button</param>
        public static void SetButtonTextAsync(string name, string text)
        {
            // Use fire-and-forget pattern with proper error handling
            // This ensures we don't block async operations or cause threading issues
            Task.Run(() =>
            {
                try
                {
                    // Small delay to ensure Revit UI is ready
                    Thread.Sleep(100);

                    // Call the thread-safe SetButtonText method
                    SetButtonText(name, text);
                }
                catch (Exception ex)
                {
                    Log.Write($"ERROR in SetButtonTextAsync: {ex.GetType().Name} - {ex.Message}");
                    // Don't rethrow - this is best-effort UI update
                }
            });
        }

        /// <summary>
        /// Safely updates ribbon state from async context.
        /// Combines button text update with registry persistence.
        /// </summary>
        /// <param name="projectId">Project ID to store in registry</param>
        /// <param name="projectName">Project name to store in registry and display</param>
        public static void UpdateProjectButtonAsync(string projectId, string projectName)
        {
            try
            {
                // Update registry synchronously (safe from any thread)
                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(projectName))
                {
                    SpecpointRegistry.SetValue("ProjectId", "");
                    SpecpointRegistry.SetValue("ProjectName", "");
                    SetButtonTextAsync("Current Project", "No current project");
                }
                else
                {
                    SpecpointRegistry.SetValue("ProjectId", projectId);
                    SpecpointRegistry.SetValue("ProjectName", projectName);
                    SetButtonTextAsync("Current Project", projectName);
                }
            }
            catch (Exception ex)
            {
                Log.Write($"ERROR in UpdateProjectButtonAsync: {ex.Message}");
            }
        }

        public static async ValueTask<bool> AddAssemblyToProject(string projectId, ProjectElementNode node)
        {
            try
            {
                string title = ""; ;
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
            catch (TokenExpiredException)
            {
                Globals.OnTokenExpired();

                return false;
            }
        }

        public static string ExtractAssemblyCode(string value)
        {
            try
            {
                string name = value;
                name = name.Replace("|", " - ");

                // Extract Specpoint Assembly Code
                string assemblyCode = "";
                if (!string.IsNullOrEmpty(name))
                {
                    int dash = name.IndexOf("-");
                    if (dash > 0)
                    {
                        assemblyCode = name.Substring(0, dash).Trim();
                    }
                }

                return assemblyCode;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return string.Empty;
        }

        public static string ExtractAssemblyDescription(string value)
        {
            try
            {
                string name = value;
                name = name.Replace("|", " - ");

                // Extract Specpoint Assembly Decription
                string assemblyDescription = "";
                if (!string.IsNullOrEmpty(name))
                {
                    int dash = name.IndexOf("-");
                    if (dash > 0)
                    {
                        assemblyDescription = name.Substring(dash + 1).Trim();
                    }
                }

                return assemblyDescription;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return string.Empty;
        }

        public static void ProcessNodesToAddInThread(string projectId)
        {
            // Show confirmation form before processing
            if (Globals.nodesToAdd != null && Globals.nodesToAdd.Count > 0)
            {
                DialogResult result = DialogResult.Cancel;

                // Show form on UI thread
                var revitWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                var window = HwndSource.FromHwnd(revitWindow)?.RootVisual as Window;

                if (window != null)
                {
                    window.Dispatcher.Invoke(() =>
                    {
                        UpdateSpecpointForm confirmForm = new UpdateSpecpointForm(Globals.nodesToAdd);
                        result = confirmForm.ShowDialog();
                    });
                }
                else
                {
                    // Fallback if window is not available
                    UpdateSpecpointForm confirmForm = new UpdateSpecpointForm(Globals.nodesToAdd);
                    result = confirmForm.ShowDialog();
                }

                // If user cancelled, don't proceed with processing
                if (result != DialogResult.OK)
                {
                    Globals.Log.Write("User cancelled node addition.");
                    return;
                }
            }

            Thread processingThread = new Thread(() =>
            {
                try
                {
                    SpecpointRegistry.SetValue("UpdateSync", "1");

                    bool cancelled = false;
                    if (Globals.nodesToAdd != null && Globals.nodesToAdd.Count > 0)
                    {
                        foreach (var nodeToAdd in Globals.nodesToAdd)
                        {
                            string updateSync = SpecpointRegistry.GetValue("UpdateSync");
                            cancelled = (updateSync != "1");

                            if (cancelled)
                            {
                                // If Update Sync is cancelled, stop processing further nodes
                                Globals.Log.Write("Update Sync cancelled. Stopping node processing.");
                                break;
                            }

                            // Call AddAssemblyToProject synchronously within the thread
                            bool result = Globals.AddAssemblyToProject(projectId, nodeToAdd).GetAwaiter().GetResult();

                            if (!result)
                            {
                                Globals.Log.Write(string.Format("Failed to add node: {0}", nodeToAdd.treePath));
                            }
                        }

                        Globals.nodesToAdd.Clear();
                        Globals.Log.Write("ProcessNodesToAddInThread Done");

                        SpecpointRegistry.SetValue("UpdateSync", cancelled ? "0" : "2");

                        // Enable panel Update button when done
                        var revitWindow = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                        var window = HwndSource.FromHwnd(revitWindow)?.RootVisual as Window;
                        window?.Dispatcher.Invoke(() =>
                        {
                            Globals.EnablePanelButton("Update", true);

                        });

                        // Hide update sync progress when done
                        if (Globals.modelValidationForm != null &&
                            Globals.modelValidationForm.Visible == true)
                        {
                            if (cancelled)
                            {
                                Globals.modelValidationForm.Invoke((Action)(() =>
                                {
                                    Globals.modelValidationForm.ShowUpdateSyncControls(false);
                                    MessageBox.Show("Update Sync was cancelled.\nRefresh Data to stay up to date.", "Specpoint", MessageBoxButton.OK, MessageBoxImage.Information);
                                }));
                            }
                            else
                            {
                                Globals.modelValidationForm.Invoke((Action)(() =>
                                {
                                    Globals.modelValidationForm.ShowUpdateSyncControls(false);
                                    MessageBox.Show("Update Sync is complete.\nRefresh Data to stay up to date.", "Specpoint", MessageBoxButton.OK, MessageBoxImage.Information);
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string func = MethodBase.GetCurrentMethod().Name;
                    string msg = string.Format("{0} {1}", func, ex.Message);
                    Globals.Log.Write(msg);
                }
            });

            processingThread.IsBackground = true;
            processingThread.Start();
        }

        // Get node of element type assembly only from Revit Category
        public static async Task<GetProjectElementNodes> GetSpecpointAssemblies(string projectId,
            string category, bool onlyElementsAssignedToProject)
        {
            int currentCount = 0;
            Dictionary<int, GetProjectElementNodes> finalResults = new Dictionary<int, GetProjectElementNodes>();

            int totalCount = 0;
            GetProjectElementNodes results = null;

            do
            {
                // If BIMCategoryId is not available
                Assembly revitProjectCategory = new Assembly();
                if (revitProjectCategory != null &&
                    revitProjectCategory.BIMCategoryId == 0 &&
                    Globals.specpointCategories.ContainsKey(category))
                {
                    // Get it from list of Specpoint categories
                    ProductFamilyCategory pfc = Globals.specpointCategories[category];
                    revitProjectCategory.BIMCategoryId = Convert.ToInt32(pfc.revitCategory);
                }

                // Get specific project elements given the Specpoint category ID
                GetProjectElementNodesInput specific = new GetProjectElementNodesInput()
                {
                    projectId = projectId,
                    categoryId = Globals.mapCategoryIDs[revitProjectCategory.BIMCategoryId.ToString()],
                    skip = currentCount,
                    projectElementType = new List<ProjectElementType>() {
                        ProjectElementType.ASSEMBLY,
                        ProjectElementType.PRODUCTFAMILY
                    },
                    limit = 2000
                };

                int setAssemblyCodeMode = SetAssemblyCodeMode.AddedToProject;
                string mode = SpecpointRegistry.GetValue("SetAssemblyCodeMode");
                if (!string.IsNullOrEmpty(mode))
                {
                    setAssemblyCodeMode = Convert.ToInt32(mode);
                }

                if (specific != null && revitProjectCategory != null)
                {
                    specific.onlyElementsAssignedToProject = onlyElementsAssignedToProject;
                }

                Query query = new Query("Globals.GetSpecpointAssemblies");
                results = await query.getProjectElementsNodes(specific);

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
            results = ConsolidateResults(finalResults);

            return results;
        }

        public static GetProjectElementNodes ConsolidateResults(Dictionary<int, GetProjectElementNodes> finalResults)
        {
            try
            {
                GetProjectElementNodes results = new GetProjectElementNodes();
                foreach (var r in finalResults)
                {
                    if (r.Value != null)
                    {
                        results.Add(r.Value.ListNodes);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(ex);
            }

            return null;
        }

        public static bool DoesAssemblyCodeExist(string code,
            List<ProjectElementNode> nodes, out string desc)
        {
            foreach (var node in nodes)
            {
                string nodeCode = Globals.ExtractAssemblyCode(node.text);
                if (nodeCode == code)
                {
                    desc = Globals.ExtractAssemblyDescription(node.text);
                    return true;
                }
            }

            desc = "";
            return false;
        }

        public static bool IsLastSubassembly(Dictionary<string, SortedDictionary<string, Assembly>> categories, string assemblyCode)
        {
            if (string.IsNullOrEmpty(assemblyCode))
                return false;

            if (categories == null ||
                categories.Count == 0)
                return false;

            return categories
                .Values
                .Any(dict => dict.TryGetValue(assemblyCode, out var assembly) && assembly.IsLastSubassembly);
        }

        public static Assembly FindAssemblyByCode(Dictionary<string, SortedDictionary<string, Assembly>> categories, 
            string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            if (categories == null ||
                categories.Count == 0)
                return null;

            foreach (var category in categories.Keys)
            {
                if (categories[category] != null &&
                    categories[category].ContainsKey(code))
                {
                    var assembly = categories[category][code];
                    return new Assembly
                    {
                        AssemblyCode = assembly.AssemblyCode,
                        AssemblyDescription = assembly.AssemblyDescription
                    };
                }
            }            

            return null; // not found in any category
        }

        public static Assembly FindAssemblyByCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            Assembly assembly = FindAssemblyByCode(Globals.specpointOnlyCategories, code);
            if (assembly != null)
                return assembly;

            assembly = FindAssemblyByCode(Globals.otherSpecpointOnlyCategories, code);
            if (assembly != null)
                return assembly;

            return null; // not found in any category
        }

        public static Assembly FindAssemblyByDescription(string desc)
        {
            if (string.IsNullOrEmpty(desc))
                return null;

            if (Globals.specpointOnlyCategories == null ||
                Globals.specpointOnlyCategories.Count == 0)
                return null;

            foreach (var category in Globals.specpointOnlyCategories.Keys)
            {
                foreach (var node in Globals.specpointOnlyCategories[category]
                             .Where(n => n.Value.AssemblyDescription == desc))
                {
                    return new Assembly
                    {
                        AssemblyCode = node.Value.AssemblyCode,
                        AssemblyDescription = node.Value.AssemblyDescription
                    };
                }
            }

            if (Globals.otherSpecpointOnlyCategories == null ||
                Globals.otherSpecpointOnlyCategories.Count == 0)
                return null;

            foreach (var category in Globals.otherSpecpointOnlyCategories.Keys)
            {
                foreach (var node in Globals.otherSpecpointOnlyCategories[category]
                             .Where(n => n.Value.AssemblyDescription == desc))
                {
                    return new Assembly
                    {
                        AssemblyCode = node.Value.AssemblyCode,
                        AssemblyDescription = node.Value.AssemblyDescription
                    };
                }
            }

            return null; // not found in any category
        }

        public static async Task LoadAssemblies(string projectId, List<string> categories)
        {
            if (string.IsNullOrEmpty(projectId)) return;
            if (categories == null || categories.Count == 0) return;

            // Load once
            if (Globals.specpointOnlyCategories != null) return;

            Globals.specpointOnlyCategories = new Dictionary<string, SortedDictionary<string, Assembly>>();
            Globals.specpointOnlyCategoriesByNode = new Dictionary<string, SortedDictionary<string, ProjectElementNode>>();
            Globals.otherSpecpointOnlyCategories = new Dictionary<string, SortedDictionary<string, Assembly>>();
            Globals.otherSpecpointOnlyCategoriesByNode = new Dictionary<string, SortedDictionary<string, ProjectElementNode>>();
            Globals.specpointOnlyFamilies = new Dictionary<string, Dictionary<string, Assembly>>();
            Globals.otherSpecpointOnlyFamilies = new Dictionary<string, Dictionary<string, Assembly>>();

            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category)) continue;

                // Assemblies added to project
                var nodes = await Globals.GetSpecpointAssemblies(projectId, category, true);
                Globals.ConvertToDictionary(nodes, out var assemblies, out var assembliesByNode);
                Globals.specpointOnlyCategories[category] = assemblies;
                Globals.specpointOnlyCategoriesByNode[category] = assembliesByNode;

                Globals.specpointOnlyFamilies[category] = Globals.ConvertToDictionary(category, nodes.ListNodes);

                // Assemblies NOT added to project
                nodes = await Globals.GetSpecpointAssemblies(projectId, category, false);
                Globals.ConvertToDictionary(nodes, out assemblies, out assembliesByNode);
                Globals.otherSpecpointOnlyCategories[category] = assemblies;
                Globals.otherSpecpointOnlyCategoriesByNode[category] = assembliesByNode;
                
                Globals.otherSpecpointOnlyFamilies[category] = Globals.ConvertToDictionary(category, nodes.ListNodes);

            }
        }

        public static Dictionary<string, Assembly> ConvertToDictionary(string categoryName, List<ProjectElementNode> nodes)
        {
            if (categoryName == "") return null;
            Dictionary<string, Assembly> result = new Dictionary<string, Assembly>();

            foreach (ProjectElementNode node in nodes)
            {
                // Filter out none families
                if (node.elementType != ProjectElementType.PRODUCTFAMILY) continue;
                if (!node.treePath.Contains("|")) continue;

                // Get node of all element types (assembly, family,.product type) from Revit Category
                // For Specpoint only categories, use Generic Models
                Category c = Globals.revitCategories.ContainsKey(categoryName) ?
                    Globals.revitCategories[categoryName] :
                    Globals.revitCategories["Generic Models"];

                char[] pipe = { '|' };
                string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    if (list.Length == 8)
                    {
                        Assembly row = new Assembly()
                        {
                            BIMCategory = categoryName,
                            BIMCategoryId = c.Id.Value,
                            RevitFamily = list[1],
                            RevitType = list[3],
                            AssemblyCode = list[4],
                            AssemblyDescription = list[5],
                            SpecpointFamilyNumber = list[6],
                            SpecpointFamily = list[7],
                            IsLastSubassembly = node.isLastSubAssembly,
                            IsInProject = node.isInProject

                        };

                        result[node.treePath] = row;
                    }
                    else if (list.Length == 6)
                    {
                        Assembly row = new Assembly()
                        {
                            BIMCategory = categoryName,
                            BIMCategoryId = c.Id.Value,
                            RevitFamily = list[1],
                            AssemblyCode = list[2],
                            AssemblyDescription = list[3],
                            SpecpointFamilyNumber = list[4],
                            SpecpointFamily = list[5],
                            IsLastSubassembly = node.isLastSubAssembly,
                            IsInProject = node.isInProject
                        };

                        result[node.treePath] = row;
                    }
                }                
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(ex);
                }
            }

            return result;
        }

        public static void ConvertToDictionary(GetProjectElementNodes source,
            out SortedDictionary<string, Assembly> assemblies,
            out SortedDictionary<string, ProjectElementNode> assembliesByNode)
        {
            assemblies = new SortedDictionary<string, Assembly>();
            assembliesByNode = new SortedDictionary<string, ProjectElementNode>();

            foreach (var node in source.ListNodes)
            {
                // Filter out none assemblies
                if (node.elementType != ProjectElementType.ASSEMBLY) continue;
                if (!node.treePath.Contains("|")) continue;

                char[] pipe = { '|' };
                string[] list = node.treePath.Split(pipe, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    if (list.Length == 6)
                    {
                        Assembly row = new Assembly()
                        {
                            RevitFamily = list[1],
                            RevitType = list[3],
                            AssemblyCode = list[4],
                            AssemblyDescription = list[5],
                            IsLastSubassembly = node.isLastSubAssembly,
                            IsInProject = node.isInProject

                        };

                        assemblies[list[4]] = row;
                        assembliesByNode[list[4]] = node;
                    }
                    else if (list.Length == 4)
                    {
                        Assembly row = new Assembly()
                        {
                            RevitFamily = list[1],
                            AssemblyCode = list[2],
                            AssemblyDescription = list[3],
                            IsLastSubassembly = node.isLastSubAssembly,
                            IsInProject = node.isInProject
                        };

                        assemblies[list[2]] = row;
                        assembliesByNode[list[2]] = node;
                    }
                    else if (list.Length == 2)
                    {
                        Assembly row = new Assembly()
                        {
                            AssemblyCode = list[0],
                            AssemblyDescription = list[1],
                            IsLastSubassembly = node.isLastSubAssembly,
                            IsInProject = node.isInProject
                        };

                        assemblies[list[0]] = row;
                        assembliesByNode[list[0]] = node;
                    }
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(ex);
                }
            }
        }
    }

    public static class ColumnName
    {
        public const string BIMCategory = "BIM Category";
        public const string RevitFamily = "Revit Family";
        public const string RevitType = "Revit Type";
        public const string AssemblyCode = "Assembly Code";
        public const string AssemblyDescription = "Assembly Description";
        public const string SpecpointFamilyNumber = "Specpoint Family Number";
        public const string SpecpointFamily = "Specpoint Family";
        public const string Status = "Coordination Status";
        public const string RevitTypeId = "Revit Type Id";
    }

    public static class CoordinationStatus
    {
        public const string Green = "Verified";

        // Assembly is added to Specpoint Project and Model Element but no Families exist
        public const string Yellow1 = "Add a Family under the Assembly in the Specpoint Project";

        // Assembly Value Exists on the Model Element and is valid in Specpoint, but it is not added to the Project
        public const string Yellow2 = "Add Assembly in the Specpoint Project";

        // No Assembly Code Assigned to Model Element
        public const string Red1 = "No Assembly Code associated to the Revit Family";

        // Assembly code does not exist in Specpoint
        public const string Red2 = "Assembly Code does not exist in the Specpoint library";

        // Assembly code is valid, but not the lowest level of the tree 
        public const string Red3 = "The last Sub-Assembly must be associated to the Revit Family";

        public const string Red4 = "Unassigned Specpoint Assembly: Link a Revit Model Element";
    }

    public static class ColumnIndex
    {
        public const int BIMCategory = 0;
        public const int RevitFamily = 1;
        public const int RevitType = 2;
        public const int AssemblyCode = 3;
        public const int AssemblyDescription = 4;
        public const int SpecpointFamilyNumber = 5;
        public const int SpecpointFamily = 6;
        public const int Status = 7;
        public const int RevitTypeId = 8;
    }

    public static class SetAssemblyCodeMode
    {
        public const int AddedToProject = 0;
        public const int AllProjectElements = 1;
    }

    public static class OurConvert
    {
        /// <summary>
        /// Converts an object to a Boolean.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to a Boolean or false if object is DBNull.</returns>
        public static bool ToBoolean(object obj)
        {
            if (obj is DBNull)
            {
                return false;
            }
            else
            {
                return Convert.ToBoolean(obj);
            }
        }

        /// <summary>
        /// Converts an object to a Guid (it must be a string representation of a guid).
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to a Guid or Guid.Empty if object is DBNull.</returns>
        public static Guid ToGuid(Object obj)
        {
            if (obj is DBNull)
            {
                return Guid.Empty;
            }
            else
            {
                string str = OurConvert.ToString(obj);
                if (string.IsNullOrEmpty(str))
                {
                    return Guid.Empty;
                }

                return new Guid(str);
            }
        }

        /// <summary>
        /// Converts an object to an int.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to an integer or 0 if object is DBNull.</returns>
        public static int ToInt(object obj)
        {
            if (obj is DBNull)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(obj);
            }
        }

        /// <summary>
        /// Converts an object to an unsigned integer.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to a UInt or 0 if object is DBNull.</returns>
        public static uint ToUInt(object obj)
        {
            if (obj is DBNull)
            {
                return 0;
            }
            else
            {
                return Convert.ToUInt32(obj);
            }
        }

        /// <summary>
        /// Converts an object to a long integer.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to a long or 0 if object is DBNull.</returns>
        public static long ToLong(object obj)
        {
            if (obj is DBNull)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt64(obj);
            }
        }

        /// <summary>
        /// Converts an object to a string.
        /// </summary>
        /// <param name="obj">Object to convert.</param>
        /// <returns>Object converted to a string, or string.Empty if object is DBnull.</returns>
        public static string ToString(object obj)
        {
            if (obj is DBNull)
            {
                return string.Empty;
            }
            else
            {
                return Convert.ToString(obj);
            }
        }

        /// <summary>
        /// Converts the specified object to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Object converted to a DateTime, or <see cref="MinimumDateTime"/> if object is DBNull.</returns>
        public static DateTime ToDateTime(object obj)
        {
            if (obj is DBNull)
            {
                return MinimumDateTime;
            }
            else
            {
                return Convert.ToDateTime(obj);
            }
        }

        /// <summary>
        /// Gets the minimum MFC date time. Not the same as <see cref="DateTime.MinValue"/> because
        /// Win32 file dates cannot be set to that date.
        /// </summary>
        public static DateTime MinimumDateTime
        {
            get
            {
                return new DateTime(1970, 1, 1);
            }
        }
    }

    public static class LoginUser
    {
        public static void LogOff()
        {
            Globals.SetButtonText("Current User", "No current user");
            SpecpointRegistry.SetValue("exp", "");
            SpecpointRegistry.SetValue("extension_AccountId", "");
            SpecpointRegistry.SetValue("extension_AccountType", "");
            SpecpointRegistry.SetValue("extension_Role", "");
            SpecpointRegistry.SetValue("name", "");
            SpecpointRegistry.SetValue("email", "");
            SpecpointRegistry.SetValue("Token", "");
            SpecpointRegistry.SetValue("UpdateSync", "");
            Globals.LoginUser = "";

            Globals.TokenExpired = false;
            SpecpointRegistry.SetValue("TokenExpired", "0");


            if (Globals.modelValidationForm != null)
            {
                Globals.modelValidationForm.Close();
                Globals.modelValidationForm = null;
            }

            if (Globals.insertSubAssembliesProgress != null)
            {
                Globals.insertSubAssembliesProgress.Close();
                Globals.insertSubAssembliesProgress = null;
            }
        }

        public static bool IsLoggedIn()
        {
            // Get expiration time from registry
            string exp = SpecpointRegistry.GetValue("exp");

            if (string.IsNullOrEmpty(exp))
            {
                // Fallback: Check if Token exists (backward compatibility)
                // This may mean DocumentCompleted did not fire after login
                string token = SpecpointRegistry.GetValue("Token");
                string name = SpecpointRegistry.GetValue("name");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name))
                {
                    return true;
                }

                return false;
            }

            try
            {
                // Parse expiration time
                int gmtPosition = exp.IndexOf("GMT");
                if (gmtPosition > -1)
                {
                    // Remove GMT component
                    exp = exp.Substring(0, gmtPosition).Trim();
                }

                // Expected format: "Thu Oct 27 2022 12:55:35"
                string format = "ddd MMM dd yyyy HH:mm:ss";
                DateTime expiration = DateTime.ParseExact(exp, format, CultureInfo.InvariantCulture);

                // Check if token is expired
                if (expiration < DateTime.Now)
                {
                    Globals.Log.Write($"Token expired at {expiration}, current time is {DateTime.Now}");
                    return false;
                }

                // Token is still valid
                return true;
            }
            catch (Exception ex)
            {
                // Log parsing errors but don't block
                Globals.Log.Write($"ERROR parsing token expiration: {ex.Message}");
                return false;
            }
        }
    }
}
