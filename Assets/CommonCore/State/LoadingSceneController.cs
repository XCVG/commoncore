using CommonCore.World;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneControllers
{
    
    public class LoadingSceneController : MonoBehaviour
    {
        public GameObject DefaultLoadingObject;
        public Canvas DefaultLoadingCanvas;

        AsyncOperation sceneLoadOperation;

	    // Use this for initialization
	    void Start ()
        {
            //the loading screen cannot be truly skipped, but it can be hidden
            if (!MetaState.Instance.SkipLoadingScreen)
            {
                //appear the overlay
                DefaultLoadingCanvas.gameObject.SetActive(true);

            }

            if(MetaState.Instance.TransitionType == SceneTransitionType.ChangeScene)
            {
                MetaState.Instance.IntentsExecuteLoading();
                //we are merely changing scenes, go straight to loading the next scene
                SceneManager.LoadScene(MetaState.Instance.NextScene);
            }
            else if(MetaState.Instance.TransitionType == SceneTransitionType.LoadGame)
            {
                //we are loading a game, so load the game data and then load the next scene (which is part of save data)
                Loader.LoadGame();
                SceneManager.LoadScene(MetaState.Instance.NextScene);
            }
            else if(MetaState.Instance.TransitionType == SceneTransitionType.NewGame)
            {
                Loader.CreateNewGame();
                SceneManager.LoadScene(MetaState.Instance.NextScene);
            }

            //TODO actually make this asynchronous, right now it just "loads" and locks up

            //clear certain metagamestate on use
            MetaState.Instance.SkipLoadingScreen = false;
            MetaState.Instance.LoadingScreenPropOverride = null;
		
	    }
	
	    // Update is called once per frame
	    void Update ()
        {

		
	    }

        protected void LoadScene()
        {

        }

        //TODO handle all the nasty game init/game load/scene load stuff

    }

}