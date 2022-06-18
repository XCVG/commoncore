using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * Hook class for event functions etc that can only be called on MonoBehaviour
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

        public void JSCallThunk(string str)
        {
            JSCrossCall.JSCallThunk(str);
        }

    }

    //the actual delegate signature
    internal delegate void LifecycleEventDelegate();
}