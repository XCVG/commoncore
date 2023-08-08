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
        public bool CheckParentsForEntity = true;

        void Start()
        {
            RestoreState();
        }

        void OnTriggerExit(Collider other)
        {
            if (Triggered)
                return;

            var otherGameObject = other.gameObject;
            BaseController entity;
            if (CheckParentsForEntity)
            {
                entity = other.gameObject.GetEntity();
                if (entity != null)
                    otherGameObject = entity.gameObject;
            }
            else
            {
                entity = otherGameObject.GetComponent<BaseController>();
            }

            //reject not-player if we're not allowing not-player
            if (OnPlayerOnly && !otherGameObject.IsPlayer())
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && !otherGameObject.IsActor())
                return;

            //execute special
            var data = new ActionInvokerData() { Activator = entity, Caller = this };
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