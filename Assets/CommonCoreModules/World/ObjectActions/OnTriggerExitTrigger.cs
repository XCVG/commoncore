using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action when an object leaves
    /// </summary>
    /// <remarks>Can optionally check for the player object or actor object</remarks>
    public class OnTriggerExitTrigger : ActionTrigger
    {
        [Header("On Trigger Exit Options")]
        public bool OnPlayerOnly = true;
        public bool OnActorsOnly = false;

        public bool CheckAllCollisions = false;

        void Start()
        {
            RestoreState();
        }

        void OnTriggerExit(Collider other)
        {
            if (Triggered)
                return;

            //reject not-player if we're not allowing not-player
            if (OnPlayerOnly && !other.gameObject.IsPlayer())
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && !other.gameObject.IsActor())
                return;

            //execute special
            var activator = other.GetComponent<BaseController>();
            var data = new ActionInvokerData() { Activator = activator, Caller = this };
            Special.Invoke(data);

            //lock if not repeatable
            if (!Repeatable)
            {
                Triggered = true;
                SaveState();
            }

        }

        void OnCollisionExit(Collision collision)
        {
            if (CheckAllCollisions)
                OnTriggerExit(collision.collider);
        }
    }
}