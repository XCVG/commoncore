using CommonCore.Config;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.RpgGame.World
{
    [RequireComponent(typeof(ActorController))]
    public abstract class ActorAttackComponentBase : MonoBehaviour
    {
        [Header("Base"), SerializeField]
        protected ActorController ActorController;

        public virtual void Init()
        {
            FindComponents();
        }

        protected virtual void FindComponents()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{nameof(ActorAttackComponent)} on {name} is missing ActorController!");


        }

        public abstract void BeginAttack();

        public abstract void UpdateAttack();

        public abstract void EndAttack();

        public abstract bool AttackIsDone { get; }
        public abstract ActorAiState PostAttackState { get; }

        public abstract bool ReadyToAttack { get; }

        //TODO chase handling (optional)
    }
}