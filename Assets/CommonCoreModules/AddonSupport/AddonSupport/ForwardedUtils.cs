using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.AddonSupport
{

    /// <summary>
    /// Utility methods forwarded from SharedUtils, SaveUtils, WorldUtils (and others?)
    /// </summary>
    public static class ForwardedUtils
    {
        #region SharedUtils

        private const string SharedUtilsTypeName = "CommonCore.SharedUtils";

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to default start scene
        /// </summary>
        public static void StartGame()
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "StartGame");
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void StartGame(string sceneOverride)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "StartGame", sceneOverride);
        }

        /// <summary>
        /// Transitions to the GameOverScene, does not clear game data
        /// </summary>
        public static void ShowGameOver()
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "ShowGameOver");
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to main menu scene
        /// </summary>
        public static void EndGame()
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "EndGame");
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void EndGame(string sceneOverride)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "EndGame", sceneOverride);
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "ChangeScene", scene);
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene, bool skipLoading)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "ChangeScene", scene, skipLoading);

        }

        /// <summary>
        /// Loads a saved game to state and transitions to its scene
        /// </summary>
        /// <param name="saveName">The name of the save file, with prefix and extension but without path</param>
        public static void LoadGame(string saveName, bool force)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "LoadGame", saveName, force);
        }

        /// <summary>
        /// Saves the current state to file
        /// </summary>
        /// <param name="saveName">The name of the save file, with prefix and extension but without path</param>
        /// <param name="commit">Whether to commit or not</param>
        public static void SaveGame(string saveName, bool commit, bool force)
        {
            ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "SaveGame", saveName, commit, force);
        }

        /// <summary>
        /// Gets the scene controller (returns null on fail)
        /// </summary>
        public static MonoBehaviour TryGetSceneController()
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "TryGetSceneController");
        }

        /// <summary>
        /// Gets the scene controller (throws on fail)
        /// </summary>
        public static MonoBehaviour GetSceneController()
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "GetSceneController");
        }

        /// <summary>
        /// Gets the HUD controller (returns null on fail)
        /// </summary>
        public static MonoBehaviour TryGetHudController()
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "TryGetHudController");
        }

        /// <summary>
        /// Gets the HUD controller (throws on fail)
        /// </summary>
        public static MonoBehaviour GetHudController()
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(SharedUtilsTypeName, "GetHudController");
        }

        #endregion

        #region SaveUtils

        private const string SaveUtilsTypeName = "CommonCore.State.SaveUtils";

        /// <summary>
        /// Gets a safe name for a save
        /// </summary>
        public static string GetSafeName(string name)
        {
            return (string)ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "GetSafeName", name);
        }

        /// <summary>
        /// Gets the last save file, or null if it doesn't exist
        /// </summary>
        public static string GetLastSave()
        {
            return (string)ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "GetLastSave");
        }

        /// <summary>
        /// Gets the clean save path given a save name which may or may not contain a path and may or may not contain an extension
        /// </summary>
        public static string GetCleanSavePath(string saveName)
        {
            return (string)ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "GetCleanSavePath", saveName);
        }

        /// <summary>
        /// Creates default metadata with specified nicename, drawing other info from game state and thumbnail generation script
        /// </summary>
        public static object CreateDefaultMetadata(string niceName)
        {
            return ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "CreateDefaultMetadata", niceName);
        }

        /// <summary>
        /// Loads save metadata from a save file
        /// </summary>
        public static object LoadSaveMetadata(string saveName)
        {
            return ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "LoadSaveMetadata", saveName);
        }

        //all non-Ex should be "safe" (log errors instead of throwing) and check conditions themselves

        /// <summary>
        /// Creates a quicksave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        /// <remarks>Defaults to commit=true</remarks>
        public static void DoQuickSave()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoQuickSave");
        }

        /// <summary>
        /// Creates a quicksave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoQuickSave(bool commit)
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoQuickSave", commit);
        }

        /// <summary>
        /// Creates a quicksave
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoQuickSaveEx(bool commit, bool force)
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoQuickSaveEx", commit, force);
        }

        /// <summary>
        /// Loads the quicksave if it exists
        /// </summary>
        public static void DoQuickLoad()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoQuickLoad");
        }

        /// <summary>
        /// Loads the quicksave if it exists
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoQuickLoadEx()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoQuickLoadEx");
        }

        /// <summary>
        /// Creates an autosave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        /// <remarks>Defaults to commit=false</remarks>
        public static void DoAutoSave()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoAutoSave");
        }

        /// <summary>
        /// Creates an autosave, if allowed to do so, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoAutoSave(bool commit)
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoAutoSave", commit);
        }

        /// <summary>
        /// Creates an autosave
        /// </summary>
        /// <remarks>Note that this does not display the indicator and can throw exceptions</remarks>
        public static void DoAutoSaveEx(bool commit, bool force)
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoAutoSaveEx", commit, force);
        }

        /// <summary>
        /// Creates a full finalsave, displaying an indicator and suppressing exceptions
        /// </summary>
        public static void DoFinalSave()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoFinalSave");
        }

        /// <summary>
        /// Creates a full finalsave
        /// </summary>
        /// <remarks>
        /// <para>Ignores all restrictions on saving; finalsaves are special</para>
        /// <para>Does not commit scene state before saving</para>
        /// </remarks>
        public static void DoFinalSaveEx()
        {
            ProxyUtils.InvokeStaticProxied(SaveUtilsTypeName, "DoFinalSaveEx");
        }

        #endregion

        #region SceneUtils

        private const string SceneUtilsTypeName = "CommonCore.SceneUtils";

        public static void DestroyAllChildren(Transform root)
        {
            ProxyUtils.InvokeStaticProxied(SceneUtilsTypeName, "DestroyAllChildren");
        }

        public static Transform FindDeepChild(Transform aParent, string aName)
        {
            return (Transform)ProxyUtils.InvokeStaticProxied(SceneUtilsTypeName, "FindDeepChild", aParent, aName);
        }

        public static List<Transform> FindDeepChildren(Transform aParent, string aName)
        {
            return (List<Transform>)ProxyUtils.InvokeStaticProxied(SceneUtilsTypeName, "FindDeepChildren", aParent, aName);
        }

        #endregion

        #region WorldUtils

        private const string WorldUtilsTypeName = "CommonCore.World.WorldUtils";

        /// <summary>
        /// Gets the player object (or null if it doesn't exist)
        /// </summary>
        public static GameObject GetPlayerObject()
        {
            return (GameObject)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "GetPlayerObject");
        }

        /// <summary>
        /// Finds the player and returns their controller (does not guarantee an actual PlayerController!)
        /// </summary>
        public static MonoBehaviour GetPlayerController()
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "GetPlayerController");
        }

        /// <summary>
        /// Finds the default player spawn point
        /// </summary>
        /// <remarks>
        /// <para>Selects "DefaultPlayerSpawn" from active PlayerSpawnPoints, then "DefaultPlayerSpawn" from active without PlayerSpawnPoint, then any active PlayerSpawnPoint</para>
        /// </remarks>
        public static GameObject FindDefaultPlayerSpawn()
        {
            return (GameObject)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindDefaultPlayerSpawn");
        }

        /// <summary>
        /// Finds the player spawn point by name
        /// </summary>
        /// <remarks>
        /// <para>Selects from active PlayerSpawnPoints, then active without PlayerSpawnPoint</para>
        /// </remarks>
        public static GameObject FindPlayerSpawn(string spawnPointName)
        {
            return (GameObject)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindPlayerSpawn", spawnPointName);
        }

        /// <summary>
        /// Checks if this scene is considered a world scene (ie has WorldSceneController)
        /// </summary>
        public static bool IsWorldScene()
        {
            return (bool)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "IsWorldScene");
        }

        /// <summary>
        /// Finds a child by name, recursively, and ignores placeholders
        /// </summary>
        public static Transform FindDeepChildIgnorePlaceholders(Transform aParent, string aName)
        {
            return (Transform)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindDeepChildIgnorePlaceholders", aParent, aName);
        }

        /// <summary>
        /// Finds all children by name, recursively, and ignores placeholders
        /// </summary>
        public static List<Transform> FindDeepChildrenIgnorePlaceholders(Transform aParent, string aName)
        {
            return (List<Transform>)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindDeepChildrenIgnorePlaceholders", aParent, aName);
        }

        /// <summary>
        /// Finds an object by thing ID (name)
        /// </summary>
        public static GameObject FindObjectByTID(string TID)
        {
            return (GameObject)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindObjectByTID", TID);
        }

        /// <summary>
        /// Finds an entity by thing ID (name)
        /// </summary>
        public static MonoBehaviour FindEntityByTID(string TID)
        {
            return (MonoBehaviour)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindEntityByTID", TID);
        }

        /// <summary>
        /// Finds all entities with form ID (entity name)
        /// </summary>
        public static IList FindEntitiesWithFormID(string formID)
        {
            return (IList)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindEntitiesWithFormID", formID);
        }

        /// <summary>
        /// Finds all entities with CommonCore tag
        /// </summary>
        public static IList FindEntitiesWithTag(string tag)
        {
            return (IList)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "FindEntitiesWithTag", tag);
        }

        /// <summary>
        /// Checks if an ITakeDamage is considered alive
        /// </summary>
        public static bool IsDamageableAlive(object itd)
        {
            return (bool)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "IsDamageableAlive", itd);
        }

        /// <summary>
        /// Checks if an entity is considered alive
        /// </summary>
        public static bool IsEntityAlive(MonoBehaviour entity)
        {
            return (bool)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "IsEntityAlive", entity);
        }

        /// <summary>
        /// Checks if an object is considered alive
        /// </summary>
        public static bool IsObjectAlive(GameObject obj)
        {
            return (bool)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "IsObjectAlive", new InvokeStaticProxyOptions() { MatchParameterTypes = true, ParameterMatchTypes = new Type[] { typeof(GameObject) } }, obj);
        }

        /// <summary>
        /// Checks if an object is considered alive
        /// </summary>
        public static bool IsObjectAlive(Transform transform)
        {
            return (bool)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "IsObjectAlive", new InvokeStaticProxyOptions() { MatchParameterTypes = true, ParameterMatchTypes = new Type[] { typeof(Transform) } }, transform);
        }

        /// <summary>
        /// Sets parameters and loads a different scene
        /// </summary>
        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Quaternion rotation, bool skipLoading)
        {
            ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "ChangeScene", new InvokeStaticProxyOptions() { MatchParameterTypes = true,
                ParameterMatchTypes = new Type[] {typeof(string), typeof(string), typeof(Vector3), typeof(Quaternion), typeof(bool) } },
                scene, spawnPoint, position, rotation, skipLoading);
        }

        /// <summary>
        /// Spawn an entity into the world (entities/*)
        /// </summary>
        public static GameObject SpawnEntity(string formID, string thingID, Vector3 position, Quaternion rotation, Transform parent)
        {
            return (GameObject)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SpawnEntity", new InvokeStaticProxyOptions()
            {
                MatchParameterTypes = true,
                ParameterMatchTypes = new Type[] { typeof(string), typeof(string), typeof(Vector3), typeof(Quaternion), typeof(Transform) }
            },
                formID, thingID, position, rotation, parent);
        }

        /// <summary>
        /// Spawn an effect into the world (Effects/*)
        /// </summary>
        public static GameObject SpawnEffect(string effectID, Vector3 position, Quaternion rotation, Transform parent, bool useUniqueId)
        {
            return (GameObject)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SpawnEffect", new InvokeStaticProxyOptions()
            {
                MatchParameterTypes = true,
                ParameterMatchTypes = new Type[] { typeof(string), typeof(Vector3), typeof(Quaternion), typeof(Transform), typeof(bool) }
            },
               effectID, position, rotation, parent, useUniqueId);
        }


        /// <summary>
        /// Check if this object is considered a CommonCore Entity
        /// </summary>
        public static bool IsEntity(GameObject gameObject)
        {
            return (bool)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "IsEntity", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, gameObject);
        }

        /// <summary>
        /// Checks if this object is considered the player object
        /// </summary>
        public static bool IsPlayer(GameObject gameObject)
        {
            return (bool)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "IsPlayer", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, gameObject);
        }

        /// <summary>
        /// Checks if this object is considered an "actor" object
        /// </summary>
        public static bool IsActor(GameObject gameObject)
        {
            return (bool)ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "IsActor", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, gameObject);
        }

        /// <summary>
        /// Gets the currently active "main" camera
        /// </summary>
        /// <remarks>
        /// <para>The logic for this is different than Camera.main. It searches the player object, if it exists, first.</para>
        /// <para>Note that this is potentially very slow: it has good best-case but horrendous worst-case performance.</para>
        /// </remarks>
        public static Camera GetActiveCamera()
        {
            return (Camera)ProxyUtils.InvokeStaticProxied(WorldUtilsTypeName, "GetActiveCamera");
        }

        /// <summary>
        /// Sets this audio listener active, disabling all other audio listeners
        /// </summary>
        public static void SetAudioListener(AudioListener audioListener)
        {
            ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SetAudioListener", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, audioListener);
        }

        /// <summary>
        /// Sets the audio listener on the camera active (it will be created if it does not exist), disabling all other audio listeners
        /// </summary>
        public static void SetAudioListener(Camera camera)
        {
            ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SetAudioListener", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, camera);
        }

        /// <summary>
        /// Sets the audio listener on this entity active (it will be created if it does not exist), disabling all other audio listeners
        /// </summary>
        public static void SetAudioListener(MonoBehaviour controller)
        {
            ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SetAudioListener", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, controller);
        }

        /// <summary>
        /// Sets the audio listener on this object active (it will be created if it does not exist), disabling all other audio listeners
        /// </summary>
        public static void SetAudioListener(GameObject obj)
        {
            ProxyUtils.InvokeStaticProxiedEx(WorldUtilsTypeName, "SetAudioListener", new InvokeStaticProxyOptions() { MatchParameterTypes = true }, obj);
        }

        #endregion
    }
}