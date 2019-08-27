using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the movement for an Actor (abstract base class)
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public abstract class ActorMovementComponentBase : MonoBehaviour
    {
        [Header("Base"), SerializeField]
        protected ActorController ActorController;

        [SerializeField, FormerlySerializedAs("NavThreshold"), Tooltip("Distance to be considered at the target")]
        protected float TargetThreshold = 1.0f;

        [field: SerializeField] //probably not safe but okay for debuggering
        public virtual Vector3 MovementTarget { get; set; } //TODO visibility?

        [field: SerializeField]
        public virtual bool IsRunning { get; set; }

        [field: SerializeField]
        public virtual bool ControlRotation { get; set; } = true;

        public virtual void Init()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();
        }

        protected virtual void Start()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();
        }

        protected virtual void Update()
        {

        }

        /// <summary>
        /// Sets the destination and starts the actor moving toward it
        /// </summary>
        public abstract void SetDestination(Vector3 dest); //redundant?

        /// <summary>
        /// Aborts the actor's movement
        /// </summary>
        public abstract void AbortMove();

        /// <summary>
        /// If the actor is at the target (or close enough)
        /// </summary>
        public virtual bool AtTarget => DistToTarget <= TargetThreshold;

        /// <summary>
        /// How far away the actor is to the target
        /// </summary>
        public virtual float DistToTarget => (MovementTarget - transform.position).magnitude;

    }
}