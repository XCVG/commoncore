using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    public class OnStartTrigger : ActionTrigger
    {

        void Start()
        {
            RestoreState();

            if(!Triggered)
            {
                ActionInvokerData d = new ActionInvokerData();
                Special.Invoke(d);
                Triggered = true;
                SaveState();
            }            

        }


    }
}
