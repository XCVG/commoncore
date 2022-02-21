using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Handles the movement for an Actor (generic/legacy version)
    /// </summary>
    /// <remarks>
    /// <para>Suitable for most grount units</para>
    /// <para>This can use a NavMeshAgent but doesn't necessarily need one</para>
    /// </remarks>
    [RequireComponent(typeof(ActorController))]
    public class GenericActorMovementComponent : ActorMovementComponentBase
    {

        [Header("Components"), SerializeField]
        private NavMeshAgent NavComponent;
        [SerializeField]
        private CharacterController CharController;

        [Header("Options"), SerializeField] //TODO visibility
        private bool ForceNavmeshOff = false;
        [SerializeField]
        private bool UseControllerGravity = true;
        [SerializeField]
        private float WalkSpeed = 1.0f;
        [SerializeField]
        private float RunSpeed = 2.0f;
        [SerializeField]
        private float RotateSpeed = 90.0f;
        [SerializeField]
        private bool ShrinkColliderOnDeath = false;
        [SerializeField]
        private bool DisableColliderOnDeath = false;

        [SerializeField, Header("Physics")]
        private bool EnablePhysics = true;
        [SerializeField]
        private float BrakeFactor = 2f;
        [SerializeField]
        private float CollidedBrakeFactor = 10f;
        [SerializeField]
        private float YBrakeFactor = 10f;
        [SerializeField, Tooltip("Use to simulate mass (lower multiplier=higher mass)"), Obsolete("Use Physics Mass instead")]
        private float VelocityMultiplier = 1f;

        //fields
        private bool NavmeshEnabled;
        private bool DeathHandled = false;

        private Vector3 OriginalControllerCenter;
        private float OriginalControllerHeight;

        protected override void Start()
        {
            base.Start();

            FindComponents();
            StoreOriginalCharControllerValues();
        }

        public override void Init()
        {
            base.Init();

            FindComponents();
            StoreOriginalCharControllerValues();

            SetInitialNavState();
        }

        public override void BeforeRestore(Dictionary<string, object> data)
        {
            FindComponents();
            StoreOriginalCharControllerValues();
        }       

        protected override void Update() //TODO may go to delegated update
        {
            base.Update();

            EmulateNav();

            HandlePhysics();
        }

        private void FindComponents()
        {
            if (ActorController == null)
                ActorController = GetComponent<ActorController>();

            if (ActorController == null)
                Debug.LogError($"{nameof(GenericActorMovementComponent)} on {name} is missing ActorController!");

            if (CharController == null)
                CharController = GetComponent<CharacterController>();

            if (CharController == null)
                Debug.LogError($"{nameof(GenericActorMovementComponent)} on {name} is missing CharController!");

            if (NavComponent == null)
                NavComponent = GetComponent<NavMeshAgent>();

            if (CharController == null)
                Debug.LogWarning($"{nameof(GenericActorMovementComponent)} on {name} is missing NavComponent!");
        }

        private void StoreOriginalCharControllerValues()
        {
            OriginalControllerCenter = CharController.center;
            OriginalControllerHeight = CharController.height;
        }

        public override void SetDestination(Vector3 dest)
        {
            MovementTarget = dest;
            if (NavmeshEnabled)
            {
                NavComponent.SetDestination(dest);
                NavComponent.enabled = true;
                if (IsRunning)
                    NavComponent.speed = RunSpeed;
                else
                    NavComponent.speed = WalkSpeed;
            }

        }

        public override void AbortMove()
        {
            IsRunning = false;
            MovementTarget = transform.position;
            if (NavmeshEnabled)
            {
                NavComponent.destination = transform.position;
                NavComponent.enabled = false;
            }
        }

        private void EmulateNav()
        {
            if (NavmeshEnabled || !CharController.enabled)
                return;

            //apply gravity
            if(UseControllerGravity)
                CharController.Move(Physics.gravity * Time.deltaTime);

            //get vector to target
            Vector3 dirVec = (MovementTarget - transform.position);
            Vector3 pathForward = dirVec.normalized;
            pathForward.y = 0; //we actually want a flat vector

            if (dirVec.magnitude <= TargetThreshold) //shouldn't hardcode that threshold!
                return;

            //move
            CharController.Move(Time.deltaTime * (IsRunning ? RunSpeed : WalkSpeed) * DifficultySpeedFactor * pathForward);

            //rotate me
            if (ControlRotation)
            {
                float maxangle = Vector3.SignedAngle(transform.forward, pathForward, Vector3.up);
                float rotangle = Mathf.Min(Time.deltaTime * RotateSpeed * DifficultySpeedFactor, Mathf.Abs(maxangle)) * Mathf.Sign(maxangle);
                transform.Rotate(Vector3.up, rotangle);
            }
        }

        private void SetInitialNavState()
        {
            if (NavComponent != null && !ForceNavmeshOff)
            {
                NavComponent.enabled = false;
                NavComponent.speed = WalkSpeed * DifficultySpeedFactor;
                NavComponent.angularSpeed = RotateSpeed * DifficultySpeedFactor;
                NavComponent.stoppingDistance = TargetThreshold;

                if (NavComponent.isOnNavMesh)
                    NavmeshEnabled = true;
            }

            MovementTarget = transform.position;
        }

        public override void HandleDeath()
        {
            if (DeathHandled)
                return;

            if(DisableColliderOnDeath)
            {
                CharController.detectCollisions = false;
                CharController.enabled = false;
            }
            else if(ShrinkColliderOnDeath)
            {
                CharController.center = Vector3.zero;
                CharController.height = CharController.height / 2f;
            }

            DeathHandled = true;
        }

        public override void HandleRaise()
        {
            if (DisableColliderOnDeath)
            {
                CharController.detectCollisions = true;
                CharController.enabled = true;
            }
            else if (ShrinkColliderOnDeath)
            {
                CharController.center = OriginalControllerCenter;
                CharController.height = OriginalControllerHeight;
            }

            DeathHandled = false;
        }

        public override void HandleDifficultyChanged()
        {
            if (NavComponent != null && !ForceNavmeshOff)
            {
                NavComponent.speed = WalkSpeed * DifficultySpeedFactor; //TODO should probably set this on config changed
                NavComponent.angularSpeed = RotateSpeed * DifficultySpeedFactor;
            }
        }

        private void HandlePhysics()
        {
            if (!EnablePhysics || !CharController.enabled)
                return;

            if(PhysicsVelocity.magnitude > 0)
            {
                CharController.Move(PhysicsVelocity * Time.deltaTime);

                if(CharController.isGrounded & PhysicsVelocity.y > 0)
                {
                    PhysicsVelocity = new Vector3(PhysicsVelocity.x, Mathf.Max(PhysicsVelocity.y - Time.deltaTime * YBrakeFactor), PhysicsVelocity.z);
                }

                var dir = PhysicsVelocity.normalized;
                float brakeMagnitude = 0;
                if (CharController.collisionFlags == CollisionFlags.Sides)
                {
                    //collision brake factor
                    brakeMagnitude = Time.deltaTime * CollidedBrakeFactor;
                }
                else
                {
                    //normal brake factor
                    brakeMagnitude = Time.deltaTime * BrakeFactor;
                }
                brakeMagnitude = Mathf.Min(brakeMagnitude, PhysicsVelocity.magnitude);
                PhysicsVelocity -= dir * brakeMagnitude;
            }          

        }

        public override void AddVelocity(Vector3 velocity)
        {
            if (!EnablePhysics)
                return;

            base.AddVelocity(velocity * VelocityMultiplier);
        }

        public override void SetVelocity(Vector3 velocity)
        {
            if (velocity == Vector3.zero) //allow zeroing velocity even when physics is disabled
                PhysicsVelocity = Vector3.zero;

            if (!EnablePhysics)
                return;

            base.SetVelocity(velocity * VelocityMultiplier);
        }

        public override void Push(Vector3 impulse)
        {
            if (!EnablePhysics)
                return;

            base.Push(impulse);
        }

        public override bool CheckLocationReachable(Vector3 location)
        {
            //use navmesh check if available
            if (NavmeshEnabled)
            {
                return NavMesh.SamplePosition(location, out var _, TargetThreshold, NavMesh.AllAreas);
            }

            //otherwise, check for space

            //based on https://roundwide.com/physics-overlap-capsule/
            Vector3 center = transform.TransformPoint(CharController.center);
            Vector3 size = transform.TransformVector(CharController.radius, CharController.height, CharController.radius);
            Vector3 point0 = new Vector3(center.x, center.y - size.y / 2 + size.x, center.z);
            Vector3 point1 = new Vector3(center.x, center.y + size.y / 2 - size.x, center.z);

            var cols = Physics.OverlapCapsule(point0, point1, size.x, LayerMask.GetMask("Default", "BlockActors"));
            bool blocked = false;
            foreach(var col in cols)
            {
                if (col == CharController)
                    continue;

                if (!col.enabled || !col.gameObject.activeInHierarchy)
                    continue;

                blocked = true;
                break;
            }
            return !blocked;
          
        }

        public override bool IsStuck => false; //currently unhandled

    }
}