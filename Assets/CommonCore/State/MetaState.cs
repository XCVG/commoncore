using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    //TODO move away from static singleton model
    public sealed class MetaState
    {
        private static MetaState instance;

        private MetaState()
        {
            //TODO initialization
            Intents = new List<Intent>();
        }

        public static MetaState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MetaState();
                }
                return instance;
            }
        }

        public static void Reset()
        {
            instance = new MetaState();
        }

        public void IntentsExecutePreload()
        {
            Debug.Log(string.Format("Executing intents preload ({0} total)",Intents.Count));
            foreach(Intent i in Intents)
            {
                i.PreloadExecute();
            }
        }

        public void IntentsExecutePostload()
        {
            Debug.Log(string.Format("Executing intents postload ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
            {
                i.PostloadExecute();
            }
        }

        public void IntentsExecuteLoading()
        {
            Debug.Log(string.Format("Executing intents loading ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
            {
                i.LoadingExecute();
            }
        }

        //actual instance data
        public SceneTransitionType TransitionType { get; set; }
        public string PreviousScene { get; set; }
        public string NextScene { get; set; }
        public string LoadSave { get; set; }
        public List<Intent> Intents { get; set; }
        public PlayerSpawnIntent PlayerIntent { get; set; } //horrible for cleanliness but should be faster
        public string LoadingScreenPropOverride { get; set; }
        public bool SkipLoadingScreen { get; set; }

    }

    public enum SceneTransitionType
    {
        NewGame, LoadGame, ChangeScene
    }

}