using CommonCore.DelayedEvents;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.State;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    //EDIT THIS FILE AND PUT YOUR GAME DATA HERE

    public partial class GameState
    {
        // actual instance data

        /// <summary>
        /// [RPGGame] Delayed events that will be checked and executed some time in the future
        /// </summary>
        public List<DelayedEvent> DelayedEvents { get; private set; } = new List<DelayedEvent>();

        /// <summary>
        /// [RPGGame] State data for shared containers (chests, stores, etc)
        /// </summary>
        public Dictionary<string, ContainerModel> ContainerState { get; private set; } = new Dictionary<string, ContainerModel>();

        /// <summary>
        /// [RPGGame] State data for map markers status
        /// </summary>
        public Dictionary<string, MapMarkerState> MapMarkers { get; private set; } = new Dictionary<string, MapMarkerState>();

        /// <summary>
        /// [RPGGame] State data for book/library unlocks
        /// </summary>
        /// <remarks>
        /// In fact, just a set of books the player has unlocked
        /// </remarks>
        public HashSet<string> LibraryUnlocks { get; private set; } = new HashSet<string>();

        /// <summary>
        /// [RPGGame] State data of the player's RPG character (stats, inventory, etc)
        /// </summary>
        public CharacterModel PlayerRpgState { get; private set; } = new CharacterModel();

        /// <summary>
        /// Initializes player state and containers from defs files
        /// </summary>
        /// <remarks>
        /// Player data is in Data/RPGDefs/init_player and container data is in Data/RPGDefs/init_containers
        /// </remarks>
        [Init]
        private void RpgInit()
        {
            //Debug.LogWarning(nameof(RpgInit));

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
                PlayerFlags.RegisterSource(instance.PlayerRpgState);
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

        /// <summary>
        /// Re-registers player flags source after a load
        /// </summary>
        /// <remarks>Will eventually do more, probably</remarks>
        [AfterLoad]
        private void RpgAfterLoad()
        {
            //need to register this since it's lost on load
            PlayerFlags.RegisterSource(PlayerRpgState);
        }

    }

}