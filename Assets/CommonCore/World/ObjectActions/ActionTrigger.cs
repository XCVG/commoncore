using UnityEngine;
using System.Collections;

namespace CommonCore.ObjectActions
{

    public abstract class ActionTrigger : MonoBehaviour
    {
        public bool Repeatable = false;

        public ActionSpecialEvent Special;

    }
}