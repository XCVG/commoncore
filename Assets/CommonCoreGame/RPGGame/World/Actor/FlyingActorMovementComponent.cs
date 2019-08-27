using System;
using UnityEngine;
using UnityEngine.AI;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Handles the movement for an Actor (classic floating enemies style)
    /// </summary>
    [RequireComponent(typeof(ActorController))]
    public class FlyingActorMovementComponent : ActorMovementComponentBase
    {
        private const float AltimeterRaycastDistance = 500f;
        private const float AngleThreshold = 0.5f;
        private const float HoverThreshold = 0.5f; //TODO unconst this?
        private const float DropThreshold = 0.2f;

        [SerializeField, Header("Flying Options")]
        private float PreferredHeight = 10f;
        [SerializeField]
        private float VerticalWander = 3f;
        [SerializeField]
        private float MaxSpeed = 1f;
        [SerializeField]
        private float MaxBurstSpeed = 2f;
        [SerializeField]
        private float MaxVerticalSpeed = 3f;
        [SerializeField]
        private float RotateSpeed = 90f;
        [SerializeField]
        private bool UseRealAltitude = true;
        [SerializeField]
        private bool FaceActorTarget = true;
        [SerializeField]
        private bool DropWhenDead = true;
        [SerializeField]
        private float DropAccleration = 5f;
        [SerializeField]
        private float MaxDropSpeed = 15f;

        //override altitude, for scripted movement or whatever
        public float? YPositionOverride { get; set; } //TODO implement this

        //vars for hovering
        private float? HoverTargetHeight;

        //vars for dropping
        private float CurrentDropVelocity;

        public override void Init()
        {
            base.Init();

            MovementTarget = transform.position;
            CurrentDropVelocity = 0;
        }

        protected override void Update()
        {
            base.Update();

            //uh, should we be in fixedupdate? 
            //probably lol

            if(DropWhenDead && ActorController.CurrentAiState == ActorAiState.Dead)
            {
                Drop();
            }
            else if(AtTarget)
            {
                Hover();
            }
            else
            {
                MoveToTarget();
            }
        }

        //TODO character controllers?

        private void MoveToTarget()
        {
            //turn to face target
            if(ControlRotation)
                FacePoint(MovementTarget);

            //TODO slow when nearing target (arrive)

            //WIP horizontal movement
            Vector2 vecToTarget = (MovementTarget - transform.position).ToFlatVec();
            float moveMagnitude = Mathf.Min(Time.deltaTime * (IsRunning ? MaxBurstSpeed : MaxSpeed), vecToTarget.magnitude);
            Vector2 moveVector = vecToTarget.normalized * moveMagnitude;
            transform.Translate(new Vector3(moveVector.x, 0, moveVector.y), Space.World);

            //WIP vertical movement
            float verticalMove = (YPositionOverride ?? PreferredHeight) - HeightAboveGround;
            float verticalMoveMagnitude = Mathf.Min(Time.deltaTime * MaxVerticalSpeed, Mathf.Abs(verticalMove));
            transform.Translate(verticalMoveMagnitude * Mathf.Sign(verticalMove) * Vector3.up);
            
        }

        /// <summary>
        /// The height of this actor above the ground
        /// </summary>
        public float HeightAboveGround
        {
            get
            {
                if(UseRealAltitude)
                {
                    var hits = Physics.RaycastAll(transform.position, Vector3.down, AltimeterRaycastDistance, ~0, QueryTriggerInteraction.Ignore);
                    foreach(var hit in hits)
                    {
                        if(hit.collider is TerrainCollider)
                        {
                            return hit.distance;
                        }
                    }
                }

                return transform.position.y;
            }
        }

        private void Hover()
        {
            //floatbob/hover handling
            if (!HoverTargetHeight.HasValue || Mathf.Abs(HeightAboveGround - HoverTargetHeight.Value) < HoverThreshold)
            {
                //decide a target height
                HoverTargetHeight = PreferredHeight + UnityEngine.Random.Range(-VerticalWander, VerticalWander);
            }
            else
            {
                //move toward that target height
                float verticalMove = (YPositionOverride ?? HoverTargetHeight.Value) - HeightAboveGround;
                float verticalMoveMagnitude = Mathf.Min(Time.deltaTime * MaxVerticalSpeed, Mathf.Abs(verticalMove));
                transform.Translate(verticalMoveMagnitude * Mathf.Sign(verticalMove) * Vector3.up);
            }

            //turn to face target
            if (ControlRotation && FaceActorTarget && ActorController.Target != null)
                FacePoint(ActorController.Target.position);
            //FacePoint(ActorController.Target != null ? ActorController.Target.position : MovementTarget);
        }


        private void FacePoint(Vector3 target)
        {
            Vector2 vecToTarget = (target - transform.position).ToFlatVec();
            float angleToTarget = Vector2.SignedAngle(transform.forward.ToFlatVec(), vecToTarget);
            if (Mathf.Abs(angleToTarget) > AngleThreshold) //"close enough" handling
            {
                float angleToRotate = Mathf.Min(Mathf.Abs(angleToTarget), RotateSpeed * Time.deltaTime) * Mathf.Sign(angleToTarget);
                transform.Rotate(Vector3.down, angleToRotate);
            }
        }

        private void Drop()
        {
            //drop to the ground when dead
            if(HeightAboveGround <= DropThreshold)
            {
                CurrentDropVelocity = 0;
            }
            else
            {
                CurrentDropVelocity = Mathf.Min(CurrentDropVelocity + DropAccleration * Time.deltaTime, MaxDropSpeed);
                transform.Translate(Vector3.down * CurrentDropVelocity * Time.deltaTime);
            }
        }

        public override void AbortMove()
        {
            MovementTarget = transform.position;
        }

        public override void SetDestination(Vector3 dest)
        {
            MovementTarget = dest;
        }

        public override float DistToTarget => (MovementTarget.ToFlatVec() - transform.position.ToFlatVec()).magnitude;
    }
}