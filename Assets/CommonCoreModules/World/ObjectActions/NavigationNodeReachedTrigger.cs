using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action when the attached navigation node is reached
    /// </summary>
    /// <remarks>
    /// <para>Requires the path follower or other script to explicitly trigger this when the node is reached</para>
    /// </remarks>
    [RequireComponent(typeof(NavigationNode))]
    public class NavigationNodeReachedTrigger : ActionTrigger
    {
        [Header("Navigation Node Trigger Options")]
        public bool OnPlayerOnly = true;
        public bool OnActorsOnly = false;

        void Start()
        {
            RestoreState();
        }

        public void Activate(BaseController activator)
        {
            if (Triggered)
                return;

            //reject not-player if we're not allowing not-player
            if (OnPlayerOnly && !activator.gameObject.IsPlayer())
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && !activator.gameObject.IsActor())
                return;

            //execute special
            var data = new ActionInvokerData() { Activator = activator, Caller = this, Position = transform.position, Rotation = transform.rotation };
            Special.Invoke(data);

            //lock if not repeatable
            if (!Repeatable)
            {
                Triggered = true;
                SaveState();
            }
        }

    }
}