using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.State;
using UnityEngine.EventSystems;

namespace CommonCore.World
{
    public abstract class BaseSceneController : MonoBehaviour
    {
        public bool AutoinitUi = true;

        public static BaseSceneController Current { get; protected set; }

        public Dictionary<string, System.Object> LocalStore { get; protected set; }

        public virtual void Awake()
        {
            Debug.Log("Base Scene Controller Awake");

            Current = this;
            if (AutoinitUi)
                InitUI();
        }

        public virtual void Start()
        {
            Debug.Log("Base Scene Controller Start");
        }

        public virtual void Update()
        {

        }

        public virtual void ExitScene()
        {
            //whatever triggered the exit is responsible for setting up most of metastate
            MetaState.Instance.PreviousScene = SceneManager.GetActiveScene().name;
            MetaState.Instance.TransitionType = SceneTransitionType.ChangeScene;
            Debug.Log("Exiting scene: ");
            Save();
            SceneManager.LoadScene("LoadingScene");
        }

        protected void InitUI()
        {
            if (transform.Find("WorldHUD") == null)
                Instantiate<GameObject>(Resources.Load<GameObject>("UI/DefaultWorldHUD"), transform).name = "WorldHUD";
            if (transform.Find("InGameMenu") == null)
                Instantiate<GameObject>(Resources.Load<GameObject>("UI/IGUI_Menu"), transform).name = "InGameMenu";
            if (EventSystem.current == null)
                Instantiate<GameObject>(Resources.Load<GameObject>("UI/DefaultEventSystem"), transform).name = "EventSystem";
        }

        public virtual void Save()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;
            gs.CurrentScene = name;
            Debug.Log("Saving scene: " + name);

            //purge and copy local data store
            if (gs.LocalDataState.ContainsKey(name))
            {
                gs.LocalDataState.Remove(name);
            }
            gs.LocalDataState.Add(name, LocalStore);
        }

        public virtual void Restore()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;

            Debug.Log("Restoring scene: " + name);

            //restore local store
            LocalStore = gs.LocalDataState.ContainsKey(name) ? gs.LocalDataState[name] : new Dictionary<string, System.Object>();
        }
    }
}
