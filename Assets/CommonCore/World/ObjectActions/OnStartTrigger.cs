using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    public class OnStartTrigger : ActionTrigger
    {

        void Start()
        {

            ActionInvokerData d = new ActionInvokerData();
            Special.Invoke(d);

        }


    }
}
