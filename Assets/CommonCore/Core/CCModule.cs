using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    public abstract class CCModule : IDisposable
    {
        //constructor=onapplicationstart

        //other lifecycle events
        public virtual void OnAllModulesLoaded() { }
        public virtual void OnGameStart() { }
        public virtual void OnSceneLoaded() { }
        public virtual void OnFrameUpdate() { }
        public virtual void OnSceneUnloaded() { }
        public virtual void OnGameEnd() { }
        public virtual void Dispose() { }
    }

    public class CCExplicitModuleAttribute : System.Attribute
    {

    }

    public class CCEarlyModuleAttribute : System.Attribute
    {

    }

    public class CCLateModuleAttribute : System.Attribute
    {

    }
}
