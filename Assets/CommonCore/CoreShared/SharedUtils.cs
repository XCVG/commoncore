using CommonCore.State;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore
{
    /// <summary>
    /// Functions for manipulating general game state
    /// </summary>
    public static class SharedUtils
    {

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to default start scene
        /// </summary>
        public static void StartGame()
        {
            StartGame(null);
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void StartGame(string sceneOverride)
        {            
            MetaState.Instance.Clear();
            MetaState.Instance.NextScene = sceneOverride;
            MetaState.Instance.TransitionType = SceneTransitionType.NewGame;
            //GC.Collect();            
            SceneManager.LoadScene("LoadingScene");            
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to main menu scene
        /// </summary>
        public static void EndGame()
        {
            EndGame(null);
        }

        /// <summary>
        /// Clears data, sets up MetaState, and transitions to specified scene
        /// </summary>
        public static void EndGame(string sceneOverride)
        {
            //MetaState.Instance.Clear();
            MetaState.Instance.NextScene = sceneOverride;
            MetaState.Instance.TransitionType = SceneTransitionType.EndGame;
            //GC.Collect();
            SceneManager.LoadScene("LoadingScene");
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene)
        {
            ChangeScene(scene, false);
        }

        /// <summary>
        /// Changes to a new scene, setting up state and calling transitions appropriately
        /// </summary>
        public static void ChangeScene(string scene, bool skipLoading)
        {
            MetaState mgs = MetaState.Instance;
            mgs.PreviousScene = SceneManager.GetActiveScene().name;
            mgs.NextScene = scene;
            mgs.LoadSave = null;
            mgs.TransitionType = SceneTransitionType.ChangeScene;
            mgs.SkipLoadingScreen = skipLoading;

            TryGetSceneController().Ref()?.ExitScene(); //run scene exit routine if a scene controller exists

            if (CoreParams.UseDirectSceneTransitions) // || skipLoading)
            {
                MetaState.Instance.IntentsExecuteLoading();
                SceneManager.LoadScene(scene);
            }
            else
            {
                SceneManager.LoadScene("LoadingScene"); //TODO put loading scene name somewhere
            }
            
        }

        /// <summary>
        /// Loads a saved game to state and transitions to its scene
        /// </summary>
        public static void LoadGame(string saveName)
        {
            MetaState.Instance.Clear();
            MetaState mgs = MetaState.Instance;
            mgs.LoadSave = saveName;
            mgs.TransitionType = SceneTransitionType.LoadGame;

            if (CoreParams.UseDirectSceneTransitions)
            {
                throw new NotImplementedException(); //TODO move LoadingSceneController functionality here or something
            }
            else
            {
                SceneManager.LoadScene("LoadingScene");
            }
        }

        /// <summary>
        /// Gets the scene controller (returns null on fail)
        /// </summary>
        public static BaseSceneController TryGetSceneController()
        {
            //TODO refactor this to use CoreUtils.GetWorldRoot

            GameObject go = GameObject.FindGameObjectWithTag("GameController");
            if (go != null)
            {
                BaseSceneController bsc = go.GetComponent<BaseSceneController>();

                if (bsc != null)
                    return bsc;
            }

            //couldn't find it, try grabbing WorldRoot
            go = GameObject.Find("WorldRoot");
            if (go != null)
            {
                BaseSceneController bsc = go.GetComponent<BaseSceneController>();

                if (bsc != null)
                    return bsc;
            }

            return null;
        }

        /// <summary>
        /// Gets the scene controller (throws on fail)
        /// </summary>
        public static BaseSceneController GetSceneController()
        {
            BaseSceneController bsc = TryGetSceneController();

            if (bsc != null)
                return bsc;

            //still couldn't find it, throw an error
            Debug.LogError("Couldn't find SceneController");

            throw new NullReferenceException(); //not having a scene controller is fatal
        }

    }
}