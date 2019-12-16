using CommonCore.DebugLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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


        //actual config data here (WIP)

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

        //VIDEO CONFIG (EXTENDED)
        public QualityLevel ShadowQuality { get; set; } = QualityLevel.Medium;
        public float ShadowDistance { get; set; } = 40.0f;
        public QualityLevel LightingQuality { get; set; } = QualityLevel.Medium;
        public QualityLevel MeshQuality { get; set; } = QualityLevel.Medium;
        public TextureScale TextureScale { get; set; } = TextureScale.Full;
        public AnisotropicFiltering AnisotropicFiltering { get; set; } = AnisotropicFiltering.Enable;
        public QualityLevel RenderingQuality { get; set; } = QualityLevel.Medium;


        //INPUT CONFIG
        public string InputMapper { get; set; } = "UnityInputMapper";
        public Dictionary<string, object> InputMapperData { get; set; } = new Dictionary<string, object>();
        public float LookSpeed { get; set; } = 1.0f;
        public KeyCode ScreenshotKey { get; set; } = KeyCode.F12;

        //EXTRA/GAME-SPECIFIC CONFIG
        public HashSet<string> CustomConfigFlags { get; private set; } = new HashSet<string>();
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public Dictionary<string, object> CustomConfigVars { get; private set; } = new Dictionary<string, object>(); //note that serialization/deserialization can explode in edge cases
        
    }


}