using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public class TooltipInteractableComponent : InteractableComponent
    {
        public override void Start()
        {
            base.Start();

            if (string.IsNullOrEmpty(Tooltip) && TooltipObject == null)
            {
                Debug.LogWarning(string.Format("TooltipInteractableComponent attached to {0} has no tooltip!", gameObject.name));
                Tooltip = gameObject.name;
            }
        }

        public override void OnActivate(GameObject activator)
        {
            //do nothing
        }

    }
}