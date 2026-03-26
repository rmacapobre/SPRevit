using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Web;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace Specpoint.Revit2026
{
    public partial class LoginForm : System.Windows.Forms.Form
    {
        public bool LoginSuccessful { get; set; }
        public bool IsBPMUser { get; set; }

        // Revit Model which contains the linked Specpoint Project's ProjectId
        public Document Doc { get; set; }
        public RevitDrawing Dwg { get; set; }

        public string UserDataFolder { get; set; }

        // 11001 (0x2AF9) - IE11.Webpages are displayed in IE11 edge mode
        private const long IE11EdgeMode = 11001;

        private string title = "Account";

        private CoreWebView2Environment webView2Env;

        public LoginForm()
        {
            InitializeComponent();
        }

        private async void Login_Load(object sender, EventArgs e)
        {
            try
            {
                UserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                      "AppData",
                      "Local",
                      "Temp",
                      "WebView2Temp");

                webView2Env = await CoreWebView2Environment.CreateAsync(null, UserDataFolder);
                await webView2.EnsureCoreWebView2Async(webView2Env);

                SpecpointEnvironment env = new SpecpointEnvironment();
                webView2.Source = new Uri(env.LoginPage, UriKind.Absolute);

                this.Activate();

            }
            catch (UnauthorizedAccessException ex)
            {
                string msg = string.Format("Unauthorized Access in Login_Load. {0}\n{1}", ex.Message, webView2Env.UserDataFolder);
                Globals.Log.WriteError(msg);

                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }

            Text = title;
        }

        private string GetSpanValue(ref string body, string span)
        {
            string endSpan = @"</span>";
            int pos = body.IndexOf(span);
            if (pos > -1)
            {
                int startpos = pos + span.Length;
                int endpos = body.IndexOf(endSpan, pos + span.Length);
                return body.Substring(startpos, endpos - startpos);
            }

            return string.Empty;
        }

        private int GetSpanValue(ref string body, string span, ref string value, int startPos)
        {
            string endSpan = @"</span>";
            int pos = body.IndexOf(span, startPos);
            if (pos > -1)
            {
                int startpos = pos + span.Length;
                int endpos = body.IndexOf(endSpan, pos + span.Length);
                value = body.Substring(startpos, endpos - startpos);
                return endpos;
            }

            return pos;
        }

        private string ExtractToken(ref string body)
        {
            try
            {
                string spanHeader = @"<span class=""jwtHeader"">";
                string spanClaims = @"<span class=""jwtClaims"">";
                string spanSignature = @"<span class=""jwtSignature"">";

                string jwtHeader = GetSpanValue(ref body, spanHeader);
                string jwtClaims = GetSpanValue(ref body, spanClaims);
                string jwtSignature = GetSpanValue(ref body, spanSignature);

                return jwtHeader + "." + jwtClaims + "." + jwtSignature;
            }
            catch (Exception ex)
            {
                string msg = string.Format("ExtractToken. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }

            return string.Empty;
        }

        private void ExtractTokenProperties(ref string body)
        {
            try
            {
                List<string> names = new List<string>();
                List<string> values = new List<string>();

                string ignore = "' + claimType + '";

                // Properties that use prewrapbreakword
                List<string> usesSpanValue1 = new List<string>();
                usesSpanValue1.Add("exp");
                usesSpanValue1.Add("nbf");
                usesSpanValue1.Add("iat");
                usesSpanValue1.Add("auth_time");

                // Get property names and values
                int startPos = 0;
                string spanName = @"<span class=""mono prewrapbreakword"">";
                string spanValue1 = @"<span class=""formattedvalue prewrapbreakword"">";
                string spanValue2 = @"<span class=""formattedvalue mono forcebreakword"">";
                while (startPos > -1)
                {
                    // Get name
                    string name = "";
                    int endPos = GetSpanValue(ref body, spanName, ref name, startPos);
                    if (endPos == -1) break;
                    if (name == ignore) break;

                    names.Add(name);
                    startPos = endPos;

                    // Determine what spanValue is used
                    string spanValue = usesSpanValue1.Contains(name) ? spanValue1 : spanValue2;

                    // Get value
                    string value = "";
                    endPos = GetSpanValue(ref body, spanValue, ref value, startPos);
                    if (endPos == -1) break;

                    values.Add(value);
                    startPos = endPos;
                }

                if (names.Count == values.Count)
                {
                    for (int i = 0; i < names.Count; ++i)
                    {
                        SpecpointRegistry.SetValue(names[i], values[i]);
                        if (names[i] == "extension_AccountId")
                        {
                            Globals.extension_AccountId = values[i];
                        }
                        else if (names[i] == "name")
                        {
                            Globals.LoginUser = values[i];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("ExtractTokenProperties. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }
        }

        private async void DocumentCompleted()
        {
            Globals.Log.Write("DocumentCompleted");

            try
            {
                if (LoginSuccessful)
                {
                    // Extract body
                    string body = await webView2.ExecuteScriptAsync("document.body.innerHTML");
                    // Remove JSON string quotes and unescape
                    body = body.Trim('"').Replace("\\", "").Replace(@"\u003C", "<");

                    // Extract token from body
                    Globals.Token = ExtractToken(ref body);

                    // Extract token properties and save to the registry
                    ExtractTokenProperties(ref body);

                    Close();
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("DocumentCompleted. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }
            finally
            {
                if (LoginSuccessful == true &&
                    !string.IsNullOrEmpty(Globals.SpecpointProjectID))
                {
                    string msg = string.Format("Check if user {0} has access to project: {1}",
                        Globals.LoginUser, Globals.SpecpointProjectID);
                    Globals.Log.Write(msg);

                    // Check if user has access to the model's linked project
                    using (LinkProjectForm dlg = new LinkProjectForm(Doc.Title, Globals.SpecpointProjectID)
                    {
                        // Exit the form when user has access to the model's linked project
                        ExitRightAway = true,
                        Text = "Confirming Access to Specpoint Project"
                    })
                    {
                        DialogResult dr = dlg.ShowDialog(this);
                        if (dr == DialogResult.OK && Doc != null && Dwg != null)
                        {
                            // Set the projectId of associated Specpoint project with this Revit drawing
                            ProjectItem selectedProject = dlg.selectedProject;
                            if (selectedProject != null)
                            {
                                Dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, selectedProject.id);
                                Globals.SpecpointProjectID = selectedProject.id;
                            }
                            else
                            {
                                Dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, "");
                                Globals.SpecpointProjectID = "";
                            }

                            SpecpointRegistry.SetValue("ProjectId", Globals.SpecpointProjectID);
                            SpecpointRegistry.SetValue("ProjectName", selectedProject.name);

                            // Use async-safe button update from async DocumentCompleted context
                            Globals.SetButtonTextAsync("Current Project", selectedProject.name);

                            msg = string.Format("User {0} logged in and has access to project: {1} {2}",
                                Globals.LoginUser, Globals.SpecpointProjectID, selectedProject.name);
                            Globals.Log.Write(msg);

                            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }

            // Reset
            Globals.ResetGlobals();
            Globals.SpecpointKeynotes = null;
            Globals.KeynoteDivisions = null;
            Globals.KeynoteAssemblies = null;
            Globals.KeynoteAssembliesByCategory = null;

            Globals.CurrentUserFirm = null;
            Globals.CurrentUserActiveFirm = null;
        }

        private void NavigatedPasswordExpiration(string uri)
        {
            string codeExpiring = "B2C_1A_SignIn_PrePrd";
            string codeExpired = "B2C_1A_SignIn_7DayLogin_Prd";
            string codeExpireWarning = "B2C_1A_SignIn_7DayLogin_PrePrd";
            string api = "CombinedSigninAndSignup";
            if ((uri.Contains(codeExpiring) || uri.Contains(codeExpired) || uri.Contains(codeExpireWarning)) &&
                uri.Contains(api))
            {
                Globals.Log.WriteError("NavigatedPasswordExpiration: " + uri);

                // Unknown/Expired user
                SpecpointRegistry.SetValue("extension_AccountId", "");
                SpecpointRegistry.SetValue("extension_AccountType", "");
                SpecpointRegistry.SetValue("extension_Role", "");
                SpecpointRegistry.SetValue("name", "");
                SpecpointRegistry.SetValue("email", "");
                SpecpointRegistry.SetValue("Token", "");
                Globals.LoginUser = "";

                using (ResetYourPasswordForm form = new ResetYourPasswordForm())
                {
                    form.ShowDialog(this);
                }

                // Close the Account form
                this.Close();
            }
        }

        private void NavigatedAccessDenied(string uri)
        {
            // https://jwt.ms/#error=access_denied&error_description=AADB2C90118%3a+The+user+has+forgotten+their+password.%0d%0aCorrelation+ID%3a+352d35fe-ccff-4710-9a55-441a8c4bc912%0d%0aTimestamp%3a+2023-08-08+02%3a22%3a53Z%0d%0a

            string access_denied = "https://jwt.ms/#error=access_denied";
            if (uri.StartsWith(access_denied))
            {
                Globals.Log.WriteError("NavigatedAccessDenied: " + uri);

                Uri url = new Uri(uri);
                string fragment = url.Fragment;

                char[] ampersand = { '&' };
                string[] parameters = fragment.Split(ampersand);
                foreach (string p in parameters)
                {
                    char[] equal = { '=' };
                    string[] keyValues = p.Split(equal);

                    string key = keyValues[0];
                    string value = HttpUtility.UrlDecode(keyValues[1]);

                    string msg = string.Format("{0} = {1}", key, value);
                    Globals.Log.WriteError(msg);

                    // The user has forgotten their password
                    if (value.Contains("AADB2C90118"))
                    {
                        // Close the Account form
                        this.Close();

                        SpecpointEnvironment env = new SpecpointEnvironment();

                        // Launch Forget Password in browser
                        Browser b = new Browser();
                        b.OpenUrl(env.ForgotPassword);
                    }
                }
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private async void NavigatedLoginSuccess(string uri)
        {
            string func = "NavigatedLoginSuccess";
            string loggedInAddress = @"https://jwt.ms/#id_token=";

            if (uri.StartsWith(loggedInAddress))
            {
                this.DialogResult = DialogResult.OK;

                Globals.Log.Write(func);

                LoginUser.LogOff();

                // Hide the form itself to keep the token from displaying 
                this.Hide();

                // WebView2 doesn't need IE compatibility mode settings

                // Extract token from URI (ahead of DocumentCompleted)
                Globals.Token = uri.Substring(loggedInAddress.Length);
                SpecpointRegistry.SaveToken(Globals.Token);
                Globals.Log.Write(func + " Token: " + Globals.Token);

                try
                {
                    Query query = new Query();
                    Globals.CurrentUserActiveFirm = await query.CurrentUserActiveFirmQuery();
                    if (Globals.CurrentUserActiveFirm != null)
                    {
                        string userId = Globals.CurrentUserActiveFirm.myActiveFirm.userId;
                        Globals.extension_AccountId = Globals.CurrentUserActiveFirm.myActiveFirm.firmId;
                        string extension_Role = Globals.CurrentUserActiveFirm.myActiveFirm.firmRole;
                        string extension_AccountType = Globals.CurrentUserActiveFirm.myActiveFirm.firmAccountType;

                        if (extension_AccountType != null &&
                            extension_AccountType == "BuildingProductManufacturer")
                        {
                            Globals.Log.WriteError("Failed to login. BuildingProductManufacturer");

                            LoginUser.LogOff();
                            LoginSuccessful = false;
                            IsBPMUser = true;
                            Close();
                        }
                        else
                        {
                            SpecpointRegistry.SetValue("extension_AccountId", Globals.extension_AccountId);
                            SpecpointRegistry.SetValue("extension_AccountType", extension_AccountType);
                            SpecpointRegistry.SetValue("extension_Role", extension_Role);

                            User user = await query.user(
                                Globals.CurrentUserActiveFirm.myActiveFirm.firmId,
                                Globals.CurrentUserActiveFirm.myActiveFirm.userId);
                            if (user != null)
                            {
                                SpecpointRegistry.SetValue("name", user.userFullName);
                                SpecpointRegistry.SetValue("email", user.email);

                                Globals.LoginUser = SpecpointRegistry.GetValue("name");

                                string msgLoginSuccessful = string.Format("{0} is successfully logged in.", Globals.LoginUser);

                                LoginSuccessful = true;

                                if (LoginUser.IsLoggedIn())
                                {
                                    // Get Revit Apps main window handle
                                    Process revitApp = Process.GetCurrentProcess();
                                    IntPtr main = revitApp.MainWindowHandle;

                                    MessageBox.Show(msgLoginSuccessful, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Globals.Log.Write(msgLoginSuccessful);

                                    Globals.SetProject();

                                    // Use async-safe button update from async NavigatedLoginSuccess context
                                    Globals.SetButtonTextAsync("Current User", Globals.LoginUser);

                                    Globals.NeedsRibbonRefresh = true;
                                    Globals.RefreshRibbon();
                                    Globals.RefreshButtons();

                                    int WM_ACTIVATE = 0x0006;
                                    int WA_CLICKACTIVE = 2;
                                    PostMessage(main, WM_ACTIVATE, WA_CLICKACTIVE, 0);
                                }
                            }
                            else
                            {
                                LoginUser.LogOff();
                                LoginSuccessful = false;
                                Close();
                            }
                        }
                    }
                    else
                    {
                        LoginUser.LogOff();
                        LoginSuccessful = false;
                        Close();
                    }
                }
                catch (TokenExpiredException)
                {
                    LoginSuccessful = false;
                    Close();
                }
            }
        }

        private void Navigated(string uri)
        {
            try
            {
                // NavigatedPasswordExpiration(uri);
                NavigatedAccessDenied(uri);
                NavigatedLoginSuccess(uri);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Navigated. Error in Navigated. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }
            finally
            {
                if (LoginSuccessful == true &&
                    !string.IsNullOrEmpty(Globals.SpecpointProjectID))
                {
                    string msg = string.Format("Check if user {0} has access to project: {1}",
                        Globals.LoginUser, Globals.SpecpointProjectID);
                    Globals.Log.Write(msg);

                    // Check if user has access to the model's linked project
                    using (LinkProjectForm dlg = new LinkProjectForm(Doc.Title, Globals.SpecpointProjectID)
                    {
                        // Exit the form when user has access to the model's linked project
                        ExitRightAway = true,
                        Text = "Confirming Access to Specpoint Project"
                    })
                    {
                        DialogResult dr = dlg.ShowDialog(this);
                        if (dr == DialogResult.OK && Doc != null && Dwg != null)
                        {
                            // Set the projectId of associated Specpoint project with this Revit drawing
                            ProjectItem selectedProject = dlg.selectedProject;
                            if (selectedProject != null)
                            {
                                Dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, selectedProject.id);
                                Globals.SpecpointProjectID = selectedProject.id;
                            }
                            else
                            {
                                Dwg.SetSharedParameter(RevitDrawing.SPECPOINT_ID, "");
                                Globals.SpecpointProjectID = "";
                            }

                            SpecpointRegistry.SetValue("ProjectId", Globals.SpecpointProjectID);
                            SpecpointRegistry.SetValue("ProjectName", selectedProject.name);

                            // Use async-safe button update from async NavigatedLoginSuccess context
                            Globals.SetButtonTextAsync("Current Project", selectedProject.name);

                            msg = string.Format("User {0} logged in and has access to project: {1} {2}",
                                Globals.LoginUser, Globals.SpecpointProjectID, selectedProject.name);
                            Globals.Log.Write(msg);

                            MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }

                // Reset
                Globals.ResetGlobals();
                Globals.SpecpointKeynotes = null;
                Globals.KeynoteDivisions = null;
                Globals.KeynoteAssemblies = null;
                Globals.KeynoteAssembliesByCategory = null;

                Globals.CurrentUserFirm = null;
                Globals.CurrentUserActiveFirm = null;
            }
        }

        private void webView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            string uri = webView2.Source?.ToString() ?? "";
            Globals.LoginNavigating = false;

            try
            {
                if (e.IsSuccess)
                {
                    Navigated(uri);

                    string loggedInAddress = @"https://jwt.ms/#id_token=";
                    if (uri.StartsWith(loggedInAddress))
                    {
                        DocumentCompleted();
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("webView2_NavigationCompleted. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }
        }

        private void webView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            Globals.LoginNavigating = true;
        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //In case windows is trying to shut down, don't hold the process up
            if (e.CloseReason == CloseReason.WindowsShutDown) return;

            if (this.DialogResult == DialogResult.Cancel)
            {
                // Assume that X has been clicked and act accordingly.
                LoginUser.LogOff();
            }
        }

        private void webView2_ContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {
            try
            {
                string uri = webView2.Source?.ToString() ?? "";

                string loggedInAddress = @"https://jwt.ms/#id_token=";
                string access_denied = "https://jwt.ms/#error=access_denied";
                if (uri.StartsWith(access_denied) ||
                    uri.StartsWith(loggedInAddress))
                {
                    this.Hide();
                }

            }
            catch (Exception ex)
            {
                string msg = string.Format("webView2_NavigationCompleted. {0}", ex.Message);
                Globals.Log.WriteError(msg);
            }
        }
    }

    public class TokenProperty
    {
        public string name;
        public string value;
    }
}
