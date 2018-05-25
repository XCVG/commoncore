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

        //***** LOAD/SAVE

        //force a full load from file with scene transition
        [Command]
        static void Load(string name)
        {
            throw new NotImplementedException();
        }

        //force loading from file
        [Command]
        static void Restore(string name)
        {
            GameState.DeserializeFromFile(Application.persistentDataPath + @"\" + name);

            Restore();
        }

        //force loading from state
        [Command]
        static void Restore()
        {
            MetaState.Instance.TransitionType = SceneTransitionType.LoadGame;
            BaseSceneController bsc = SceneUtils.GetSceneController();
            bsc.Restore();
        }

        //force saving to gamestate
        [Command]
        static void Save()
        {
            BaseSceneController bsc = SceneUtils.GetSceneController();
            bsc.Save();
        }

        //force saving to a file
        [Command]
        static void Save(string name)
        {
            Save();

            GameState.SerializeToFile(Application.persistentDataPath + @"\" + name);
        }

        //***** STAT/RPG MANIPULATION
        //will be replaced by new setAV/base/derived system and probably moved

        /*
        [Command]
        static void AddStat(string stat)
        {
            var s = (Stat)Enum.Parse(typeof(Stat), stat);
            var pm = GameState.Instance.PlayerRpgState;
            pm.InvestPoints(s);
        }

        [Command]
        static void AddXP(int xp)
        {
            GameState.Instance.PlayerRpgState.XP += xp;
        }

        [Command]
        static void GetAllStats()
        {
            var pm = GameState.Instance.PlayerRpgState;
            string str = string.Empty;

            foreach (Stat s in Enum.GetValues(typeof(Stat)))
            {
                string name = s.ToString();
                int value = pm.GetStat(s);
                str += string.Format("{0} : {1}\n", name, value);
            }

            DevConsole.Console.Log(str);
        }

        [Command]
        static void GetStat(string stat)
        {
            var s = (Stat)Enum.Parse(typeof(Stat), stat);
            var pm = GameState.Instance.PlayerRpgState;
            DevConsole.Console.Log(s.ToString() + ":" + pm.GetStat(s));
        }

        [Command]
        static void GetXP()
        {
            DevConsole.Console.Log(GameState.Instance.PlayerRpgState.XP);
        }

        */

        //***** SCENE WARP

        [Command]
        static void Warp(string scene)
        {
            Warp(scene, null);
        }

        [Command]
        static void Warp(string scene, string spawnPoint)
        {
            SceneUtils.ChangeScene(scene, spawnPoint, Vector3.zero, Vector3.zero);
        }

        [Command]
        static void Warp(string scene, Vector3 position)
        {
            Warp(scene, position, Vector3.zero);
        }

        [Command]
        static void Warp(string scene, Vector3 position, Vector3 rotation)
        {
            SceneUtils.ChangeScene(scene, null, position, rotation);
        }

        [Command]
        static void WarpEx(string scene, bool hideloading)
        {
            SceneUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, null);
        }

        [Command]
        static void WarpEx(string scene, bool hideloading, string overrideobject)
        {
            SceneUtils.ChangeScene(scene, null, Vector3.zero, Vector3.zero, hideloading, overrideobject);
        }

        //***** OBJECT MANIPULATION

        //pick object by form id
        [Command]
        static void Prid(string fid)
        {
            var objs = SceneUtils.FindObjectsWithFormID(fid);
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
            var objs = SceneUtils.FindObjectsWithTag(tag);
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
            var obj = SceneUtils.FindObjectByTID(tid);
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


    }
}
