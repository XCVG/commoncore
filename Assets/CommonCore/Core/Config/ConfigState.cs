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
        public int QualityLevel { get; set; } = 4;
        public bool FxaaEnabled { get; set; } = true; //TODO add support for selecting TAA and SMAA
        public bool ShowFps { get; set; } = false;

        //INPUT CONFIG
        public string InputMapper { get; set; } = "UnityInputMapper";
        public Dictionary<string, object> InputMapperData { get; set; } = new Dictionary<string, object>();
        public float LookSpeed { get; set; } = 1.0f;
        
    }


}