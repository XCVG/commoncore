using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    public abstract class CCModule
    {
        //constructor=onapplicationstart

        //other lifecycle events
        public virtual void OnApplicationQuit() { }
        public virtual void OnGameStart() { }
        public virtual void OnSceneLoaded() { }
        public virtual void OnFrameUpdate() { }
        public virtual void OnSceneUnloaded() { }
        public virtual void OnGameEnd() { }
    }

    public class CCExplicitModule : System.Attribute
    {

    }
}
