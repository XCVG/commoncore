using CommonCore.Console;
using CommonCore.DebugLog;
using CommonCore.Input;
using CommonCore.Messaging;
using CommonCore.Scripting;
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
            ApplyConfiguration(true);

            RegisterConfigPanel("GraphicsOptionsPanel", 1000, CoreUtils.LoadResource<GameObject>("UI/GraphicsOptionsPanel"));
        }

        public override void Dispose()
        {
            //set safe resolution on exit. Hacky code ahead!
            if(!CoreParams.IsEditor && CoreParams.Platform != RuntimePlatform.WebGLPlayer && CoreParams.SetSafeResolutionOnExit)
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
        /// Registers a config panel to be displayed in options menus (prefab variant)
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

            ConfigPanels.Add(name, new ConfigPanelData(priority, (t) => GameObject.Instantiate(prefab, t)));
        }

        /// <summary>
        /// Registers a config panel to be displayed in options menus (build function variant)
        /// </summary>
        public void RegisterConfigPanel(string name, int priority, Func<Transform, GameObject> builder)
        {
            if (ConfigPanels.ContainsKey(name))
            {
                LogWarning($"A config panel \"{name}\" is already registered");
                ConfigPanels.Remove(name);
            }

            ConfigPanels.Add(name, new ConfigPanelData(priority, builder));
        }

        /// <summary>
        /// Unregisters a config panel
        /// </summary>
        public void UnregisterConfigPanel(string name)
        {
            ConfigPanels.Remove(name);
        }

        /// <summary>
        /// A sorted view (highest to lowest priority) of the config panel builders
        /// </summary>
        public IReadOnlyList<Func<Transform, GameObject>> SortedConfigPanelBuilders => ConfigPanels.Select(kvp => kvp.Value).OrderByDescending(d => d.Priority).Select(d => d.Builder).ToArray();

        /// <summary>
        /// Apply the current ConfigState configuration to the game
        /// </summary>
        public void ApplyConfiguration() => ApplyConfiguration(false);


        /// <summary>
        /// Apply the current ConfigState configuration to the game
        /// </summary>
        public void ApplyConfiguration(bool isInitialConfig)
        {

            //AUDIO CONFIG
            AudioListener.volume = ConfigState.Instance.SoundVolume;
            if(isInitialConfig) //only safe to do this if resources aren't loaded yet
            {
                var ac = AudioSettings.GetConfiguration();
#if UNITY_WSA
                if ((int)ConfigState.Instance.SpeakerMode == 0)
                    ConfigState.Instance.SpeakerMode = AudioSpeakerMode.Stereo;
#endif
                ac.speakerMode = ConfigState.Instance.SpeakerMode;
                AudioSettings.Reset(ac);
            }

            //VIDEO CONFIG
            QualitySettings.SetQualityLevel(ConfigState.Instance.GraphicsQuality, true);
            if (ConfigState.Instance.UseCustomVideoSettings)
            {
                ApplyExtendedGraphicsConfiguration();
            }

            if(!CoreParams.IsEditor && CoreParams.Platform != RuntimePlatform.WebGLPlayer)
            {
                var refreshRate = GetClosestAvailableRefreshRate(ConfigState.Instance.Resolution.x, ConfigState.Instance.Resolution.y, ConfigState.Instance.RefreshRate);
                Screen.SetResolution(ConfigState.Instance.Resolution.x, ConfigState.Instance.Resolution.y, ConfigState.Instance.FullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, refreshRate);
            }
                

            QualitySettings.vSyncCount = ConfigState.Instance.VsyncCount;
            Application.targetFrameRate = ConfigState.Instance.MaxFrames;

            //INPUT CONFIG
            MappedInput.SetMapper(ConfigState.Instance.InputMapper); //safe?

            //let other things handle it on their own
            QdmsMessageBus.Instance.PushBroadcast(new ConfigChangedMessage());
            ScriptingModule.CallHooked(ScriptHook.OnConfigChange, this);

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
            QualitySettings.skinWeights = meshQuality.blendWeights;
            //QualitySettings.maximumLODLevel = meshQuality.maximumLODLevel; //is a nop

            //texture scale
            var textureScale = (int)ConfigState.Instance.TextureScale;
            QualitySettings.globalTextureMipmapLimit = textureScale;

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

        private static RefreshRate GetClosestAvailableRefreshRate(int width, int height, int refreshRate)
        {
            //fall back to just trying to set it to whatever was specified
            var fallbackRefreshRate = new RefreshRate() { numerator = (uint)ConfigState.Instance.RefreshRate, denominator = 1 };
            RefreshRate? newRefreshRate = null;

            var resolutions = Screen.resolutions;
            if (resolutions.Length > 0)
            {
                //try to find exact match of resolution and refresh rate
                foreach(var resolution in resolutions) 
                { 
                    if(resolution.height == height && resolution.width ==  width && resolution.refreshRateRatio.Equals(fallbackRefreshRate))
                    {
                        newRefreshRate = resolution.refreshRateRatio;
                        Debug.Log($"Found exact video mode match ({resolution.width}x{resolution.height}@{(double)newRefreshRate.Value.numerator / (double)newRefreshRate.Value.denominator})");
                        break;
                    }
                }
               
                if(!newRefreshRate.HasValue)
                {
                    //otherwise, find matching refresh rate at any resolution
                    foreach(var resolution in resolutions)
                    {
                        if (resolution.refreshRateRatio.Equals(fallbackRefreshRate))
                        {
                            newRefreshRate = resolution.refreshRateRatio;
                            Debug.Log($"Found exact refresh rate match ({(double)newRefreshRate.Value.numerator / (double)newRefreshRate.Value.denominator})");
                            break;
                        }
                    }
                }

                if (!newRefreshRate.HasValue)
                {
                    double smallestDifference = double.MaxValue;
                    double desiredRatio = (double)refreshRate;
                    //else, find the closest available refresh rate
                    foreach (var resolution in resolutions)
                    {
                        double refreshRateRatio = (double)newRefreshRate.Value.numerator / (double)newRefreshRate.Value.denominator;
                        double diff = Math.Abs(refreshRateRatio - desiredRatio);
                        if (diff < smallestDifference)
                        {
                            smallestDifference = refreshRateRatio;
                            newRefreshRate = resolution.refreshRateRatio;
                        }
                    }
                }
            }

            if (newRefreshRate.HasValue && !newRefreshRate.Equals(fallbackRefreshRate))
            {
                Debug.LogWarning($"Available refresh rate mismatch (expected {(double)refreshRate}, got {(double)newRefreshRate.Value.numerator / (double)newRefreshRate.Value.denominator})");
            }
            else if (!newRefreshRate.HasValue)
            {
                Debug.LogWarning("Failed to find any available matching refresh rate, will try to force explicit value");
            }

            return newRefreshRate ?? fallbackRefreshRate;
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
            public Func<Transform, GameObject> Builder;

            public ConfigPanelData(int priority, Func<Transform, GameObject> builder)
            {
                Priority = priority;
                Builder = builder;
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