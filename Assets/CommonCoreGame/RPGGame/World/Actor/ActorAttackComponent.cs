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
        public Transform ShootPoint;
        public string AttackAnimationOverride;

        //fields


        private float LastAttackTime = -1;
        public bool DidAttack { get; private set; }
        

        private void Start()
        {
            FindComponents();
        }

        public override void Init()
        {
            FindComponents();

            if(ActorController.ChaseOptimalDistance > 0 && ActorController.ChaseOptimalDistance > AttackRange)
            {
                Debug.LogWarning($"[ActorAttackComponent] {ActorController.gameObject.name} has ChaseOptimalDistance set to greater than attack range and will never be able to attack!");
            }
            
        }

        public override void BeginAttack()
        {
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

        public override bool ReadyToAttack => ReadyToAttackInternal && TargetInRange;

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

                var targetAC = target.GetComponent<ActorController>();
                if (targetAC != null && targetAC.TargetPoint != null)
                    aimPoint = targetAC.TargetPoint.position;

                var targetPC = target.GetComponent<PlayerController>();
                if (targetPC != null && targetPC.TargetPoint != null)
                    aimPoint = targetPC.TargetPoint.position;

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

        [Serializable]
        public enum FriendlyFireMode
        {
            Default, Always, Never
        }

    }
}