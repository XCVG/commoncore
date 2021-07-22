using CommonCore.Config;
using CommonCore.World;
using System;
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

        public Transform ShootPoint;

        public virtual bool HandlesChaseDestination => false;
        public virtual bool HandlesSelectTarget => false;

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

        /// <summary>
        /// Checks if this actor has line of sight between ShootPoint and the target
        /// </summary>
        protected bool CheckLineOfSight(Transform target)
        {
            Vector3 targetPos = target.position;

            //closest point on collider would probably be better but we use center mass for everything else
            var iat = target.GetComponent<IAmTargetable>();
            if(iat.Ref() != null)
            {
                targetPos = iat.TargetPoint;
            }

            Vector3 vecToTarget = targetPos - ShootPoint.position;
            Vector3 dirToTarget = vecToTarget.normalized;
            float distToTarget = vecToTarget.magnitude;

            var hits = Physics.RaycastAll(ShootPoint.position, dirToTarget, distToTarget + 1, WorldUtils.GetAttackLayerMask(), QueryTriggerInteraction.Collide);
            RaycastHit? targetHit = null;
            float closestHitDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.collider.GetComponentInParent<ActorController>() == ActorController)
                    continue;

                if (hit.transform == target || hitTargetHitbox(hit) || hitTargetEntity(hit))
                {
                    targetHit = hit;
                }

                if(hit.distance < closestHitDist)
                {
                    closestHitDist = hit.distance;
                }
            }

            if (!targetHit.HasValue)
                return false;

            return targetHit.Value.distance <= closestHitDist;

            bool hitTargetHitbox(RaycastHit hit)
            {
                var hitbox = hit.transform.GetComponent<IHitboxComponent>();
                if(hitbox.Ref() != null && hitbox.ParentController != null)
                {
                    return hitbox.ParentController.transform == target;
                }

                return false;
            }

            bool hitTargetEntity(RaycastHit hit)
            {
                var entity = hit.transform.GetComponentInParent<BaseController>();
                return entity != null && entity.transform == target;
            }
        }

        public abstract void BeginAttack();

        public abstract void UpdateAttack();

        public abstract void EndAttack();

        public abstract bool AttackIsDone { get; }
        public abstract ActorAiState PostAttackState { get; }

        public abstract bool ReadyToAttack { get; }

        //optional functionality

        public virtual Transform SelectTarget() => throw new NotSupportedException();

        public virtual Vector3 GetChaseDestination(bool initial) => throw new NotSupportedException();
    }

}