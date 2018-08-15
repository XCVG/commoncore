using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CommonCore.World;
using CommonCore.Rpg;
using UnityEngine;
using WanzyeeStudio;

namespace CommonCore.State
{ 
    //TODO move away from the static singleton model
    public sealed class GameState
    {
        private static GameState instance;

        private GameState()
        {
            WorldState = new WorldModel();
            CampaignState = new CampaignModel();
            LocalDataState = new Dictionary<string, Dictionary<string, object>>();
            LocalObjectState = new Dictionary<string, Dictionary<string, RestorableData>>(); //maps will have to deal with initialization/non-initialization themselves or on load
            MotileObjectState = new Dictionary<string, RestorableData>();
            PlayerRpgState = new CharacterModel();
        }

        public static GameState Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new GameState();
                }
                return instance;
            }
        }        

        public static void Reset()
        {
            instance = new GameState();
        }

        public static void SerializeToFile(string path)
        {
            string data = Serialize();
            File.WriteAllText(path, data);
        }

        public static string Serialize()
        {
            return JsonConvert.SerializeObject(Instance,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void DeserializeFromFile(string path)
        {
            Deserialize(File.ReadAllText(path));
        }

        public static void Deserialize(string data)
        {
            instance = JsonConvert.DeserializeObject<GameState>(data,
            new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void LoadInitial()
        {
            //load initial player
            try
            {
                instance.PlayerRpgState = new CharacterModel();
                JsonConvert.PopulateObject(CCBaseUtil.LoadResource<TextAsset>("RPGDefs/init_player").text, instance.PlayerRpgState, new JsonSerializerSettings
                {
                    Converters = JsonNetUtility.defaultSettings.Converters,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore
                });
                instance.PlayerRpgState.UpdateStats();
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to load initial player");
                Debug.LogException(e);
            }

            //load initial containers
        }

        // actual instance data
        public WorldModel WorldState;
        public CampaignModel CampaignState;

        public Dictionary<string, object> GlobalDataState;
        public Dictionary<string, Dictionary<string, object>> LocalDataState;

        public Dictionary<string, Dictionary<string, RestorableData>> LocalObjectState;        
        public Dictionary<string, RestorableData> MotileObjectState;

        public Dictionary<string, ContainerModel> ContainerState;

        public RestorableData PlayerWorldState;
        public CharacterModel PlayerRpgState;

        public string CurrentScene;
        public bool SaveLocked;

        [JsonProperty]
        private int CurrentUID;
        public int NextUID { get { return ++CurrentUID; } }

    }

}