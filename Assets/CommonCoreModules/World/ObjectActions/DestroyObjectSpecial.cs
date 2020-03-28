using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    public class DestroyObjectSpecial : ActionSpecial
    {
        public GameObject Target;

        private bool Locked;

        private void Start()
        {
            if (Target == null)
                Target = this.gameObject;
        }

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if (Target == null)
                Debug.LogWarning(string.Format("DestroyObjectSpecial on {0} has no target!", gameObject.name));
            else
                Destroy(Target);

            if (!Repeatable)
                Locked = true;
        }

    }
}