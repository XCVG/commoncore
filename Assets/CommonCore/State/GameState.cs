using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CommonCore.World;
using CommonCore.Rpg;

namespace CommonCore.State
{ 
    //TODO move away from the static singleton model
    public sealed class GameState
    {
        private static GameState instance;

        private GameState()
        {
            WorldState = new WorldModel();
            LocalDataState = new Dictionary<string, Dictionary<string, string>>();
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
            return JsonConvert.SerializeObject(Instance);
        }

        public static void DeserializeFromFile(string path)
        {
            Deserialize(File.ReadAllText(path));
        }

        public static void Deserialize(string data)
        {
            instance = JsonConvert.DeserializeObject<GameState>(data);
        }

        // actual instance data
        public WorldModel WorldState;
        public Dictionary<string, Dictionary<string, string>> LocalDataState; //TODO fully implement this
        public Dictionary<string, Dictionary<string, RestorableData>> LocalObjectState;        
        public Dictionary<string, RestorableData> MotileObjectState;
        public RestorableData PlayerWorldState;
        public PlayerModel PlayerRpgState;
        //TODO more RPG/quest/etc data

    }

}