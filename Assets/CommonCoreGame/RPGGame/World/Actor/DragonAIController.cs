using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.LockPause;
using System;
using CommonCore.World;
using CommonCore.Audio;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Dragon AI override component
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public class DragonAIController : MonoBehaviour
    {
        [SerializeField, Header("Components")]
        private ActorController ActorController;
        [SerializeField]
        private ParticleSystem FireParticleSystem; //for testing only
        [SerializeField]
        private Transform FirePoint;

        [SerializeField, Header("Circling Behavior")]
        private Vector3 CirclingCenter;
        [SerializeField]
        private float CirclingRadius = 25f;
        [SerializeField, Tooltip("How often to make a decision when circling")]
        private float CirclingDecideInterval = 3f;
        [SerializeField]
        private float CirclingAttackChance = 0.5f;

        [SerializeField, Header("Hover Attack Behavior")]
        private float HoverBreakoffDistance = 10f;
        [SerializeField]
        private float HoverDecideInterval = 1f;
        [SerializeField]
        private float HoverBreakoffChance = 0.5f;
        [SerializeField]
        private float HoverHeight = 5f;

        [SerializeField, Header("Dive Attack Behavior")]
        private float DiveStartDistance = 10f;
        [SerializeField]
        private float DiveAbortDistance = 25f;

        [SerializeField, Header("Flame Attack")]
        private ActorHitInfo FlameHit;
        [SerializeField]
        private float FlameInterval = 0.1f;
        [SerializeField]
        private float FlameRange = 5f;

        [SerializeField, Header("Claw Attack")]
        private ActorHitInfo ClawHit;
        [SerializeField]
        private float ClawRange = 3f;

        [SerializeField, Header("Misc")]
        private AIState State; //yeah it's another fucking state machine

        [SerializeField, Header("Audio")]
        private AudioSource FlySound;
        [SerializeField]
        private AudioSource FlameSound;
        [SerializeField]
        private AudioSource RoarSound;
        [SerializeField]
        private AudioSource DeathScreamSound;
        [SerializeField]
        private AudioClip HitSoundClip;
        [SerializeField]
        private AudioClip CrashSoundClip;

        private float TimeInState;

        private Vector3[] CirclingWaypoints; //waypoints to circle
        private int CurrentTargetWaypoint;
        private float TimeSinceLastDecision = 0;
        private float TimeSinceLastFlame = 0;

        private Vector3 SwoopDiveVector;
        private bool SwoopHitTarget;


        void Start()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            ActorController.LockAiState = true;

            ActorController.EnterState(ActorAiState.Idle);

            EnterState(State);
        }

        
        void Update()
        {
            if (LockPauseModule.IsPaused())
                return;

            UpdateState();
        }

        private void UpdateState()
        {
            if (!(State == AIState.Dead || State == AIState.Dying) && ActorController.Health <= 0)
                ChangeState(AIState.Dying);

            TimeInState += Time.deltaTime;
            TimeSinceLastDecision += Time.deltaTime;

            switch (State)
            {
                case AIState.Circling:
                    {
                        //if we're close enough to the waypoint, set waypoint to the next one
                        if (ActorController.MovementComponent.AtTarget)
                        {
                            CurrentTargetWaypoint++;
                            if (CurrentTargetWaypoint >= CirclingWaypoints.Length)
                                CurrentTargetWaypoint = 0;
                            ActorController.MovementComponent.SetDestination(CirclingWaypoints[CurrentTargetWaypoint]);
                        }

                        //if it's time to make a decision, make a decision
                        if(TimeSinceLastDecision >= CirclingDecideInterval)
                        {
                            TimeSinceLastDecision = 0;

                            if (UnityEngine.Random.Range(0f, 1f) < CirclingAttackChance)
                            {
                                //pick a target!
                                PickTarget();

                                if (ActorController.Target != null)
                                {
                                    //pick an attack and start attacking!
                                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                                        ChangeState(AIState.HoverChasingTarget);
                                    else
                                       ChangeState(AIState.SwoopLiningUp);
                                    //ChangeState(AIState.SwoopLiningUp);
                                }
                            }
                            else
                            {
                                //nop; continue circling
                            }
                            //we are the panzer elite, never retreat
                        }
                    }
                    break;
                case AIState.Escaping:
                    break;
                case AIState.Dying:
                    if (((FlyingActorMovementComponent)ActorController.MovementComponent).HeightAboveGround <= 0.5f)
                        ChangeState(AIState.Dead);
                    break;
                case AIState.Dead:
                    break;
                case AIState.HoverChasingTarget:
                    {
                        //set nav target
                        ActorController.MovementComponent.SetDestination(ActorController.Target.position);
                        if(ActorController.MovementComponent.AtTarget)
                        {
                            ChangeState(AIState.HoverAttackingTarget);
                        }
                    }
                    break;
                case AIState.HoverAttackingTarget:
                    {

                        //align flames
                        Vector3 dirMouthToTarget = (ActorController.Target.position - FirePoint.position).normalized;
                        FirePoint.transform.forward = dirMouthToTarget;

                        //deal damage to target
                        TimeSinceLastFlame += Time.deltaTime;
                        if (TimeSinceLastFlame >= FlameInterval)
                        {
                            TimeSinceLastFlame = 0;

                            DoFlameAttack();
                        }

                        //ActorController.MovementComponent.SetDestination(ActorController.Target.position); //continue chase?

                        if (TimeSinceLastDecision >= HoverDecideInterval)
                        {
                            TimeSinceLastDecision = 0;

                            if ((ActorController.Target.position - transform.position).ToFlatVec().magnitude >= HoverBreakoffDistance)
                                abortHoverAttack(); //TODO? may cascade this into a second breakoff decision
                            else if (UnityEngine.Random.Range(0f, 1f) < HoverBreakoffChance)
                                abortHoverAttack();
                        }

                        void abortHoverAttack()
                        {
                            ChangeState(AIState.Circling);
                        }
                    }
                    break;
                case AIState.SwoopLiningUp:
                    {
                        ActorController.MovementComponent.SetDestination(ActorController.Target.position);

                        if (ActorController.MovementComponent.DistToTarget <= DiveStartDistance)
                        {
                            //TODO check if we have room to do a dive

                            //start dive
                            ChangeState(AIState.SwoopDiving);
                        }
                        else if (ActorController.MovementComponent.DistToTarget > DiveAbortDistance)
                        {
                            ChangeState(AIState.Circling);
                        }
                    }
                    break;
                case AIState.SwoopDiving:
                    {
                        float trueDistToTarget = (ActorController.Target.position - transform.position).magnitude;
                        //Debug.Log($"d: {trueDistToTarget:F4}");
                        if (!SwoopHitTarget && trueDistToTarget < ClawRange)
                        {
                            DoClawAttack();
                            SwoopHitTarget = true;
                            ChangeState(AIState.SwoopFollowThrough);
                        }
                        else if(ActorController.MovementComponent.AtTarget)
                        {
                            ChangeState(AIState.SwoopFollowThrough);
                        }
                    }
                    break;
                case AIState.SwoopFollowThrough:
                    {
                        float trueDistToTarget = (ActorController.Target.position - transform.position).magnitude;
                        //Debug.Log($"f: {trueDistToTarget:F4}");
                        if (!SwoopHitTarget && trueDistToTarget < ClawRange)
                        {
                            SwoopHitTarget = true;
                            DoClawAttack();
                        }
                        if (ActorController.MovementComponent.AtTarget)
                            ChangeState(AIState.Circling);
                    }
                    break;
                default:
                    break;
            }
        }

        public void ChangeState(AIState newState)
        {
            TimeInState = 0;
            TimeSinceLastDecision = 0;
            ExitState();
            EnterState(newState);
        }

        private void EnterState(AIState newState)
        {

            switch (newState)
            {
                case AIState.Circling:
                    {
                        //recalculate waypoints from origin and set target to nearest waypoint
                        FlySound.Play();
                        ActorController.AnimationComponent.SetAnimationForced(ActorAnimState.Walking);
                        RecalculateWaypoints();
                        CurrentTargetWaypoint = GetNearestWaypoint(transform.position);
                        ActorController.MovementComponent.SetDestination(CirclingWaypoints[CurrentTargetWaypoint]);
                    }
                    break;
                case AIState.Escaping:
                    break;
                case AIState.Dead:
                    FlySound.Stop();
                    AudioPlayer.Instance.PlaySoundPositional(CrashSoundClip, false, transform.position);
                    ActorController.ForceEnterState(ActorAiState.Dead);
                    //ActorController.AnimationComponent.SetAnimationForced(ActorAnimState.Dead);
                    break;
                case AIState.Dying:
                    FlySound.Stop();
                    DeathScreamSound.Ref()?.Play();
                    ActorController.ForceEnterState(ActorAiState.Dead);
                    ActorController.AnimationComponent.SetAnimationForced(ActorAnimState.Dying);
                    break;
                case AIState.HoverChasingTarget:
                    FlySound.Play();
                    ActorController.MovementComponent.IsRunning = true;
                    ActorController.MovementComponent.SetDestination(ActorController.Target.position);
                    break;
                case AIState.HoverAttackingTarget:
                    TimeSinceLastFlame = 0;
                    FlameSound.Ref()?.Play();
                    FireParticleSystem.Ref()?.Play();
                    ((FlyingActorMovementComponent)ActorController.MovementComponent).YPositionOverride = ActorController.Target.position.y + HoverHeight;
                    break;
                case AIState.SwoopDiving:
                    FlySound.Stop();
                    ActorController.AnimationComponent.SetAnimationForced("glide");
                    SwoopHitTarget = false;
                    ActorController.MovementComponent.IsRunning = true;
                    RoarSound.Ref()?.Play();
                    ((FlyingActorMovementComponent)ActorController.MovementComponent).YPositionOverride = ActorController.Target.position.y;
                    SwoopDiveVector = (ActorController.Target.position - transform.position);
                    break;
                case AIState.SwoopFollowThrough:
                    FlySound.Play();
                    ActorController.AnimationComponent.SetAnimationForced(ActorAnimState.Walking);
                    ActorController.MovementComponent.SetDestination((transform.position + SwoopDiveVector).ToFlatVec());
                    break;
                default:
                    break;
            }


            State = newState;
        }

        private void ExitState()
        {
            switch (State)
            {
                case AIState.HoverChasingTarget:
                    ActorController.MovementComponent.IsRunning = false;
                    break;
                case AIState.HoverAttackingTarget:
                    FlameSound.Ref()?.Stop();
                    FireParticleSystem.Ref()?.Stop();
                    ((FlyingActorMovementComponent)ActorController.MovementComponent).YPositionOverride = null;
                    break;
                case AIState.SwoopDiving:
                    ActorController.MovementComponent.IsRunning = false;
                    ((FlyingActorMovementComponent)ActorController.MovementComponent).YPositionOverride = null;
                    break;
            }
        }

        private void DoFlameAttack()
        {
            //basically copied from ActorAttackComponent

            var modHit = FlameHit;
            modHit.Originator = ActorController;
            LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");

            var rc = Physics.RaycastAll(FirePoint.position, FirePoint.forward, FlameRange, lm, QueryTriggerInteraction.Collide);
            BaseController ac = null;
            foreach (var r in rc) //TODO move some of this to utils (RaycastForHit?)
            {
                var go = r.collider.gameObject;
                var ahgo = go.GetComponent<ActorHitboxComponent>();
                if (ahgo != null)
                {
                    ac = ahgo.ParentController;
                    break;
                }
                var acgo = go.GetComponent<ActorController>();
                if (acgo != null)
                {
                    ac = acgo;
                    break;
                }
            }
            if (ac != null && ac is ITakeDamage dac)
            {
                dac.TakeDamage(modHit);
            }
        }

        private void DoClawAttack()
        {
            Debug.LogWarning("ClawAttack!");

            //do damage to what we know is the target
            //TODO could be improved by a raycast or something
            var itd = ActorController.Target.GetComponent<BaseController>() as ITakeDamage;
            if(itd != null)
            {
                var modHit = ClawHit;
                modHit.Originator = ActorController;
                itd.TakeDamage(modHit);
                if(HitSoundClip != null)
                    AudioPlayer.Instance.PlaySoundPositional(HitSoundClip, false, ActorController.Target.position);
            }
        }

        private void PickTarget()
        {
            //always target the player for now
            ActorController.Target = RpgWorldUtils.GetPlayerController().transform;
        }

        /// <summary>
        /// Recalculate the waypoints based on the circling centre and radius
        /// </summary>
        private void RecalculateWaypoints()
        {
            //I hope 8 waypoints is enough
            int numWaypoints = 8;
            CirclingWaypoints = new Vector3[numWaypoints];

            float sliceAngle = 360f / numWaypoints; //45 degrees
            for(int i = 0; i < numWaypoints; i++)
            {
                Vector3 vecCenterToPoint = (Quaternion.AngleAxis(i * sliceAngle, Vector3.up) * Vector3.right) * CirclingRadius;
                Vector3 waypoint = CirclingCenter + vecCenterToPoint;
                CirclingWaypoints[i] = waypoint;
            }

        }

        /// <summary>
        /// Finds the index of the nearest waypoint to a point
        /// </summary>
        private int GetNearestWaypoint(Vector3 point)
        {
            if (CirclingWaypoints == null || CirclingWaypoints.Length == 0)
                return -1;

            int closestWaypoint = 0;
            float closestDistance = float.MaxValue;
            for(int i = 0; i < CirclingWaypoints.Length; i++)
            {
                float distance = (point - CirclingWaypoints[i]).ToFlatVec().magnitude;
                if(distance < closestDistance)
                {
                    closestWaypoint = i;
                    closestDistance = distance;
                }
            }

            return closestWaypoint;
        }

        public enum AIState
        {
            Circling, Escaping, Dying, Dead, //not attacking
            HoverChasingTarget, HoverAttackingTarget, //hover attack
            LandHeadingToSpot, LandAttackingTarget, LandAtSpot, //land-and-burn attack
            SwoopLiningUp, SwoopDiving, SwoopFollowThrough //swoop attack
        }

    }
}