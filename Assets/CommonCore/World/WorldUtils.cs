using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore.Rpg;
using CommonCore.DebugLog;

namespace CommonCore.World
{
    /*
     * Utility class for world manipulation
     * Some of these will be moved soon
     */
    public static class WorldUtils
    {
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

        public static List<T> GetComponentsInDescendants<T>(Transform root)
        {
            List<T> components = new List<T>();

            GetComponentsInDescendants<T>(root, components);

            return components;
        }

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

        public static GameObject GetPlayerObject()
        {
            if (PlayerObject != null)
                return PlayerObject;

            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if(go != null)
            {
                PlayerObject = go;
                return go;
            }

            go = GameObject.Find("Player");

            if(go != null)
            {
                PlayerObject = go;
                return go;
            }

            var tf = CCBaseUtil.GetWorldRoot().FindDeepChild("Player");

            if(tf != null)
                go = tf.gameObject;

            if (go != null)
            {
                PlayerObject = go;
                return go;
            }

            Debug.LogWarning("Couldn't find player!");

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
            var targetTransform = GameObject.Find("WorldRoot").transform.FindDeepChild(TID);
            if (targetTransform != null)
                return targetTransform.gameObject;
            return null;
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
            if (spawnPoint != null)
                mgs.PlayerIntent = new PlayerSpawnIntent(spawnPoint); //handle string.Empty as default spawn point
            else
                mgs.PlayerIntent = new PlayerSpawnIntent(position, Quaternion.Euler(rotation));            
            mgs.LoadSave = null;
            mgs.TransitionType = SceneTransitionType.ChangeScene;
            WorldUtils.GetSceneController().ExitScene();
        }

        //spawn object methods
        [Obsolete]
        public static GameObject SpawnObject(string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(Resources.Load("entities/" + formID), position, Quaternion.Euler(rotation), CCBaseUtil.GetWorldRoot()) as GameObject;
        }
        [Obsolete]
        public static GameObject SpawnObject(Transform parent, string formID, Vector3 position, Vector3 rotation)
        {
            return UnityEngine.Object.Instantiate(Resources.Load("entities/" + formID),position,Quaternion.Euler(rotation), parent) as GameObject;
        }

        public static GameObject SpawnObject(string formID, string thingID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CCBaseUtil.GetWorldRoot();

            var prefab = Resources.Load("entities/" + formID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;
            if (string.IsNullOrEmpty(thingID))
                thingID = string.Format("{0}_{1}", go.name.Replace("(Clone)", "").Trim(), GameState.Instance.NextUID);
            go.name = thingID;
            return go;
        }

        public static GameObject SpawnEffect(string effectID, Vector3 position, Vector3 rotation, Transform parent)
        {
            if (parent == null)
                parent = CCBaseUtil.GetWorldRoot();

            var prefab = Resources.Load("DynamicFX/" + effectID);
            if (prefab == null)
                return null;

            var go = UnityEngine.Object.Instantiate(prefab, position, Quaternion.Euler(rotation), parent) as GameObject;

            return go;
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

        public static bool TargetIsAlive(Transform target)
        {
            if (target == null)
                return false;

            var bc = target.GetComponent<BaseController>();
            if (bc == null)
                return target.gameObject.activeInHierarchy;

            bool healthPass = true;
            if (bc is PlayerController)
                healthPass = GameState.Instance.PlayerRpgState.HealthFraction > 0;
            else if (bc is ActorController)
                healthPass = ((ActorController)bc).Health > 0;

            return target.gameObject.activeInHierarchy && healthPass;
        }

        //hacky inventory hack
        public static void DropItem(InventoryItemModel item, int quantity, Vector3 position)
        {
            string spawnName = "spec_item";

            if(!string.IsNullOrEmpty(item.WorldModel))
            {
                var ent = Resources.Load("entities/" + item.WorldModel); //yup, you can't check this, you have to try it
                if (ent != null)
                    spawnName = item.WorldModel;
            }

            var go = SpawnObject(spawnName, "inv_drop_" + GameState.Instance.NextUID, position, Vector3.zero, null); //TODO switch to one that works with loaded prefab
            var ic = go.GetComponent<ItemController>();
            ic.ItemId = item.Name;
            ic.ItemQuantity = quantity;
        }

    }
}
