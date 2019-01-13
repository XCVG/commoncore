using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    public struct ActionInvokerData
    {
        public BaseController Activator;
    }

    [System.Serializable]
    public class ActionSpecialEvent : UnityEvent<ActionInvokerData> { }

    //used to implement callback pattern for certain ActionSpecials
    [System.Serializable]
    public class ObjectSpawnEvent : UnityEvent<GameObject, ActionSpecial> { }

    public abstract class ActionSpecial : MonoBehaviour
    {

        public bool Repeatable = true;

        public abstract void Execute(ActionInvokerData data);
        	
    }
}