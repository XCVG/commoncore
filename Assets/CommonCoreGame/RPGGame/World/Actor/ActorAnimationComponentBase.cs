using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the animations for an Actor (abstract base class)
    /// </summary>
    public abstract class ActorAnimationComponentBase : MonoBehaviour
    {

        [Header("Components"), SerializeField, Tooltip("You will need to set this if ActorAnimationComponent is not on the sme object")]
        protected ActorController ActorController;

        [Header("Etc")]
        public ActorAnimState CurrentAnimState = ActorAnimState.Idle;
        public bool LockAnimState = false;

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        /// <summary>
        /// Explicit initialization, call from ActorController
        /// </summary>
        public virtual void Init()
        {
            FindComponents();

            SetAnimationForced(CurrentAnimState);
        }


        protected virtual void FindComponents() //best to override this rather than overriding init or start
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{GetType().Name} on {name} is missing ActorController!");

        }

        public virtual void SetAnimation(ActorAnimState state)
        {
            if (LockAnimState)
                return;

            CurrentAnimState = state;

            SetAnimationForced(state);
        }

        public abstract void SetAnimationForced(ActorAnimState state);

        public abstract void SetAnimationForced(string stateName);

    }
}