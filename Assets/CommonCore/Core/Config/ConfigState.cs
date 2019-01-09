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
            MusicVolume = 0.8f;
            SoundVolume = 0.8f;

            QualityLevel = 4;
            FxaaEnabled = true;

            LookSpeed = 1.0f;
        }

        //actual config data here (TODO)

        //AUDIO CONFIG
        public float MusicVolume { get; set; }
        public float SoundVolume { get; set; }
        public AudioSpeakerMode SpeakerMode { get; set; }

        //VIDEO CONFIG
        public int QualityLevel { get; set; }
        public bool FxaaEnabled { get; set; }

        //INPUT CONFIG
        public float LookSpeed { get; set; }
        
    }


}