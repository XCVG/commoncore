using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{

    public class StateModule : CCModule
    {
        public StateModule()
        {
            //poke delayed event handler to create it

            //create event hook
            HookTimeUpdate();
        }

        private void HookTimeUpdate()
        {
            //hook update loop for time update
            GameObject hookObject = new GameObject();
            TimeHook hookScript = hookObject.AddComponent<TimeHook>();
        }
    }
}