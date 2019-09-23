using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CommonCore.Config
{
    public class ConfigState
    {
        private static readonly string Path = CoreParams.PersistentDataPath + "/config.json";

        public static ConfigState Instance { get; private set; }

        public static void Load()
        {
            Instance = CoreUtils.LoadExternalJson<ConfigState>(Path);
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