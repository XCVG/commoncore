using System.Collections.Generic;
using UnityEngine;
using CommonCore.RpgGame.State;

namespace CommonCore.State
{
    //EDIT THIS FILE AND PUT YOUR GAME DATA HERE

    public sealed partial class MetaState
    {
        //TODO refactor the way Intents work

        public void IntentsExecutePreload()
        {
            Debug.Log(string.Format("Executing intents preload ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
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


        public List<Intent> Intents { get; set; } = new List<Intent>();
        public PlayerSpawnIntent PlayerIntent { get; set; } //horrible for cleanliness but should be faster

    }

    

}