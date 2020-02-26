using CommonCore.DebugLog;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

namespace CommonCore.Config
{
    public class ConfigState
    {
        private static readonly string Path = CoreParams.PersistentDataPath + "/config.json";

        public static ConfigState Instance { get; private set; }

        public static void Load()
        {
            try
            {
                //backup the config file first
                try
                {
                    if (File.Exists(Path))
                        File.Copy(Path, System.IO.Path.Combine(CoreParams.PersistentDataPath, "config.backup.json"), true);
                }
                catch(Exception)
                {
                    
                }

                Instance = CoreUtils.LoadExternalJson<ConfigState>(Path);
            }
            catch(Exception e)
            {                
                Debug.LogError("[Config] Failed to load config from file, using defaults");
                Debug.LogException(e);
                try
                {
                    if(File.Exists(Path))
                        DebugUtils.TextWrite(File.ReadAllText(Path), "brokenconfig");
                }
                catch(Exception)
                {

                }
            }

            if (Instance == null)
                Instance = new ConfigState();
        }

        public static void Save()
        {
            CoreUtils.SaveExternalJson(Path, Instance);
        }

        //set defaults in constructor
        [JsonConstructor]
        private ConfigState()
        {

        }

        /// <summary>
        /// Adds an object to the custom config vars if and only if it does not already exist
        /// </summary>
        public void AddCustomVarIfNotExists(string name, object customVar)
        {
            if(CustomConfigVars.ContainsKey(name))
            {
                if(CustomConfigVars[name] == null || CustomConfigVars[name].GetType() != customVar.GetType())
                {
                    Debug.LogWarning($"[Config] Custom config var {name} exists but contains a {CustomConfigVars[name]?.GetType()?.Name} instead of a {customVar.GetType().Name}");
                }
            }
            else
            {
                CustomConfigVars.Add(name, customVar);
            }
        }

        /// <summary>
        /// Adds an object to the custom config vars if and only if it does not already exist (function version)
        /// </summary>
        public void AddCustomVarIfNotExists<T>(string name, Func<T> customVarBuilder)
        {
            if (CustomConfigVars.ContainsKey(name))
            {
                if (CustomConfigVars[name] == null || CustomConfigVars[name].GetType() != typeof(T))
                {
                    Debug.LogWarning($"[Config] Custom config var {name} exists but contains a {CustomConfigVars[name]?.GetType()?.Name} instead of a {typeof(T).Name}");
                }
            }
            else
            {
                CustomConfigVars.Add(name, customVarBuilder());
            }
        }

        /// <summary>
        /// Handle (some) config file errors
        /// </summary>
        /// <remarks>
        /// This is mostly to deal with custom variables where the types may no longer exist if a module or addon was added and then removed.
        /// </remarks>
        [OnError]
        internal void OnError(StreamingContext context, ErrorContext errorContext)
        {

            if (errorContext.Path.StartsWith(nameof(CustomConfigVars)))
            {
                Debug.LogWarning($"Failed to load a custom config var (path: {errorContext.Path}). Please check your config file.");
                errorContext.Handled = true;
            }
            else if (errorContext.Path.StartsWith(nameof(InputMapperData)))
            {
                Debug.LogWarning($"Failed to load input mapper data (path: {errorContext.Path}). Please check your config file.");
                errorContext.Handled = true;
            }
            else
            {
                Debug.LogError($"A fatal error occurred during config file loading. Please check your config file. \n{errorContext.Error.Message}");
                //errorContext.Handled = true;
            }

        }

        //actual config data here (WIP)

        //SYSTEM CONFIG
        public bool UseVerboseLogging { get; set; } = true;
        public bool UseCampaignIdentifier { get; set; } = true;
        public float DefaultTimescale { get; set; } = 1;

        //AUDIO CONFIG
        public float MusicVolume { get; set; } = 0.8f;
        public float SoundVolume { get; set; } = 0.8f;
        public AudioSpeakerMode SpeakerMode { get; set; }

        //VIDEO CONFIG
        [JsonIgnore]
        public bool UseCustomVideoSettings => (QualitySettings.GetQualityLevel() >= QualitySettings.names.Length - 1);
        public int MaxFrames { get; set; } = -1;
        public int VsyncCount { get; set; } = 0;
        public int AntialiasingQuality { get; set; } = 1;
        public float ViewDistance { get; set; } = 1000.0f;
        public bool ShowFps { get; set; } = false;
        public float EffectDwellTime { get; set; } = 30;

        //VIDEO CONFIG (EXTENDED)
        public QualityLevel ShadowQuality { get; set; } = QualityLevel.Medium;
        public float ShadowDistance { get; set; } = 40.0f;
        public QualityLevel LightingQuality { get; set; } = QualityLevel.Medium;
        public QualityLevel MeshQuality { get; set; } = QualityLevel.Medium;
        public TextureScale TextureScale { get; set; } = TextureScale.Full;
        public AnisotropicFiltering AnisotropicFiltering { get; set; } = AnisotropicFiltering.Enable;
        public QualityLevel RenderingQuality { get; set; } = QualityLevel.Medium;

        //GAME/GAMEPLAY CONFIG
        public SubtitlesLevel Subtitles { get; set; } = SubtitlesLevel.Always;
        public bool ShakeEffects { get; set; } = true;
        public bool FlashEffects { get; set; } = true;        
        public float GameSpeed { get; set; } = 1; //experimental, must be explicitly handled        
        public int AutosaveCount { get; set; } = 3;

        //INPUT CONFIG
        public string InputMapper { get; set; } = "UnityInputMapper";
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public Dictionary<string, object> InputMapperData { get; set; } = new Dictionary<string, object>();
        public float LookSpeed { get; set; } = 1.0f;
        public bool LookInvert { get; set; } = false;
        public KeyCode ScreenshotKey { get; set; } = KeyCode.F12;
        public KeyCode QuicksaveKey { get; set; } = KeyCode.F5;
        public KeyCode QuickloadKey { get; set; } = KeyCode.F9;

        //EXTRA/GAME-SPECIFIC CONFIG
        public HashSet<string> CustomConfigFlags { get; private set; } = new HashSet<string>();
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public Dictionary<string, object> CustomConfigVars { get; private set; } = new Dictionary<string, object>(); //note that serialization/deserialization can explode in edge cases

    }


}