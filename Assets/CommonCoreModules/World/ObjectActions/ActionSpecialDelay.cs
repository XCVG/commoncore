using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    public class ActionSpecialDelay : ActionSpecial
    {
        public ActionSpecialEvent Special;
        public float Delay;
        public bool Concurrent;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            StartCoroutine(WaitAndExecute(data));

            if (!Concurrent || !Repeatable)
                Locked = true;
        }

        private IEnumerator WaitAndExecute(ActionInvokerData data)
        {
            yield return new WaitForSeconds(Delay);
            Special.Invoke(data);

            if (Repeatable)
                Locked = false;
        }

    }
}