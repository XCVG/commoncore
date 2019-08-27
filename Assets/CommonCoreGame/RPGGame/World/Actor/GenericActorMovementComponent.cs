using System;
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
        //TODO make this abstract, we will need to do that soon to support dragons

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

        //fields
        private bool NavmeshEnabled;        

        protected override void Start()
        {
            base.Start();

            FindComponents();
        }

        public override void Init()
        {
            base.Init();

            FindComponents();

            SetInitialNavState();
        }

        protected override void Update() //TODO may go to delegated update
        {
            base.Update();

            EmulateNav();
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
            if (NavmeshEnabled)
                return;

            //apply gravity
            CharController.Move(Physics.gravity * Time.deltaTime);

            //get vector to target
            Vector3 dirVec = (MovementTarget - transform.position);
            Vector3 pathForward = dirVec.normalized;
            pathForward.y = 0; //we actually want a flat vector

            if (dirVec.magnitude <= TargetThreshold) //shouldn't hardcode that threshold!
                return;

            //move
            CharController.Move(Time.deltaTime * (IsRunning ? RunSpeed : WalkSpeed) * pathForward);

            //rotate me
            if (ControlRotation)
            {
                float maxangle = Vector3.SignedAngle(transform.forward, pathForward, Vector3.up);
                float rotangle = Mathf.Min(Time.deltaTime * RotateSpeed, Mathf.Abs(maxangle)) * Mathf.Sign(maxangle);
                transform.Rotate(Vector3.up, rotangle);
            }
            //transform.forward = pathForward; //TODO make this actual motion instead of a snap.
        }

        private void SetInitialNavState()
        {
            if (NavComponent != null && !ForceNavmeshOff)
            {
                NavComponent.enabled = false;
                NavComponent.speed = WalkSpeed;
                NavComponent.angularSpeed = RotateSpeed;
                NavComponent.stoppingDistance = TargetThreshold;

                if (NavComponent.isOnNavMesh)
                    NavmeshEnabled = true;
            }

            MovementTarget = transform.position;
        }
    }
}