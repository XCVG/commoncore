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
                
        /// <summary>
        /// Gets a list of scenes (by name) in the game
        /// </summary>
        /// <returns>A list of scenes in the game</returns>
        public static string[] GetSceneList() //TODO we'll probably move this into some kind of CommonCore.SceneManagement
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new List<string>(sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                try
                {
                    scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                }
                catch (Exception e)
                {
                    //ignore it, we've gone over or some stupid bullshit
                }

            }

            return scenes.ToArray();
            
        }

        /// <summary>
        /// Finds all game objects with a given name. No, I don't know what it's for either.
        /// </summary>
        public static List<GameObject> FindAllGameObjects(string name)
        {
            var goList = new List<GameObject>();

            foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                if (go.name == name)
                    goList.Add(go);
            }

            return goList;
        }

        [Obsolete("use GameObject.GetComponentsInChildren<T> instead")]
        public static List<T> GetComponentsInDescendants<T>(Transform root)
        {
            List<T> components = new List<T>();

            GetComponentsInDescendants<T>(root, components);

            return components;
        }

        [Obsolete("use GameObject.GetComponentsInChildren<T> instead")]
        public static void GetComponentsInDescendants<T>(Transform root, List<T> components)
        {
            //base case: root has no children
            if (root.childCount == 0)
            {
                T component = root.GetComponent<T>();
                if (!((UnityEngine.Object)(object)component == null)) //!= null doesn't work, it doesn't return true null and instead they've overloaded ==... but not !=
                {
                    components.Add(component);
                }
            }
            else //other case (could be cleaned up a bit)
            {
                T component = root.GetComponent<T>();
                if (!((UnityEngine.Object)(object)component == null))
                {
                    components.Add(component);
                }
                foreach (Transform t in root)
                {
                    GetComponentsInDescendants<T>(t, components);
                }
            }
        }

        private static GameObject PlayerObject;

        /// <summary>
        /// Gets the player object (or null)
        /// </summary>
        public static GameObject GetPlayerObject() //TODO split into Get() and TryGet()
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
        /// Finds the player and returns their controller (does not guarantee a PlayerController!)
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
        /// Gets the scene controller (returns null on fail)
        /// </summary>
        public static BaseSceneController TryGetSceneController()
        {
            //TODO refactor this to use CoreUtils.GetWorldRoot

            GameObject go = GameObject.FindGameObjectWithTag("GameController");
            if (go != null)
            {
                BaseSceneController bsc = go.GetComponent<BaseSceneController>();

                if (bsc != null)
                    return bsc;
            }

            //couldn't find it, try grabbing WorldRoot
            go = GameObject.Find("WorldRoot");
            if (go != null)
            {
                BaseSceneController bsc = go.GetComponent<BaseSceneController>();

                if (bsc != null)
                    return bsc;
            }

            return null;
        }

        /// <summary>
        /// Gets the scene controller (throws on fail)
        /// </summary>
        public static BaseSceneController GetSceneController()
        {
            BaseSceneController bsc = TryGetSceneController();

            if (bsc != null)
                return bsc;

            //still couldn't find it, throw an error
            Debug.LogError("Couldn't find SceneController");

            throw new NullReferenceException(); //not having a scene controller is fatal
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
            List<BaseController> bcs = new List<BaseController>();
            GetComponentsInDescendants<BaseController>(GameObject.Find("WorldRoot").transform, bcs);
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (BaseController c in bcs)
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
            List<BaseController> bcs = new List<BaseController>();
            GetComponentsInDescendants<BaseController>(GameObject.Find("WorldRoot").transform, bcs);
            List<GameObject> foundObjects = new List<GameObject>();
            foreach (BaseController c in bcs)
            {
                if (c.Tags.Contains(tag))
                {
                    foundObjects.Add(c.gameObject);
                }
            }

            return foundObjects.ToArray();
        }

        [Obsolete]
        public static GameObject SpawnObject(string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(CoreUtils.LoadResource<GameObject>("entities/" + formID), position, Quaternion.Euler(rotation), CoreUtils.GetWorldRoot()) as GameObject;
        }
        [Obsolete]
        public static GameObject SpawnObject(Transform parent, string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(CoreUtils.LoadResource<GameObject>("entities/" + formID), position, Quaternion.Euler(rotation), parent) as GameObject;
        }

        /// <summary>
        /// Spawn an entity into the world (entities/*)
        /// </summary>
        public static GameObject SpawnObject(string formID, string thingID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CoreUtils.GetWorldRoot();

            var prefab = CoreUtils.LoadResource<GameObject>("entities/" + formID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;
            if (string.IsNullOrEmpty(thingID))
                thingID = string.Format("{0}_{1}", go.name.Replace("(Clone)", "").Trim(), GameState.Instance.NextUID);
            go.name = thingID;
            return go;
        }

        /// <summary>
        /// Spawn an effect into the world (DynamicFX/*)
        /// </summary>
        public static GameObject SpawnEffect(string effectID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CoreUtils.GetWorldRoot();

            var prefab = CoreUtils.LoadResource<GameObject>("DynamicFX/" + effectID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;

            return go;
        }

        /// <summary>
        /// Find a child by name, recursively
        /// </summary>
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }


    }
}