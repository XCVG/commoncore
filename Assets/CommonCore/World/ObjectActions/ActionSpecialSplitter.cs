using UnityEngine;
using System.Collections;
using System;

namespace CommonCore.ObjectActions
{
    public class ActionSpecialSplitter : ActionSpecial
    {
        public ActionSpecialEvent[] Specials;

        private bool Locked;
        
        public override void Execute(ActionInvokerData data)
        {
            if (Locked)
                return;

            foreach (ActionSpecialEvent sp in Specials)
            {
                sp.Invoke(data);
            }

            if (!Repeatable)
                Locked = true;
           
        }

    }
}