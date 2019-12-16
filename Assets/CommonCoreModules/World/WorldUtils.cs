using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.State;

namespace CommonCore.World
{

    /// <summary>
    /// General utilities for working with (CommonCore) scenes and the objects in them
    /// </summary>
    public static class WorldUtils
    {

        private static GameObject PlayerObject;

        /// <summary>
        /// Gets the player object (or null if it doesn't exist)
        /// </summary>
        public static GameObject GetPlayerObject()
        {
            if (PlayerObject != null)
                return PlayerObject;

            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                PlayerObject = go;
                return go;
            }

            go = GameObject.Find("Player");

            if (go != null)
            {
                PlayerObject = go;
                return go;
            }

            var tf = CoreUtils.GetWorldRoot().FindDeepChild("Player");

            if (tf != null)
                go = tf.gameObject;

            if (go != null)
            {
                PlayerObject = go;
                return go;
            }

            Debug.LogWarning("Couldn't find player!");

            return null;
        }

        /// <summary>
        /// Finds the player and returns their controller (does not guarantee an actual PlayerController!)
        /// </summary>
        public static BaseController GetPlayerController() //TODO split into Get() and TryGet()
        {
            var pc = WorldUtils.GetPlayerObject()?.GetComponent<BaseController>(); //should be safe because GetPlayerObject returns true null
            if (pc != null)
            {
                return pc;
            }
            else
            {
                Debug.LogWarning("Couldn't find PlayerController!");
                return null;
            }
        }
        
        /// <summary>
        /// Finds a child by name, recursively, and ignores placeholders
        /// </summary>
        public static Transform FindDeepChildIgnorePlaceholders(this Transform aParent, string aName)
        {
            Transform result = null;
            foreach (Transform child in aParent)
            {
                if (child.gameObject.name == aName && child.GetComponent<IPlaceholderComponent>() == null)
                {
                    result = child;
                    break;
                }
            }
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChildIgnorePlaceholders(aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Finds an object by thing ID (name)
        /// </summary>
        public static GameObject FindObjectByTID(string TID)
        {
            var targetTransform = GameObject.Find("WorldRoot").transform.FindDeepChild(TID);
            if (targetTransform != null)
                return targetTransform.gameObject;
            return null;
        }

        /// <summary>
        /// Finds all objects with form ID (entity name)
        /// </summary>
        public static GameObject[] FindObjectsWithFormID(string formID)
        {
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (BaseController c in CoreUtils.GetWorldRoot().gameObject.GetComponentsInChildren<BaseController>(true))
            {
                if (c.FormID == formID)
                {
                    foundObjects.Add(c.gameObject);
                }
            }

            return foundObjects.ToArray();
        }

        /// <summary>
        /// Finds all objects with CommonCore tag
        /// </summary>
        public static GameObject[] FindObjectsWithTag(string tag)
        {
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (BaseController c in CoreUtils.GetWorldRoot().gameObject.GetComponentsInChildren<BaseController>(true))
            {
                if (c.Tags.Contains(tag))
                {
                    foundObjects.Add(c.gameObject);
                }
            }

            return foundObjects.ToArray();
        }

        /// <summary>
        /// Sets parameters and loads a different scene
        /// </summary>
        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading)
        {
            MetaState mgs = MetaState.Instance;
            if (spawnPoint != null)
                mgs.PlayerIntent = new PlayerSpawnIntent(spawnPoint); //handle string.Empty as default spawn point
            else
                mgs.PlayerIntent = new PlayerSpawnIntent(position, Quaternion.Euler(rotation));

            SharedUtils.ChangeScene(scene, skipLoading);
        }

        /// <summary>
        /// Sets parameters and loads a different scene
        /// </summary>
        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation)
        {
            ChangeScene(scene, spawnPoint, position, rotation, false);
        }

        /// <summary>
        /// Spawn an entity into the world (entities/*)
        /// </summary>
        public static GameObject SpawnEntity(string formID, string thingID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CoreUtils.GetWorldRoot();

            var prefab = CoreUtils.LoadResource<GameObject>("Entities/" + formID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;
            if (string.IsNullOrEmpty(thingID))
                thingID = string.Format("{0}_{1}", go.name.Replace("(Clone)", "").Trim(), GameState.Instance.NextUID);
            go.name = thingID;
            return go;
        }

        /// <summary>
        /// Spawn an effect into the world (Effects/*)
        /// </summary>
        public static GameObject SpawnEffect(string effectID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CoreUtils.GetWorldRoot();

            var prefab = CoreUtils.LoadResource<GameObject>("Effects/" + effectID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;
            go.name = string.Format("{0}_{1}", go.name.Replace("(Clone)", "").Trim(), GameState.Instance.NextUID);

            return go;
        }

        /// <summary>
        /// Check if this object is considered a CommonCore Entity
        /// </summary>
        public static bool IsEntity(this GameObject gameObject)
        {
            return gameObject.Ref()?.GetComponent<BaseController>() != null;
        }

        /// <summary>
        /// Checks if this object is considered the player object
        /// </summary>
        public static bool IsPlayer(this GameObject gameObject)
        {
            return gameObject == GetPlayerObject();
        }

        /// <summary>
        /// Checks if this object is considered an "actor" object
        /// </summary>
        public static bool IsActor(this GameObject gameObject)
        {
            var bc = gameObject.Ref()?.GetComponent<BaseController>();
            if (bc != null && bc.Tags.Contains("Actor"))
                return true;

            return false;
        }


    }
}