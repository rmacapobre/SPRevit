using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Specpoint.Revit2026
{
    public class Browser
    {
        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Globals.Log.WriteError(ex.Message);

                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    try
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                    }
                    catch
                    {
                        string msg = string.Format("Unable to open {0}", url);
                        Globals.Log.WriteError(msg);
                    }
                }
            }
        }
    }
}
