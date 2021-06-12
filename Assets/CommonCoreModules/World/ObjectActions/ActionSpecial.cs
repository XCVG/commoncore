using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using CommonCore.World;
using System.Collections.Generic;

namespace CommonCore.ObjectActions
{

    public struct ActionInvokerData
    {
        public BaseController Activator;
        public object Caller;

        public Vector3? Position;
        public Quaternion? Rotation;
        public Vector3? Velocity;

        public IDictionary<string, object> ExtraData;
    }

    [System.Serializable]
    public class ActionSpecialEvent : UnityEvent<ActionInvokerData> { }

    //used to implement callback pattern for certain ActionSpecials
    [System.Serializable]
    public class ObjectSpawnEvent : UnityEvent<GameObject, ActionSpecial> { }

    public abstract class ActionSpecial : MonoBehaviour
    {

        public bool Repeatable = true;
        public bool AllowInvokeWhenDisabled = false;

        public abstract void Execute(ActionInvokerData data);
        	
    }
}