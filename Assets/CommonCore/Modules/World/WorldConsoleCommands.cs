using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using CommonCore.State;
using CommonCore.Console;

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

        //***** UTILITIES 


        [Command]
        static void PrintDataPath() //TODO move elsewhere
        {
            ConsoleModule.WriteLine(Application.persistentDataPath);
        }

        [Command]
        static void PrintPlayerInfo()
        {
            ConsoleModule.WriteLine(JsonConvert.SerializeObject(GameState.Instance.PlayerRpgState));
        }

        [Command]
        static void PrintSceneList()
        {
            try
            {
                var sceneNames = CoreUtils.GetSceneList();
                StringBuilder sb = new StringBuilder(sceneNames.Length * 16);
                foreach (var s in sceneNames)
                {
                    sb.AppendLine(s);
                }
                ConsoleModule.WriteLine(sb.ToString());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }

        //***** CHEATS

        [Command]
        static void God()
        {
            if (MetaState.Instance.SessionFlags.Contains("GodMode"))
                MetaState.Instance.SessionFlags.Remove("GodMode");
            else
                MetaState.Instance.SessionFlags.Add("GodMode");
        }

        [Command]
        static void Notarget()
        {
            if (MetaState.Instance.SessionFlags.Contains("NoTarget"))
                MetaState.Instance.SessionFlags.Remove("NoTarget");
            else
                MetaState.Instance.SessionFlags.Add("NoTarget");
        }

        //***** LOAD/SAVE

        //force a full load from file with scene transition
        [Command]
        static void Load(string name)
        {
            MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
            MetaState.Instance.LoadSave = CoreParams.SavePath + @"\" + name;
            MetaState.Instance.Intents.Clear();
            SceneManager.LoadScene("LoadingScene");
        }

        //force loading from file
        [Command]
        static void Restore(string name)
        {
            GameState.DeserializeFromFile(CoreParams.SavePath + @"\" + name);

            Restore();
        }

        //force loading from state
        [Command]
        static void Restore()
        {
            MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
            BaseSceneController bsc = WorldUtils.GetSceneController();
            bsc.Restore();
        }

        //force saving to gamestate
        [Command]
        static void Save()
        {
            BaseSceneController bsc = WorldUtils.GetSceneController();
            bsc.Save();
        }

        //force saving to a file
        [Command]
        static void Save(string name)
        {
            Save();

            GameState.SerializeToFile(CoreParams.SavePath + @"\" + name);
        }

        //***** SCENE WARP

        [Command]
        static void Warp(string scene)
        {
            Warp(scene, string.Empty);
        }

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

        //***** OBJECT MANIPULATION

        [Command]
        static void Spawn(string fid)
        {
            WorldUtils.SpawnObject(fid, null, (WorldUtils.GetPlayerObject().transform.position + (WorldUtils.GetPlayerObject().transform.forward * 1.0f)), Vector3.zero, null);
        }

        //pick object by form id
        [Command]
        static void Prid(string fid)
        {
            var objs = WorldUtils.FindObjectsWithFormID(fid);
            if (objs == null || objs.Length < 1)
            {
                ConsoleModule.WriteLine("No refs found with form id!");
                return;
            }
            if (objs.Length > 1)
                ConsoleModule.WriteLine("More than one ref with form id!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.name;
                SelectedObject = obj;
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
            var objs = WorldUtils.FindObjectsWithTag(tag);
            if (objs == null || objs.Length < 1)
            {
                ConsoleModule.WriteLine("No refs found with tag!");
                return;
            }
            if (objs.Length > 1)
                ConsoleModule.WriteLine("More than one ref with tag!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.name;
                SelectedObject = obj;
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
                sb.AppendFormat("TID: {0} | FID: {1} \n", SelectedObject.name, SelectedObject.GetComponent<BaseController>().FormID);

                //coords
                sb.AppendFormat("Location: ({0:f2},{1:f2},{2:f2})\n", SelectedObject.transform.position.x, SelectedObject.transform.position.y, SelectedObject.transform.position.z);

                //enabled? active?
                sb.AppendFormat("Active: {0} | Visible: {1}", SelectedObject.activeSelf, SelectedObject.GetComponent<BaseController>().GetVisibility());

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

        //***** ACTOR MANIPULATION
        

        [Command]
        static void Enable()
        {
            SelectedObject.SetActive(false);
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

    }
}
