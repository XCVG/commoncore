using UnityEngine;
using System.Collections;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    public abstract class ActionTrigger : MonoBehaviour
    {
        public bool Persistent = false;
        public bool Repeatable = false;

        protected bool Triggered;

        public ActionSpecialEvent Special;

        protected virtual string LookupName { get {
                return string.Format("{0}_{1}_{2}", gameObject.name, GetType().Name, "ActionTrigger");
            } }

        protected virtual void RestoreState()
        {
            if (!Persistent)
                return;

            object status;
            if (BaseSceneController.Current.LocalStore.TryGetValue(LookupName, out status))
            {
                Triggered = (bool)status;
            }
        }

        protected virtual void SaveState()
        {
            if (!Persistent)
                return;

            BaseSceneController.Current.LocalStore[LookupName] = Triggered;
        }
    }
}