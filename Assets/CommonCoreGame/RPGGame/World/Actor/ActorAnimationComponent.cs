using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the animations for an Actor
    /// </summary>
    public class ActorAnimationComponent : MonoBehaviour //eventually we'll make an abstract class so we can swap them in and out
    {
        //TODO custom overrides

        [Header("Components"), SerializeField]
        private Animator AnimController;

        [SerializeField, Tooltip("You will need to set this if ActorAnimationComponent ")]
        private ActorController ActorController;

        [Header("Options"), SerializeField]
        private string AttackAnimationOverride;

        //TODO visibility, proxy fields and attribute spam
        [Header("Etc")]        
        public ActorAnimState CurrentAnimState = ActorAnimState.Idle;
        public bool LockAnimState = false;

        private void Start()
        {
            FindComponents();

        }

        private void FindComponents()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{nameof(ActorAnimationComponent)} on {name} is missing ActorController!");

            if (AnimController == null)
                AnimController = GetComponentInChildren<Animator>();

            if (AnimController == null)
                Debug.LogError($"{nameof(ActorAnimationComponent)} on {name} is missing Animator!");
        }

        //let's go with explicit initialization
        public void Init()
        {
            FindComponents();

            SetAnimationForced(CurrentAnimState);
        }

        public void SetAnimation(ActorAnimState state)
        {
            if (LockAnimState)
                return;

            CurrentAnimState = state;

            SetAnimationForced(state);

        }

        public void SetAnimationForced(ActorAnimState state)
        {
            if (AnimController != null)
            {
                string stateName = GetNameForAnimation(state);

                if (!string.IsNullOrEmpty(AttackAnimationOverride) && (state == ActorAnimState.Punching || state == ActorAnimState.Shooting))
                    stateName = AttackAnimationOverride;

                AnimController.Play(stateName);

                //TODO sounds, eventually
            }
        }

        public void SetAnimationForced(string stateName)
        {
            if (AnimController != null)
            {
                AnimController.Play(stateName);
            }
        }

        private static string GetNameForAnimation(ActorAnimState state) //TODO non-stupid standard names
        {
            //TODO allow overrides?

            switch (state)
            {
                case ActorAnimState.Idle:
                    return "idle";
                case ActorAnimState.Dead:
                    return "dead";
                case ActorAnimState.Dying:
                    return "dying";
                case ActorAnimState.Hurting:
                    return "pain";
                case ActorAnimState.Walking:
                    return "walk";
                case ActorAnimState.Talking:
                    return "talk";
                case ActorAnimState.Running:
                    return "run";
                case ActorAnimState.Shooting:
                    return "shoot";
                case ActorAnimState.Punching:
                    return "punch";
                default:
                    return string.Empty;
            }
        }
    }
}