using CommonCore.ObjectActions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public class SpecialInteractableComponent : InteractableComponent
    {
        public ActionSpecialEvent Special;
        public bool Repeatable;

        private bool Locked;

        public override void OnActivate(GameObject activator)
        {
            //abort if locked or not enabled
            if (Locked || !enabled)
                return;

            //check eligibility of activator
            if (!CheckEligibility(activator))
                return;

            //execute special
            var activatorController = activator.GetComponent<BaseController>();
            var data = new ActionInvokerData() { Activator = activatorController };
            Special.Invoke(data);

            //lock if not repeatable
            if (!Repeatable)
                Locked = true;

        }
    }
}