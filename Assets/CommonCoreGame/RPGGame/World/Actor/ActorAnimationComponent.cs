using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the animations for an Actor
    /// </summary>
    public class ActorAnimationComponent : ActorAnimationComponentBase
    {
        //TODO custom overrides (?!)

        [Header("Components"), SerializeField]
        private Animator AnimController;

        [Header("Options"), SerializeField]
        private string AttackAnimationOverride;

        protected override void FindComponents()
        {
            base.FindComponents();

            if (AnimController == null)
                AnimController = GetComponentInChildren<Animator>();

            if (AnimController == null)
                Debug.LogError($"{nameof(ActorAnimationComponent)} on {name} is missing Animator!");
        }

        public override void SetAnimation(ActorAnimState state)
        {
            if (LockAnimState)
                return;

            CurrentAnimState = state;

            SetAnimationForced(state);

        }

        public override void SetAnimationForced(ActorAnimState state)
        {
            if (AnimController != null)
            {
                string stateName = GetNameForAnimation(state);

                if (!string.IsNullOrEmpty(AttackAnimationOverride) && (state == ActorAnimState.Punching || state == ActorAnimState.Shooting))
                    stateName = AttackAnimationOverride;

                AnimController.Play(stateName);
            }
        }

        public override void SetAnimationForced(string stateName)
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