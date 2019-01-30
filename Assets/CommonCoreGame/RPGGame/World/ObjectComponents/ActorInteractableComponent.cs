using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.ObjectActions;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{

    public class ActorInteractableComponent : InteractableComponent
    {
        public delegate void ControllerOnInteract(ActionInvokerData data);

        public ControllerOnInteract ControllerOnInteractDelegate { get; set; }

        public override void OnActivate(GameObject activator)
        {
            ControllerOnInteractDelegate.Invoke(new ActionInvokerData{ Activator = activator.GetComponent<BaseController>() });
        }
    }
}