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
using CommonCore.LockPause;

namespace CommonCore.World
{
    //TODO restorable, animation, and eventually a full refactor
    //I say that now, but I bet this will still be mostly the same until, say, Downwarren
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
        private ActorAiState LastAiState;
        public bool LockAiState = false;
        public ActorAnimState CurrentAnimState = ActorAnimState.Idle;
        public bool LockAnimState = false;
        public Transform Target;
        public Vector3 AltTarget;
        public string SavedTarget = null;
        private float TimeInState;
        private int TotalTickCount;

        [Header("Damage")]
        public float Health = 1.0f;
        private float MaxHealth;
        public bool DieImmediately = false;
        public bool DestroyOnDeath = false;
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Radiation")]
        public float[] DamageResistance = { 0, 0, 0, 0, 0, 0};
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Radiation")]
        public float[] DamageThreshold = { 0, 0, 0, 0, 0, 0 };
        public ActionSpecial OnDeathSpecial;
        public bool FeelPain = true;
        public float PainWaitTime = 1.0f;

        [Header("Aggression")]
        public bool Aggressive = false;
        public bool TargetPlayer = false;
        public bool TargetNpc = false; //TODO factions, but not yet
        public bool Defensive = true;        
        public bool Relentless = false;
        public bool UseLineOfSight = false;
        public float SearchRadius = 25.0f;
        public int SearchInterval = 70;
        public bool DisableInteractionOnHit = true;
        private bool BeenHit = false;

        [Header("Attack")]
        public bool UseMelee = true;
        public float AttackRange = 1.0f;
        public float AttackStateDelay = 1.0f;
        public float AttackInterval = 1.0f;
        public float BulletSpeed = 100;
        public ActorHitInfo AttackHit;
        public GameObject BulletPrefab;
        public GameObject AttackEffectPrefab;
        public Transform ShootPoint;
        public string AttackAnimationOverride;

        private float LastAttackTime = -1;

        [Header("Interaction")]
        public ActorInteractionType Interaction;
        public string InteractionTarget;
        public ActionSpecial InteractionSpecial;

        public EditorConditional AlternateCondition;
        public ActorInteractionType AltInteraction;
        public string AltInteractionTarget;
        public ActionSpecial AltInteractionSpecial;

        public bool InteractionForceDisabled;
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
        public float NavThreshold = 1.0f;
        public float WanderThreshold = 1.0f;
        public float WanderTimeout = 10.0f;
        public Vector2 WanderRadius = new Vector2(10.0f, 10.0f);
        private Vector3 InitialPosition;

        public override void Start() //TODO register into a list for AI and stuff
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

            MaxHealth = Health;

            EnterState(CurrentAiState);
        }

        public override void Update()
        {
            base.Update();

            if (LockPauseModule.IsPaused())
                return;

            TotalTickCount++;

            TimeInState += Time.deltaTime;
            UpdateState();
            EmulateNav();
        }

        //TODO handle aggression
        public void EnterState(ActorAiState newState)
        {
            if (LockAiState)
                return;

            LastAiState = CurrentAiState;

            ExitState(CurrentAiState); //good place or no?

            TimeInState = 0;

            switch (newState)
            {
                case ActorAiState.Idle:
                    SetAnimation(ActorAnimState.Idle);
                    break;
                case ActorAiState.Dead:
                    if (CurrentAiState == ActorAiState.Dead) //fix for glitchy looking behaviour
                        break;

                    AbortNav();
                    if (DieImmediately)
                        SetAnimation(ActorAnimState.Dead);
                    else
                        SetAnimation(ActorAnimState.Dying);

                    if (DestroyOnDeath)
                        this.gameObject.SetActive(false); //actually destroying the object breaks saving

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
                case ActorAiState.ScriptedMoveTo:
                    if (RunOnChase)
                    {
                        IsRunning = true;
                        SetAnimation(ActorAnimState.Running);
                    }
                    else
                        SetAnimation(ActorAnimState.Walking);
                    SetDestination(AltTarget);
                    break;
                case ActorAiState.Attacking:
                    //set animation, fire projectile, set timer
                    SetAnimation(UseMelee ? ActorAnimState.Punching : ActorAnimState.Shooting);
                    transform.forward = CCBaseUtil.GetFlatVectorToTarget(transform.position, Target.position); //ugly but workable
                    DoAttack(); //waaaaay too complicated to cram here
                    LastAttackTime = Time.time;
                    break;
                case ActorAiState.Covering:
                    break;
                case ActorAiState.Hurting:
                    SetAnimation(ActorAnimState.Hurting);
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

            //hack to retrieve swizzled target after a load
            if(!string.IsNullOrEmpty(SavedTarget))
            {
                var goList = WorldUtils.FindAllGameObjects(SavedTarget);
                if(goList.Count == 1)
                {
                    Target = goList[0].transform;
                }
                else if(goList.Count == 0)
                {
                    CDebug.LogEx(string.Format("Couldn't find target '{0}' when restoring {1}", SavedTarget, this.name), LogLevel.Error, this);
                }
                else
                {
                    CDebug.LogEx(string.Format("Found multiple target '{0}' when restoring {1}", SavedTarget, this.name), LogLevel.Error, this);
                }

                SavedTarget = null;
            }

            switch (CurrentAiState)
            {
                case ActorAiState.Idle:
                    if (Aggressive)
                    {
                        //search for targets, select target
                        SelectTarget();
                        if (Target != null)
                            EnterState(ActorAiState.Chasing);
                    }
                    break;
                case ActorAiState.Wandering:
                    //TODO aggression
                    if((transform.position - AltTarget).magnitude <= WanderThreshold || TimeInState >= WanderTimeout)
                    {
                        Vector2 newpos = CCBaseUtil.GetRandomVector(InitialPosition.ToFlatVec(), WanderRadius);
                        SetDestination(newpos.ToSpaceVec());
                        TimeInState = 0;
                    }
                    if(Aggressive)
                    {
                        //search for targets, select target
                        SelectTarget();
                        if (Target != null)
                            EnterState(ActorAiState.Chasing);
                    }
                    break;
                case ActorAiState.Chasing:
                    //TODO actually go to attack
                    if(!WorldUtils.TargetIsAlive(Target))
                    {
                        EnterState(BaseAiState);
                        break;
                    }

                    if((Time.time - LastAttackTime >= AttackInterval) && (Target.position - transform.position).magnitude <= AttackRange)
                    {
                        EnterState(ActorAiState.Attacking);
                        return;
                    }
                    else
                    {
                        //set target
                        var d = Target.position;
                        SetDestination(d);
                    }
                    if(!Relentless)
                    {
                        //break off if we are too far away or too badly hurt
                        if(Health <= (MaxHealth * 0.2f))
                        {
                            EnterState(ActorAiState.Fleeing);                            
                        }
                        else if((Target.position - transform.position).magnitude > SearchRadius)
                        {
                            EnterState(BaseAiState);
                            Target = null;
                        }                        
                    }
                    break;
                case ActorAiState.ScriptedMoveTo:
                    if((AltTarget - transform.position).magnitude <= NavThreshold) //we made it!
                    {
                        EnterState(ActorAiState.Idle); //don't wander off if you were sent there!
                    }
                    break;
                case ActorAiState.Attacking:
                    //wait...
                    if(TimeInState >= AttackStateDelay)
                    {
                        //just return
                        if (!WorldUtils.TargetIsAlive(Target))
                        {
                            EnterState(BaseAiState);
                        }
                        else
                        {
                            EnterState(ActorAiState.Chasing);
                        }
                    }
                    break;
                case ActorAiState.Hurting:
                    if(TimeInState >= PainWaitTime)
                    {
                        if (BeenHit && Target != null)
                            EnterState(ActorAiState.Chasing);
                        else
                            EnterState(LastAiState);
                    }
                    break;
                case ActorAiState.Fleeing:
                    //stop running if far enough away, or target is gone
                    if(!WorldUtils.TargetIsAlive(Target) || (Target.position - transform.position).magnitude > SearchRadius)
                    {
                        EnterState(BaseAiState);
                        Target = null;
                        break;
                    }
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

        private void DoAttack()
        {
            Vector3 shootPos = ShootPoint == null ? (transform.position + (transform.forward * 0.6f) + (transform.up * 1.25f)) : ShootPoint.position;
            Vector3 shootVec = (Target.position - shootPos).normalized; //I screwed this up the first time
            //TODO spread

            var modHit = AttackHit;
            modHit.Originator = this;

            if(UseMelee)
            {
                //melee path (raycast)
                LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");
                var rc = Physics.RaycastAll(shootPos, shootVec, AttackRange, lm, QueryTriggerInteraction.Collide);
                BaseController ac = null;
                foreach (var r in rc)
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
                if (ac != null)
                {
                    if(ac is ActorController)
                        ((ActorController)ac).TakeDamage(modHit);
                    else if (ac is PlayerController)
                        ((PlayerController)ac).TakeDamage(modHit);
                }
                    

            }
            else if(BulletPrefab != null)
            {
                //bullet path (shoot)
                var bullet = Instantiate<GameObject>(BulletPrefab, shootPos + (shootVec * 0.25f), Quaternion.identity, transform.root);
                var bulletRigidbody = bullet.GetComponent<Rigidbody>();
                bulletRigidbody.velocity = (shootVec * BulletSpeed);
                bullet.GetComponent<BulletScript>().HitInfo = modHit;
            }
            else
            {
                CDebug.LogEx(string.Format("{0} tried to shoot a bullet, but has no prefab defined!", name), LogLevel.Error, this);
            }

            //show the effect, if applicable
            if(AttackEffectPrefab != null)
            {
                Instantiate(AttackEffectPrefab, shootPos, Quaternion.identity, (ShootPoint == null ? transform : ShootPoint));
            }
        }

        private void SelectTarget()
        {
            if (TotalTickCount % SearchInterval != 0)
                return;

            //check player first since it's (relatively) cheap
            if(TargetPlayer)
            {
                var playerObj = GameObject.FindGameObjectWithTag("Player");
                if(playerObj != null)
                {
                    if((playerObj.transform.position - transform.position).magnitude <= SearchRadius)
                    {
                        if(UseLineOfSight)
                        {
                            //additional check
                            RaycastHit hitinfo;
                            if(Physics.Raycast(transform.position + new Vector3(0, 1.0f, 0), (playerObj.transform.position - transform.position), out hitinfo))
                            {
                                if (hitinfo.collider.gameObject == playerObj)
                                {
                                    Target = playerObj.transform;
                                    return;
                                }                                    
                            }
                        }
                        else
                        {
                            //otherwise, close enough
                            Target = playerObj.transform;
                            return;
                        }
                    }
                }
            }            

            //stupid and inefficient; we'll fix it later
            //should work well enough as long as n is small and your computer is fast enough
            if(TargetNpc)
            {
                var potentialTargets = transform.root.GetComponentsInChildren<ActorController>();
                foreach (var potentialTarget in potentialTargets)
                {
                    if(WorldUtils.TargetIsAlive(potentialTarget.transform) && (potentialTarget.transform.position - transform.position).magnitude <= SearchRadius)
                    {
                        if (UseLineOfSight)
                        {
                            //additional check
                            RaycastHit hitinfo;
                            if (Physics.Raycast(transform.position + new Vector3(0, 1.0f, 0), (potentialTarget.transform.position - transform.position), out hitinfo))
                            {
                                if (hitinfo.collider.gameObject == potentialTarget.gameObject)
                                {
                                    Target = potentialTarget.transform;
                                    return;
                                }
                            }
                        }
                        else
                        {
                            //otherwise, close enough
                            Target = potentialTarget.transform;
                            return;
                        }
                    }
                }
            }
            
        }

        private void SetDestination(Vector3 dest)
        {
            AltTarget = dest;
            if (NavEnabled)
            {
                NavComponent.SetDestination(dest);
                NavComponent.enabled = true;
                if(IsRunning)
                    NavComponent.speed = RunSpeed;
                else
                    NavComponent.speed = WalkSpeed;
            }
                
        }

        private void AbortNav()
        {
            IsRunning = false;
            AltTarget = transform.position;
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
            Vector3 dirVec = (AltTarget - transform.position);
            Vector3 pathForward = dirVec.normalized;
            pathForward.y = 0; //we actually want a flat vector

            if (dirVec.magnitude <= NavThreshold) //shouldn't hardcode that threshold!
                return;

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
                NavComponent.speed = WalkSpeed;
                NavComponent.angularSpeed = RotateSpeed;

                if (NavComponent.isOnNavMesh)
                    NavEnabled = true;
            }                
        }

        public void SetAnimation(ActorAnimState state)
        {
            if (LockAnimState)
                return;

            if(AnimController != null)
            {
                string stateName = GetNameForAnimation(state);

                if (string.IsNullOrEmpty(AttackAnimationOverride) && (state == ActorAnimState.Punching || state == ActorAnimState.Shooting))
                    stateName = AttackAnimationOverride;

                AnimController.Play(stateName);

                //TODO sounds, eventually
            }

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
                case ActorAnimState.Hurting:
                    return "pain";
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
            if (InteractionForceDisabled)
                return;

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
                    break; //do nothing
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
            float dr = DamageResistance[(int)data.DType];
            float damageTaken = CCBaseUtil.CalculateDamage(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == ActorBodyPart.LeftArm || data.HitLocation == ActorBodyPart.LeftLeg || data.HitLocation == ActorBodyPart.RightArm || data.HitLocation == ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            Health -= damageTaken;

            if (CurrentAiState == ActorAiState.Dead) //abort if we're already dead
                return;

            if(Defensive && data.Originator != null)
            {
                Target = data.Originator.transform;
                BeenHit = true;

                if (DisableInteractionOnHit)
                    InteractionForceDisabled = true;

                if (FeelPain)
                    EnterState(ActorAiState.Hurting);
                else
                    EnterState(ActorAiState.Chasing);
            }
            else if(FeelPain)
                EnterState(ActorAiState.Hurting);
        }

        //these are both done stupidly and could probably be done through reflection instead but for now...

        public override Dictionary<string, object> GetExtraData()
        {
            var extraData = base.GetExtraData(); //not sure if we should keep this here...

            var actorData = new ActorExtraData();

            //save!
            actorData.CurrentAiState = CurrentAiState;
            actorData.LastAiState = LastAiState;
            actorData.LockAiState = LockAiState;
            actorData.CurrentAnimState = CurrentAnimState;
            actorData.LockAnimState = LockAnimState;
            actorData.SavedTarget = Target.name;
            actorData.AltTarget = AltTarget;
            actorData.TimeInState = TimeInState;

            actorData.Health = Health;
            actorData.BeenHit = BeenHit;

            actorData.IsRunning = IsRunning;

            actorData.InteractionForceDisabled = InteractionForceDisabled;

            extraData["Actor"] = actorData;

            return extraData;
        }

        public override void SetExtraData(Dictionary<string, object> data)
        {
            base.SetExtraData(data);

            if(data.ContainsKey("Actor"))
            {
                ActorExtraData actorData = data["Actor"] as ActorExtraData;
                if(actorData != null)
                {
                    //restore!

                    CurrentAiState = actorData.CurrentAiState;
                    LastAiState = actorData.LastAiState;
                    LockAiState = actorData.LockAiState;
                    CurrentAnimState = actorData.CurrentAnimState;
                    LockAnimState = actorData.LockAnimState;
                    SavedTarget = actorData.SavedTarget;
                    AltTarget = actorData.AltTarget;
                    TimeInState = actorData.TimeInState;

                    Health = actorData.Health;
                    BeenHit = actorData.BeenHit;

                    IsRunning = actorData.IsRunning;

                    InteractionForceDisabled = actorData.InteractionForceDisabled;

                }
                else
                {
                    CDebug.LogEx(string.Format("Invalid actor data for {0} found on restore!", this.name), LogLevel.Error, this);
                }
            }
            else
            {
                CDebug.LogEx(string.Format("No actor data for {0} found on restore!", this.name), LogLevel.Error, this);
            }
        }

    }

}