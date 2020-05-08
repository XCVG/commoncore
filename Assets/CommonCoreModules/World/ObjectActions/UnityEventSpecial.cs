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
        public ActionSpecialEvent ActionEvent;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if(Event != null)
                Event.Invoke();
            if(ActionEvent != null)
                ActionEvent.Invoke(data);

            if (!Repeatable)
                Locked = true;
        }
    }
}