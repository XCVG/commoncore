using UnityEngine;
using System.Collections;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    public abstract class ActionTrigger : MonoBehaviour
    {
        public bool Persistent = false;
        public bool Repeatable = true;
        [Tooltip("Set this to something unique for saving if the object isn't uniquely named")]
        public string SaveTag = string.Empty;

        protected bool Triggered;

        public ActionSpecialEvent Special;

        protected virtual string LookupName { get {
                return string.Format("{0}_{1}_{2}", gameObject.name, GetType().Name, string.IsNullOrEmpty(SaveTag) ? "ActionTrigger" : SaveTag);
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