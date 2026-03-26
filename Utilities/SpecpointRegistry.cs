using System;
using System.Configuration;
using System.IO;

namespace Specpoint.Revit2026
{
    public static class SpecpointRegistry
    {
        public static void SaveToken(string value)
        {
            SetValue("Token", value);
        }

        public static void SetValue(string name, string value)
        {
            string configPath = GetConfigFile();
            if (!File.Exists(configPath))
            {
                CreateConfigFile();
            }

            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = configPath
                };

                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                if (config == null) return;

                config.AppSettings.Settings.Remove(name);
                config.AppSettings.Settings.Add(name, value);

                config.Save(ConfigurationSaveMode.Minimal);

            }
            catch (Exception ex)
            {
                string msg = string.Format("Unable to set {0} {1} = {2}",
                    configPath, name, value);
                ErrorReporter.ReportError(ex, msg);
            }
        }

        public static string GetConfigFile()
        {
            string tempFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "Local", "Temp", Globals.RevitVersion);

            // Create log path if it doesn't exist
            Directory.CreateDirectory(tempFolder);

            string configPath = tempFolder + "\\SpecpointRevit.config";
            return configPath;
        }

        public static void CreateConfigFile()
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
            {
                ExeConfigFilename = GetConfigFile()
            };

            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            if (config == null) return;

            config.AppSettings.Settings.Add("env", "p");
            config.Save(ConfigurationSaveMode.Minimal);
        }

        public static string GetValue(string name)
        {
            string configPath = GetConfigFile();
            if (!File.Exists(configPath))
            {
                CreateConfigFile();
            }

            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename = configPath
                };

                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                if (config == null) return string.Empty;
                if (config.AppSettings.Settings[name] == null) return string.Empty;

                return config.AppSettings.Settings[name].Value;

            }
            catch (Exception ex)
            {
                string msg = string.Format("Unable to open {0} {1}",
                    configPath, name);
                ErrorReporter.ReportError(ex, msg);
            }

            return string.Empty;
        }
    }
}
