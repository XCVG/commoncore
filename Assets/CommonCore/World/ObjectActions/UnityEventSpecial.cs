using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore.ObjectActions
{
    //simple bridge necessary for a few edge cases
    public class UnityEventSpecial : ActionSpecial
    {
        public UnityEvent Event;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            Event.Invoke();

            if (!Repeatable)
                Locked = true;
        }
    }
}