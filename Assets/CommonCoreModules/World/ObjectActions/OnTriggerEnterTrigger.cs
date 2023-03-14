using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action when an object enters
    /// </summary>
    /// <remarks>Can optionally check for the player object or actor object</remarks>
    public class OnTriggerEnterTrigger : ActionTrigger
    {
        [Header("On Trigger Enter Options")]
        public bool OnPlayerOnly = true;
        public bool OnActorsOnly = false;

        public bool CheckAllCollisions = false;

        void Start()
        {
            RestoreState();
        }

        void OnTriggerEnter(Collider other)
        {
            if (Triggered)
                return;

            var otherGameObject = other.gameObject;
            var entity = other.gameObject.GetEntity();
            if (entity != null)
                otherGameObject = entity.gameObject;

            //reject not-player if we're not allowing not-player
            if (OnPlayerOnly && !otherGameObject.IsPlayer())
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && !otherGameObject.IsActor())
                return;

            //execute special
            var activator = entity;
            var data = new ActionInvokerData() { Activator = activator, Caller = this };
            Special.Invoke(data);

            //lock if not repeatable
            if (!Repeatable)
            {
                Triggered = true;
                SaveState();
            }

        }

        void OnCollisionEnter(Collision collision)
        {
            if (CheckAllCollisions)
                OnTriggerEnter(collision.collider);
        }
    }
}