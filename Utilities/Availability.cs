using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Specpoint.Revit2026
{
    public class AlwaysAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            // Account is available always except when Model Validation form is up
            return true;
        }
    }

    public class AlwaysNotAvailable : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            return false;
        }
    }

    public class AvailableIfGuest : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            string accountType = SpecpointRegistry.GetValue("extension_AccountType");
            if (!string.IsNullOrEmpty(accountType) &&
                accountType == "BuildingProductManufacturer")
            {
                return false;
            }

            if (a.ActiveUIDocument == null) return false;

            // Get the projectId of associated Specpoint project with this Revit drawing
            RevitDrawing dwg = new RevitDrawing(a.ActiveUIDocument, a);
            Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
            
            // Try reading from HKCU\Software\Specpoint
            bool isLoggedIn = LoginUser.IsLoggedIn() && Globals.LoginNavigating == false;

            // Link Projct is available when the user is logged in and Model Validation form is down
            return isLoggedIn;
        }
    }

    public class AvailableWhenProjectIsSet : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            string accountType = SpecpointRegistry.GetValue("extension_AccountType");
            if (!string.IsNullOrEmpty(accountType) &&
                accountType == "BuildingProductManufacturer")
            {
                return false;
            }

            string role = SpecpointRegistry.GetValue("extension_Role");
            if (!string.IsNullOrEmpty(role) && role == "Reviewer" &&
                !string.IsNullOrEmpty(accountType) && accountType == "AEFirm")
            {
                return false;
            }

            // Try reading from HKCU\Software\Specpoint
            bool isLoggedIn = LoginUser.IsLoggedIn() && Globals.LoginNavigating == false;

            if (a.ActiveUIDocument == null) return false;

            // Get the projectId of associated Specpoint project with this Revit drawing
            RevitDrawing dwg = new RevitDrawing(a.ActiveUIDocument, a);
            Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
            
            bool isProjectSet = !string.IsNullOrEmpty(Globals.SpecpointProjectID);
            if (!isProjectSet) return false;

            // User cannot access this project
            bool userHasAccessToProject =
                SpecpointRegistry.GetValue("UserCanAccessProject") == "" ||
                SpecpointRegistry.GetValue("UserCanAccessProject") == "1";
            if (!userHasAccessToProject) return false;

            // Except when Model Validation form is already up
            return isLoggedIn;
        }
    }

    public class OnUpdateSync : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            string accountType = SpecpointRegistry.GetValue("extension_AccountType");
            if (!string.IsNullOrEmpty(accountType) &&
                accountType == "BuildingProductManufacturer")
            {
                return false;
            }

            string role = SpecpointRegistry.GetValue("extension_Role");
            if (!string.IsNullOrEmpty(role) && role == "Reviewer" &&
                !string.IsNullOrEmpty(accountType) && accountType == "AEFirm")
            {
                return false;
            }

            // Try reading from HKCU\Software\Specpoint
            bool isLoggedIn = LoginUser.IsLoggedIn() && Globals.LoginNavigating == false;

            if (a.ActiveUIDocument == null) return false;

            // Get the projectId of associated Specpoint project with this Revit drawing
            RevitDrawing dwg = new RevitDrawing(a.ActiveUIDocument, a);
            Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);

            bool isProjectSet = !string.IsNullOrEmpty(Globals.SpecpointProjectID);
            if (!isProjectSet) return false;

            // User cannot access this project
            bool userHasAccessToProject =
                SpecpointRegistry.GetValue("UserCanAccessProject") == "" ||
                SpecpointRegistry.GetValue("UserCanAccessProject") == "1";
            if (!userHasAccessToProject) return false;

            // During update sync, this flag is set to 1
            bool updateSync =
                SpecpointRegistry.GetValue("UpdateSync") == "1";
            if (updateSync) return false;

            // Except when Model Validation form is already up
            return isLoggedIn;
        }
    }

    public class AvailableWhenOneRevitElementIsSelected : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b)
        {
            string accountType = SpecpointRegistry.GetValue("extension_AccountType");
            if (!string.IsNullOrEmpty(accountType) &&
                accountType == "BuildingProductManufacturer")
            {
                return false;
            }

            string role = SpecpointRegistry.GetValue("extension_Role");
            if (!string.IsNullOrEmpty(role) && role == "Reviewer" &&
                !string.IsNullOrEmpty(accountType) && accountType == "AEFirm")
            {
                return false;
            }

            // Try reading from HKCU\Software\Specpoint
            bool isLoggedIn = LoginUser.IsLoggedIn() && Globals.LoginNavigating == false;
            if (isLoggedIn == false) return false;

            if (a.ActiveUIDocument == null) return false;

            // Get the projectId of associated Specpoint project with this Revit drawing
            RevitDrawing dwg = new RevitDrawing(a.ActiveUIDocument, a);
            Globals.SpecpointProjectID = dwg.GetSharedParameter(RevitDrawing.SPECPOINT_ID);
         
            bool isProjectSet = !string.IsNullOrEmpty(Globals.SpecpointProjectID);
            if (!isProjectSet) return false;

            // User cannot access this project
            bool userHasAccessToProject =
                SpecpointRegistry.GetValue("UserCanAccessProject") == "" ||
                SpecpointRegistry.GetValue("UserCanAccessProject") == "1";
            if (!userHasAccessToProject) return false;

            // Get the number of selected Revit elements.
            int count = a.ActiveUIDocument.Selection.GetElementIds().Count;

            // There should be 1 selected element
            return (count == 1);
        }
    }
}
