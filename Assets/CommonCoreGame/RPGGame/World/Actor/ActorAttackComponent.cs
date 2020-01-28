using CommonCore.Config;
using CommonCore.DebugLog;
using CommonCore.World;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the attacks of an Actor
    /// </summary>
    public class ActorAttackComponent : MonoBehaviour
    {
        //can probably hold off on making this abstract for a while yet

        [SerializeField]
        private ActorController ActorController;

        //TODO add capability to have both melee and ranged attacks

        //TODO visibility etc
        public bool UseMelee = true;
        public float AttackRange = 1.0f;
        public float AttackStateWarmup = 0;
        public float AttackStateDelay = 1.0f;
        public float AttackInterval = 1.0f;
        public float AttackSpread = 0.25f;
        public float BulletSpeed = 100;
        public ActorHitInfo AttackHit;
        public string BulletPrefab;
        public string AttackEffectPrefab;
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

        public void Init()
        {
            FindComponents();

            
        }

        private void FindComponents()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{nameof(ActorAttackComponent)} on {name} is missing ActorController!");


        }

        public void BeginAttack()
        {
            ActorController.AnimationComponent.Ref()?.SetAnimation(UseMelee ? ActorAnimState.Punching : ActorAnimState.Shooting);
            ActorController.transform.forward = VectorUtils.GetFlatVectorToTarget(transform.position, ActorController.Target.position); //ugly but workable for now
            if (AttackStateWarmup <= 0)
            {
                DoAttack(); //waaaaay too complicated to cram here
                LastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// Ends an attack if the Actor's attack state was exited
        /// </summary>
        public void EndAttack()
        {
            DidAttack = false;
        }

        /// <summary>
        /// If we are ready to attack again (attack interval)
        /// </summary>
        public bool ReadyToAttack => (Time.time - LastAttackTime >= AttackInterval * (1f / ConfigState.Instance.GetGameplayConfig().Difficulty.ActorAggression));

        /// <summary>
        /// If our actor's target is within range of us
        /// </summary>
        public bool TargetInRange => (ActorController.Target.position - transform.position).magnitude <= AttackRange;

        /// <summary>
        /// If we have a warmup and it is done (crude hack based on time in state for now)
        /// </summary>
        public bool WarmupIsDone => AttackStateWarmup > 0 && ActorController.TimeInState >= AttackStateWarmup;

        /// <summary>
        /// If our attack is done (crude hack based on time in state for now)
        /// </summary>
        public bool AttackIsDone => ActorController.TimeInState >= AttackStateDelay + AttackStateWarmup;

        public void DoAttack()
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

            Vector3 shootPos = ShootPoint == null ? (transform.position + (transform.forward * 0.6f) + (transform.up * 1.25f)) : ShootPoint.position;
            Vector3 shootVec = (aimPoint - shootPos).normalized; //I screwed this up the first time

            var modHit = AttackHit;
            var gameplayConfig = ConfigState.Instance.GetGameplayConfig();
            modHit.Damage *= gameplayConfig.Difficulty.ActorStrength;
            modHit.DamagePierce *= gameplayConfig.Difficulty.ActorStrength;
            modHit.Originator = ActorController;

            if (UseMelee)
            {
                //melee path (raycast)
                LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");

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
                //bullet path (shoot)
                //var bullet = Instantiate<GameObject>(BulletPrefab, shootPos + (shootVec * 0.25f), Quaternion.identity, transform.root);
                var bullet = WorldUtils.SpawnEffect(BulletPrefab, shootPos + (shootVec * 0.25f), Vector3.zero, transform.root);
                var bulletRigidbody = bullet.GetComponent<Rigidbody>();
                bulletRigidbody.velocity = (shootVec * BulletSpeed);
                bullet.GetComponent<BulletScript>().HitInfo = modHit;
            }
            else
            {
                CDebug.LogEx(string.Format("{0} tried to shoot a bullet, but has no prefab defined!", name), LogLevel.Error, this);
            }

            //show the effect, if applicable            
            if (AttackEffectPrefab != null)
            {
                WorldUtils.SpawnEffect(AttackEffectPrefab, shootPos, Vector3.zero, (ShootPoint == null ? transform : ShootPoint));
                //Instantiate(AttackEffectPrefab, shootPos, Quaternion.identity, (ShootPoint == null ? transform : ShootPoint));
            }
            if(AttackSound != null)
            {
                AttackSound.Play();
            }

            DidAttack = true;
            LastAttackTime = Time.time;
        }
    }
}