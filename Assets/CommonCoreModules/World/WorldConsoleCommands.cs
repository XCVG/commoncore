using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using CommonCore.State;
using CommonCore.Console;
using CommonCore.Messaging;

namespace CommonCore.World
{
    /// <summary>
    /// World/object console commands (general)
    /// </summary>
    public static class WorldConsoleCommands
    {
        //TODO cleanup and XML comments

        public static GameObject SelectedObject { get; private set; }
        public static string SelectedTID { get; private set; }

        //***** CHEATS

        [Command]
        static void God()
        {
            if (MetaState.Instance.SessionFlags.Contains("GodMode"))
            {
                ConsoleModule.WriteLine("Degreelessness mode off");
                MetaState.Instance.SessionFlags.Remove("GodMode");
            }
            else
            {
                ConsoleModule.WriteLine("Degreelessness mode on");
                MetaState.Instance.SessionFlags.Add("GodMode");
            }
        }

        [Command]
        static void Noclip()
        {
            if (MetaState.Instance.SessionFlags.Contains("NoClip"))
                MetaState.Instance.SessionFlags.Remove("NoClip");
            else
                MetaState.Instance.SessionFlags.Add("NoClip");
        }

        [Command]
        static void Notarget()
        {
            if (MetaState.Instance.SessionFlags.Contains("NoTarget"))
                MetaState.Instance.SessionFlags.Remove("NoTarget");
            else
                MetaState.Instance.SessionFlags.Add("NoTarget");

            QdmsMessageBus.Instance.PushBroadcast(new ClearAllTargetsMessage());
        }

        //***** SCENE WARP

        [Command]
        static void Warp(string scene, string spawnPoint)
        {
            WorldUtils.ChangeScene(scene, spawnPoint, Vector3.zero, Vector3.zero);
        }

        [Command]
        static void Warp(string scene, Vector3 position)
        {
            Warp(scene, position, Vector3.zero);
        }

        [Command]
        static void Warp(string scene, Vector3 position, Vector3 rotation)
        {
            WorldUtils.ChangeScene(scene, null, position, rotation);
        }

        //***** STATE/PLAYERFLAG MANIPULATION

        [Command (alias = "ListFlags", className = "Player")]
        static void ListPlayerFlags()
        {
            string playerFlags = GameState.Instance.PlayerFlags.GetAllFlags().ToNiceString();
            ConsoleModule.WriteLine(playerFlags);
        }

        [Command(alias = "AddFlag", className = "Player")]
        static void AddPlayerFlag(string flag)
        {
            GameState.Instance.PlayerFlags.Add(flag);
        }

        [Command(alias = "RemoveFlag", className = "Player")]
        static void RemovePlayerFlag(string flag)
        {
            GameState.Instance.PlayerFlags.Remove(flag);
        }

        [Command(alias = "AddTempFlag", className = "Player")]
        static void AddTempPlayerFlag(string flag)
        {
            var sceneController = SharedUtils.TryGetSceneController() as WorldSceneController;
            if (sceneController != null)
                sceneController.TempPlayerFlags.Add(flag);
        }

        [Command(alias = "RemoveTempFlag", className = "Player")]
        static void RemoveTempPlayerFlag(string flag)
        {
            var sceneController = SharedUtils.TryGetSceneController() as WorldSceneController;
            if (sceneController != null)
                sceneController.TempPlayerFlags.Remove(flag);
        }

        //***** OBJECT MANIPULATION

        [Command]
        static void Spawn(string fid)
        {
            WorldUtils.SpawnEntity(fid, null, (WorldUtils.GetPlayerObject().transform.position + (WorldUtils.GetPlayerObject().transform.forward * 1.0f)), Vector3.zero, null);
        }

        [Command]
        static void ListEntitiesInScene()
        {
            StringBuilder sb = new StringBuilder();
            var entities = CoreUtils.GetWorldRoot().GetComponentsInChildren<BaseController>(true);
            foreach(var entity in entities)
            {
                sb.AppendLine($"{entity.gameObject.name} ({entity.FormID ?? entity.EditorFormID} : {entity.GetType().Name}) [{entity.transform.position.ToString("F2")}] {(entity.isActiveAndEnabled ? "" : "(disabled)")} {(entity.IsEntityAlive() ? "" : "(dead)")}");
            }

            ConsoleModule.WriteLine(sb.ToString());
        }

        //pick object by form id
        [Command]
        static void Prid(string fid)
        {
            var objs = WorldUtils.FindEntitiesWithFormID(fid);
            if (objs == null || objs.Count < 1)
            {
                ConsoleModule.WriteLine("No refs found with form id!");
                return;
            }
            if (objs.Count > 1)
                ConsoleModule.WriteLine("More than one ref with form id!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.gameObject.name;
                SelectedObject = obj.gameObject;
            }
            else
            {
                ConsoleModule.WriteLine("Ref null or invalid!");
            }

            ConsoleModule.WriteLine("Found TID: " + SelectedTID);
        }

        //pick object by tag (ARES tag, not Unity tag)
        [Command]
        static void Prbt(string tag)
        {
            var objs = WorldUtils.FindEntitiesWithTag(tag);
            if (objs == null || objs.Count < 1)
            {
                ConsoleModule.WriteLine("No refs found with tag!");
                return;
            }
            if (objs.Count > 1)
                ConsoleModule.WriteLine("More than one ref with tag!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.gameObject.name;
                SelectedObject = obj.gameObject;
            }
            else
            {
                ConsoleModule.WriteLine("Ref null or invalid!");
            }

            ConsoleModule.WriteLine("Found TID: " + SelectedTID);
        }

        //pick object by TID
        [Command]
        static void Pick(string tid)
        {
            var obj = WorldUtils.FindObjectByTID(tid);
            if (obj == null)
            {
                ConsoleModule.WriteLine("Couldn't find TID!");
                return;
            }
            if (obj.GetComponent<BaseController>() == null)
            {
                ConsoleModule.WriteLine("Ref has no controller!");
            }

            SelectedTID = tid;
            SelectedObject = obj;

            ConsoleModule.WriteLine("Found TID: " + tid);
        }

        //display info of selected ref
        [Command]
        static void GetInfo()
        {
            if (SelectedObject != null)
            {
                StringBuilder sb = new StringBuilder(256);

                //name, form id
                sb.AppendFormat("TID: {0} | FID: {1} \n", SelectedObject.name, SelectedObject.GetComponent<BaseController>().Ref()?.FormID ?? "N/A");

                //tags etc
                sb.AppendFormat("Unity Tag: {0} | Unity Layer: {1} | Entity tags: {2} \n", SelectedObject.tag, SelectedObject.layer, 
                    SelectedObject.GetComponent<BaseController>().Ref()?.Tags?.ToNiceString() ?? "N/A");

                //coords
                sb.AppendFormat("Location: ({0:f2},{1:f2},{2:f2})\n", SelectedObject.transform.position.x, SelectedObject.transform.position.y, SelectedObject.transform.position.z);

                //enabled? active?
                sb.AppendFormat("Active: {0} | Visible: {1}", SelectedObject.activeSelf, SelectedObject.GetComponent<BaseController>().Ref()?.GetVisibility().ToString() ?? "N/A");

                ConsoleModule.WriteLine(sb.ToString());
            }
            else
            {
                ConsoleModule.WriteLine("No object selected!");
            }
        }

        //deselect ref
        [Command]
        static void Dref()
        {
            SelectedTID = null;
            SelectedObject = null;
        }

        [Command]
        static void TeleportToMe()
        {
            var playerTransform = WorldUtils.GetPlayerObject().transform;

            Vector3 targetPos = playerTransform.position + (playerTransform.forward.GetFlatVector().GetSpaceVector() * 1f);
            Quaternion targetRot = playerTransform.rotation;

            SelectedObject.transform.position = targetPos;
            SelectedObject.transform.rotation = targetRot;
        }

        [Command]
        static void TeleportToTarget()
        {
            var targetTransform = SelectedObject.transform;

            Vector3 targetPos = targetTransform.position + (targetTransform.forward.GetFlatVector().GetSpaceVector() * 1f);
            Quaternion targetRot = targetTransform.rotation;

            var playerTransform = WorldUtils.GetPlayerObject().transform;

            playerTransform.position = targetPos;
            playerTransform.rotation = targetRot;
        }

        //***** ACTOR MANIPULATION
        

        [Command]
        static void Enable()
        {
            SelectedObject.SetActive(true);
        }

        [Command]
        static void Disable()
        {
            SelectedObject.SetActive(false);
        }

        [Command]
        static void Destroy()
        {
            GameObject.Destroy(SelectedObject);
        }

        //***** MISCELLANEOUS

        [Command]
        static void GetActiveCamera()
        {
            var camera = WorldUtils.GetActiveCamera();
            if (camera != null)
                Debug.Log($"Found active camera on {camera.gameObject.name}");
            else
                Debug.Log($"Couldn't find active camera!");
        }

        [Command]
        static void ListAllEntities()
        {
            var entities = CoreUtils.LoadResources<GameObject>("Entities/");
            StringBuilder sb = new StringBuilder(entities.Length * 32);
            foreach(var entity in entities)
            {
                sb.AppendLine(entity.name);
            }
            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command]
        static void ListAllEffects()
        {
            var effects = CoreUtils.LoadResources<GameObject>("Effects/");
            StringBuilder sb = new StringBuilder(effects.Length * 32);
            foreach (var effect in effects)
            {
                sb.AppendLine(effect.name);
            }
            ConsoleModule.WriteLine(sb.ToString());
        }

        [Command(alias = "Reset", useClassName = false)]
        static void Reset()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            GameState.Instance.LocalDataState.Remove(sceneName);
            GameState.Instance.LocalObjectState.Remove(sceneName);

            SceneManager.LoadScene(sceneName);
        }

        [Command]
        static void Kill()
        {
            var itd = WorldConsoleCommands.SelectedObject.GetComponent<ITakeDamage>();
            if (itd != null)
                itd.Kill();
        }

    }
}
