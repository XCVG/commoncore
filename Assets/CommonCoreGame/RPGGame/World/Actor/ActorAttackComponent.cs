using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.RpgGame.Rpg;
using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the attacks of an Actor
    /// </summary>
    public class ActorAttackComponent : ActorAttackComponentBase
    {
        //can probably hold off on making this abstract for a while yet

        //TODO add capability to have both melee and ranged attacks

        //TODO visibility etc
        [Header("ActorAttackComponent")]        
        public bool UseMelee = true;
        public bool UseSuicide = false;
        public bool AutoDamageEffector = true;
        public FriendlyFireMode FriendlyFire = FriendlyFireMode.Default;
        public float AttackRange = 1.0f;
        public float AttackStateWarmup = 0;
        public float AttackStateDelay = 1.0f;
        public float AttackInterval = 1.0f;
        public float AttackSpread = 0.25f;
        public float BulletSpeed = 100;
        public ActorHitInfo AttackHit;
        public float AttackRandomFactor = 0;
        public BuiltinHitFlags[] AttackHitFlags;
        public string BulletPrefab;
        public string AttackEffectPrefab;
        public bool ParentAttackEffect = true;
        public AudioSource AttackSound;        
        public string AttackAnimationOverride;
        [Tooltip("Experimental. Will check LoS and try to maneuver to get a clear shot")]
        public bool UseLosChase = false;
        [Tooltip("Experimental, works with UseLosChase")]
        public float LosChaseDestinationRadius = 4f;

        //fields

        private Vector3? LastChaseDestination;
        private float LastAttackTime = -1;
        public bool DidAttack { get; private set; }
        

        private void Start()
        {
            FindComponents();
        }

        public override void Init()
        {
            FindComponents();

            if(ChaseOptimalDistance > 0 && ChaseOptimalDistance > AttackRange)
            {
                Debug.LogWarning($"[{nameof(ActorAttackComponent)}] {ActorController.gameObject.name} has ChaseOptimalDistance set to greater than attack range and will never be able to attack!");
            }

            if(UseLosChase && (ChaseOptimalDistance <= 0 || UseMelee))
            {
                Debug.LogWarning($"[{nameof(ActorAttackComponent)}] {ActorController.gameObject.name} has invalid settings for UseLosChase (ChaseOptimalDistance must be non-zero and UseMelee must be false), UseLosChase has been disabled.");
                UseLosChase = false;
            }
            
        }

        public override void BeginAttack()
        {
            LastChaseDestination = null; //needed?

            ActorController.AnimationComponent.Ref()?.SetAnimation(UseMelee ? ActorAnimState.Punching : ActorAnimState.Shooting);
            ActorController.transform.forward = VectorUtils.GetFlatVectorToTarget(transform.position, ActorController.Target.position); //ugly but workable for now
            if (AttackStateWarmup <= 0)
            {
                DoAttack(); //waaaaay too complicated to cram here
                LastAttackTime = Time.time;
            }
        }

        public override void UpdateAttack()
        {
            if (!DidAttack && WarmupIsDone)
            {
                DoAttack(); //waaaaay too complicated to cram here                                               
            }
        }

        /// <summary>
        /// Ends an attack if the Actor's attack state was exited
        /// </summary>
        public override void EndAttack()
        {
            DidAttack = false;
        }

        public override bool ReadyToAttack => ReadyToAttackInternal && (UseLosChase ? LosTargetInRange : TargetInRange);

        /// <summary>
        /// If we are ready to attack again (attack interval)
        /// </summary>
        protected bool ReadyToAttackInternal => (Time.time - LastAttackTime >= AttackInterval * (1f / ActorController.EffectiveAggression));

        /// <summary>
        /// If our actor's target is within range of us
        /// </summary>
        protected bool TargetInRange => (ActorController.Target.position - transform.position).magnitude <= AttackRange;

        /// <summary>
        /// If we have a warmup and it is done (crude hack based on time in state for now)
        /// </summary>
        protected bool WarmupIsDone => AttackStateWarmup > 0 && ActorController.TimeInState >= AttackStateWarmup;

        /// <summary>
        /// If our attack is done (crude hack based on time in state for now)
        /// </summary>
        public override bool AttackIsDone => ActorController.TimeInState >= AttackStateDelay + AttackStateWarmup;

        public override ActorAiState PostAttackState
        {
            get
            {
                if (!RpgWorldUtils.TargetIsAlive(ActorController.Target))
                    return ActorController.BaseAiState;
                else
                    return ActorAiState.Chasing;
            }
        }

        protected void DoAttack()
        {
            Vector3 shootPos = ShootPoint == null ? (transform.position + (transform.forward * 0.6f) + (transform.up * 1.25f)) : ShootPoint.position;

            if (UseSuicide)
            {
                //ActorController.TakeDamage(new ActorHitInfo(0, Mathf.Min(ActorController.MaxHealth * 100, float.MaxValue), 0, 0, 0, ActorController));
                ActorController.Health = 0;
            }
            else
            {
                var target = ActorController.Target;

                Vector3 aimPoint = target.position;

                var targetIAT = target.GetComponent<IAmTargetable>();
                if (targetIAT != null)
                    aimPoint = targetIAT.TargetPoint;

                aimPoint.y += UnityEngine.Random.Range(-AttackSpread, AttackSpread);
                aimPoint.x += UnityEngine.Random.Range(-AttackSpread, AttackSpread);
                aimPoint.z += UnityEngine.Random.Range(-AttackSpread, AttackSpread);

                
                Vector3 shootVec = (aimPoint - shootPos).normalized; //I screwed this up the first time

                float randomFactor = Mathf.Max(0, 1 + UnityEngine.Random.Range(-AttackRandomFactor, AttackRandomFactor));

                var modHit = new ActorHitInfo(AttackHit);
                var gameplayConfig = ConfigState.Instance.GetGameplayConfig();
                modHit.Damage *= ActorController.EffectiveStrength * randomFactor;
                modHit.DamagePierce *= ActorController.EffectiveStrength * randomFactor;
                modHit.Originator = ActorController;
                if (FriendlyFire == FriendlyFireMode.Always)
                    modHit.HarmFriendly = true;
                else if (FriendlyFire == FriendlyFireMode.Never)
                    modHit.HarmFriendly = false;
                else
                    modHit.HarmFriendly = GameParams.UseFriendlyFire;
                if (string.IsNullOrEmpty(modHit.OriginatorFaction))
                    modHit.OriginatorFaction = ActorController.Faction;

                modHit.HitFlags = TypeUtils.FlagsFromCollection(AttackHitFlags);

                if (UseMelee)
                {
                    if (AutoDamageEffector)
                        modHit.DamageEffector = (int)DamageEffector.Melee;

                    //melee path (raycast)
                    LayerMask lm = WorldUtils.GetAttackLayerMask();

                    //TODO 2D/3D attack range, or just increase attack range?

                    var rc = Physics.RaycastAll(shootPos, shootVec, AttackRange, lm, QueryTriggerInteraction.Collide);
                    BaseController ac = null;
                    foreach (var r in rc)
                    {
                        var go = r.collider.gameObject;
                        var ahgo = go.GetComponent<IHitboxComponent>();
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
                        if (ac is ITakeDamage itd)
                            itd.TakeDamage(modHit);
                    }


                }
                else if (BulletPrefab != null)
                {
                    if (AutoDamageEffector)
                        modHit.DamageEffector = (int)DamageEffector.Projectile;

                    //bullet path (shoot)
                    //var bullet = Instantiate<GameObject>(BulletPrefab, shootPos + (shootVec * 0.25f), Quaternion.identity, transform.root);
                    Quaternion bulletRotation = Quaternion.LookRotation(shootVec.normalized, Vector3.up);
                    var bullet = WorldUtils.SpawnEffect(BulletPrefab, shootPos + (shootVec * 0.25f), bulletRotation.eulerAngles, transform.root);
                    var bulletRigidbody = bullet.GetComponent<Rigidbody>();
                    bulletRigidbody.velocity = (shootVec * BulletSpeed);
                    var bulletScript = bullet.GetComponent<BulletScript>();
                    bulletScript.HitInfo = modHit;
                    bulletScript.Target = target;
                }
                else
                {
                    CDebug.LogEx(string.Format("{0} tried to shoot a bullet, but has no prefab defined!", name), LogLevel.Error, this);
                }
            }

            //show the effect, if applicable            
            if (!string.IsNullOrEmpty(AttackEffectPrefab))
            {
                WorldUtils.SpawnEffect(AttackEffectPrefab, shootPos, Vector3.zero, ParentAttackEffect ? (ShootPoint == null ? transform : ShootPoint) : null);
                //Instantiate(AttackEffectPrefab, shootPos, Quaternion.identity, (ShootPoint == null ? transform : ShootPoint));
            }
            if(AttackSound != null)
            {
                AttackSound.Play();
            }

            DidAttack = true;
            LastAttackTime = Time.time;
        }

        public override bool HandlesChaseDestination => UseLosChase;

        public override Vector3 GetChaseDestination(bool initial)
        {
            if(initial || !LastChaseDestination.HasValue || !CheckLineOfSight(ShootPoint.position, ActorController.Target) || !checkDestinationInRange(LastChaseDestination.Value))
            {
                LastChaseDestination = null;

                Vector3 defaultDestination = getDefaultDestination();
                if (checkDestinationLineOfSight(defaultDestination))
                {
                    //destination has line of sight, 
                    LastChaseDestination = defaultDestination;
                }
                else
                {
                    //find a chase destination that works
                    for (int i = 0; i < 6; i++) //6 iterations is a completely arbitrary number
                    {
                        float displacement = Mathf.Min(LosChaseDestinationRadius, UnityEngine.Random.Range(0f, LosChaseDestinationRadius) * i);
                        float angle = UnityEngine.Random.Range(0, 360f);
                        Vector3 displacementVector = (Quaternion.AngleAxis(angle, Vector3.up) * Vector3.right) * displacement;
                        var destination = defaultDestination + displacementVector;

                        if(ActorController.MovementComponent != null)
                        {
                            if (!ActorController.MovementComponent.CheckLocationReachable(destination)) //must be reachable (we really need that implemented better for this to work right)
                                continue;
                        }

                        if (checkDestinationInRange(destination) && checkDestinationLineOfSight(destination)) //must be able to hit target from it
                        {
                            LastChaseDestination = destination;
                            break;
                        }
                    }
                }

                //if we can't find a destination that works, stay in place
                if(!LastChaseDestination.HasValue)
                    LastChaseDestination = transform.position;
            }

            return LastChaseDestination.Value;

            //TODO probably move these all out into the class

            bool checkDestinationInRange(Vector3 destination)
            {
                float dist = (ActorController.Target.position - destination).magnitude;
                return dist <= AttackRange;
            }

            bool checkDestinationLineOfSight(Vector3 destination)
            {
                Vector3 vecToShootPoint = ShootPoint.position - ActorController.transform.position;
                Vector3 destShootPoint = destination + vecToShootPoint;

                return CheckLineOfSight(destShootPoint, ActorController.Target);
            }

            Vector3 getDefaultDestination()
            {
                //copied from ActorController for now
                Vector2 vecToTarget = (ActorController.Target.position - ActorController.transform.position).GetFlatVector();
                float distToTarget = vecToTarget.magnitude;
                float distToDestination = Mathf.Min(distToTarget, ChaseOptimalDistance);
                Vector2 dirToTarget = vecToTarget.normalized;
                Vector2 destination2d = transform.position.GetFlatVector() + dirToTarget * distToDestination;
                Vector3 destination = new Vector3(destination2d.x, ActorController.transform.position.y, destination2d.y);
                return destination;
            }
        }

        private bool LosTargetInRange
        {
            get
            {
                if (!UseLosChase)
                    return false;

                //Debug.Log("LosTargetInRange: " + CheckLineOfSight(ShootPoint.position, ActorController.Target));

                return TargetInRange && CheckLineOfSight(ShootPoint.position, ActorController.Target);
            }
        }

        [Serializable]
        public enum FriendlyFireMode
        {
            Default, Always, Never
        }

    }
}