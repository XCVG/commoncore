using UnityEngine;
using System;
using System.Collections.Generic;
using CommonCore.State;
using UnityEngine.SceneManagement;
using CommonCore.DebugLog;

namespace CommonCore.World
{

    public abstract class RestorableComponent : MonoBehaviour
    {
        public abstract RestorableData Save();
        public abstract void Restore(RestorableData data);
    }

    //blank restorable components are meant for simple, fast data rather than fully restoring dynamic objects
    //these are ALWAYS local, but cannot inherit from Local for obvious reasons
    public abstract class BlankRestorableComponent : RestorableComponent
    {

    }


}