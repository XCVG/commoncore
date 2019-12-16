using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Triggers an action when an object enters
    /// </summary>
    /// <remarks>Can optionally check for the player object</remarks>
    public class OnTriggerEnterTrigger : ActionTrigger
    {
        public bool OnPlayerOnly = true;

        public bool CheckAllCollisions = false;

        void Start()
        {
            RestoreState();
        }

        void OnTriggerEnter(Collider other)
        {
            if (Triggered)
                return;

            //reject not-player if we're not allowing not-player
            if (OnPlayerOnly && other.gameObject == WorldUtils.GetPlayerObject())
                return;

            //execute special
            var activator = other.GetComponent<BaseController>();
            var data = new ActionInvokerData() { Activator = activator };
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