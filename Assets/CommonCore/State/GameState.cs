using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CommonCore.World;
using CommonCore.Rpg;
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
            PlayerRpgState = new PlayerModel();
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

        // actual instance data
        public WorldModel WorldState;
        public CampaignModel CampaignState;
        public Dictionary<string, Dictionary<string, object>> LocalDataState;
        public Dictionary<string, Dictionary<string, RestorableData>> LocalObjectState;        
        public Dictionary<string, RestorableData> MotileObjectState;
        public RestorableData PlayerWorldState;
        public PlayerModel PlayerRpgState;
        public string CurrentScene;

    }

}