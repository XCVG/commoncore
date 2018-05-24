using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CommonCore.Config
{
    public class ConfigState
    {
        private static readonly string Path = CCParams.PersistentDataPath + "/config.json";

        public static ConfigState Instance { get; private set; }

        internal static void Load()
        {
            Instance = CCBaseUtil.LoadExternalJson<ConfigState>(Path);
            if (Instance == null)
                Instance = new ConfigState();
        }

        internal static void Save()
        {
            CCBaseUtil.SaveExternalJson(Path, Instance);
        }

        //set defaults in constructor
        [JsonConstructor]
        private ConfigState()
        {

        }

        //actual config data here (TODO)
        
    }
}