using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.DelayedEvents
{

    public class DelayedEventsModule : CCModule
    {
        public DelayedEventsModule()
        {
            //hook update loop for time update
            GameObject hookObject = new GameObject();
            TimeHook hookScript = hookObject.AddComponent<TimeHook>();
        }

    }
}