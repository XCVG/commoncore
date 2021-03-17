using CommonCore.Config;
using CommonCore.Scripting;
using CommonCore.UI;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.State
{

    /// <summary>
    /// Controller for loading scene, handles loading scenes
    /// </summary>
    public class LoadingSceneController : MonoBehaviour
    {
        [SerializeField]
        private Canvas DefaultLoadingCanvas = null;

        /// <summary>
        /// Immediately begin loading the scene
        /// </summary>
	    void Start ()
        {
            //the loading screen cannot be truly skipped, but it can be hidden
            if (!MetaState.Instance.SkipLoadingScreen)
            {
                //appear the overlay
                DefaultLoadingCanvas.gameObject.SetActive(true);

            }

            CoreUtils.CollectGarbage(false);

            Application.logMessageReceived += HandleLog;

            try
            {
                if (MetaState.Instance.TransitionType == SceneTransitionType.ChangeScene)
                {
                    MetaState.Instance.IntentsExecuteLoading();
                    MetaState.Instance.IntentsRemoveInvalid();
                    //we are merely changing scenes, go straight to loading the next scene
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    ScriptingModule.CallHooked(ScriptHook.OnSceneTransition, this);
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.LoadGame)
                {
                    //we are loading a game, so load the game data and then load the next scene (which is part of save data)
                    GameState.DeserializeFromFile(CoreParams.SavePath + Path.DirectorySeparatorChar + MetaState.Instance.LoadSave);
                    PersistState.Instance.LastCampaignIdentifier = GameState.Instance.CampaignIdentifier;
                    MetaState.Instance.NextScene = GameState.Instance.CurrentScene;
                    ScriptingModule.CallHooked(ScriptHook.OnGameLoad, this);
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.NewGame)
                {
                    GameState.Recreate();
                    MetaState.Instance.Clear();
                    if(string.IsNullOrEmpty(MetaState.Instance.NextScene))
                        MetaState.Instance.NextScene = CoreParams.InitialScene;                    
                    GameState.LoadInitial();
                    PersistState.Instance.LastCampaignIdentifier = GameState.Instance.CampaignIdentifier;
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    CCBase.OnGameStart();
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if(MetaState.Instance.TransitionType == SceneTransitionType.EndGame)
                {
                    CCBase.OnGameEnd();
                    GameState.Clear();
                    MetaState.Instance.Clear();
                    if (string.IsNullOrEmpty(MetaState.Instance.NextScene))
                        MetaState.Instance.NextScene = CoreParams.MainMenuScene;                    
                    StartCoroutine(LoadNextSceneAsync());
                }

            }
            catch(Exception e)
            {
                //pokemon exception handling

                Debug.LogError($"Error loading scene ({e.GetType().Name})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);                
                Modal.PushConfirmModal(string.Format("{0}\n{1}", e.ToString(), e.StackTrace), "Error loading scene", "Exit", "Return", null, OnErrorConfirmed);
            }            

            //clear certain metagamestate on use
            MetaState.Instance.SkipLoadingScreen = false;

            ScriptingModule.CallHooked(ScriptHook.OnLoadingSceneOpen, this);
	    }

        /// <summary>
        /// Loads the next scene using LoadSceneAsync
        /// </summary>
        private IEnumerator LoadNextSceneAsync()
        {
            yield return null;

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(MetaState.Instance.NextScene, LoadSceneMode.Single);

            if (asyncLoad == null)
            {
                Modal.PushConfirmModal("Async load operation failed", "Error loading scene", "Exit", "Return", null, OnErrorConfirmed);
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Handler for when the user acknowledges there was an error, sends them back
        /// </summary>
        private void OnErrorConfirmed(ModalStatusCode status, string tag, bool result)
        {
            if (result)
            {
                GameState.Clear();
                MetaState.Instance.Clear();                
                SceneManager.LoadScene(CoreParams.MainMenuScene);
            }
            else
            {
                MetaState.Instance.NextScene = MetaState.Instance.PreviousScene;
                SceneManager.LoadScene(CoreParams.LoadingScene);
            }
        }

        /// <summary>
        /// The absolutely stupid "solution" to catching scene load errors: we hook the debug log
        /// </summary>
        private void HandleLog(string log, string stackTrace, LogType type)
        {
            //yes, this is really the best way to handle this
            //we will change to keeping our own list of scenes in the future because Unity's datastructures are fucking useless        
            //but that probably won't be until mod support is added in Citadel
            //we will also investigate LoadSceneAsync but something tells me the design won't be much better

            if (type == LogType.Error)
            {
                if (log.Contains("Cannot load scene"))
                    Modal.PushConfirmModal(log, "Error loading game (failed to load scene)", "Exit", "Return", null, OnErrorConfirmed);

            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

    }

}