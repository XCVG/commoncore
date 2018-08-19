using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SickDev.CommandSystem;
using Newtonsoft.Json;
using CommonCore.State;

namespace CommonCore.World
{
    /*
     * WorldConsoleInterpreter
     * Console commands from ARES predescessor, may be refactored and/or split
     */
    public class WorldConsoleCommands
    {
        static GameObject SelectedObject;
        static string SelectedTID;

        //***** UTILITIES 


        [Command]
        static void PrintDataPath() //TODO move elsewhere
        {
            DevConsole.singleton.Log(Application.persistentDataPath);
        }

        [Command]
        static void PrintPlayerInfo()
        {
            DevConsole.singleton.Log(JsonConvert.SerializeObject(GameState.Instance.PlayerRpgState));
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
        static void Noclip()
        {
            var player = WorldUtils.GetPlayerController();

            player.Clipping = !(player.Clipping);
        }

        //***** LOAD/SAVE

        //force a full load from file with scene transition
        [Command]
        static void Load(string name)
        {
            MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
            MetaState.Instance.LoadSave = CCParams.SavePath + @"\" + name;
            MetaState.Instance.Intents.Clear();
            SceneManager.LoadScene("LoadingScene");
        }

        //force loading from file
        [Command]
        static void Restore(string name)
        {
            GameState.DeserializeFromFile(CCParams.SavePath + @"\" + name);

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

            GameState.SerializeToFile(CCParams.SavePath + @"\" + name);
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

        [Command]
        static void WarpEx(string scene, bool hideloading)
        {
            WorldUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, null);
        }

        [Command]
        static void WarpEx(string scene, bool hideloading, string overrideobject)
        {
            WorldUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, overrideobject);
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
                DevConsole.singleton.LogWarning("No refs found with form id!");
                return;
            }
            if (objs.Length > 1)
                DevConsole.singleton.LogWarning("More than one ref with form id!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.name;
                SelectedObject = obj;
            }
            else
            {
                DevConsole.singleton.LogWarning("Ref null or invalid!");
            }

            DevConsole.singleton.Log("Found TID: " + SelectedTID);
        }

        //pick object by tag (ARES tag, not Unity tag)
        [Command]
        static void Prbt(string tag)
        {
            var objs = WorldUtils.FindObjectsWithTag(tag);
            if (objs == null || objs.Length < 1)
            {
                DevConsole.singleton.LogWarning("No refs found with tag!");
                return;
            }
            if (objs.Length > 1)
                DevConsole.singleton.LogWarning("More than one ref with tag!");
            var obj = objs[0];
            if (obj != null && obj.GetComponent<BaseController>())
            {
                SelectedTID = obj.name;
                SelectedObject = obj;
            }
            else
            {
                DevConsole.singleton.LogWarning("Ref null or invalid!");
            }

            DevConsole.singleton.Log("Found TID: " + SelectedTID);
        }

        //pick object by TID
        [Command]
        static void Pick(string tid)
        {
            var obj = WorldUtils.FindObjectByTID(tid);
            if (obj == null)
            {
                DevConsole.singleton.LogWarning("Couldn't find TID!");
                return;
            }
            if (obj.GetComponent<BaseController>() == null)
            {
                DevConsole.singleton.LogWarning("Ref has no controller!");
            }

            SelectedTID = tid;
            SelectedObject = obj;

            DevConsole.singleton.Log("Found TID: " + tid);
        }

        //display info of selected ref
        [Command]
        static void GetInfo()
        {
            if(SelectedObject != null)
            {
                StringBuilder sb = new StringBuilder(256);

                //name, form id
                sb.AppendFormat("TID: {0} | FID: {1} \n", SelectedObject.name, SelectedObject.GetComponent<BaseController>().FormID);

                //coords
                sb.AppendFormat("Location: ({0:f2},{1:f2},{2:f2})\n", SelectedObject.transform.position.x, SelectedObject.transform.position.y, SelectedObject.transform.position.z);

                //enabled? active?
                sb.AppendFormat("Active: {0} | Visible: {1}", SelectedObject.activeSelf, SelectedObject.GetComponent<BaseController>().GetVisibility());

                DevConsole.singleton.Log(sb.ToString());
            }
            else
            {
                DevConsole.singleton.Log("No object selected!");
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
        static void SetAiState(string newState, bool lockState)
        {
            var ac = SelectedObject.GetComponent<ActorController>();
            bool wasLocked = ac.LockAiState;
            if (wasLocked)
                ac.LockAiState = false;
            ac.EnterState((ActorAiState)Enum.Parse(typeof(ActorAiState), newState));
            ac.LockAiState = wasLocked || lockState;
        }

        [Command]
        static void SetAnimState(string newState, bool lockState)
        {
            var ac = SelectedObject.GetComponent<ActorController>();
            bool wasLocked = ac.LockAnimState;
            if (wasLocked)
                ac.LockAnimState = false;
            ac.SetAnimation((ActorAnimState)Enum.Parse(typeof(ActorAnimState), newState));
            ac.LockAnimState = wasLocked || lockState;
        }

        [Command]
        static void Kill()
        {
            var ac = SelectedObject.GetComponent<ActorController>();
            ac.Health = 0;
        }

        [Command]
        static void Disable()
        {
            var ac = SelectedObject.GetComponent<BaseController>();
            ac.gameObject.SetActive(false);
        }

        [Command]
        static void Destroy()
        {
            GameObject.Destroy(SelectedObject);
        }

        [Command]
        static void Resurrect()
        {
            var ac = SelectedObject.GetComponent<ActorController>();
            ac.gameObject.SetActive(true);
            ac.Health = ac.MaxHealth;
            ac.EnterState(ac.BaseAiState);
        }


    }
}
