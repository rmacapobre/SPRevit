using System;
using Autodesk.Revit.UI;
using System.Windows;

namespace Specpoint.Revit2026
{
    /// <summary>
    /// External event handler for triggering login when token expires
    /// This allows us to post commands outside of an executing command context
    /// </summary>
    public class LoginExternalEventHandler : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                string panelName = $"Specpoint {version.ToString()}";
                string commandId = string.Format("CustomCtrl_%CustomCtrl_%Specpoint%{0}%{1}", panelName, "Account");

                RevitCommandId accountCommandId = RevitCommandId.LookupCommandId(commandId);
                if (accountCommandId != null)
                {
                    Specpoint.Revit2026.LoginUser.LogOff();
                    app.PostCommand(accountCommandId);
                }
                else
                {
                    Globals.Log.Write("WARNING: Could not find Account command to trigger login");
                }
            }
            catch (Exception ex)
            {
                Globals.Log.Write($"ERROR in LoginExternalEventHandler.Execute: {ex.Message}");
                Globals.Log.GraphQL(ex);
            }
        }

        public string GetName()
        {
            return "LoginExternalEventHandler";
        }
    }
}
