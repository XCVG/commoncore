using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CommonCore.State;

namespace CommonCore
{
    public abstract class BaseSceneController : MonoBehaviour
    {
        public bool AutoinitUi = true;
        public bool AutoinitHud = false;
        public bool AutoinitState = true;

        public static BaseSceneController Current { get; protected set; }

        public Dictionary<string, System.Object> LocalStore { get; protected set; }

        public virtual void Awake()
        {
            Debug.Log("Base Scene Controller Awake");

            Current = this;
            if (AutoinitUi)
                InitUI();
            if (AutoinitHud)
                InitHUD();

            //mostly an editor hack
            if (AutoinitState && !GameState.Instance.InitialLoaded)
                GameState.LoadInitial();
        }

        public virtual void Start()
        {
            Debug.Log("Base Scene Controller Start");
        }

        public virtual void Update()
        {

        }

        /// <summary>
        /// Called when a scene is exiting. Should not actually exit the scene
        /// </summary>
        public virtual void ExitScene()
        {
            Debug.Log("Exiting scene: ");
            Commit();
        }

        protected void InitUI()
        {
            if (CoreUtils.GetUIRoot().Find("InGameMenu") == null)
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/IGUI_Menu"), CoreUtils.GetUIRoot()).name = "InGameMenu";
            if (EventSystem.current == null)
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"), CoreUtils.GetUIRoot()).name = "EventSystem";
        }

        protected void InitHUD()
        {
            if (CoreUtils.GetUIRoot().Find("WorldHUD") == null)
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultWorldHUD"), CoreUtils.GetUIRoot()).name = "WorldHUD";
        }

        public virtual void Commit()
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
