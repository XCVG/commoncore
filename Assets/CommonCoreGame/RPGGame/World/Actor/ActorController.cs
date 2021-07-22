using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.LockPause;
using CommonCore.Messaging;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    //TODO restorable, animation, and eventually a full refactor
    //I say that now, but I bet this will still be mostly the same until, say, Downwarren
    public class ActorController : BaseController, ITakeDamage, IAmTargetable
    {
        //public string CharacterModelIdOverride; //does nothing lol

        [Header("Components")]
        public CharacterController CharController; //optional, only used for targeting

        public ActorInteractableComponent InteractComponent;
        public Transform TargetPoint;
        public ActorAnimationComponentBase AnimationComponent;
        public ActorMovementComponentBase MovementComponent;
        public ActorAttackComponentBase AttackComponent;
        public ActorInteractionComponent InteractionComponent;
        public ActorAudioComponent AudioComponent;

        //basically each of these headers can be moved into a separate component but we might not get to all of it yet

        [Header("State")]
        public ActorAiState BaseAiState = ActorAiState.Idle;
        public ActorAiState CurrentAiState = ActorAiState.Idle;
        private ActorAiState LastAiState;
        public bool LockAiState = false;

        public Transform Target;
        //public Vector3 AltTarget;
        public string SavedTarget = null; //I think this is a hack that didn't work out
        public float TimeInState { get; private set; }
        public int TotalTickCount { get; private set; }

        [Header("Damage")]
        public float Health = 1.0f;
        public float MaxHealth { get; private set; }
        public bool Invincible = false;
        public bool DieImmediately = false;
        public bool DestroyOnDeath = false;
        public bool DisableCollidersOnDeath = false;
        public bool DisableHitboxesOnDeath = false;
        public DamageResistanceNode[] DamageResistances = null;
        public ActionSpecial OnDeathSpecial;
        public string DefaultHitPuff = "DefaultHitPuff";
        [Tooltip("If positive, intepret is multiplier of maxhealth. If negative, interpret as absolute value")]
        public float ExtremeDeathThreshold = 1;

        [Header("Pain")]
        public bool FeelPain = true;
        [Tooltip("Chance to enter pain state at zero damage")]
        public float PainChance = 0.5f;
        public float PainWaitTime = 1.0f;
        public float PainMaxChance = 0.9f; //prevents stunlocking
        [Tooltip("Amount of damage at or above where pain will always be MaxChance")]
        public float PainGuaranteeThreshold = 0;
        [Tooltip("If set, PainGuaranteeThreshold will be treated as a multiplier on MaxHealth")]
        public bool PainGuaranteeRelative = false;
        public bool PainStateAllowRestart = false;

        [Header("Aggression")]
        public bool Aggressive = false;
        public string Faction = "None";
        public float Detectability = 1.0f;
        public bool IsTarget = true;
        public bool Defensive = true;
        public bool Infighting = false;
        [Tooltip("If set, will continue to chase target until target is dead")]
        public bool Relentless = false;
        [Tooltip("Fractional")]
        public float FleeHealthThreshold = 0.2f;
        public bool UseLineOfSight = false;
        public float SearchRadius = 25.0f;
        public int SearchInterval = 70;
        [Tooltip("If this is larger than max attack range, the actor will not be able to attack!")]
        public float ChaseOptimalDistance = 0;
        public bool DisableInteractionOnHit = true;


        [Header("Interaction")] //TODO move this out into ActorInteractionComponent
        public int GrantXpOnDeath;

        [Header("Movement")]
        public bool RunOnChase = true;
        public bool RunOnFlee = true;

        [field: SerializeField, Tooltip("For debugging only- changing this may result in unpredicted results")]
        public Vector3 InitialPosition { get; private set; } //TODO should we save this? I don't think so

        [Header("Wander")]
        public float WanderThreshold = 1.0f;
        public float WanderTimeout = 10.0f;
        public Vector2 WanderRadius = new Vector2(10.0f, 10.0f);

        [Header("Misc")]
        public ActorDifficultyHandling DifficultyHandling = ActorDifficultyHandling.AsActor; //default for historical reasons

        private QdmsMessageInterface MessageInterface;

        //handlers
        public Func<ActorHitInfo, ActorDamageHandlerResult> DamageHandler = null;
        public Func<Transform> TargetPicker = null;

        public bool BeenHit { get; protected set; } = false;
        public ActorHitInfo? LastHit { get; protected set; } = null;
        public float LastHitDamage { get; protected set; } = 0;
        public bool WasExtremeDeath { get; protected set; } = false;

        float ITakeDamage.Health => Health;

        public override HashSet<string> Tags
        {
            get
            {
                if (_Tags == null)
                {
                    _Tags = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                    _Tags.Add("Actor");
                    if (EntityTags != null && EntityTags.Length > 0)
                        Tags.UnionWith(EntityTags);
                }

                return _Tags;
            }
        }

        bool IAmTargetable.ValidTarget => IsTarget && !(CurrentAiState == ActorAiState.Dead) && isActiveAndEnabled;

        string IAmTargetable.Faction => Faction;

        float IAmTargetable.Detectability => Detectability;

        Vector3 IAmTargetable.TargetPoint => TargetPoint.Ref()?.position ?? (CharController == null ? null : (Vector3?)transform.TransformPoint(CharController.center)) ?? transform.position;

        protected override bool DeferComponentInitToSubclass => true;

        public override void Awake()
        {
            base.Awake();

            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessage);            
        }

        public override void Start() //TODO register into a list for AI and stuff
        {
            base.Start();

            //TODO may remove some warnings, TODO change to Debug.Log

            if (AnimationComponent == null)
                AnimationComponent = GetComponent<ActorAnimationComponentBase>();
            if (AnimationComponent == null)
                CDebug.LogEx(name + " couldn't find AnimationComponent", LogLevel.Warning, this);

            if (MovementComponent == null)
                MovementComponent = GetComponent<ActorMovementComponentBase>();
            if (MovementComponent == null)
                CDebug.LogEx(name + " couldn't find MovementComponent", LogLevel.Error, this);

            if (AttackComponent == null)
                AttackComponent = GetComponent<ActorAttackComponentBase>();
            if (AttackComponent == null)
                CDebug.LogEx(name + " couldn't find AttackComponent", LogLevel.Warning, this);

            if (InteractionComponent == null)
                InteractionComponent = GetComponent<ActorInteractionComponent>();
            if (InteractionComponent == null)
                CDebug.LogEx(name + " couldn't find InteractionComponent", LogLevel.Warning, this);

            if (AudioComponent == null)
                AudioComponent = GetComponentInChildren<ActorAudioComponent>();
            if (AudioComponent == null)
                CDebug.LogEx(name + " couldn't find AudioComponent", LogLevel.Verbose, this);

            InitialPosition = transform.position;

            MovementComponent.Init();

            if (CharController == null) //optional, only used for targeting
                CharController = GetComponent<CharacterController>();

            if (InteractComponent == null)
                InteractComponent = GetComponent<ActorInteractableComponent>();

            if (InteractComponent == null)
                InteractComponent = GetComponentInChildren<ActorInteractableComponent>();

            InteractionComponent.Ref()?.Init();

            if (InteractComponent != null && InteractionComponent != null)
            {
                InteractComponent.ControllerOnInteractDelegate = InteractionComponent.OnInteract;
                InteractComponent.Tooltip = InteractionComponent.Tooltip;
                //TODO may move this to ActorInteractionComponent
            }
            else
            {
                CDebug.LogEx(name + " couldn't find ActorInteractableComponent", LogLevel.Error, this);
            }

            MaxHealth = Health;

            AnimationComponent.Init();

            AttackComponent.Ref()?.Init();

            TryExecuteOnComponents(component => component.Init(this));

            Initialized = true;

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
            
        }

        private void HandleMessage(QdmsMessage message)
        {
            if(message is ConfigChangedMessage)
            {
                MovementComponent.HandleDifficultyChanged();  
            }
            else if(message is ClearAllTargetsMessage)
            {
                Target = null;
            }
        }

        public void ForceEnterState(ActorAiState newState)
        {
            //disgusting hack, need to rework how this is handled
            bool oldLockAiState = LockAiState;
            LockAiState = false;
            EnterState(newState);
            LockAiState = oldLockAiState;
        }

        public void EnterState(ActorAiState newState)
        {
            if (LockAiState)
                return;

            if(newState != CurrentAiState)
                LastAiState = CurrentAiState;

            ExitState(CurrentAiState); //good place or no?

            TimeInState = 0;

            switch (newState)
            {
                case ActorAiState.Idle:
                    Target = null;
                    AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Idle);
                    MovementComponent.AbortMove();
                    break;
                case ActorAiState.Dead:
                    {
                        if (CurrentAiState == ActorAiState.Dead) //fix for glitchy looking behaviour
                            break;

                        MovementComponent.AbortMove();
                        MovementComponent.HandleDeath();
                        var deathStateArgs = new DeathStateActorAnimationArgs() { DamageEffector = LastHit?.DamageEffector ?? 0, DamageType = LastHit?.DamageType ?? 0, ExtremeDeath = WasExtremeDeath, HitLocation = LastHit?.HitLocation ?? 0, HitMaterial = LastHit?.HitMaterial ?? 0 };
                        if (DieImmediately)
                            AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Dead, deathStateArgs);
                        else
                            AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Dying, deathStateArgs);

                        AudioComponent.Ref()?.StopLivingSounds();
                        if(WasExtremeDeath)
                            AudioComponent.Ref()?.PlayExtremeDeathSound();
                        else
                            AudioComponent.Ref()?.PlayDeathSound();

                        if (InteractionComponent != null)
                            InteractionComponent.InteractionDisabledByHit = false;

                        if (DestroyOnDeath)
                            this.gameObject.SetActive(false); //actually destroying the object breaks saving

                        if (OnDeathSpecial != null)
                            OnDeathSpecial.Execute(new ActionInvokerData { Activator = LastHit?.Originator, Caller = this, Position = transform.position, Rotation = transform.rotation, Velocity = MovementComponent.Ref()?.PhysicsVelocity });

                        if (DisableHitboxesOnDeath)
                        {
                            var hitboxComponents = GetComponentsInChildren<IHitboxComponent>(true);
                            foreach (var hitboxComponent in hitboxComponents)
                                if (hitboxComponent is MonoBehaviour mb) //IHitboxComponent does not actually imply MonoBehaviour
                                    mb.gameObject.SetActive(false);
                        }

                        if (DisableCollidersOnDeath)
                        {
                            var colliders = GetComponentsInChildren<Collider>(true);
                            foreach (var collider in colliders)
                                collider.enabled = false;
                        }

                        if (
                            (   (LastHit != null && LastHit.Value.Originator != null && LastHit.Value.Originator is PlayerController)
                                || (Target != null && Target.GetComponent<PlayerController>())
                            )
                            && GrantXpOnDeath > 0
                           )
                            GameState.Instance.PlayerRpgState.GrantXPScaled(GrantXpOnDeath);
                    }
                    break;
                case ActorAiState.Wandering:
                    Target = null;
                    AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Walking);
                    //set initial destination
                    Vector2 newpos = VectorUtils.GetRandomVector2(InitialPosition.GetFlatVector(), WanderRadius);
                    MovementComponent.SetDestination(newpos.GetSpaceVector());
                    AudioComponent.Ref()?.StartWalkSound();
                    break;
                case ActorAiState.Chasing:
                    if (RunOnChase)
                    {
                        MovementComponent.IsRunning = true;
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Running);
                        AudioComponent.Ref()?.StartRunSound();
                    }
                    else
                    {
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Walking);
                        AudioComponent.Ref()?.StartWalkSound();
                    }

                    {
                        //set target

                        if (Target == null)
                            GetSwizzledTarget(); //fix for loading saves

                        SetChaseDestination(true);
                    }
                    break;
                case ActorAiState.ScriptedMoveTo:
                    if (RunOnChase)
                    {
                        MovementComponent.IsRunning = true;
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Running);
                        AudioComponent.Ref()?.StartRunSound();
                    }
                    else
                    {
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Walking);
                        AudioComponent.Ref()?.StartWalkSound();
                    }
                    MovementComponent.SetDestination(MovementComponent.MovementTarget);
                    break;
                case ActorAiState.Attacking:
                    if(AttackComponent == null)
                    {
                        Debug.LogError($"{name} tried to attack, but has no attack component!");
                        EnterState(ActorAiState.Idle);
                        return;
                    }

                    if (Target == null)
                        GetSwizzledTarget(); //fix for loading saves

                    //set animation, fire projectile, set timer
                    AttackComponent.BeginAttack();                 
                    break;
                case ActorAiState.Covering:
                    break;
                case ActorAiState.Hurting:
                    AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Hurting);
                    AudioComponent.Ref()?.PlayPainSound();
                    break;
                case ActorAiState.Fleeing:
                    if (RunOnFlee)
                    {
                        MovementComponent.IsRunning = true;
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Running);
                        AudioComponent.Ref()?.StartRunSound();
                    }
                    else
                    {
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Walking);
                        AudioComponent.Ref()?.StartWalkSound();
                    }

                    {
                        //set target
                        var d = transform.position + ((Target.position - transform.position).normalized * -(1 + Mathf.Abs(MovementComponent.TargetThreshold)));
                        MovementComponent.SetDestination(d);
                    }                    
                    break;
                case ActorAiState.ScriptedAction:
                    //nop
                    break;
                default:
                    break;
            }

            CurrentAiState = newState;
        }

        private void UpdateState()
        {
            //forced death check
            if (CurrentAiState != ActorAiState.Dead)
            {
                if (Health <= 0)
                {
                    EnterState(ActorAiState.Dead);
                }
            }

            //hack to retrieve swizzled target after a load
            GetSwizzledTarget();

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
                    if (MovementComponent.DistToTarget <= WanderThreshold || TimeInState >= WanderTimeout)
                    {
                        Vector2 newpos = VectorUtils.GetRandomVector2(InitialPosition.GetFlatVector(), WanderRadius);
                        MovementComponent.SetDestination(newpos.GetSpaceVector());
                        TimeInState = 0;
                    }
                    if (Aggressive)
                    {
                        //search for targets, select target
                        SelectTarget();
                        if (Target != null)
                        {
                            EnterState(ActorAiState.Chasing);
                            AudioComponent.Ref()?.PlayAlertSound();
                        }
                    }
                    break;
                case ActorAiState.Chasing:
                    if (!RpgWorldUtils.TargetIsAlive(Target))
                    {
                        EnterState(BaseAiState);
                        break;
                    }

                    if ((MetaState.Instance.SessionFlags.Contains("NoTarget") || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoTarget)) && Target.GetComponent<PlayerController>())
                    {
                        EnterState(BaseAiState);
                        break;
                    }

                    if (AttackComponent != null && AttackComponent.ReadyToAttack)
                    {
                        EnterState(ActorAiState.Attacking);
                        return;
                    }
                    else
                    {
                        //set target
                        SetChaseDestination(false);
                    }

                    if (!Relentless)
                    {
                        //break off if we are too far away or too badly hurt
                        if (FleeHealthThreshold > 0 && Health <= (MaxHealth * FleeHealthThreshold))
                        {
                            EnterState(ActorAiState.Fleeing);
                        }
                        else if ((Target.position - transform.position).magnitude > SearchRadius)
                        {
                            EnterState(BaseAiState);
                            Target = null;
                        }
                    }
                    break;
                case ActorAiState.ScriptedMoveTo:
                    if (MovementComponent.AtTarget) //we made it!
                    {
                        EnterState(ActorAiState.Idle); //don't wander off if you were sent there!
                    }
                    break;
                case ActorAiState.Attacking:
                    //wait...
                    AttackComponent.UpdateAttack();
                    if (AttackComponent.AttackIsDone)
                    {
                        EnterState(AttackComponent.PostAttackState);
                    }
                    break;
                case ActorAiState.Hurting:
                    if (TimeInState >= PainWaitTime)
                    {
                        if (BeenHit && Target != null)
                            EnterState(ActorAiState.Chasing);
                        else
                            EnterState(LastAiState);
                    }
                    break;
                case ActorAiState.Fleeing:
                    //stop running if far enough away, or target is gone
                    if (!RpgWorldUtils.TargetIsAlive(Target) || (Target.position - transform.position).magnitude > SearchRadius)
                    {
                        EnterState(BaseAiState);
                        Target = null;
                        break;
                    }
                    {
                        //set target
                        var d = transform.position + ((Target.position - transform.position).normalized * -(1 + Mathf.Abs(MovementComponent.TargetThreshold)));
                        MovementComponent.SetDestination(d);
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
                    MovementComponent.AbortMove();
                    AudioComponent.Ref()?.StopMoveSound();
                    break;
                case ActorAiState.Chasing:
                    MovementComponent.AbortMove();
                    AudioComponent.Ref()?.StopMoveSound();
                    break;
                case ActorAiState.Attacking:
                    AttackComponent.Ref()?.EndAttack();
                    break;
                case ActorAiState.Covering:
                    break;
                case ActorAiState.Fleeing:
                    MovementComponent.AbortMove();
                    AudioComponent.Ref()?.StopMoveSound();
                    break;
                default:
                    break;
            }
        }

        private void GetSwizzledTarget()
        {
            if (!string.IsNullOrEmpty(SavedTarget))
            {
                var goList = SceneUtils.FindAllGameObjects(SavedTarget);
                if (goList.Count == 1)
                {
                    Target = goList[0].transform;
                }
                else if (goList.Count == 0)
                {
                    CDebug.LogEx(string.Format("Couldn't find target '{0}' when restoring {1}", SavedTarget, this.name), LogLevel.Error, this);
                }
                else
                {
                    CDebug.LogEx(string.Format("Found multiple target '{0}' when restoring {1}", SavedTarget, this.name), LogLevel.Error, this);
                }

                SavedTarget = null;
            }
        }

        private void SetChaseDestination(bool initial)
        {
            if(AttackComponent != null && AttackComponent.HandlesChaseDestination)
            {
                MovementComponent.SetDestination(AttackComponent.GetChaseDestination(initial));
                return;
            }

            if(Target == null)
            {
                Debug.LogWarning($"[ActorController] {gameObject.name} can't set a chase destination because target is null!");
                return;
            }

            if (ChaseOptimalDistance > 0)
            {
                Vector2 vecToTarget = (Target.position - transform.position).GetFlatVector();
                float distToTarget = vecToTarget.magnitude;
                float distToDestination = Mathf.Min(distToTarget, ChaseOptimalDistance);
                Vector2 dirToTarget = vecToTarget.normalized;
                Vector2 destination2d = transform.position.GetFlatVector() + dirToTarget * distToDestination;
                Vector3 destination = new Vector3(destination2d.x, transform.position.y, destination2d.y);
                MovementComponent.SetDestination(destination);
            }
            else
            {
                var d = Target.position;
                MovementComponent.SetDestination(d);
            }
        }

        private void SelectTarget()
        {
            var gameplayConfig = ConfigState.Instance.GetGameplayConfig();

            if (TotalTickCount % SearchInterval * (1f / EffectiveAggression) != 0)
                return;

            if(TargetPicker != null)
            {
                Target = TargetPicker();
                return;
            }

            if(AttackComponent != null && AttackComponent.HandlesSelectTarget)
            {
                Target = AttackComponent.SelectTarget();
                return;
            }

            var detectionDifficultyFactor = 1f / EffectivePerception;

            //check player first since it's (relatively) cheap
            if (GameState.Instance.FactionState.GetRelation(Faction, "Player") == FactionRelationStatus.Hostile && !MetaState.Instance.SessionFlags.Contains("NoTarget") && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoTarget))
            {
                var playerObj = WorldUtils.GetPlayerObject();
                if(playerObj != null && RpgWorldUtils.TargetIsAlive(playerObj.transform))
                {
                    PlayerController pc = playerObj.GetComponent<PlayerController>();

                    if((playerObj.transform.position - transform.position).magnitude <= SearchRadius
                        && UnityEngine.Random.Range(0f, 1f * detectionDifficultyFactor) <= ((IAmTargetable)pc).Detectability)
                    {
                        if(UseLineOfSight)
                        {
                            if(CheckLineOfSight(pc))
                            {
                                Target = playerObj.transform;
                                return;
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

            //if(TargetNpc)
            {
                //var sw = System.Diagnostics.Stopwatch.StartNew();

                //new code should be faster if n is large but it may not bear out in practice, and it probably allocs more dedotated wam
                
                var colliders = Physics.OverlapSphere(transform.position, SearchRadius, WorldUtils.GetAttackLayerMask());
                HashSet<IAmTargetable> potentialTargets = new HashSet<IAmTargetable>();

                foreach(var collider in colliders)
                {
                    var baseController = collider.GetComponent<BaseController>();
                    if(baseController != null)
                    {
                        if(baseController is IAmTargetable iat && iat.ValidTarget)
                            potentialTargets.Add(iat);
                        continue; //continue anyway since we've found a base controller and it's either targetable or it's not
                    }

                    var hitboxComponent = collider.GetComponent<IHitboxComponent>();
                    if (hitboxComponent != null && hitboxComponent.ParentController is IAmTargetable iat2 && iat2.ValidTarget)
                    {
                        potentialTargets.Add(iat2);
                        continue;
                    }
                }
                

                //old stupid code: should work well enough as long as n is small and your computer is fast enough
                //IEnumerable<ActorController> potentialTargets = transform.root.GetComponentsInChildren<ActorController>();

                foreach (var potentialTarget in potentialTargets)
                {
                    BaseController targetController = potentialTarget as BaseController;
                    if (targetController == null)
                        continue;

                    if(RpgWorldUtils.TargetIsAlive(targetController.transform) 
                        && (targetController.transform.position - transform.position).magnitude <= SearchRadius
                        && GameState.Instance.FactionState.GetRelation(Faction, potentialTarget.Faction) == FactionRelationStatus.Hostile
                        && !(potentialTarget == this))
                    {                       
                        //roll some dice
                        if (potentialTarget.Detectability < 1 && UnityEngine.Random.Range(0f, 1f * detectionDifficultyFactor) > potentialTarget.Detectability) //probably correct
                            continue;

                        if (UseLineOfSight)
                        {
                            if(CheckLineOfSight(targetController))
                            {
                                Target = targetController.transform;
                                break;
                            }
                        }
                        else
                        {
                            //otherwise, close enough
                            Target = targetController.transform;
                            break;
                        }
                    }
                }

                //sw.Stop();
                //Debug.Log($"Target lookup: {sw.Elapsed.TotalMilliseconds:F4} ms");
            }

            if (!RpgWorldUtils.TargetIsAlive(Target))
                Target = null;
            
        }

        private bool CheckLineOfSight(BaseController targetController)
        {
            //this will kinda work
            RaycastHit hitinfo;
            if (Physics.Raycast(transform.position + new Vector3(0, 1.0f, 0), (targetController.transform.position - transform.position), out hitinfo, SearchRadius, WorldUtils.GetAttackLayerMask(), QueryTriggerInteraction.Ignore))
            {
                //Debug.Log($"LoS raycast hit {hitinfo.collider.gameObject.name}");

                if (hitinfo.collider.gameObject == targetController.gameObject || hitinfo.collider.GetComponentInParent<BaseController>() == targetController)
                {
                    return true;
                }
            }

            return false;
        }

        public void TakeDamage(ActorHitInfo data)
        {
            LastHit = null;
            LastHitDamage = 0;

            if (!data.HarmFriendly)
            {
                string hitFaction = data.OriginatorFaction;
                if(!string.IsNullOrEmpty(hitFaction))
                {
                    FactionRelationStatus relation = GameState.Instance.FactionState.GetRelation(hitFaction, Faction); //this looks backwards but it's because we're checking if the Bullet is-friendly-to the Actor
                    if (relation == FactionRelationStatus.Friendly)
                        return; //no friendly fire
                }
            }

            ActorDamageHandlerResult? damageHandlerResult = null;
            if (DamageHandler != null)
            {
                damageHandlerResult = DamageHandler(data);
                if (damageHandlerResult.Value.HitInfo.HasValue)
                    data = damageHandlerResult.Value.HitInfo.Value;
                else
                    return;
            }

            LastHit = data;

            float damageTaken;
            if(damageHandlerResult?.DamageTaken != null)
            {
                damageTaken = damageHandlerResult.Value.DamageTaken.Value;
            }
            else
            {
                //new way of doing dr/dt
                float dt = 0, dr = 0;
                foreach (var dNode in DamageResistances)
                {
                    if ((int)dNode.DamageType == data.DamageType)
                    {
                        dt = dNode.DamageThreshold;
                        dr = dNode.DamageResistance;
                    }
                }

                damageTaken = RpgValues.DamageTaken(data, dt, dr);

                if (!data.HitFlags.HasFlag(BuiltinHitFlags.IgnoreHitLocation))
                {
                    if (data.HitLocation == (int)ActorBodyPart.Head)
                        damageTaken *= 2.0f; //do we want more flexibility here?
                    else if (data.HitLocation == (int)ActorBodyPart.LeftArm || data.HitLocation == (int)ActorBodyPart.LeftLeg || data.HitLocation == (int)ActorBodyPart.RightArm || data.HitLocation == (int)ActorBodyPart.RightLeg)
                        damageTaken *= 0.75f;
                }

                damageTaken *= (1f / EffectiveEndurance);
            }

            if (!Invincible)
            {
                LastHitDamage = damageTaken;
                Health -= damageTaken;
            }

            //handle extreme death
            if (damageHandlerResult?.ExtremeDeath != null)
            {
                WasExtremeDeath = damageHandlerResult.Value.ExtremeDeath.Value;
            }
            else
            {
                if (data.HitFlags.HasFlag(BuiltinHitFlags.AlwaysExtremeDeath))
                {
                    WasExtremeDeath = true;
                }
                else if (data.HitFlags.HasFlag(BuiltinHitFlags.NeverExtremeDeath))
                {
                    WasExtremeDeath = false;
                }
                else
                {
                    if(ExtremeDeathThreshold > 0)
                    {
                        //interpret as -(maxhealth * threshold)
                        WasExtremeDeath = Health < (-(MaxHealth * ExtremeDeathThreshold));
                    }
                    else if(ExtremeDeathThreshold < 0)
                    {
                        //interpret as absolute value
                        WasExtremeDeath = Health < ExtremeDeathThreshold;
                    }
                    else
                    {
                        //ExtremeDeathThreshold == 0, no extreme death
                        WasExtremeDeath = false;
                    }
                }
            }

            //TODO do we force into death state here?

            //TODO consider moving this, but probably wait until we start thinking about abuse of corpses
            if (CurrentAiState == ActorAiState.Dead) //abort if we're already dead
                return;

            bool didTakePain;

            if (damageHandlerResult?.TookPain != null)
            {
                didTakePain = damageHandlerResult.Value.TookPain.Value;
            }
            else
            {
                float derivedPainChance = PainChance;
                if (PainGuaranteeThreshold != 0)
                {
                    float damageForMaxPain = Mathf.Abs(PainGuaranteeRelative ? (PainGuaranteeThreshold * MaxHealth) : PainGuaranteeThreshold);
                    derivedPainChance = MathUtils.ScaleRange(damageTaken, 0, damageForMaxPain, PainChance, 1);
                }
                derivedPainChance = Mathf.Min(derivedPainChance, PainMaxChance);

                didTakePain = (data.HitFlags.HasFlag(BuiltinHitFlags.AlwaysPain) || UnityEngine.Random.Range(0f, 1f) < derivedPainChance) && !data.HitFlags.HasFlag(BuiltinHitFlags.NoPain); //TODO shouldn't this be weighted by damage?
            }

            if (Defensive && data.Originator != null && data.Originator != this && !data.HitFlags.HasFlag(BuiltinHitFlags.NeverAlert))
            {
                FactionRelationStatus relation = FactionRelationStatus.Neutral;
                if (data.Originator is PlayerController)
                {
                    relation = GameState.Instance.FactionState.GetRelation(Faction, "Player");
                }
                else if (data.Originator is ActorController)
                {
                    relation = GameState.Instance.FactionState.GetRelation(Faction, ((ActorController)data.Originator).Faction);
                }

                if (relation != FactionRelationStatus.Friendly || Infighting)
                {
                    Target = data.Originator.transform;
                    BeenHit = true;

                    if (DisableInteractionOnHit && InteractionComponent != null)
                        InteractionComponent.InteractionDisabledByHit = true;

                    if (FeelPain && didTakePain && CurrentAiState != ActorAiState.ScriptedAction && CurrentAiState != ActorAiState.ScriptedMoveTo)
                    {
                        if (PainStateAllowRestart || CurrentAiState != ActorAiState.Hurting)
                            EnterState(ActorAiState.Hurting);
                    }
                    else if(CurrentAiState != ActorAiState.Chasing && CurrentAiState != ActorAiState.Attacking && CurrentAiState != ActorAiState.ScriptedAction && CurrentAiState != ActorAiState.ScriptedMoveTo)
                        EnterState(ActorAiState.Chasing);
                }
                else if ((PainStateAllowRestart || CurrentAiState != ActorAiState.Hurting) && FeelPain && CurrentAiState != ActorAiState.ScriptedAction && CurrentAiState != ActorAiState.ScriptedMoveTo)
                    EnterState(ActorAiState.Hurting);

            }
            else if (FeelPain && didTakePain && (PainStateAllowRestart || CurrentAiState != ActorAiState.Hurting) && CurrentAiState != ActorAiState.ScriptedAction && CurrentAiState != ActorAiState.ScriptedMoveTo)
                EnterState(ActorAiState.Hurting);
        }

        public void SetInitialPosition(Vector3 newInitialPosition)
        {
            InitialPosition = newInitialPosition;
        }

        public void SetInitialPosition()
        {
            InitialPosition = transform.position;
        }

        public void Kill()
        {
            Health = 0;
        }

        public void Raise()
        {
            //more logic because we don't execute resurrect on entering idle state

            gameObject.SetActive(true);
            Health = MaxHealth;
            MovementComponent.HandleRaise();

            if (DisableHitboxesOnDeath)
            {
                var hitboxComponents = GetComponentsInChildren<IHitboxComponent>(true);
                foreach (var hitboxComponent in hitboxComponents)
                    if (hitboxComponent is MonoBehaviour mb) //IHitboxComponent does not actually imply MonoBehaviour according to C#
                        mb.gameObject.SetActive(true);
            }

            if (DisableCollidersOnDeath)
            {
                var colliders = GetComponentsInChildren<Collider>(true);
                foreach (var collider in colliders)
                    collider.enabled = true;
            }

            EnterState(BaseAiState);
        }

        public float EffectiveAggression
        {
            get
            {
                switch (DifficultyHandling)
                {
                    case ActorDifficultyHandling.AsActor:
                    case ActorDifficultyHandling.AsFollower:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorPerception;
                    default:
                        return 1f;
                }
            }
        }

        //if we need to we can speed these getters up with caching

        public float EffectiveStrength
        {
            get
            {
                switch (DifficultyHandling)
                {
                    case ActorDifficultyHandling.AsActor:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorStrength;
                    case ActorDifficultyHandling.AsFollower:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorStrength * ConfigState.Instance.GetGameplayConfig().Difficulty.FollowerStrength;
                    case ActorDifficultyHandling.AsFollowerOnly:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.FollowerStrength;
                    default:
                        return 1f;
                }
            }
        }

        public float EffectiveEndurance
        {
            get
            {
                switch (DifficultyHandling)
                {
                    case ActorDifficultyHandling.AsActor:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorStrength;
                    case ActorDifficultyHandling.AsFollower:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorStrength * ConfigState.Instance.GetGameplayConfig().Difficulty.FollowerEndurance;
                    case ActorDifficultyHandling.AsFollowerOnly:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.FollowerEndurance;
                    default:
                        return 1f;
                }
            }
        }

        public float EffectivePerception
        {
            get
            {
                switch (DifficultyHandling)
                {
                    case ActorDifficultyHandling.AsActor:                        
                    case ActorDifficultyHandling.AsFollower:
                        return ConfigState.Instance.GetGameplayConfig().Difficulty.ActorPerception;
                    default:
                        return 1f;
                }
            }
        }

        //these are both done stupidly and could probably be done through reflection instead but for now...

        public override Dictionary<string, object> CommitEntityData()
        {
            Dictionary<string, object> extraData = base.CommitEntityData();

            var actorData = new ActorExtraData();

            //save!
            actorData.CurrentAiState = CurrentAiState;
            actorData.LastAiState = LastAiState;
            actorData.LockAiState = LockAiState;
            actorData.CurrentAnimState = AnimationComponent.Ref()?.CurrentAnimState ?? default;
            actorData.LockAnimState = AnimationComponent.Ref()?.LockAnimState ?? default;
            actorData.SavedTarget = Target != null ? Target.name : string.Empty; //damn it!
            actorData.AltTarget = MovementComponent.MovementTarget;
            actorData.TimeInState = TimeInState;

            actorData.Health = Health;
            actorData.BeenHit = BeenHit;

            if (LastHit.HasValue)
            {
                var modLastHit = LastHit.Value;
                modLastHit.Originator = null; //hack because this breaks saves and we don't need it for death handling anyway
                actorData.LastHit = modLastHit;
            }
            actorData.LastHitDamage = LastHitDamage;
            actorData.WasExtremeDeath = WasExtremeDeath;

            MovementComponent.BeforeCommit(extraData);
            actorData.IsRunning = MovementComponent.IsRunning;

            if (InteractionComponent != null)
            {
                actorData.InteractionForceDisabled = InteractionComponent.InteractionDisabledByHit;

                if (InteractionComponent.CorpseContainer != null)
                    extraData["Container"] = SerializableContainerModel.MakeSerializableContainerModel(InteractionComponent.CorpseContainer);
            }

            extraData["Actor"] = actorData;

            return extraData;
        }

        public override void RestoreEntityData(Dictionary<string, object> data)
        {
            base.RestoreEntityData(data);

            if(data.ContainsKey("Actor"))
            {
                ActorExtraData actorData = data["Actor"] as ActorExtraData;
                if(actorData != null)
                {
                    //restore!

                    CurrentAiState = actorData.CurrentAiState;
                    LastAiState = actorData.LastAiState;
                    LockAiState = actorData.LockAiState;
                    if (AnimationComponent != null)
                    {
                        AnimationComponent.CurrentAnimState = actorData.CurrentAnimState;
                        AnimationComponent.LockAnimState = actorData.LockAnimState;
                    }
                    SavedTarget = actorData.SavedTarget;                    
                    TimeInState = actorData.TimeInState;

                    Health = actorData.Health;
                    BeenHit = actorData.BeenHit;

                    LastHit = actorData.LastHit;
                    LastHitDamage = actorData.LastHitDamage;
                    WasExtremeDeath = actorData.WasExtremeDeath;

                    MovementComponent.BeforeRestore(data);
                    MovementComponent.MovementTarget = actorData.AltTarget;
                    MovementComponent.IsRunning = actorData.IsRunning;
                    if (CurrentAiState == ActorAiState.Dead)
                        MovementComponent.HandleDeath();

                    if (InteractionComponent != null)
                    {
                        InteractionComponent.InteractionDisabledByHit = actorData.InteractionForceDisabled;

                        if (data.ContainsKey("Container"))
                            InteractionComponent.CorpseContainer = SerializableContainerModel.MakeContainerModel((SerializableContainerModel)data["Container"]);
                    }

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