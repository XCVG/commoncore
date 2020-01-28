using CommonCore.Scripting;
using CommonCore.UI;
using System;
using System.Collections;
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

            GC.Collect();

            Application.logMessageReceived += HandleLog;

            try
            {
                if (MetaState.Instance.TransitionType == SceneTransitionType.ChangeScene)
                {
                    MetaState.Instance.IntentsExecuteLoading();
                    //we are merely changing scenes, go straight to loading the next scene
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    ScriptingModule.CallHooked(ScriptHook.OnSceneTransition, this);
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.LoadGame)
                {
                    //we are loading a game, so load the game data and then load the next scene (which is part of save data)
                    GameState.DeserializeFromFile(CoreParams.SavePath + @"\" + MetaState.Instance.LoadSave);
                    MetaState.Instance.NextScene = GameState.Instance.CurrentScene;
                    ScriptingModule.CallHooked(ScriptHook.OnGameLoad, this);
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.NewGame)
                {
                    GameState.Reset();
                    MetaState.Instance.Clear();
                    if(string.IsNullOrEmpty(MetaState.Instance.NextScene))
                        MetaState.Instance.NextScene = CoreParams.InitialScene;
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    GameState.LoadInitial();
                    CCBase.OnGameStart();
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if(MetaState.Instance.TransitionType == SceneTransitionType.EndGame)
                {
                    GameState.Reset();
                    MetaState.Instance.Clear();
                    if (string.IsNullOrEmpty(MetaState.Instance.NextScene))
                        MetaState.Instance.NextScene = "MainMenuScene";
                    CCBase.OnGameEnd();
                    StartCoroutine(LoadNextSceneAsync());
                }
                //TODO move endgame into here
            }
            catch(Exception e)
            {
                //pokemon exception handling

                Modal.PushMessageModal(string.Format("{0}\n{1}", e.ToString(), e.StackTrace), "Error loading scene", null, OnErrorConfirmed);
            }            

            //clear certain metagamestate on use
            MetaState.Instance.SkipLoadingScreen = false;		
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
                Modal.PushMessageModal("Async load operation failed", "Error loading scene", null, OnErrorConfirmed);
                yield break;
            }

            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }

        /// <summary>
        /// Handler for when the user acknowledges there was an error, sends them back to the main menu
        /// </summary>
        private void OnErrorConfirmed(ModalStatusCode status, string tag)
        {
            GameState.Reset();
            MetaState.Reset();
            GC.Collect();
            SceneManager.LoadScene("MainMenuScene");
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
                    Modal.PushMessageModal(log, "Error loading game (failed to load scene)", null, OnErrorConfirmed);
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }

    }

}