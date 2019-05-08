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
        private Canvas DefaultLoadingCanvas;

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

            System.GC.Collect();

            Application.logMessageReceived += HandleLog;

            try
            {
                if (MetaState.Instance.TransitionType == SceneTransitionType.ChangeScene)
                {
                    MetaState.Instance.IntentsExecuteLoading();
                    //we are merely changing scenes, go straight to loading the next scene
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    StartCoroutine(LoadNextSceneAsync());
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.LoadGame)
                {
                    //we are loading a game, so load the game data and then load the next scene (which is part of save data)
                    GameState.DeserializeFromFile(CoreParams.SavePath + @"\" + MetaState.Instance.LoadSave);
                    MetaState.Instance.NextScene = GameState.Instance.CurrentScene;
                    StartCoroutine(LoadNextSceneAsync()); //when this fails, it doesn't return a status code or throw an exception, only logs an error
                    //oh, and no, it doesn't halt execution, either, so you can't check that way (which would be dumb, but at least workable)
                    //excuse me, but WHO THE FUCK DESIGNED THIS
                    //I guess the assumption is that you know what you have, don't load anything you don't, and if you do it's a fatal error
                    //but those assumptions fail in myriad ways in a bigger game and can and should be handled gracefully
                    //see below for the absolutely stupid "solution" courtesy of https://forum.unity.com/threads/how-to-tell-if-a-scene-was-loaded-successfully-or-exists-in-the-project.479406/
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.NewGame)
                {
                    GameState.Reset();
                    MetaState.Instance.Clear();
                    if(string.IsNullOrEmpty(MetaState.Instance.NextScene))
                        MetaState.Instance.NextScene = CoreParams.InitialScene;
                    GameState.Instance.CurrentScene = MetaState.Instance.NextScene;
                    GameState.LoadInitial();
                    StartCoroutine(LoadNextSceneAsync());
                }
            }
            catch(Exception e)
            {
                //pokemon exception handling

                Modal.PushMessageModal(string.Format("{0}\n{1}", e.ToString(), e.StackTrace), "Error loading game", null, OnErrorConfirmed);
            }
            //TODO actually make this asynchronous, right now it just "loads" and locks up

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
            System.GC.Collect();
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