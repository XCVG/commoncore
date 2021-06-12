using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action on start
    /// </summary>
    public class OnStartTrigger : ActionTrigger
    {

        void Start()
        {
            RestoreState();

            if(!Triggered)
            {
                ActionInvokerData d = new ActionInvokerData() { Caller = this };
                Special.Invoke(d);
                Triggered = true;
                SaveState();
            }            

        }


    }
}
