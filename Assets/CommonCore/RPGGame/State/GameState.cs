using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using CommonCore.DelayedEvents;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;

namespace CommonCore.State
{
    //EDIT THIS FILE AND PUT YOUR GAME DATA HERE

    public partial class GameState
    {
        // actual instance data
        public WorldModel WorldState = new WorldModel();
        public CampaignModel CampaignState = new CampaignModel();

        public List<DelayedEvent> DelayedEvents = new List<DelayedEvent>();

        public Dictionary<string, object> GlobalDataState = new Dictionary<string, object>();
        public Dictionary<string, Dictionary<string, object>> LocalDataState = new Dictionary<string, Dictionary<string, object>>();

        public Dictionary<string, Dictionary<string, RestorableData>> LocalObjectState = new Dictionary<string, Dictionary<string, RestorableData>>();
        public Dictionary<string, RestorableData> MotileObjectState = new Dictionary<string, RestorableData>();

        public Dictionary<string, ContainerModel> ContainerState = new Dictionary<string, ContainerModel>();
        public Dictionary<string, MapMarkerState> MapMarkers = new Dictionary<string, MapMarkerState>();
        public HashSet<string> LibraryUnlocks = new HashSet<string>();

        public RestorableData PlayerWorldState;
        public CharacterModel PlayerRpgState = new CharacterModel();

        public string CurrentScene;
        public bool SaveLocked;
        public bool InitialLoaded; //mostly for editor hacks

        [JsonProperty]
        private int CurrentUID;
        [JsonIgnore]
        public int NextUID { get { return ++CurrentUID; } }

        partial void Init()
        {
            //TODO better debugging and logging

            //load initial player
            try
            {
                instance.PlayerRpgState = new CharacterModel();
                JsonConvert.PopulateObject(CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/init_player").text, instance.PlayerRpgState, new JsonSerializerSettings
                {
                    Converters = CCJsonConverters.Defaults.Converters,
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore
                });
                instance.PlayerRpgState.UpdateStats();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load initial player");
                Debug.LogException(e);
            }

            //load initial containers (requires some decoding)
            //we will actually need to load additional containers ex post facto when we add mod support
            try
            {
                var rawContainers = CoreUtils.LoadJson<Dictionary<string, SerializableContainerModel>>(CoreUtils.LoadResource<TextAsset>("Data/RPGDefs/init_containers").text);
                foreach (var key in rawContainers.Keys)
                {
                    var value = rawContainers[key];
                    try
                    {
                        var realContainer = SerializableContainerModel.MakeContainerModel(value);
                        instance.ContainerState.Add(key, realContainer);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Failed to load one container");
                        Debug.LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load initial container state");
                Debug.LogException(e);
            }

            instance.InitialLoaded = true;
        }

    }

}