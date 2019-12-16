using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.Input;
using CommonCore.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public static ConfigModule Instance { get; private set; }

        private Dictionary<string, ConfigPanelData> ConfigPanels = new Dictionary<string, ConfigPanelData>();

        public ConfigModule()
        {
            Instance = this;

            RegisterConfigPanel("GraphicsOptionsPanel", 1000, CoreUtils.LoadResource<GameObject>("UI/GraphicsOptionsPanel"));

            ConfigState.Load();
            ConfigState.Save();
        }

        public override void OnAllModulesLoaded()
        {
            ConfigState.Save();
        }

        /// <summary>
        /// Registers a config panel to be displayed in options menus
        /// </summary>
        public void RegisterConfigPanel(string name, int priority, GameObject prefab)
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab), "Prefab must be non-null!");

            if (ConfigPanels.ContainsKey(name))
            {
                LogWarning($"A config panel \"{name}\" is already registered");
                ConfigPanels.Remove(name);
            }

            ConfigPanels.Add(name, new ConfigPanelData(priority, prefab));
        }

        /// <summary>
        /// Unregisters a config panel
        /// </summary>
        public void UnregisterConfigPanel(string name)
        {
            ConfigPanels.Remove(name);
        }

        /// <summary>
        /// A sorted view (highest to lowest priority) of the config panel prefabs
        /// </summary>
        public IReadOnlyList<GameObject> SortedConfigPanels => ConfigPanels.Select(kvp => kvp.Value).OrderByDescending(d => d.Priority).Select(d => d.Prefab).ToArray();

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
            if (ConfigState.Instance.UseCustomVideoSettings)
            {
                ApplyExtendedGraphicsConfiguration();
            }

            QualitySettings.vSyncCount = ConfigState.Instance.VsyncCount;
            Application.targetFrameRate = ConfigState.Instance.MaxFrames;

            //INPUT CONFIG
            MappedInput.SetMapper(ConfigState.Instance.InputMapper); //safe?

            //let other things handle it on their own
            QdmsMessageBus.Instance.PushBroadcast(new ConfigChangedMessage());

        }

        /// <summary>
        /// Apply the extended graphics configuration (separate settings for different things
        /// </summary>
        private void ApplyExtendedGraphicsConfiguration()
        {           
            //shadow quality
            var shadowQuality = ShadowQuality.Presets[ConfigState.Instance.ShadowQuality];
            QualitySettings.shadows = shadowQuality.shadows;
            QualitySettings.shadowCascades = shadowQuality.shadowCascades;
            QualitySettings.shadowmaskMode = shadowQuality.shadowmaskMode;
            QualitySettings.shadowResolution = shadowQuality.shadowResolution;

            //shadow distance
            var shadowDistance = ConfigState.Instance.ShadowDistance;
            QualitySettings.shadowDistance = shadowDistance;

            //lighting quality
            var lightingQuality = LightingQuality.Presets[ConfigState.Instance.LightingQuality];
            QualitySettings.pixelLightCount = lightingQuality.pixelLightCount;
            QualitySettings.realtimeReflectionProbes = lightingQuality.realtimeReflectionProbes;

            //mesh quality
            var meshQuality = MeshQuality.Presets[ConfigState.Instance.MeshQuality];
            QualitySettings.lodBias = meshQuality.lodBias;
            QualitySettings.blendWeights = meshQuality.blendWeights;
            //QualitySettings.maximumLODLevel = meshQuality.maximumLODLevel; //is a nop

            //texture scale
            var textureScale = (int)ConfigState.Instance.TextureScale;
            QualitySettings.masterTextureLimit = textureScale;

            //texture filtering
            var textureFiltering = ConfigState.Instance.AnisotropicFiltering;
            QualitySettings.anisotropicFiltering = textureFiltering;

            //rendering quality
            var renderQuality = RenderingQuality.Presets[ConfigState.Instance.RenderingQuality];
            QualitySettings.billboardsFaceCameraPosition = renderQuality.billboardsFaceCameraPosition;
            QualitySettings.particleRaycastBudget = renderQuality.particleRaycastBudget;
            QualitySettings.softParticles = renderQuality.softParticles;
            QualitySettings.softVegetation = renderQuality.softVegetation;
            
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
        private static void SetConfig(string configOption, string newValue)
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
        private static void SetCustomVar(string customVar, string newValue, string typeName)
        {
            //coerce the value
            object value = TypeUtils.Parse(newValue, System.Type.GetType(typeName));
            ConfigState.Instance.CustomConfigVars[customVar] = value;
        }

        private struct ConfigPanelData
        {
            public int Priority;
            public GameObject Prefab;

            public ConfigPanelData(int priority, GameObject prefab)
            {
                Priority = priority;
                Prefab = prefab;
            }
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