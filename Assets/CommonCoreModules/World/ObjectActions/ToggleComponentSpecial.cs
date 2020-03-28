using UnityEngine;
using System.Collections;
using System;

namespace CommonCore.ObjectActions
{

    public class ToggleComponentSpecial : ActionSpecial
    {
        public Behaviour Target;
        public bool AlwaysForceDisable = false;
        public bool AlwaysForceEnable = false;

        private bool Locked;

        private void Start()
        {
            if (AlwaysForceDisable && AlwaysForceEnable)
                Debug.LogWarning(string.Format("ToggleComponentSpecial on {0} has both disable/enable flags set!", gameObject.name));

            if (Target == null)
                Debug.LogWarning(string.Format("ToggleComponentSpecial on {0} has null target!", gameObject.name));
        }

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if (Target == null)
            {
                Debug.LogWarning(string.Format("ToggleComponentSpecial on {0} has null target!", gameObject.name));
                return;
            }

            if (AlwaysForceDisable)
                Target.enabled = false;
            else if (AlwaysForceEnable)
                Target.enabled = true;
            else
                Target.enabled = !Target.enabled;

            if (!Repeatable)
                Locked = true;
        }

    }
}