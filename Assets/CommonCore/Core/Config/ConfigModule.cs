using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.Messaging;
using System.Collections;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace CommonCore.Config
{
    /// <summary>
    /// Provides settings save/load/apply and handles settings/config state
    /// </summary>
    [CCExplicitModule]
    public class ConfigModule : CCModule
    {
        private static ConfigModule Instance;

        public ConfigModule()
        {
            Instance = this;

            ConfigState.Load();
            ConfigState.Save();
        }

        /// <summary>
        /// Apply the current ConfigState configuration to the game
        /// </summary>
        public void ApplyConfiguration()
        {

            //AUDIO CONFIG
            AudioListener.volume = ConfigState.Instance.SoundVolume;
            var ac = AudioSettings.GetConfiguration();
            ac.speakerMode = ConfigState.Instance.SpeakerMode;
            AudioSettings.Reset(ac);

            //VIDEO CONFIG
            if (QualitySettings.GetQualityLevel() >= QualitySettings.names.Length - 1) //only apply quality settings if set to "custom" in the launcher
            {
                //QualitySettings.SetQualityLevel(ConfigState.Instance.QualityLevel, true);
                QualitySettings.vSyncCount = ConfigState.Instance.VsyncCount;
            }

            //TODO implement full config and clean this up
            Application.targetFrameRate = ConfigState.Instance.MaxFrames;

            //INPUT CONFIG

            //let other things handle it on their own
            QdmsMessageBus.Instance.PushBroadcast(new ConfigChangedMessage());

        }

        /// <summary>
        /// Apply the current ConfigState configuration to the game
        /// </summary>
        public static void Apply()
        {
            Instance.ApplyConfiguration();
        }

        /// <summary>
        /// Console command. Lists all config options.
        /// </summary>
        [Command(alias = "List", className = "Config", useClassName = true)]
        public static void ListConfig()
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
        /// Console command. Dumps all config options to console.
        /// </summary>
        [Command(alias = "Print", className = "Config", useClassName = true)]
        public static void PrintConfig()
        {
            ConsoleModule.WriteLine(DebugUtils.JsonStringify(ConfigState.Instance));
        }

        /// <summary>
        /// Console command. Gets the set value of a config option.
        /// </summary>
        [Command(alias = "Get", className = "Config", useClassName = true)]
        public static void GetConfig(string configOption)
        {
            var property = ConfigState.Instance.GetType().GetProperty(configOption, BindingFlags.Instance | BindingFlags.Public);
            if(property != null)
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
        /// Console command. Sets the value of a config option
        /// </summary>
        [Command(alias = "Set", className = "Config", useClassName = true)]
        public static void SetConfig(string configOption, string newValue)
        {
            var property = ConfigState.Instance.GetType().GetProperty(configOption, BindingFlags.Instance | BindingFlags.Public);
            if (property != null)
            {
                property.SetValue(ConfigState.Instance, TypeUtils.Parse(newValue, property.PropertyType)); //TODO handle enums
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
        public static void SetCustomFlag(string customFlag, bool newState)
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
        public static void SetCustomVar(string customVar, string newValue)
        {
            if (ConfigState.Instance.CustomConfigVars.ContainsKey(customVar))
            {
                //value exists: coerce the value
                object value = ConfigState.Instance.CustomConfigVars[customVar];
                object newValueParsed = TypeUtils.Parse(newValue, value.GetType());
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
        public static void SetCustomVar(string customVar, string newValue, string typeName)
        {
            //coerce the value
            object value = TypeUtils.Parse(newValue, System.Type.GetType(typeName));
            ConfigState.Instance.CustomConfigVars[customVar] = value;
        }

    }

    /// <summary>
    /// Config Changed flag message
    /// </summary>
    public class ConfigChangedMessage : QdmsFlagMessage
    {
        public ConfigChangedMessage() : base("ConfigChanged")
        {

        }
    }
}