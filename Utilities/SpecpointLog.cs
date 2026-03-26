using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Specpoint.Revit2026
{
    public class SpecpointLog
    {
        private static SpecpointLog _instance;
        private static readonly object _lock = new object();

        public bool LoggingEnabled { get; set; }

        public static SpecpointLog Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new SpecpointLog();
                    }
                }
                return _instance;
            }
        }

        private SpecpointLog()
        {
            string logFile = GetLogFile();
            LoggingEnabled = IsLoggingEnabled();

            try
            {
                // Write to a text file in the project folder 
                Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText(logFile)));
                Trace.AutoFlush = true;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        public void Write(string value,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            DateTime now = DateTime.Now;
            string msg = string.Format("{0}: {1}", now.ToString(), value);
            Trace.WriteLine(msg);
        }
        public void Write(Exception ex,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            WriteError(ex.Message, filePath, lineNumber);
        }

        public void WriteError(string value,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            DateTime now = DateTime.Now;
            string msg = string.Format("{0}: {1} ({2}) ERROR: {3}",
                now.ToString(), filePath, lineNumber, value);
            Trace.WriteLine(msg);
        }

        private bool IsLoggingEnabled()
        {
            bool enabled = false;

            // Get the flag from HKCU\Software\Specpoint
            long logging = 0;
            string value = SpecpointRegistry.GetValue("Logging");
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    logging = long.Parse(value);
                }
                catch
                {
                    // Do nothing
                }
            }

            if (logging != 0)
            {
                enabled = true;
            }

            return enabled;
        }

        private string GetLogFile()
        {
            string tempFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "Local", "Temp");
            string logPath = tempFolder + "\\" + Globals.RevitVersion;

            // Create log path if it doesn't exist
            Directory.CreateDirectory(logPath);

            string projectName = SpecpointRegistry.GetValue("ProjectName");
            string dt = DateTime.Now.ToString("yyyyMMdd");

            string logFile = logPath + (projectName != "" ? 
                $"\\SpecpointRevitLog_{projectName}.{dt}.txt" :
                $"\\SpecpointRevitLog.{dt}.txt");
            return logFile;
        }

        public void GraphQL(string value,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            DateTime now = DateTime.Now;
            string msg = string.Format("{0}: GraphQL: {1}", now.ToString(), value);
            Trace.WriteLine(msg);
        }

        public void GraphQL(Exception ex,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            GraphQLError(ex.Message, filePath, lineNumber);
        }

        public void GraphQLError(string value,
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (!LoggingEnabled) return;

            DateTime now = DateTime.Now;
            string msg = string.Format("{0}: {1} ({2}) GRAPHQL ERROR: {3}",
                now.ToString(), filePath, lineNumber, value);
            Trace.WriteLine(msg);
        }

        internal void Write(object value)
        {
            throw new NotImplementedException();
        }
    }
}
