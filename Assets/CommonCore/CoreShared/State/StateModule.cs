using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    /// <summary>
    /// CommonCore State helper module.
    /// Currently handles PersistState and that's about it.
    /// </summary>
    [CCEarlyModule]
    public class StateModule : CCModule
    {
        public StateModule()
        {
            PersistState.Load();
            PersistState.Save();
        }
    }
}