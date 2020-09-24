using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore.Audio;
using CommonCore.Scripting;
using CommonCore.Messaging;
using CommonCore.Config;

namespace CommonCore.World
{

    public class WorldSceneController : BaseSceneController
    {
        public bool AllowQuicksave = true;
        public bool AutoGameover = true;
        public string SetMusic;
        public Bounds WorldBounds = new Bounds(Vector3.zero, new Vector3(2000f, 2000f, 1000f));

        public SetPlayerFlagsSource TempPlayerFlags { get; private set; } = new SetPlayerFlagsSource();

        protected override bool DeferAfterSceneLoadToSubclass => true;
        protected override bool DeferEnterAutosaveToSubclass => true;
        protected override bool DeferInitialRestoreToSubclass => true;
        protected override bool AllowQuicksaveInScene => AllowQuicksave;
        protected override string DefaultHud => "DefaultWorldHUD";

        public override void Awake()
        {
            base.Awake();

            GameState.Instance.PlayerFlags.RegisterSource(TempPlayerFlags);

            if (ConfigState.Instance.UseVerboseLogging)
                Debug.Log("World Scene Controller Awake");
        }

        public override void Start()
        {
            base.Start();
            if (ConfigState.Instance.UseVerboseLogging)
                Debug.Log("World Scene Controller Start");
            if(AutoRestore)
            {
                MetaState.Instance.IntentsExecutePreload();
                Restore();
                MetaState.Instance.IntentsExecutePostload();
            }

            if (!string.IsNullOrEmpty(SetMusic))
            {
                AudioPlayer.Instance.SetMusic(SetMusic, MusicSlot.Ambient, 1.0f, true, false);
                AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);
            }

            ScriptingModule.CallHooked(ScriptHook.AfterSceneLoad, this);
            if(AutosaveOnEnter && MetaState.Instance.TransitionType != SceneTransitionType.LoadGame)
                SaveUtils.DoAutoSave();
        }

        public override void Update()
        {
            base.Update();

        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            GameState.Instance.PlayerFlags.UnregisterSource(TempPlayerFlags);
        }

        protected override bool HandleMessage(QdmsMessage message)
        {
            if (base.HandleMessage(message))
            {
                return true;
            }
            else if(message is QdmsFlagMessage flagMessage)
            {
                switch (flagMessage.Flag)
                {
                    case "PlayerDead":
                    case "EndGame":
                        HandleGameOver();
                        return true;
                }
            }

            return false;
            
        }

        private void HandleGameOver()
        {
            if (!AutoGameover)
                return;

            SharedUtils.ShowGameOver();
        }

        public override void Commit()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;
            gs.CurrentScene = name;
            Debug.Log("Saving scene: " + name);

            //get restorable components
            List<RestorableComponent> rcs = new List<RestorableComponent>();
            rcs.AddRange(transform.gameObject.GetComponentsInChildren<RestorableComponent>());
            //WorldUtils.GetComponentsInDescendants(transform, rcs);

            //purge local object state
            Dictionary<string, RestorableData> localState;
            if (gs.LocalObjectState.ContainsKey(name))
            {
                localState = gs.LocalObjectState[name];
                localState.Clear();
            }
            else
            {
                localState = new Dictionary<string, RestorableData>();
                gs.LocalObjectState[name] = localState;
            }

            foreach (RestorableComponent rc in rcs)
            {
                try
                {
                    if (localState.ContainsKey(rc.gameObject.name))
                        Debug.LogWarning($"Committed an object with duplicate TID ({rc.gameObject.name})");

                    RestorableData rd = rc.Save();
                    if (rc is LocalRestorableComponent || rc is BlankRestorableComponent)
                    {
                        localState[rc.gameObject.name] = rd;
                    }
                    else if (rc is MotileRestorableComponent)
                    {
                        gs.MotileObjectState[rc.gameObject.name] = rd;
                    }
                    else if (rc is PlayerRestorableComponent)
                    {
                        gs.PlayerWorldState = rd;
                    }
                    else
                    {
                        Debug.LogWarning("Unknown restorable type in " + rc.gameObject.name);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to save an object!");
                    Debug.LogException(e);
                }
            }

            //purge and copy local data store
            if (gs.LocalDataState.ContainsKey(name))
            {
                gs.LocalDataState.Remove(name);
            }
            gs.LocalDataState.Add(name, LocalStore);

        }

        public override void Restore()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;

            Debug.Log("Restoring scene: " + name);

            //restore local store
            LocalStore = gs.LocalDataState.ContainsKey(name) ? gs.LocalDataState[name] : new Dictionary<string, System.Object>();

            //activate entity placeholders
            ActivateEntityPlaceholders();

            //restore local object state
            RestoreLocalObjects(gs, name);

            //restore motile objects
            RestoreMotileObjects(gs, name);

            //restore player
            RestorePlayer(gs);

        }

        protected void ActivateEntityPlaceholders()
        {
            var entityPlaceholders = CoreUtils.GetWorldRoot().GetComponentsInChildren<EntityPlaceholder>(false);
            foreach (var ep in entityPlaceholders)
            {
                try
                {
                    ep.SpawnEntity();
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to activate an entity placeholder!");
                    Debug.LogException(e);
                }
            }
        }

        protected void RestoreLocalObjects(GameState gs, string name)
        {
            if (gs.LocalObjectState.ContainsKey(name))
            {
                Dictionary<string, RestorableData> localState = gs.LocalObjectState[name];

                foreach (KeyValuePair<string, RestorableData> kvp in localState)
                {
                    try
                    {
                        if (kvp.Value is DynamicRestorableData)
                            RestoreLocalObject(kvp);
                        else
                            RestoreBlankObject(kvp);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError("Failed to restore an object!");
                        Debug.LogException(e);
                    }
                }
            }
            else
            {
                //no data, skip local
                Debug.Log("No local object data for scene!");
            }
        }

        private void RestoreBlankObject(KeyValuePair<string, RestorableData> kvp)
        {
            Transform t = transform.FindDeepChildIgnorePlaceholders(kvp.Key);
            if (t != null)
            {
                GameObject go = t.gameObject;

                //if it exists, restore it
                BlankRestorableComponent rc = go.GetComponent<BlankRestorableComponent>();
                if (rc != null)
                {
                    rc.Restore(kvp.Value);
                }
                else
                {
                    Debug.LogWarning("Blank object " + go.name + " has no restorable component!");
                }
            }
            else
            {
                Debug.LogWarning("Attempted to restore " + kvp.Key + " but object doesn't exist!");
            }
        }

        private void RestoreLocalObject(KeyValuePair<string, RestorableData> kvp)
        {
            DynamicRestorableData rd = kvp.Value as DynamicRestorableData;

            Transform t = transform.FindDeepChildIgnorePlaceholders(kvp.Key);

            if (t != null)
            {
                GameObject go = t.gameObject;

                //if it exists, restore it
                LocalRestorableComponent rc = go.GetComponent<LocalRestorableComponent>();
                if (rc != null)
                {
                    rc.Restore(rd);
                }
                else
                {
                    Debug.LogWarning("Local object " + go.name + " has no restorable component!");
                }
            }
            else
            {
                //if it doesn't, create it
                try
                {
                    GameObject go = Instantiate(CoreUtils.LoadResource<GameObject>("Entities/" + rd.FormID), transform) as GameObject;

                    if (go != null)
                    {
                        go.name = kvp.Key;

                        LocalRestorableComponent rc = go.GetComponent<LocalRestorableComponent>();
                        if (rc != null)
                        {
                            rc.Restore(rd);
                        }
                        else
                        {
                            Debug.LogWarning("Local object " + go.name + " has no restorable component!");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Debug.LogWarning("Tried to spawn " + rd.FormID + " but couldn't find prefab!");
                }
            }
        }

        protected void RestoreMotileObjects(GameState gs, string name)
        {
            foreach (KeyValuePair<string, RestorableData> kvp in gs.MotileObjectState)
            {
                try
                {
                    DynamicRestorableData rd = kvp.Value as DynamicRestorableData;

                    if (rd == null)
                    {
                        Debug.LogError("Local object " + kvp.Key + " has invalid data!");
                    }

                    //is it in this scene
                    string objectSceneName = rd.Scene;
                    if (objectSceneName == name)
                    {
                        //we have a match!
                        try
                        {
                            Transform t = transform.FindDeepChildIgnorePlaceholders(kvp.Key);
                            GameObject go = null;
                            if (t == null)
                                go = Instantiate(CoreUtils.LoadResource<GameObject>("Entities/" + rd.FormID), transform) as GameObject;
                            else
                                go = t.gameObject;
                            //this *should* work but hasn't been tested

                            {
                                go.name = kvp.Key;

                                MotileRestorableComponent mrc = go.GetComponent<MotileRestorableComponent>();
                                if (mrc != null)
                                {
                                    mrc.Restore(rd);
                                }
                                else
                                {
                                    Debug.LogWarning("Motile object " + go.name + " has no restorable component!");
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                            Debug.LogWarning("Tried to spawn " + rd.FormID + " but couldn't find prefab!");
                        }

                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to restore an object!");
                    Debug.LogException(e);
                }
            }
        }

        protected void RestorePlayer(GameState gs)
        {
            MetaState mgs = MetaState.Instance;
            GameObject player = WorldUtils.GetPlayerObject();
            RestorableData prd = gs.PlayerWorldState;
            if (prd != null)
            {
                if (player == null)
                {
                    //spawn the player object in
                    player = Instantiate(CoreUtils.LoadResource<GameObject>("Entities/" + "spec_player"), transform) as GameObject;
                    player.name = "Player";
                    if (mgs.TransitionType == SceneTransitionType.LoadGame)
                    {
                        player.GetComponent<PlayerRestorableComponent>().Restore(prd);
                    }
                    else
                    {
                        // get intent and move
                        RestorePlayerToIntent(mgs, player);
                    }

                }
                else
                {
                    //restore player if relevant, warn either way
                    if (mgs.TransitionType == SceneTransitionType.LoadGame)
                    {
                        player.GetComponent<PlayerRestorableComponent>().Restore(prd);
                        Debug.LogWarning("Player already exists, restoring anyway");
                    }
                    else
                    {
                        //if an intent exists, move
                        RestorePlayerToIntent(mgs, player);
                        Debug.LogWarning("Player already exists");
                    }


                }
            }
            else
            {
                if(player == null)
                {
                    player = Instantiate(CoreUtils.LoadResource<GameObject>("Entities/" + "spec_player"), transform) as GameObject;
                    player.name = "Player";
                }
                    

                RestorePlayerToIntent(mgs, player);

                //warn that no player data exists
                Debug.LogWarning("No player world data exists!");
            }

            ScriptingModule.CallHooked(ScriptHook.OnPlayerSpawn, this, player);
        }

        private void RestorePlayerToIntent(MetaState mgs, GameObject player)
        {
            if (mgs.PlayerIntent != null)
            {
                if (!string.IsNullOrEmpty(mgs.PlayerIntent.SpawnPoint))
                {
                    GameObject spawnPoint = WorldUtils.FindObjectByTID(mgs.PlayerIntent.SpawnPoint);
                    player.transform.position = spawnPoint.transform.position;
                    player.transform.rotation = spawnPoint.transform.rotation;
                }
                else if(mgs.PlayerIntent.SpawnPoint != null) //not null, but is empty
                {
                    GameObject spawnPoint = WorldUtils.FindObjectByTID("DefaultPlayerSpawn");
                    if(spawnPoint != null)
                    {
                        player.transform.position = spawnPoint.transform.position;
                        player.transform.rotation = spawnPoint.transform.rotation;
                    }                    
                }
                else
                {
                    player.transform.position = mgs.PlayerIntent.Position;
                    player.transform.rotation = mgs.PlayerIntent.Rotation;
                }
            }
            else
            {

                GameObject spawnPoint = WorldUtils.FindObjectByTID("DefaultPlayerSpawn");
                if (spawnPoint != null)
                {
                    player.transform.position = spawnPoint.transform.position;
                    player.transform.rotation = spawnPoint.transform.rotation;
                }

                Debug.LogWarning("No player spawn intent exists!");
            }
        }

    }

}