#region Namespaces
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonItem = Autodesk.Revit.UI.RibbonItem;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;
#endregion


namespace Specpoint.Revit2026
{
    class PluginApp : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            Globals.Token = "";
            Globals.extension_AccountId = "";
            Globals.SpecpointProjectID = "";
            Globals.Log = SpecpointLog.Instance;

            // Initialize the ExternalEvent for token expiration handling
            LoginExternalEventHandler loginHandler = new LoginExternalEventHandler();
            Globals.LoginExternalEvent = ExternalEvent.Create(loginHandler);

            // Initialize the ExternalEvent for deferred document saves
            Globals.DeferredSaveHandler = new DeferredSaveHandler();
            Globals.DeferredSaveEvent = ExternalEvent.Create(Globals.DeferredSaveHandler);

            application.ViewActivated += new EventHandler<Autodesk.Revit.UI.Events.ViewActivatedEventArgs>(application_ViewActivated);
            application.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(application_Idling);

            string tabName = "Specpoint";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            string panelName = $"Specpoint {version.ToString()}";

            var cmRibbon = ComponentManager.Ribbon;
            var existingTab = cmRibbon.FindTab(tabName);

            if (existingTab == null)
            {
                application.CreateRibbonTab(tabName);

                RibbonPanel ribbon = null;
                // Add the Specpoint ribbon panel.
                try
                {
                    ribbon = application.CreateRibbonPanel(tabName, panelName);
                }
                catch (InvalidOperationException)
                {
                    ribbon = application.CreateRibbonPanel(tabName);
                }

                // Add "Account" button
                PushButtonData btnAccount = new PushButtonData(
                    "Account", "Account",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(AccountCommand).FullName)
                {
                    Image = GetResourceBitmap("Account.png"),
                    LargeImage = GetResourceBitmap("Account.png"),
                    ToolTip = "Allows you to login to Specpoint to authenticate you as a user.",
                    AvailabilityClassName = typeof(AlwaysAvailable).FullName
                };
                ribbon.AddItem(btnAccount);

                // Add "Link Project" button
                PushButtonData btnLinkProject = new PushButtonData(
                    "Link Project", "Link Project",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(LinkProjectCommand).FullName)
                {
                    Image = GetResourceBitmap("Link Project.png"),
                    LargeImage = GetResourceBitmap("Link Project.png"),
                    ToolTip = "Creates a link between a Specpoint project and a Revit project.",
                    AvailabilityClassName = typeof(AvailableIfGuest).FullName
                };
                ribbon.AddItem(btnLinkProject);

                // Add "Validation Schedule" button
                PushButtonData btnValidationSchedule = new PushButtonData(
                    "Validation Schedule", "Validation\nSchedule",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(ValidationScheduleCommand).FullName)
                {
                    Image = GetResourceBitmap("Validation Schedule.png"),
                    LargeImage = GetResourceBitmap("Validation Schedule.png"),
                    AvailabilityClassName = typeof(AvailableWhenProjectIsSet).FullName,
                    ToolTip = "Generates an editable report to view and adjust assembly codes and descriptions in bulk across the project or select categories."
                };
                ribbon.AddItem(btnValidationSchedule);

                // Add "Keynotes Manager" button.
                PushButtonData btnAssignKeynotes = new PushButtonData(
                    "Keynotes Manager", "Keynotes\nManager",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(KeynotesManagerCommand).FullName)
                {
                    Image = GetResourceBitmap("Keynotes Manager.png"),
                    LargeImage = GetResourceBitmap("Keynotes Manager.png"),
                    AvailabilityClassName = typeof(AvailableWhenProjectIsSet).FullName,
                    ToolTip = "View, Create, Edit, Copy, or Remove Keynotes and Apply them to Model Elements or Materials."
                };
                // ribbon.AddItem(btnAssignKeynotes);

                // Add "Set Assembly Code" button
                PushButtonData btnSetAssemblyCode = new PushButtonData(
                    "Set Assembly Code", "Set\nAssembly Code",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(SetAssemblyCodeCommand).FullName)
                {
                    Image = GetResourceBitmap("Set Assembly Code.png"),
                    LargeImage = GetResourceBitmap("Set Assembly Code.png"),
                    ToolTip = "Sets the Assembly Code and Assembly Description Parameters to your selected Type.",
                    AvailabilityClassName = typeof(AvailableWhenOneRevitElementIsSelected).FullName
                };
                ribbon.AddItem(btnSetAssemblyCode);

                // Add "View Specifications" button
                PushButtonData btnViewSpecs = new PushButtonData(
                    "View Specifications", "View\nSpecifications",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(ViewSpecificationsCommand).FullName)
                {
                    Image = GetResourceBitmap("View Specifications.png"),
                    LargeImage = GetResourceBitmap("View Specifications.png"),
                    ToolTip = "Re-Directs you to the linked Specpoint project to view elements for authoring, review, or markup.",
                    AvailabilityClassName = typeof(AvailableWhenOneRevitElementIsSelected).FullName
                };
                ribbon.AddItem(btnViewSpecs);

                // Add "Product Selection" button
                PushButtonData btnProductSelection = new PushButtonData(
                    "Product Selection", "Product\nSelection",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(ProductSelectionCommand).FullName)
                {
                    Image = GetResourceBitmap("Product Selection.png"),
                    LargeImage = GetResourceBitmap("Product Selection.png"),
                    ToolTip = "Allows you to view or select products and materials to the linked Specpoint project.",
                    AvailabilityClassName = typeof(AvailableWhenOneRevitElementIsSelected).FullName
                };
                ribbon.AddItem(btnProductSelection);

                // Add "Validation Schedule" button
                PushButtonData btnUpdate = new PushButtonData(
                    "Update", "Update\nSpecpoint",
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(UpdateCommand).FullName)
                {
                    Image = GetResourceBitmap("Validation Schedule.png"),
                    LargeImage = GetResourceBitmap("Validation Schedule.png"),
                    AvailabilityClassName = typeof(OnUpdateSync).FullName,
                    ToolTip = "Updates your Specpoint project with assembly information from your model."
                };
                ribbon.AddItem(btnUpdate);

                // Add vertical separator.
                ribbon.AddSeparator();

                string name = SpecpointRegistry.GetValue("name");
                if (string.IsNullOrEmpty(name))
                {
                    // Set to whitespace because emoty is not allowed
                    name = "No current user";
                }
                Globals.btnCurrentUser = new PushButtonData(
                    "Current User", name,
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(AccountCommand).FullName)
                {
                    ToolTip = "Current User",
                    AvailabilityClassName = typeof(AlwaysAvailable).FullName,
                };

                string projectName = SpecpointRegistry.GetValue("ProjectName");
                if (string.IsNullOrEmpty(projectName))
                {
                    // Set to whitespace because emoty is not allowed
                    projectName = "No current project";
                }
                Globals.btnCurrentProject = new PushButtonData(
                    "Current Project", projectName,
                    System.Reflection.Assembly.GetExecutingAssembly().Location,
                    typeof(LinkProjectCommand).FullName)
                {
                    ToolTip = "Current Project",
                    AvailabilityClassName = typeof(AvailableIfGuest).FullName,
                };

                IList<RibbonItem> stackedItems = ribbon.AddStackedItems(
                    Globals.btnCurrentUser,
                    Globals.btnCurrentProject);

                SpecpointRegistry.SetValue("UpdateSync", "0");

            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        void application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            // Only refresh ribbon when explicitly requested
            if (Globals.NeedsRibbonRefresh)
            {
                try
                {
                    Globals.RefreshButtons();
                    Globals.RefreshRibbon();
                    Globals.RefreshRibbon();

                    Globals.Log.Write(nameof(application_Idling));

                }
                catch (Exception ex)
                {
                    Globals.Log?.Write($"Error in application_Idling: {ex.Message}");
                }
                finally
                {
                    Globals.NeedsRibbonRefresh = false;
                }
            }
        }


        //For each view activated we want to check document title and reload dockable window data if needed.
        void application_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
        }

        void Document_DocumentClosing(object sender, Autodesk.Revit.DB.Events.DocumentClosingEventArgs e)
        {
        }

        /// <summary>
        /// Gets the resource bitmap.
        /// </summary>
        /// <param name="name">The name of the bitmap file.</param>
        /// <returns>Resource bitmap from file.</returns>
        private static BitmapImage GetResourceBitmap(String name)
        {
            string currentDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Uri uri = new Uri(currentDir + "\\" + name);

            // Adjust width and height in pixel to avoid cropping
            BitmapImage source = new BitmapImage();
            source.BeginInit();
            source.UriSource = uri;
            source.DecodePixelHeight = 27;
            source.DecodePixelWidth = 27;
            source.EndInit();

            return source;
        }

    }

}
