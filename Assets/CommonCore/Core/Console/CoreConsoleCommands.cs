using CommonCore.Config;
using CommonCore.DebugLog;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CommonCore.Console
{
    /// <summary>
    /// Basic/core commands for the command system
    /// </summary>
    public static class CoreConsoleCommands
    {

        [Command]
        private static string GetVersion()
        {
            return string.Format("{0} {1} (Unity {2})", CoreParams.VersionCode, CoreParams.VersionName, CoreParams.UnityVersionName);
        }

        [Command]
        private static void Quit()
        {
            CoreUtils.Quit();
        }

        /// <summary>
        /// Console command. Lists all config options.
        /// </summary>
        [Command(alias = "List", className = "Config", useClassName = true)]
        private static void ListConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            var properties = ConfigState.Instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                string key = property.Name;
                string value = property.GetValue(ConfigState.Instance)?.ToString();

                sb.AppendLine($"{key}={value}");
            }

            ConsoleModule.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Console command. Dumps system info to console
        /// </summary>
        [Command(useClassName = false)]
        private static void PrintSystemText()
        {
            ConsoleModule.WriteLine(CoreParams.GetLongSystemText());
        }

        /// <summary>
        /// Console command. Dumps all config options to console.
        /// </summary>
        [Command(alias = "Print", className = "Config", useClassName = true)]
        private static void PrintConfig()
        {
            ConsoleModule.WriteLine(DebugUtils.JsonStringify(ConfigState.Instance));
        }

        /// <summary>
        /// Console command. Gets the set value of a config option.
        /// </summary>
        [Command(alias = "Get", className = "Config", useClassName = true)]
        private static void GetConfig(string configOption)
        {
            var property = ConfigState.Instance.GetType().GetProperty(configOption, BindingFlags.Instance | BindingFlags.Public);
            if (property != null)
            {
                var value = property.GetValue(ConfigState.Instance);
                if (value is IEnumerable ievalue)
                    ConsoleModule.WriteLine(ievalue.ToNiceString());
                else
                    ConsoleModule.WriteLine(value.ToString());
            }
            else
            {
                ConsoleModule.WriteLine("not found");
            }
        }

        /// <summary>
        /// Console command. Gets the value of a custom flag
        /// </summary>
        [Command(alias = "GetCustomFlag", className = "Config", useClassName = true)]
        private static void GetCustomFlag(string customFlag)
        {
            if(ConfigState.Instance.CustomConfigFlags.Contains(customFlag))
            {
                ConsoleModule.WriteLine("set");
            }
            else
            {
                ConsoleModule.WriteLine("not set");
            }
        }

        /// <summary>
        /// Console command. Gets the value of a custom var
        /// </summary>
        [Command(alias = "GetCustomVar", className = "Config", useClassName = true)]
        private static void GetCustomVar(string customVar)
        {
            if (ConfigState.Instance.CustomConfigVars.ContainsKey(customVar) && ConfigState.Instance.CustomConfigVars[customVar] != null)
            {
                ConsoleModule.WriteLine(DebugUtils.JsonStringify(ConfigState.Instance.CustomConfigVars[customVar]));
            }
            else
            {
                //value doesn't exist: warn and exit
                ConsoleModule.WriteLine("Not Found");
            }
        }

        /// <summary>
        /// Console command. Sets the value of a config option
        /// </summary>
        [Command(alias = "Set", className = "Config", useClassName = true)]
        private static void SetConfig(string configOption, string newValue)
        {
            var property = ConfigState.Instance.GetType().GetProperty(configOption, BindingFlags.Instance | BindingFlags.Public);
            if (property != null)
            {
                property.SetValue(ConfigState.Instance, TypeUtils.CoerceValue(newValue, property.PropertyType)); //TODO handle enums
            }
            else
            {
                ConsoleModule.WriteLine("not found");
            }
        }

        /// <summary>
        /// Console command. Sets or unsets a custom config flag
        /// </summary>
        [Command(alias = "SetCustomFlag", className = "Config", useClassName = true)]
        private static void SetCustomFlag(string customFlag, bool newState)
        {
            if (ConfigState.Instance.CustomConfigFlags.Contains(customFlag) && !newState)
                ConfigState.Instance.CustomConfigFlags.Remove(customFlag);
            else if (!ConfigState.Instance.CustomConfigFlags.Contains(customFlag) && newState)
                ConfigState.Instance.CustomConfigFlags.Add(customFlag);
        }

        /// <summary>
        /// Console command. Sets or unsets a custom config var
        /// </summary>
        [Command(alias = "SetCustomVar", className = "Config", useClassName = true)]
        private static void SetCustomVar(string customVar, string newValue)
        {
            if (ConfigState.Instance.CustomConfigVars.ContainsKey(customVar))
            {
                //value exists: coerce the value
                object value = ConfigState.Instance.CustomConfigVars[customVar];
                object newValueParsed = TypeUtils.CoerceValue(newValue, value.GetType());
                ConfigState.Instance.CustomConfigVars[customVar] = newValueParsed;
            }
            else
            {
                //value doesn't exist: warn and exit
                ConsoleModule.WriteLine("Value doesn't already exist; can't determine type to set. Use SetCustomVarTyped instead.");
            }
        }

        /// <summary>
        /// Console command. Sets or unsets a custom config var
        /// </summary>
        [Command(alias = "SetCustomVarTyped", className = "Config", useClassName = true)]
        private static void SetCustomVar(string customVar, string newValue, string typeName)
        {
            //coerce the value
            object value = TypeUtils.CoerceValue(newValue, System.Type.GetType(typeName));
            ConfigState.Instance.CustomConfigVars[customVar] = value;
        }

        /// <summary>
        /// Console command. Gets the current graphics quality setting
        /// </summary>
        [Command(alias = "GetGraphicsQuality", className = "Config", useClassName = true)]
        private static void GetGraphicsQuality()
        {
            int qualityLevel = QualitySettings.GetQualityLevel();
            string qualityName = string.Empty;
            if (qualityLevel < QualitySettings.names.Length)
                qualityName = QualitySettings.names[qualityLevel];

            ConsoleModule.WriteLine($"Graphics quality: {qualityLevel} ({qualityName})");
        }

        /// <summary>
        /// Console command. Sets the graphics quality
        /// </summary>
        [Command(alias = "SetGraphicsQuality", className = "Config", useClassName = true)]
        private static void SetGraphicsQuality(string quality)
        {
            int qualityLevel = int.Parse(quality);
            if(qualityLevel >= QualitySettings.names.Length || qualityLevel < 0)
            {
                ConsoleModule.WriteLine($"Can't set graphics quality (level {qualityLevel} is not defined)");
                return;
            }

            QualitySettings.SetQualityLevel(qualityLevel, true);

            string qualityName = string.Empty;
            if (qualityLevel < QualitySettings.names.Length)
                qualityName = QualitySettings.names[qualityLevel];

            ConsoleModule.WriteLine($"Set graphics quality to: {qualityLevel} ({qualityName})");
        }

        /// <summary>
        /// Console command. Applies config state
        /// </summary>
        [Command(alias = "Apply", className = "Config", useClassName = true)]
        private static void ApplyConfig()
        {
            CCBase.GetModule<ConfigModule>().ApplyConfiguration();
        }

        /// <summary>
        /// Console command. Saves config state
        /// </summary>
        [Command(alias = "Save", className = "Config", useClassName = true)]
        private static void SaveConfig()
        {
            ConfigState.Save();
        }

        /// <summary>
        /// Console command. Reloads config state
        /// </summary>
        [Command(alias = "Load", className = "Config", useClassName = true)]
        private static void LoadConfig()
        {
            ConfigState.Load();
        }
    }
}