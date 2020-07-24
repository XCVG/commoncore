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

            ConfigState.Load();
            ConfigState.Save();
        }

        public override void OnAllModulesLoaded()
        {
            ConfigState.Save();
            ApplyConfiguration();

            RegisterConfigPanel("GraphicsOptionsPanel", 1000, CoreUtils.LoadResource<GameObject>("UI/GraphicsOptionsPanel"));
        }

        public override void Dispose()
        {
            //set safe resolution on exit. Hacky code ahead!
            if(!CoreParams.IsEditor && CoreParams.SetSafeResolutionOnExit)
            {
                //Screen.SetResolution(CoreParams.SafeResolution.x, CoreParams.SafeResolution.y, false, 60);
                //Debug.LogWarning($"W: {PlayerPrefs.GetInt("Screenmanager Resolution Width")} | H: {PlayerPrefs.GetInt("Screenmanager Resolution Height")}");
                PlayerPrefs.SetInt("Screenmanager Resolution Width", CoreParams.SafeResolution.x);
                PlayerPrefs.SetInt("Screenmanager Resolution Height", CoreParams.SafeResolution.y);
                PlayerPrefs.SetInt("Screenmanager Fullscreen mode", 3); //set to windowed
                PlayerPrefs.Save();
            }
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
#if UNITY_WSA
            if (ConfigState.Instance.SpeakerMode == AudioSpeakerMode.Raw)
                ConfigState.Instance.SpeakerMode = AudioSpeakerMode.Stereo;
#endif
            ac.speakerMode = ConfigState.Instance.SpeakerMode;
            AudioSettings.Reset(ac);

            //VIDEO CONFIG
            if (ConfigState.Instance.UseCustomVideoSettings)
            {
                ApplyExtendedGraphicsConfiguration();
            }

            if(!CoreParams.IsEditor)
                Screen.SetResolution(ConfigState.Instance.Resolution.x, ConfigState.Instance.Resolution.y, ConfigState.Instance.FullScreen, ConfigState.Instance.RefreshRate);

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