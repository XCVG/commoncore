using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using CommonCore.State;

namespace CommonCore.World
{
    public static class SceneUtils
    {

        public static void GetComponentsInDescendants<T>(Transform root, List<T> components)
        {
            //base case: root has no children
            if (root.childCount == 0)
            {
                T component = root.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
            }
            else //other case (could be cleaned up a bit)
            {
                T component = root.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
                foreach (Transform t in root)
                {
                    GetComponentsInDescendants<T>(t, components);
                }
            }
        }

        public static GameObject GetPlayerObject()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if(go != null)
            {
                return go;
            }

            go = GameObject.Find("Player");

            if(go != null)
            {
                return go;
            }

            //Debug.LogWarning("Couldn't find player!");

            return null;
        }

        public static PlayerController GetPlayerController()
        {
            PlayerController pc = GetPlayerObject().GetComponent<PlayerController>();
            if(pc != null)
            {
                return pc;
            }
            else
            {
                Debug.LogWarning("Couldn't find PlayerController!");
                return null;
            }
        }

        public static BaseSceneController GetSceneController()
        {
            GameObject go = GameObject.FindGameObjectWithTag("GameController");
            if(go != null)
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

            //still couldn't find it, throw an error
            Debug.LogError("Couldn't find SceneController");

            throw new NullReferenceException(); //not having a scene controller is fatal
        }

        public static GameObject FindObjectByTID(string TID)
        {
            return GameObject.Find("WorldRoot").transform.Find(TID).gameObject;
        }

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

        public static GameObject[] FindObjectsWithTag(string tag)
        {
            List<BaseController> bcs = new List<BaseController>();
            GetComponentsInDescendants<BaseController>(GameObject.Find("WorldRoot").transform, bcs);
            List<GameObject> foundObjects = new List<GameObject>();
            foreach(BaseController c in bcs)
            {
                if(c.Tags.Contains(tag))
                {
                    foundObjects.Add(c.gameObject);
                }
            }

            return foundObjects.ToArray();
        }

        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading, string objectOverride)
        {
            MetaState.Instance.LoadingScreenPropOverride = objectOverride;
            ChangeScene(scene, spawnPoint, position, rotation, skipLoading);
        }

        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation, bool skipLoading)
        {
            MetaState.Instance.SkipLoadingScreen = skipLoading;
            ChangeScene(scene, spawnPoint, position, rotation);
        }

        public static void ChangeScene(string scene, string spawnPoint, Vector3 position, Vector3 rotation)
        {
            MetaState mgs = MetaState.Instance;
            mgs.PreviousScene = SceneManager.GetActiveScene().name;
            mgs.NextScene = scene;
            if (!string.IsNullOrEmpty(spawnPoint))
                mgs.PlayerIntent = new PlayerSpawnIntent(spawnPoint);
            else
                mgs.PlayerIntent = new PlayerSpawnIntent(position, Quaternion.Euler(rotation));
            mgs.LoadSave = null;
            mgs.TransitionType = SceneTransitionType.ChangeScene;
            SceneUtils.GetSceneController().ExitScene();
        }

        //spawn object methods
        //TODO make it fail gracefully
        public static GameObject SpawnObject(string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(Resources.Load("entities/" + formID), position, Quaternion.Euler(rotation)) as GameObject;
        }

        public static GameObject SpawnObject(Transform parent, string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(Resources.Load("entities/" + formID),position,Quaternion.Euler(rotation), parent) as GameObject;
        }

        //from StackOverflow, an extension method
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
