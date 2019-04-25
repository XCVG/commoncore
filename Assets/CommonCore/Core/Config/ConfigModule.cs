using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Messaging;
using CommonCore.Console;
using System.Text;
using System.Reflection;
using System;

namespace CommonCore.Config
{
    /*
     * CommonCore Config Module
     * Provides settings save/load/apply, as well as maintaining PersistState
     */
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
            if(QualitySettings.GetQualityLevel() >= QualitySettings.names.Length - 1) //only apply quality settings if set to "custom" in the launcher (this won't work because the sublevels aren't implemented yet)
            {
                QualitySettings.SetQualityLevel(ConfigState.Instance.QualityLevel, true);
                Application.targetFrameRate = ConfigState.Instance.MaxFrames;
                QualitySettings.vSyncCount = ConfigState.Instance.VsyncCount;
            }


            //INPUT CONFIG

            //let other things handle it on their own
            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("ConfigChanged"));

        }

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
            foreach(var property in properties)
            {
                string key = property.Name;
                string value = property.GetValue(ConfigState.Instance)?.ToString();

                sb.AppendLine($"{key}={value}");
            }

            ConsoleModule.WriteLine(sb.ToString());
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
                property.SetValue(ConfigState.Instance, CoreUtils.Parse(newValue, property.PropertyType)); //TODO handle enums
            }
            else
            {
                ConsoleModule.WriteLine("not found");
            }
        }

    }
}