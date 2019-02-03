using CommonCore.UI;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.State
{
    //TODO move this into Core or Scenes

    //TODO refactor and document this, it's a disaster zone holdover
    public class LoadingSceneController : MonoBehaviour
    {
        public Canvas DefaultLoadingCanvas;

        AsyncOperation sceneLoadOperation;

	    // Use this for initialization
        //TODO handling of errors
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
                    SceneManager.LoadScene(MetaState.Instance.NextScene);
                }
                else if (MetaState.Instance.TransitionType == SceneTransitionType.LoadGame)
                {
                    //we are loading a game, so load the game data and then load the next scene (which is part of save data)
                    GameState.DeserializeFromFile(CoreParams.SavePath + @"\" + MetaState.Instance.LoadSave);
                    MetaState.Instance.NextScene = GameState.Instance.CurrentScene;
                    SceneManager.LoadScene(MetaState.Instance.NextScene); //when this fails, it doesn't return a status code or throw an exception, only logs an error
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
                    SceneManager.LoadScene(MetaState.Instance.NextScene);
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
            MetaState.Instance.LoadingScreenPropOverride = null;
		
	    }

        void OnErrorConfirmed(ModalStatusCode status, string tag)
        {
            GameState.Reset();
            MetaState.Reset();
            System.GC.Collect();
            SceneManager.LoadScene("MainMenuScene");
        }


        //yes, this is really the best way to handle this
        //we will change to keeping our own list of scenes in the future because Unity's datastructures are fucking useless        
        //but that probably won't be until mod support is added in Citadel
        //we will also investigate LoadSceneAsync but something tells me the design won't be much better
        void HandleLog(string log, string stackTrace, LogType type)
        {
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

        //TODO handle all the nasty game init/game load/scene load stuff

    }

}