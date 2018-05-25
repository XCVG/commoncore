using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public class WorldSceneController : BaseSceneController
    {
        public bool Autoload = true;
        
        public override void Awake()
        {
            base.Awake();
            Debug.Log("World Scene Controller Awake");
        }

        public override void Start()
        {
            base.Start();
            Debug.Log("World Scene Controller Start");
            if(Autoload)
            {
                MetaState.Instance.IntentsExecutePreload();
                Restore();
                MetaState.Instance.IntentsExecutePostload();
            }
        }
        	    
    }

}