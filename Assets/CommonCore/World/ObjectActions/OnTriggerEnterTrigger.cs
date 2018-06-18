using UnityEngine;
using System.Collections;
using CommonCore.World;

namespace CommonCore.ObjectActions
{

    public class OnTriggerEnterTrigger : ActionTrigger
    {

        public bool OnPlayerOnly = true;
        public bool OnActorsOnly = true;

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
            if (OnPlayerOnly && other.GetComponent<PlayerController>() == null)
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && other.GetComponent<ActorController>() == null)
                return;

            //execute special
            var activator = other.GetComponent<BaseController>();
            var data = new ActionInvokerData() {Activator = activator};
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
