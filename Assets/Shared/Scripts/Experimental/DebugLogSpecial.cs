using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions.Experimental
{
    public class DebugLogSpecial : ActionSpecial
    {
        private bool Activated = false;

        public override void Execute(ActionInvokerData data)
        {
            string message = "[DebugLogSpecial]";
            if (data.Activator != null)
                message += $" | activator: {data.Activator.name}";
            if (data.Caller != null)
                message += $" | caller: {data.Caller.GetType()}";

            if (!AllowInvokeWhenDisabled && !isActiveAndEnabled)
                message += " | ShouldBeDisabled";

            if (!Repeatable && Activated)
                message += " | ShouldNotBeRepeated";

            Debug.Log(message);
            Activated = true;
        }
    }
}


