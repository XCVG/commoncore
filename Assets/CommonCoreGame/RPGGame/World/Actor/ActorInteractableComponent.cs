using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.ObjectActions;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// This handles the actual interaction invocation and is attached to the hitbox
    /// </summary>
    /// <remarks>It's kinda weird, pretty sure that's because it's ported from FSVR</remarks>
    public class ActorInteractableComponent : InteractableComponent
    {
        public delegate void ControllerOnInteract(ActionInvokerData data);

        public ControllerOnInteract ControllerOnInteractDelegate { get; set; }

        public override void OnActivate(GameObject activator)
        {
            ControllerOnInteractDelegate.Invoke(new ActionInvokerData{ Activator = activator.GetComponent<BaseController>(), Caller = this });
        }
    }
}