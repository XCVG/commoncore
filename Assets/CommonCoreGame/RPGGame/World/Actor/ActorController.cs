using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.LockPause;
using CommonCore.Messaging;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    //TODO restorable, animation, and eventually a full refactor
    //I say that now, but I bet this will still be mostly the same until, say, Downwarren
    public class ActorController : BaseController, ITakeDamage, IAmTargetable
    {
        public string CharacterModelIdOverride; //does nothing lol

        [Header("Components")]
        //public CharacterController CharController;
        
        public ActorInteractableComponent InteractComponent;
        //public NavMeshAgent NavComponent;
        public Transform TargetPoint;
        public ActorAnimationComponentBase AnimationComponent;
        public ActorMovementComponentBase MovementComponent;
        public ActorAttackComponent AttackComponent;
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
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Thermnal")] //these might be absolutely fucked now
        public float[] DamageResistance = { 0, 0, 0, 0, 0, 0};
        [Tooltip("Normal, Impact, Explosive, Energy, Poison, Thermnal")]
        public float[] DamageThreshold = { 0, 0, 0, 0, 0, 0 };
        public ActionSpecial OnDeathSpecial;
        public bool FeelPain = true;
        public float PainChance = 0.5f;
        public float PainWaitTime = 1.0f;
        public string DefaultHitPuff = "DefaultHitPuff";

        [Header("Aggression")]
        public bool Aggressive = false;
        public string Faction = "None";
        public float Detectability = 1.0f;
        public bool IsTarget = true;
        public bool Defensive = true;
        public bool Infighting = false;
        public bool Relentless = false;
        public bool UseLineOfSight = false;
        public float SearchRadius = 25.0f;
        public int SearchInterval = 70;
        public bool DisableInteractionOnHit = true;
        private bool BeenHit = false;

        [Header("Interaction")] //TODO move this out into ActorInteractionComponent
        public int GrantXpOnDeath;

        [Header("Movement")]
        public bool RunOnChase = true;
        public bool RunOnFlee = true;

        [field: SerializeField, Tooltip("For debugging only- changing this may result in unpredicted results")]
        public Vector3 InitialPosition { get; private set; } //TODO should we save this? I don't think so


        public float WanderThreshold = 1.0f;
        public float WanderTimeout = 10.0f;
        public Vector2 WanderRadius = new Vector2(10.0f, 10.0f);

        private QdmsMessageInterface MessageInterface;

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
                AttackComponent = GetComponent<ActorAttackComponent>();
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
                    if (CurrentAiState == ActorAiState.Dead) //fix for glitchy looking behaviour
                        break;

                    MovementComponent.AbortMove();
                    MovementComponent.HandleDeath();
                    if (DieImmediately)
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Dead);
                    else
                        AnimationComponent.Ref()?.SetAnimation(ActorAnimState.Dying);

                    AudioComponent.Ref()?.StopLivingSounds();
                    AudioComponent.Ref()?.PlayDeathSound();

                    if(InteractionComponent != null)
                        InteractionComponent.InteractionDisabledByHit = false;

                    if (DestroyOnDeath)
                        this.gameObject.SetActive(false); //actually destroying the object breaks saving

                    if (OnDeathSpecial != null)
                        OnDeathSpecial.Execute(new ActionInvokerData { Activator = this });

                    if (Target != null && Target.GetComponent<PlayerController>() && GrantXpOnDeath > 0) //what the fuck
                        GameState.Instance.PlayerRpgState.GrantXPScaled(GrantXpOnDeath);
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

                        var d = Target.position; //FIXME what if Target is null?
                        MovementComponent.SetDestination(d);
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

                    if (AttackComponent != null && AttackComponent.ReadyToAttack && AttackComponent.TargetInRange)
                    {
                        EnterState(ActorAiState.Attacking);
                        return;
                    }
                    else
                    {
                        //set target
                        var d = Target.position;
                        MovementComponent.SetDestination(d);
                    }
                    if (!Relentless)
                    {
                        //break off if we are too far away or too badly hurt
                        if (Health <= (MaxHealth * 0.2f))
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
                    if (!AttackComponent.DidAttack && AttackComponent.WarmupIsDone)
                    {
                        AttackComponent.DoAttack(); //waaaaay too complicated to cram here                                               
                    }
                    if (AttackComponent.AttackIsDone)
                    {
                        //just return
                        if (!RpgWorldUtils.TargetIsAlive(Target))
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

        

        private void SelectTarget()
        {
            var gameplayConfig = ConfigState.Instance.GetGameplayConfig();

            if (TotalTickCount % SearchInterval * (1f / gameplayConfig.Difficulty.ActorAggression) != 0)
                return;

            var detectionDifficultyFactor = 1f / gameplayConfig.Difficulty.ActorPerception;

            //check player first since it's (relatively) cheap
            if (FactionModel.GetRelation(Faction, "Player") == FactionRelationStatus.Hostile && !MetaState.Instance.SessionFlags.Contains("NoTarget") && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoTarget))
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
                        && FactionModel.GetRelation(Faction, potentialTarget.Faction) == FactionRelationStatus.Hostile
                        && !(potentialTarget == this))
                    {                       
                        //roll some dice
                        if (potentialTarget.Detectability < 1 && UnityEngine.Random.Range(0f, 1f * detectionDifficultyFactor) > potentialTarget.Detectability) //probably correct
                            continue;

                        if (UseLineOfSight)
                        {
                            //additional check
                            RaycastHit hitinfo;
                            if (Physics.Raycast(transform.position + new Vector3(0, 1.0f, 0), (targetController.transform.position - transform.position), out hitinfo))
                            {
                                if (hitinfo.collider.gameObject == targetController.gameObject)
                                {
                                    Target = targetController.transform;
                                    break;
                                }
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

        public void TakeDamage(ActorHitInfo data)
        {
            if(!data.HarmFriendly)
            {
                string hitFaction = data.OriginatorFaction;
                if(!string.IsNullOrEmpty(hitFaction))
                {
                    FactionRelationStatus relation = FactionModel.GetRelation(hitFaction, Faction); //this looks backwards but it's because we're checking if the Bullet is-friendly-to the Actor
                    if (relation == FactionRelationStatus.Friendly)
                        return; //no friendly fire
                }
            }

            //damage model is very stupid right now, we will make it better later
            float dt = data.DamageType < DamageThreshold.Length ? DamageThreshold[(int)data.DamageType] : 0f;
            float dr = data.DamageType < DamageThreshold.Length ? DamageResistance[(int)data.DamageType] : 0f;
            float damageTaken = RpgValues.DamageTaken(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == (int)ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == (int)ActorBodyPart.LeftArm || data.HitLocation == (int)ActorBodyPart.LeftLeg || data.HitLocation == (int)ActorBodyPart.RightArm || data.HitLocation == (int)ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            damageTaken *= (1f / ConfigState.Instance.GetGameplayConfig().Difficulty.ActorStrength);

            if (!Invincible)
                Health -= damageTaken;

            if (CurrentAiState == ActorAiState.Dead) //abort if we're already dead
                return;

            bool didTakePain = UnityEngine.Random.Range(0f, 1f) < PainChance; //shouldn't this be weighted by damage?

            if (Defensive && data.Originator != null && data.Originator != this)
            {
                FactionRelationStatus relation = FactionRelationStatus.Neutral;
                if (data.Originator is PlayerController)
                {
                    relation = FactionModel.GetRelation(Faction, "Player");
                }
                else if (data.Originator is ActorController)
                {
                    relation = FactionModel.GetRelation(Faction, ((ActorController)data.Originator).Faction);
                }

                if (relation != FactionRelationStatus.Friendly || Infighting)
                {
                    Target = data.Originator.transform;
                    BeenHit = true;

                    if (DisableInteractionOnHit && InteractionComponent != null)
                        InteractionComponent.InteractionDisabledByHit = true;

                    if (FeelPain && didTakePain)
                        if (CurrentAiState != ActorAiState.Hurting)
                            EnterState(ActorAiState.Hurting);
                        else
                            EnterState(ActorAiState.Chasing);
                }
                else if (CurrentAiState != ActorAiState.Hurting)
                    EnterState(ActorAiState.Hurting);

            }
            else if (FeelPain && didTakePain && CurrentAiState != ActorAiState.Hurting)
                EnterState(ActorAiState.Hurting);
        }

        //these are both done stupidly and could probably be done through reflection instead but for now...

        public override Dictionary<string, object> GetExtraData()
        {
            Dictionary<string, object> extraData = new Dictionary<string, object>();

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

        public override void SetExtraData(Dictionary<string, object> data)
        {

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