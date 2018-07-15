using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CommonCore.ObjectActions;
using CommonCore.State;
using CommonCore.DebugLog;
using CommonCore.Dialogue;
using CommonCore.Messaging;

namespace CommonCore.World
{
    //TODO restorable, animation, and eventually a full refactor
    public class ActorController : BaseController
    {
        public string CharacterModelIdOverride;

        [Header("Components")]
        public CharacterController CharController;
        public Animator AnimController;
        public ActorInteractableComponent InteractComponent;
        public NavMeshAgent NavComponent;

        [Header("State")]
        public ActorAiState BaseAiState = ActorAiState.Idle;
        public ActorAiState CurrentAiState = ActorAiState.Idle;
        public bool LockAiState = false;
        public ActorAnimState CurrentAnimState = ActorAnimState.Idle;
        public bool LockAnimState = false;
        public Transform Target;
        public Vector3 AltTarget;
        private float TimeInState;

        [Header("Damage")]
        public float Health = 1.0f;
        public bool DieImmediately = false;
        public bool DestroyOnDeath = false;
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Radiation")]
        public float[] DamageResistance = { 0, 0, 0, 0, 0, 0};
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Radiation")]
        public float[] DamageThreshold = { 0, 0, 0, 0, 0, 0 };
        public ActionSpecial OnDeathSpecial;

        [Header("Aggression")]
        public bool Aggressive = false;
        public bool Relentless = false;
        public bool UseLineOfSight = false;
        public float SearchRadius = 25.0f;

        [Header("Interaction")]
        public ActorInteractionType Interaction;
        public string InteractionTarget;
        public ActionSpecial InteractionSpecial;

        public EditorConditional AlternateCondition;
        public ActorInteractionType AltInteraction;
        public string AltInteractionTarget;
        public ActionSpecial AltInteractionSpecial;

        public string TooltipOverride;

        [Header("Movement")]
        public bool ForceNavmeshOff = false;
        public bool UseControllerGravity = true;
        public bool RunOnChase = true;
        public bool RunOnFlee = true;
        public float WalkSpeed = 1.0f;
        public float RunSpeed = 2.0f;
        public float RotateSpeed = 90.0f;
        private bool NavEnabled;
        private bool IsRunning;
        public float WanderThreshold = 1.0f;
        public float WanderTimeout = 10.0f;
        public Vector2 WanderRadius = new Vector2(10.0f, 10.0f);
        private Vector3 InitialPosition;

        public override void Start()
        {
            base.Start();

            if (CharController == null)
                CharController = GetComponent<CharacterController>();
            if (CharController == null)
                CDebug.LogEx(name + " couldn't find CharacterController", LogLevel.Error, this);

            if (AnimController == null)
                AnimController = GetComponent<Animator>();
            if (AnimController == null)
                CDebug.LogEx(name + " couldn't find Animator", LogLevel.Warning, this);

            if (NavComponent == null)
                NavComponent = GetComponent<NavMeshAgent>();
            if( NavComponent == null)
                CDebug.LogEx(name + " couldn't find NavMeshAgent", LogLevel.Warning, this);

            SetInitialNavState();

            if (InteractComponent == null)
                InteractComponent = GetComponent<ActorInteractableComponent>();

            if (InteractComponent == null)
                InteractComponent = GetComponentInChildren<ActorInteractableComponent>();

            if(InteractComponent != null)
            {
                InteractComponent.ControllerOnInteractDelegate = OnInteract;
                if (!string.IsNullOrEmpty(TooltipOverride))
                    InteractComponent.Tooltip = TooltipOverride;
                else
                    InteractComponent.Tooltip = name;
            }
            else
            {
                CDebug.LogEx(name + " couldn't find ActorInteractableComponent", LogLevel.Error, this);
            }            

            EnterState(CurrentAiState);
        }

        public override void Update()
        {
            base.Update();

            TimeInState += Time.deltaTime;
            UpdateState();
            EmulateNav();
        }

        //TODO handle aggression
        private void EnterState(ActorAiState newState)
        {
            if (LockAiState)
                return;

            ExitState(CurrentAiState); //good place or no?

            TimeInState = 0;

            switch (newState)
            {
                case ActorAiState.Idle:
                    SetAnimation(ActorAnimState.Idle);
                    break;
                case ActorAiState.Dead:
                    AbortNav();
                    if (DieImmediately)
                        SetAnimation(ActorAnimState.Dead);
                    else
                        SetAnimation(ActorAnimState.Dying);
                    if (DestroyOnDeath)
                        Destroy(this.gameObject);

                    if (OnDeathSpecial != null)
                        OnDeathSpecial.Execute(new ActionInvokerData { Activator = this });
                    break;
                case ActorAiState.Wandering:
                    SetAnimation(ActorAnimState.Walking);
                    //set initial destination
                    Vector2 newpos = CCBaseUtil.GetRandomVector(InitialPosition.ToFlatVec(), WanderRadius);
                    SetDestination(newpos.ToSpaceVec());
                    break;
                case ActorAiState.Chasing:
                    if (RunOnChase)
                    {
                        IsRunning = true;
                        SetAnimation(ActorAnimState.Running);
                    }
                    else
                        SetAnimation(ActorAnimState.Walking);                    
                    {
                        //set target
                        var d = Target.position;
                        SetDestination(d);
                    }
                    break;
                case ActorAiState.Attacking:
                    break;
                case ActorAiState.Covering:
                    break;
                case ActorAiState.Fleeing:
                    if (RunOnFlee)
                    {
                        IsRunning = true;
                        SetAnimation(ActorAnimState.Running);
                    }
                    else
                        SetAnimation(ActorAnimState.Walking);                    
                    {
                        //set target
                        var d = transform.position + ((Target.position - transform.position).normalized * -1);
                        SetDestination(d);
                    }                    
                    break;
                default:
                    break;
            }

            CurrentAiState = newState;
        }

        private void UpdateState()
        {
            //forced death check
            if(CurrentAiState != ActorAiState.Dead)
            {
                if (Health <= 0)
                {
                    EnterState(ActorAiState.Dead);
                }
            }

            switch (CurrentAiState)
            {
                case ActorAiState.Idle:
                    //TODO aggression
                    break;

                case ActorAiState.Wandering:
                    //TODO aggression
                    if((transform.position - AltTarget).magnitude <= WanderThreshold || TimeInState >= WanderTimeout)
                    {
                        Vector2 newpos = CCBaseUtil.GetRandomVector(InitialPosition.ToFlatVec(), WanderRadius);
                        SetDestination(newpos.ToSpaceVec());
                        TimeInState = 0;
                    }
                    break;
                case ActorAiState.Chasing:
                    //TODO update dest, go to attack
                    {
                        //set target
                        var d = Target.position;
                        SetDestination(d);
                    }
                    break;
                case ActorAiState.Attacking:
                    //TODO actually attack
                    break;
                case ActorAiState.Covering:
                    //TODO 
                    break;
                case ActorAiState.Fleeing:
                    //TODO update dest?
                    {
                        //set target
                        var d = transform.position + ((Target.position - transform.position).normalized * -1);
                        SetDestination(d);
                    }
                    break;

            }
        }

        private void ExitState(ActorAiState oldState)
        {
            if (LockAiState)
                return;

            //TODO we may need this at some point
            switch (oldState)
            {
                case ActorAiState.Idle:
                    break;
                case ActorAiState.Dead:
                    break;
                case ActorAiState.Wandering:
                    AbortNav();
                    break;
                case ActorAiState.Chasing:
                    AbortNav();
                    break;
                case ActorAiState.Attacking:
                    break;
                case ActorAiState.Covering:
                    break;
                case ActorAiState.Fleeing:
                    AbortNav();
                    break;
                default:
                    break;
            }
        }

        private void SetDestination(Vector3 dest)
        {
            AltTarget = dest;
            if (NavEnabled)
            {
                NavComponent.SetDestination(dest);
                NavComponent.enabled = true;
            }
                
        }

        private void AbortNav()
        {
            IsRunning = false;
            if(NavEnabled)
            {
                NavComponent.destination = transform.position;
                NavComponent.enabled = false;
            }
        }

        private void EmulateNav()
        {
            if (NavEnabled)
                return;

            //apply gravity
            CharController.Move(Physics.gravity * Time.deltaTime);

            //get vector to target
            Vector3 pathForward = (AltTarget - transform.position).normalized;
            pathForward.y = 0; //we actually want a flat vector

            //move
            CharController.Move(Time.deltaTime * (IsRunning ? RunSpeed : WalkSpeed) * pathForward);

            //rotate me
            float maxangle = Vector3.SignedAngle(transform.forward, pathForward, Vector3.up);
            float rotangle = Mathf.Min(Time.deltaTime * RotateSpeed, Mathf.Abs(maxangle)) * Mathf.Sign(maxangle);
            transform.Rotate(Vector3.up, rotangle);
            //transform.forward = pathForward; //TODO make this actual motion instead of a snap.
        }

        private void SetInitialNavState()
        {
            InitialPosition = transform.position;

            if (NavComponent != null && !ForceNavmeshOff)
            {
                NavComponent.enabled = false;
                //TODO set nav parameters

                if (NavComponent.isOnNavMesh)
                    NavEnabled = true;
            }                
        }

        private void SetAnimation(ActorAnimState state)
        {
            if (LockAnimState)
                return;

            AnimController.Play(GetNameForAnimation(state));
            //TODO sounds, eventually
        }

        private static string GetNameForAnimation(ActorAnimState state)
        {
            switch (state)
            {
                case ActorAnimState.Idle:
                    return "idle";
                case ActorAnimState.Dead:
                    return "fall_flat_fast";
                case ActorAnimState.Dying:
                    return "fall_flat";
                case ActorAnimState.Walking:
                    return "walk";
                case ActorAnimState.Talking:
                    return "talk";
                case ActorAnimState.Running:
                    return "run";
                case ActorAnimState.Shooting:
                    return "gunplay";
                default:
                    return string.Empty;
            }
        }

        public void OnInteract(ActionInvokerData data)
        {
            if(AltInteraction != ActorInteractionType.None && AlternateCondition.Parse().Evaluate())
            {
                ExecuteInteraction(AltInteraction, AltInteractionTarget, AltInteractionSpecial, data);
            }
            else
            {
                ExecuteInteraction(Interaction, InteractionTarget, InteractionSpecial, data);
            }

        }

        private void ExecuteInteraction(ActorInteractionType type, string target, ActionSpecial special, ActionInvokerData data)
        {
            switch (type)
            {
                case ActorInteractionType.None:
                    throw new InvalidOperationException();
                case ActorInteractionType.Special:
                    special.Execute(data);
                    break;
                case ActorInteractionType.AmbientMonologue:
                    string msg = DialogueModule.GetMonologue(target).GetLineRandom(); //VERY inefficient, will fix later
                    QdmsMessageBus.Instance.PushBroadcast(new HUDPushMessage(msg));//also a very temporary display
                    //and we need to rework Monologue and implement an audio manager before we can do speech
                    break;
                case ActorInteractionType.Dialogue:
                    DialogueInitiator.InitiateDialogue(target, true, null);
                    break;
                case ActorInteractionType.Script:
                    throw new NotImplementedException(); //we will have explicit support, soon
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void TakeDamage(ActorHitInfo data)
        {
            //damage model is very stupid right now, we will make it better later
            float dt = DamageThreshold[(int)data.DType];
            float dr = DamageThreshold[(int)data.DType];
            float damageTaken = CCBaseUtil.CalculateDamage(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == ActorBodyPart.LeftArm || data.HitLocation == ActorBodyPart.LeftLeg || data.HitLocation == ActorBodyPart.RightArm || data.HitLocation == ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            Health -= damageTaken;
        }

    }

}