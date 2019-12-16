using CommonCore.ObjectActions;
using CommonCore.RpgGame.World;
using CommonCore.World;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Triggers an action when an actor or player enters
    /// </summary>
    public class OnActorEnterTrigger : ActionTrigger
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
            if (OnPlayerOnly && !other.gameObject.IsPlayer())
                return;

            //reject non-actors if we're not allowing not-actor
            if (OnActorsOnly && !other.gameObject.IsActor())
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
