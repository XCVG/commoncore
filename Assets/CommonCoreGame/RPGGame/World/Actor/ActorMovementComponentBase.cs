using CommonCore.Config;
using System.Collections.Generic;
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
        public float TargetThreshold = 1.0f;

        [SerializeField]
        public float PhysicsMass = 100f;

        [field: SerializeField] //probably not safe but okay for debuggering
        public virtual Vector3 MovementTarget { get; set; } //TODO visibility?

        [field: SerializeField]
        public virtual bool IsRunning { get; set; }

        [field: SerializeField]
        public virtual bool ControlRotation { get; set; } = true;

        [field: SerializeField]
        public Vector3 PhysicsVelocity { get; protected set; } = Vector3.zero;

        public virtual void Init()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();
        }

        public virtual void BeforeRestore(Dictionary<string, object> data) { }
        public virtual void BeforeCommit(Dictionary<string, object> data) { }

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

        /// <summary>
        /// Handles the actor dying
        /// </summary>
        public virtual void HandleDeath() { }

        /// <summary>
        /// Handles the actor resurrecting
        /// </summary>
        public virtual void HandleRaise() { }

        /// <summary>
        /// Handles a difficulty change (if necessary)
        /// </summary>
        public virtual void HandleDifficultyChanged() { }

        /// <summary>
        /// Sets the physics velocity (will have no effect if the movement component does not support physics)
        /// </summary>
        public virtual void SetVelocity(Vector3 velocity) => PhysicsVelocity = velocity;

        /// <summary>
        /// Adds to the physics velocity (will have no effect if the movement component does not support physics)
        /// </summary>
        public virtual void AddVelocity(Vector3 velocity) => PhysicsVelocity += velocity;

        /// <summary>
        /// Adds to the physics velocity taking mass into account
        /// </summary>
        public virtual void Push(Vector3 impulse) => PhysicsVelocity += impulse / PhysicsMass;

        /// <summary>
        /// Checks if this actor can reach the specified location
        /// </summary>
        public abstract bool CheckLocationReachable(Vector3 location);

        /// <summary>
        /// If this actor is stuck and cannot continue to move toward its target
        /// </summary>
        public abstract bool IsStuck { get; }

        /// <summary>
        /// The speed factor from the difficulty selection
        /// </summary>
        protected float DifficultySpeedFactor => ActorController.EffectiveAggression;

    }
}