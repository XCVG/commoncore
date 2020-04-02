using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using CommonCore.State;
using CommonCore.Scripting;
using CommonCore.Messaging;
using CommonCore.Config;

namespace CommonCore
{
    public abstract class BaseSceneController : MonoBehaviour
    {
        public bool AutosaveOnEnter = false;
        public bool AutosaveOnExit = true;

        public bool AutoRestore = true;
        public bool AutoCommit = true;

        public bool AutoinitUi = true;
        public bool AutoinitHud = false;
        public bool AutoinitState = true;

        public static BaseSceneController Current { get; protected set; }

        public Dictionary<string, System.Object> LocalStore { get; protected set; }

        protected QdmsMessageInterface MessageInterface;

        /// <summary>
        /// Set this to true if you want to handle the AfterSceneLoad scripting hook in your subclass controller
        /// </summary>
        protected virtual bool DeferAfterSceneLoadToSubclass => false;
        /// <summary>
        /// Set this to true if you want to handle the on-enter autosave in your subclass controller
        /// </summary>
        protected virtual bool DeferEnterAutosaveToSubclass => false;
        /// <summary>
        /// Set this to true if you want to handle the initial autorestore in your subclass controller
        /// </summary>
        protected virtual bool DeferInitialRestoreToSubclass => false;
        /// <summary>
        /// Set this to true to enable quicksave handling in this scene
        /// </summary>
        protected virtual bool AllowQuicksaveInScene => false;

        public virtual void Awake()
        {
            Debug.Log("Base Scene Controller Awake");

            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver((m) => HandleMessage(m));

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

            if (!DeferInitialRestoreToSubclass && AutoRestore)
            {
                MetaState.Instance.IntentsExecutePreload();
                Restore();
                MetaState.Instance.IntentsExecutePostload();
            }

            if (!DeferAfterSceneLoadToSubclass)
                ScriptingModule.CallHooked(ScriptHook.AfterSceneLoad, this);

            if (!DeferEnterAutosaveToSubclass && AutosaveOnEnter)
                SaveUtils.DoAutoSave();
        }

        public virtual void Update()
        {
            HandleQuicksave();
        }

        public virtual void OnDestroy()
        {
            
        }

        protected void HandleQuicksave()
        {
            if(AllowQuicksaveInScene && UnityEngine.Input.GetKeyDown(ConfigState.Instance.QuicksaveKey))
            { 
                SaveUtils.DoQuickSave();
                return;                
            }
            if(UnityEngine.Input.GetKeyDown(ConfigState.Instance.QuickloadKey))
            {
                SaveUtils.DoQuickLoad();
                return;
            }
        }

        /// <summary>
        /// Handles a received message
        /// </summary>
        /// <param name="message">The message to handle</param>
        /// <returns>If the message was handled</returns>
        protected virtual bool HandleMessage(QdmsMessage message)
        {
            return false;
        }

        /// <summary>
        /// Called when a scene is exiting. Should not actually exit the scene
        /// </summary>
        public virtual void ExitScene()
        {
            Debug.Log("Exiting scene: ");
            if(AutoCommit)
                Commit();
            if (AutosaveOnExit)
                SaveUtils.DoAutoSave();
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
