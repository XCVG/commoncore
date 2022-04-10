using UnityEngine;
using System.Collections;
using System;

namespace CommonCore.ObjectActions
{

    public class ToggleObjectSpecial : ActionSpecial
    {
        public GameObject Target;
        public bool AlwaysForceDisable = false;
        public bool AlwaysForceEnable = false;

        private bool Locked;

        private void Start()
        {
            if (AlwaysForceDisable && AlwaysForceEnable)
                Debug.LogWarning(string.Format("ToggleObjectSpecial on {0} has both disable/enable flags set!", gameObject.name));

            if (Target == null)
                Target = this.gameObject;
        }

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if (AlwaysForceDisable)
                Target.SetActive(false);
            else if (AlwaysForceEnable)
                Target.SetActive(true);
            else
                Target.SetActive(!Target.activeSelf);

            if (!Repeatable)
                Locked = true;
        }

    }
}