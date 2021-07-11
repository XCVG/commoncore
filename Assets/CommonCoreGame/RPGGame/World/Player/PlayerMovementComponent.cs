using CommonCore.Config;
using CommonCore.Input;
using CommonCore.LockPause;
using CommonCore.Messaging;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CommonCore.RpgGame.World
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerMovementComponent : MonoBehaviour
    {
        //TODO fix animation

        //TODO collision/fall sounds

        //TODO SerializeField
        [Header("Options"), SerializeField]
        private bool AllowSlopeJumping = false;
        [SerializeField]
        private float InputDeadzone = 0.1f;
        [SerializeField]
        private bool TeleportIfOutOfBounds = true;
        [SerializeField]
        private float CrouchYScale = 0.66f;
        [SerializeField]
        private float CrouchAnimateRate = 0.5f;

        [SerializeField]
        private float LookYLimit = 90f;

        [SerializeField, Header("Dynamics Values")]
        private float GroundFriction = 0f;
        [SerializeField]
        private float AirResistance = 1f;
        [SerializeField]
        private float GravityMultiplier = 2f;
        [SerializeField]
        private float SlopeSlideAccleration = 100f;
        [SerializeField]
        private float SlopeSlideSpeed = 6f;

        [SerializeField, Header("Movement Values")]
        private float MaxBrakeAcceleration = 1000f;
        [SerializeField]
        private float MaxWalkSpeed = 7f;
        [SerializeField]
        private float MaxWalkAcceleration = 100f;
        [SerializeField]
        private float MaxSprintSpeed = 12f;
        [SerializeField]
        private float MaxSprintAcceleration = 150f;
        [SerializeField]
        private float MaxCrouchSpeed = 4f;
        [SerializeField]
        private float MaxCrouchAcceleration = 75f;
        [SerializeField]
        private float MaxAirSpeed = 8f;
        [SerializeField]
        private float MaxAirAcceleration = 50f;
        [SerializeField]
        private Vector3 JumpInstantaneousVelocity = new Vector3(0, 6f, 0f);
        [SerializeField]
        private Vector3 JumpInstantaneousDisplacement = new Vector3(0, 0.15f, 0);
        [SerializeField]
        private Vector3 JumpCrouchBoostVelocity = new Vector3(0f, 1f, 0f);

        [SerializeField, Header("Collision Values")]
        private bool EnableCollisions = true;
        [SerializeField]
        private float CollisionMass = 100f;
        [SerializeField, Tooltip("Ratio of bounce to transfer when hitting a pushable Rigidbody")]
        private float CollisionRigidbodyBounceRatio = 0.5f;
        [SerializeField]
        private float CollisionBrakeAcceleration = 25f;

        [SerializeField, Header("Noclip Values")]
        private Vector3 NoclipVelocity = new Vector3(8f, 8f, 8f);
        [SerializeField]
        private Vector3 NoclipFastVelocity = new Vector3(20f, 20f, 20f);

        [SerializeField, Header("Fall Damage")]
        private float FallMinDetectVelocity = 5f;
        [SerializeField]
        private float FallMinDetectTime = 0.25f;
        [SerializeField]
        private float FallMinGruntVelocity = 12f;

        [Header("Visual Options")]
        public bool UseCrouchHack;
        public bool CountRotateAsMovement = false;


        [SerializeField, Header("Components")]
        private PlayerController PlayerController;
        [SerializeField]
        private CharacterController CharController;
        [SerializeField]
        private Rigidbody CharRigidbody;
        [SerializeField]
        private CapsuleCollider Hitbox;
        [SerializeField]
        public Animator AnimController;


        [Header("Sounds")] //TODO clean up audio handling
        public AudioSource WalkSound; //may move this one
        public AudioSource RunSound; //and also this one
        public AudioSource FallImpactSound; //"event" sounds will remain handled here
        public AudioSource FallPainSound;
        public AudioSource JumpSound;
        public AudioSource CrouchSound;

        public bool IsMoving { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsOnSlope { get; private set; }
        public bool IsAnimating { get; private set; }

        private int WalkableLayerMask;

        private Vector3 LastGroundNormal;
        private bool DidJump;
        private bool DidChangeCrouch;
        private bool RunWasBlocked;
        private float TimeInAir;

        private float? CrouchTargetHeight = null;

        //all this crap is necessary for crouching to work correctly (TODO STRUCTIFY)
        private float? CharControllerOriginalHeight;
        private float? CharControllerOriginalYPos;
        private float? HitboxOriginalHeight;
        private float? HitboxOriginalYPos;
        private Vector3? CameraRootOriginalLPos;
        private Vector3? ModelOriginalScale;

        //new shit
        [field: SerializeField, Header("Debug")]
        public Vector3 Velocity { get; private set; }
        [field: SerializeField]
        public bool Clipping { get; private set; } = true;

        // Start is called before the first frame update
        void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponent<PlayerController>();


            if (!CharController)
            {
                CharController = GetComponent<CharacterController>();
            }

            if (!CharRigidbody)
            {
                CharRigidbody = GetComponent<Rigidbody>();
            }

            if (!Hitbox)
            {
                Hitbox = transform.Find("Hitbox").GetComponent<CapsuleCollider>();
            }

            if (!AnimController)
            {
                AnimController = GetComponent<Animator>();
            }

            WalkableLayerMask = LayerMask.GetMask("Default", "BlockActors");

            SetBaseScaleVars();
        }

        //we may rework this to use an explicit update from PlayerController
        void Update()
        {
            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            //HandleDynamicMovement();

            //reset these
            IsMoving = false;
            IsRunning = false;
            DidJump = false;
            DidChangeCrouch = false;

            if (PlayerController.PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleLook();
                if (Clipping)
                    HandleMovement();                    
            }

            if (Clipping)
            {
                HandleDynamicMovement();
                HandleOutOfBounds();
            }

            HandleNoclip();
            HandleCrouchAnimation();

            HandleAnimation();
            HandleSounds();
        }

        //handle collider hits (will probably have to rewrite this later)
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!Clipping)
                return;

            //special case: terrain
            if (hit.collider is TerrainCollider)
            {
                LastGroundNormal = hit.normal;
                return; //ignore terrain hits
            }

            //special case: not terrain, but something we're standing on
            if (Physics.Raycast(transform.position, Vector3.down, out var raycastHit, 1f, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == raycastHit.collider && (hit.collider.attachedRigidbody == null || hit.collider.attachedRigidbody.isKinematic || hit.collider.attachedRigidbody.constraints.HasFlag(RigidbodyConstraints.FreezePosition)))
                {
                    //Debug.Log(raycastHit.collider.name);
                    LastGroundNormal = hit.normal;
                    return; //should we do this?
                }
            }

            //special case: we just smashed our head into something
            float playerYMax = CharController.bounds.center.y + CharController.bounds.extents.y;
            if (hit.point.y >= playerYMax)
            {
                Velocity += getVerticalBrakeBounceVector();
                return;
            }

            if (!EnableCollisions || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoPhysics))
                return;

            //Debug.Log($"Player hit {hit.collider.gameObject.name} ({hit.collider.GetType()})");

            Rigidbody body = hit.collider.attachedRigidbody;
            
            if(body != null)
            {
                if(body.isKinematic || body.constraints.HasFlag(RigidbodyConstraints.FreezePosition))
                {
                    //unmovable rigidbody; brake-slide behavior
                    Velocity += getBrakeVector();
                }
                else
                {
                    //holy hell this needs tweaking TODO
                    //movable rigidbody; push-bounce behavior
                    float momentum = Velocity.magnitude * CollisionMass;
                    float playerMomentum = momentum * (CollisionRigidbodyBounceRatio);
                    Velocity = Velocity.normalized * (playerMomentum / momentum);

                    float objectMomentum = momentum * (1f - CollisionRigidbodyBounceRatio);
                    body.AddForce(Velocity.normalized * (objectMomentum / body.mass), ForceMode.VelocityChange);
                }
            }
            else
            {
                //not a rigidbody; brake-slide behavior
                Velocity += getBrakeVector();
            }

            /*
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3F)
                return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.velocity = pushDir * PushFactor;
            */

            Vector3 getBrakeVector()
            {
                Vector3 vecFlatVelocity = new Vector3(Velocity.x, 0, Velocity.z);
                float magBrake = CollisionBrakeAcceleration * Time.deltaTime;
                Vector3 vecBrake = -vecFlatVelocity.normalized * Mathf.Min(magBrake, vecFlatVelocity.magnitude);
                return vecBrake;                
            }

            Vector3 getVerticalBrakeBounceVector()
            {
                if (Velocity.y > 0)
                    return new Vector3(0, -Velocity.y, 0);

                return Vector3.zero;
            }
        }

        /// <summary>
        /// Handle noclip state and movement
        /// </summary>
        protected void HandleNoclip()
        {
            if(!Clipping && !(MetaState.Instance.SessionFlags.Contains("NoClip") || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoClip)))
            {
                exitNoclipMode();
                return;
            }
            else if(Clipping && (MetaState.Instance.SessionFlags.Contains("NoClip") || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoClip)))
            {
                enterNoclipMode();
                return;
            }

            if(!Clipping)
                doNoclipMovement();

            void enterNoclipMode()
            {
                Clipping = false;

                IsMoving = false;
                IsRunning = false;
                IsGrounded = false;
                IsOnSlope = false;                
                
                IsCrouching = false;
                SetCrouchState();

                CharController.enabled = false;
                CharController.detectCollisions = false;
            }

            void exitNoclipMode()
            {
                Clipping = true;

                CharController.enabled = true;
                CharController.detectCollisions = true;

                Velocity = Vector3.zero;
            }

            void doNoclipMovement()
            {
                if (!PlayerController.PlayerInControl || LockPauseModule.IsInputLocked() || GameState.Instance.PlayerFlags.Contains(PlayerFlags.Frozen) || GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen))
                    return;

                Vector3 moveVector = Vector3.zero;
                Vector3 velocity = MappedInput.GetButton(DefaultControls.Sprint) ? NoclipFastVelocity : NoclipVelocity;

                if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > InputDeadzone)
                {
                    moveVector += (PlayerController.CameraRoot.transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * velocity.z * Time.deltaTime);
                }

                if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > InputDeadzone)
                {
                    moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * velocity.x * Time.deltaTime);
                }

                float moveY = MappedInput.GetAxis(DefaultControls.Jump) - MappedInput.GetAxis(DefaultControls.Crouch);
                if (Mathf.Abs(moveY) > InputDeadzone)
                {
                    moveVector += (Vector3.up * moveY * velocity.y * Time.deltaTime);
                }

                if (moveVector.magnitude > 0)
                    transform.Translate(moveVector, Space.World);

            }

        }

        protected void HandleDynamicMovement()
        {
            if (GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoPhysics)) //probably buggy
                return;

            float lastYVelocity = Velocity.y; //last y-velocity, ignoring gravity
            Velocity += Physics.gravity * GravityMultiplier * Time.deltaTime;

            CharController.Move(Velocity * Time.deltaTime);

            IsGrounded = CharController.isGrounded;
            IsOnSlope = Vector3.Angle(Vector3.up, LastGroundNormal) > CharController.slopeLimit;

            if (IsGrounded && !DidJump)
            {
                //float yDamp = GroundedDamping * Time.deltaTime;
                //yDamp = Mathf.Min(yDamp, Mathf.Abs(Velocity.y));
                //yDamp = yDamp * -Mathf.Sign(Velocity.y);
                //Velocity += new Vector3(0, yDamp, 0);

                if (lastYVelocity < -FallMinDetectVelocity && TimeInAir > FallMinDetectTime)
                {
                    FallImpactSound.Ref()?.Play();

                    if (lastYVelocity < -FallMinGruntVelocity)
                        FallPainSound.Ref()?.Play();

                    if(GameParams.UseFallDamage && !MetaState.Instance.SessionFlags.Contains("GodMode") && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoFallDamage) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.Invulnerable)) 
                    {
                        GameState.Instance.PlayerRpgState.Health -= RpgValues.FallDamage(GameState.Instance.PlayerRpgState, new Vector3(Velocity.x, lastYVelocity, Velocity.z));
                    }

                    //Debug.Log($"Player fell (y-velocity = {lastYVelocity:F4} m/s, time in air = {TimeInAir:F2} s)");
                }

                Velocity = new Vector3(Velocity.x, 0, Velocity.z);
                TimeInAir = 0;
            }

            if(IsGrounded && IsOnSlope)
            {
                //Debug.Log("on slope");
                //slope slide
                Vector3 slopeDir = new Vector3(LastGroundNormal.x, 1f - LastGroundNormal.y, LastGroundNormal.z).normalized;
                Vector3 slopeVelocityAdd = new Vector3(slopeDir.x, 0, slopeDir.z) * SlopeSlideAccleration * Time.deltaTime;
                Vector3 flatVelocity = new Vector3(Velocity.x, 0, Velocity.z);
                slopeVelocityAdd = slopeVelocityAdd.normalized * Mathf.Min(slopeVelocityAdd.magnitude, Mathf.Max(0, SlopeSlideSpeed - flatVelocity.magnitude));
                Velocity += slopeVelocityAdd;
            }
            else if(IsGrounded && !IsMoving)
            {
                //ground friction
                Velocity += new Vector3(-Velocity.x, 0, -Velocity.z).normalized * GroundFriction * Time.deltaTime;
            }
            else
            {
                //air resistance
                Velocity += (-Velocity).normalized * Mathf.Min(AirResistance * Time.deltaTime, Velocity.magnitude);
                TimeInAir += Time.deltaTime;
            }
        }

        protected void HandleLook()
        {
            const float deadzone = 0.1f; //this really shouldn't be here
            const float lmul = 180f; //mostly logical look multiplier   

            if (GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen))
                return;

            //looking is the same as long as we're in control
            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) != 0)
            {
                transform.Rotate(Vector3.up, lmul * ConfigState.Instance.LookSpeed * MappedInput.GetAxis(DefaultControls.LookX) * Time.deltaTime);
                if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) > deadzone && CountRotateAsMovement)
                {
                    IsMoving = true;
                }
                
            }

            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookY)) != 0)
            {
                int lookYInvert = ConfigState.Instance.LookInvert ? -1 : 1;

                //this is probably the worst clamp code ever written

                Vector3 localForward = PlayerController.CameraRoot.parent.transform.InverseTransformDirection(PlayerController.CameraRoot.transform.forward);
                float originalAngle = Vector2.SignedAngle(Vector2.right, localForward.GetSideVector());
                float deltaAngle = lmul * ConfigState.Instance.LookSpeed * lookYInvert * MappedInput.GetAxis(DefaultControls.LookY) * Time.deltaTime; //this is okay if weird

                if(deltaAngle > 0 && originalAngle + deltaAngle >= LookYLimit)
                {
                    //clamp high
                    deltaAngle = LookYLimit - originalAngle;
                }
                else if(deltaAngle < 0 && originalAngle + deltaAngle <= -LookYLimit)
                {
                    //clamp low
                    deltaAngle = -LookYLimit - originalAngle;
                }

                PlayerController.CameraRoot.transform.Rotate(Vector3.left, deltaAngle);

                //Debug.Log($"{originalAngle} + {deltaAngle}");
                
            }

        }

        protected void HandleMovement()
        {

            var playerModel = GameState.Instance.PlayerRpgState;
                        
            if (!GameState.Instance.PlayerFlags.Contains(PlayerFlags.Frozen) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen))
            {

                //handle running
                float energyToRun = RpgValues.GetRunEnergyRate(playerModel);

                IsRunning = MappedInput.GetButton(DefaultControls.Sprint);

                if (RunWasBlocked && IsRunning)
                    IsRunning = false;
                else if (RunWasBlocked && !IsRunning)
                    RunWasBlocked = false;

                if (IsRunning && playerModel.Energy < energyToRun)
                {
                    IsRunning = false;
                    RunWasBlocked = true;
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgInsufficientEnergy"));
                }


                //TODO check against energy requirements

                //request an exit from ADS
                if (IsRunning && PlayerController.WeaponComponent != null)
                {
                    PlayerController.WeaponComponent.RequestADSExit();
                }

                //handle crouching
                if (MappedInput.GetButtonDown(DefaultControls.Crouch) && !IsRunning)
                {
                    IsCrouching = !IsCrouching;
                    DidChangeCrouch = true;
                    SetCrouchState();
                }

                //uncrouch if we try to sprint
                if (IsRunning && IsCrouching)
                {
                    IsCrouching = false;
                    DidChangeCrouch = true;
                    SetCrouchState();
                }

                if (IsGrounded)
                {
                    //normal x/y movement

                    var flatVelocity = new Vector3(Velocity.x, 0, Velocity.z);

                    Vector3 moveVector = Vector3.zero;

                    float maxAcceleration = IsCrouching ? MaxCrouchAcceleration : (IsRunning ? MaxSprintAcceleration : MaxWalkAcceleration);
                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > InputDeadzone)
                    {
                        moveVector += (transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * maxAcceleration * Time.deltaTime);
                        IsMoving = true;
                    }

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > InputDeadzone)
                    {
                        moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * maxAcceleration * Time.deltaTime);
                        IsMoving = true;
                    }

                    if (Mathf.Approximately(moveVector.magnitude, 0) && !IsOnSlope)
                    {
                        moveVector = -flatVelocity.normalized * Mathf.Min(MaxBrakeAcceleration * Time.deltaTime, flatVelocity.magnitude);
                    }

                    //clamp velocity to maxwalk/maxrun/etc
                    float maxSpeed = IsCrouching ? MaxCrouchSpeed * RpgValues.GetMoveSpeedMultiplier(playerModel) : (IsRunning ? MaxSprintSpeed * RpgValues.GetRunSpeedMultiplier(playerModel) : MaxWalkSpeed * RpgValues.GetMoveSpeedMultiplier(playerModel));
                    maxSpeed *= ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
                    var newFlatVelocity = new Vector3(Velocity.x, 0, Velocity.z) + new Vector3(moveVector.x, 0, moveVector.z);
                    if (newFlatVelocity.magnitude > maxSpeed)
                    {
                        newFlatVelocity = newFlatVelocity.normalized * maxSpeed; //this actually doesn't make a ton of physical sense but it does seem to work
                    }

                    Velocity = new Vector3(newFlatVelocity.x, Velocity.y, newFlatVelocity.z);

                    if (IsRunning)
                        playerModel.Energy -= energyToRun;
                }
                else
                {
                    //air move: component wise, clamped

                    //awkward bullshit to go from world to player space
                    Vector3 refVelocity = Quaternion.AngleAxis(-transform.eulerAngles.y, Vector3.up) * Velocity;
                    Vector3 newAddVelocity = Vector3.zero;

                    float multiplier = ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
                    multiplier *= RpgValues.GetAirMoveMultiplier(playerModel);

                    float maxSpeedScaled = MaxAirSpeed * multiplier;

                    float moveZ = MappedInput.GetAxis(DefaultControls.MoveY) * MaxAirAcceleration * multiplier * Time.deltaTime;
                    if (Mathf.Abs(refVelocity.z) < maxSpeedScaled || Mathf.Sign(moveZ) != Mathf.Sign(refVelocity.z))
                        newAddVelocity += new Vector3(0, 0, moveZ);

                    float moveX = MappedInput.GetAxis(DefaultControls.MoveX) * MaxAirAcceleration * multiplier * Time.deltaTime;
                    if (Mathf.Abs(refVelocity.x) < maxSpeedScaled || Mathf.Sign(moveX) != Mathf.Sign(refVelocity.x))
                        newAddVelocity += new Vector3(moveX, 0, 0);

                    Velocity += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * newAddVelocity;
                }

                if (IsGrounded && (AllowSlopeJumping || !IsOnSlope) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoJump))
                {
                    //jumping
                    if (MappedInput.GetButtonDown(DefaultControls.Jump))
                    {

                        var jumpVelocity = JumpInstantaneousVelocity * RpgValues.GetJumpVelocityMultiplier(playerModel) * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerAgility;
                        float jumpEnergyUse = RpgValues.GetJumpEnergyUse(playerModel);

                        if (playerModel.Energy >= jumpEnergyUse)
                        {
                            playerModel.Energy -= jumpEnergyUse;
                            bool wasCrouched = IsCrouching;

                            //uncrouch if we were crouched
                            if (wasCrouched)
                            {
                                IsCrouching = false;
                                DidChangeCrouch = true;
                                SetCrouchState();
                                jumpVelocity += JumpCrouchBoostVelocity;
                            }

                            Velocity += Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * jumpVelocity;
                            CharController.Move(Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * JumpInstantaneousDisplacement);
                            JumpSound.Ref()?.Play();
                            DidJump = true;
                        }
                        else
                        {
                            //failed to jump
                            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgInsufficientEnergy"));
                        }
                    }

                }
            }

            //energy recovery
            if(!IsRunning && IsGrounded && !DidJump)
            {
                float energyGain = (IsMoving ? RpgValues.GetMovingEnergyRecoveryRate(playerModel) : RpgValues.GetIdleEnergyRecoveryRate(playerModel)) * Time.deltaTime;
                playerModel.Energy = Mathf.Min(playerModel.DerivedStats.MaxEnergy, playerModel.Energy + energyGain);
            }


        }  
        
        private void HandleOutOfBounds()
        {
            if (!TeleportIfOutOfBounds)
                return;

            var sceneController = SharedUtils.TryGetSceneController() as WorldSceneController;
            if(sceneController != null)
            {
                if(!sceneController.WorldBounds.Contains(transform.position))
                {
                    Debug.LogWarning("Player out of bounds, teleporting!");

                    Vector3 teleportPoint = sceneController.WorldBounds.center + new Vector3(0, 2f, 0);
                    GameObject spawnPoint = WorldUtils.FindPlayerSpawn();
                    if (spawnPoint != null)
                    {
                        teleportPoint = spawnPoint.transform.position;
                    }
                    transform.position = teleportPoint;
                    Velocity = Vector3.zero;
                }
            }
        }

        private void HandleAnimation()
        {
            //for now, don't bother

            /*

            //handle animation (an absolute fucking shitshow here)
            if (DidChangeCrouch && !UseCrouchHack)
            {
                IsAnimating = !IsMoving;
            }

            if (IsMoving)
            {
                if (!IsAnimating)
                {

                    //ac.Play("Run_Rifle_Foreward", 0);
                    if (IsCrouching && !UseCrouchHack)
                        AnimController.CrossFade("crouch_move", 0f);
                    else
                        AnimController.CrossFade("run", 0f);
                    IsAnimating = true;
                    //stepSound.Play();
                    if (PlayerController.RightViewModel != null)
                        PlayerController.RightViewModel.SetState(ViewModelState.Moving);
                    if (PlayerController.LeftViewModel != null)
                        PlayerController.LeftViewModel.SetState(ViewModelState.Moving);
                }
            }
            else
            {
                if (IsAnimating)
                {

                    //ac.Stop();
                    if (IsCrouching && !UseCrouchHack)
                        AnimController.CrossFade("crouch_idle", 0f);
                    else
                        AnimController.CrossFade("idle", 0f);
                    IsAnimating = false;
                    //stepSound.Stop();
                    if (PlayerController.RightViewModel != null)
                        PlayerController.RightViewModel.SetState(ViewModelState.Fixed);
                    if (PlayerController.LeftViewModel != null)
                        PlayerController.LeftViewModel.SetState(ViewModelState.Fixed);
                }
            }

            */
        }

        private void HandleSounds()
        {
            if (WalkSound == null || RunSound == null)
                return;

            //we only need to handle movement sounds not "event" sounds like jumping and landing
            bool isStepping = IsGrounded && !DidJump && IsMoving;
            //bool soundPlaying = WalkSound.isPlaying;

            if(isStepping)
            {
                if (!IsRunning) //if not running (ie walking)
                {
                    if (!RunSound.isPlaying) //if the run sound is not playing
                    {
                        if (!WalkSound.isPlaying) //start/loop the walk sound
                        {
                            WalkSound.Play();
                        }

                        WalkSound.loop = true;
                    }

                    RunSound.loop = false; //soft-stop the run sound
                }
                else if(IsRunning) //if running
                {
                    if(!WalkSound.isPlaying) //if the walk sound is not playing
                    {
                        if(!RunSound.isPlaying) //start/loop the run sound
                        {
                            RunSound.Play();
                        }

                        RunSound.loop = true;
                    }

                    WalkSound.loop = false; //soft-stop the walk sound
                }
            }
            else //if not moving, soft-stop both sounds
            {
                WalkSound.loop = false;
                RunSound.loop = false;
            }

        }

        /// <summary>
        /// Animates the player camera going up/down for crouching
        /// </summary>
        private void HandleCrouchAnimation()
        {
            if (!CrouchTargetHeight.HasValue)
                return;

            Transform cameraRoot = PlayerController.CameraRoot;
            float currentHeight = cameraRoot.localPosition.y;

            if (Mathf.Approximately(currentHeight, CrouchTargetHeight.Value))
                return;

            //animate
            float direction = Mathf.Sign(CrouchTargetHeight.Value - currentHeight);
            float newHeight;
            if (direction > 0)
                newHeight = Mathf.Min(CrouchTargetHeight.Value, currentHeight + CrouchAnimateRate * Time.deltaTime);
            else
                newHeight = Mathf.Max(CrouchTargetHeight.Value, currentHeight - CrouchAnimateRate * Time.deltaTime);
            
            cameraRoot.localPosition = new Vector3(cameraRoot.localPosition.x, newHeight, cameraRoot.localPosition.z);
        }

        /// <summary>
        /// Sets the crouch state of the player based on IsCrouching
        /// </summary>
        private void SetCrouchState(bool instant = false)
        {

            if (!CharControllerOriginalHeight.HasValue)
            {
                SetBaseScaleVars();
            }

            if (IsCrouching && CharControllerOriginalHeight.HasValue)
            {
                //set character controller, hitbox, camera root position
                CharController.height = CharControllerOriginalHeight.Value * CrouchYScale;
                CharController.center = new Vector3(CharController.center.x, CharController.height / 2f, CharController.center.z);
                Hitbox.height = HitboxOriginalHeight.Value * CrouchYScale;
                Hitbox.center = new Vector3(Hitbox.center.x, Hitbox.height / 2f, Hitbox.center.z);

                //PlayerController.CameraRoot.localPosition = new Vector3(CameraRootOriginalLPos.Value.x, CameraRootOriginalLPos.Value.y * CrouchYScale, CameraRootOriginalLPos.Value.z);
                CrouchTargetHeight = CameraRootOriginalLPos.Value.y * CrouchYScale;
                if (instant)
                    setCameraHeight();
                CrouchSound.Ref()?.Play();

                if (UseCrouchHack)
                {
                    PlayerController.ModelRoot.transform.localScale = Vector3.Scale(ModelOriginalScale.Value, new Vector3(1f, 0.66f, 1f));
                }
            }
            else if (CharControllerOriginalHeight.HasValue)
            {
                //restore character controller, hitbox, camera root position
                CharController.height = CharControllerOriginalHeight.Value;
                CharController.center = new Vector3(CharController.center.x, CharControllerOriginalYPos.Value, CharController.center.z);
                Hitbox.height = HitboxOriginalHeight.Value;
                Hitbox.center = new Vector3(Hitbox.center.x, HitboxOriginalYPos.Value, Hitbox.center.z);

                //PlayerController.CameraRoot.localPosition = CameraRootOriginalLPos.Value;
                CrouchTargetHeight = CameraRootOriginalLPos.Value.y;
                if (instant)
                    setCameraHeight();
                CrouchSound.Ref()?.Play();

                if (UseCrouchHack)
                {
                    PlayerController.ModelRoot.transform.localScale = ModelOriginalScale.Value;
                }
            }

            void setCameraHeight()
            {
                Transform cameraRoot = PlayerController.CameraRoot;
                cameraRoot.localPosition = new Vector3(cameraRoot.localPosition.x, CrouchTargetHeight.Value, cameraRoot.localPosition.z);
            }
        }

        public void SetBaseScaleVars()
        {
            ModelOriginalScale = PlayerController.ModelRoot.transform.localScale;
            CharControllerOriginalHeight = CharController.height;
            CharControllerOriginalYPos = CharController.center.y;
            HitboxOriginalHeight = ((CapsuleCollider)Hitbox).height;
            HitboxOriginalYPos = ((CapsuleCollider)Hitbox).center.y;
            CameraRootOriginalLPos = PlayerController.CameraRoot.localPosition;
        }

        /// <summary>
        /// Give the player a push!
        /// </summary>
        /// <param name="instantaneousVelocity"></param>
        /// <param name="instantaneousDisplacement"></param>
        public void Push(Vector3 instantaneousVelocity, Vector3 instantaneousDisplacement)
        {
            //Debug.Log($"Player pushed ({instantaneousVelocity}|{instantaneousDisplacement})");

            if (GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoPhysics))
                return;

            Velocity += instantaneousVelocity;
            CharController.Move(instantaneousDisplacement);
        }

        /// <summary>
        /// Sets the velocity of the player
        /// </summary>
        public void SetVelocity(Vector3 velocity)
        {
            //we may put more logic here

            Velocity = velocity;
        }

        /// <summary>
        /// Sets the crouch state of the player
        /// </summary>
        public void SetCrouchState(bool crouching, bool instant)
        {
            bool oldState = IsCrouching;
            IsCrouching = crouching;
            if (oldState != IsCrouching)
                SetCrouchState(instant);
        }




    }


}