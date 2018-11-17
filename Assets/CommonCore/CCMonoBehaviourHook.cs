using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * CommonCore Exit Hook
     * A slightly hacky way of hooking the ApplicationExit event which is only available to MonoBehaviour
     * We may rename and extend this later to hook other Monobehaviour-only events as well
     */
    internal class CCMonoBehaviourHook : MonoBehaviour
    { 
        public LifecycleEventDelegate OnUpdateDelegate;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void Update()
        {
            OnUpdateDelegate();
        }

    }

    //the actual delegate signature
    internal delegate void LifecycleEventDelegate();
}