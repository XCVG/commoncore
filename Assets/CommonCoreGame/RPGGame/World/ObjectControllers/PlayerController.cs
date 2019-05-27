using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonCore.Input;
using CommonCore.UI;
using CommonCore.LockPause;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Messaging;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.UI;

namespace CommonCore.RpgGame.World
{
    public class PlayerController : BaseController, ITakeDamage
    {
        public bool AutoinitHud = true;

        [Header("Appearance")]
        public bool UseCrouchHack;

        [Header("Movement")]
        public float CrouchYScale = 0.66f;
        public float CrouchMoveScale = 0.5f;

        [Header("Interactivity")]
        public bool PlayerInControl;
        public bool Clipping;
        public float PushFactor;

        private Vector3 AirMoveVelocity;

        public float MaxProbeDist;
        public float MaxUseDist;

        [Header("Components")]
        public RpgHUDController HUDScript;
        public CharacterController CharController;
        public Rigidbody CharRigidbody;
        public Animator AnimController;
        public Transform CameraRoot;
        public GameObject ModelRoot;
        public CapsuleCollider Hitbox;
        public Transform TargetPoint;
        public Transform LeftViewModelPoint;
        public Transform RightViewModelPoint;
        private QdmsMessageInterface MessageInterface;

        [Header("Sounds")]
        public AudioSource WalkSound;
        public AudioSource RunSound;
        public AudioSource FallSound;
        public AudioSource PainSound;
        public AudioSource JumpSound;
        public AudioSource DeathSound;

        [Header("Shooting")]
        public bool AttackEnabled = true;
        public bool AttemptToUseStats = true;
        public GameObject BulletPrefab;
        public GameObject BulletFireEffect;
        public ActorHitInfo BulletHitInfo;
        public float BulletSpeed = 50.0f;
        public ActorHitInfo MeleeHitInfo;
        public float MeleeProbeDist = 1.5f;
        public GameObject MeleeEffect;
        public Transform ShootPoint;
        private ViewModelScript LeftViewModel;
        private ViewModelScript RightViewModel;
        private float TimeToNext;
        private bool IsReloading;

        private bool IsAnimating;
        public bool IsMoving { get; private set; }
        public bool IsRunning { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsGrounded { get; private set; }
        private Renderer[] ModelRendererCache;

        //all this crap is necessary for crouching to work correctly
        private float? CharControllerOriginalHeight;
        private float? CharControllerOriginalYPos;
        private float? HitboxOriginalHeight;
        private float? HitboxOriginalYPos;
        private Vector3? CameraRootOriginalLPos;
        private Vector3? ModelOriginalScale;

        // Use this for initialization
        public override void Start()
        {
            base.Start();

            Debug.Log("Player controller start");

            if(!CharController)
            {
                CharController = GetComponent<CharacterController>();
            }

            if(!CharRigidbody)
            {
                CharRigidbody = GetComponent<Rigidbody>();
            }

            if(!CameraRoot)
            {
                CameraRoot = transform.Find("CameraRoot");
            }

            if(!ModelRoot)
            {
                ModelRoot = transform.GetChild(0).gameObject;
            }

            if(!Hitbox)
            {
                Hitbox = transform.Find("Hitbox").GetComponent<CapsuleCollider>();
            }

            if(!AnimController)
            {
                AnimController = GetComponent<Animator>();
            }

            if(!HUDScript)
            {
                HUDScript = (RpgHUDController)BaseHUDController.Current; //I would not recommend this cast
            }
            
            if(!HUDScript && AutoinitHud)
            {
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultWorldHUD"));
                if (EventSystem.current == null)
                    Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"));

                HUDScript = (RpgHUDController)BaseHUDController.Current;
            }

            MessageInterface = new QdmsMessageInterface(gameObject);

            IsAnimating = false;

            LockPauseModule.CaptureMouse = true;

            SetDefaultPlayerView();
            SetInitialViewModels();
            SetBaseScaleVars();
        }

        //TODO: still unsure about the state system, but I'll likely rewrite this whole class
        //should be fixedupdate
        public override void Update()
        {
            HandleMessages();

            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            if (PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleView();
                HandleMovement();
                HandleInteraction();
                HandleWeapons();
            }
        }

        //handle collider hits (will probably have to rewrite this later)
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic)
                return;

            if (hit.moveDirection.y < -0.3F)
                return;

            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.velocity = pushDir * PushFactor;
        }

        private void SetDefaultPlayerView()
        {
            GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
            GameObject fpCamera = CameraRoot.Find("FP Camera").gameObject;

            switch (GameParams.DefaultPlayerView)
            {
                case PlayerViewType.PreferFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(false);
                    break;
                case PlayerViewType.PreferThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(true);
                    break;
                case PlayerViewType.ForceFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(false);
                    break;
                case PlayerViewType.ForceThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(true);
                    break;
                case PlayerViewType.ExplicitOther:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(false);
                    break;
            }

            PushViewChangeMessage(GameParams.DefaultPlayerView);
        }

        private void SetInitialViewModels()
        {
            HandleWeaponChange(EquipSlot.LeftWeapon);
            HandleWeaponChange(EquipSlot.RightWeapon);
        }

        private void SetBaseScaleVars()
        {
            ModelOriginalScale = ModelRoot.transform.localScale;
            CharControllerOriginalHeight = CharController.height;
            CharControllerOriginalYPos = CharController.center.y;
            HitboxOriginalHeight = ((CapsuleCollider)Hitbox).height;
            HitboxOriginalYPos = ((CapsuleCollider)Hitbox).center.y;
            CameraRootOriginalLPos = CameraRoot.localPosition;
    }

        private void HandleMessages()
        {
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is QdmsFlagMessage)
            {
                string flag = ((QdmsFlagMessage)message).Flag;
                switch (flag)
                {
                    case "RpgChangeWeapon":
                        {
                            var kvm = message as QdmsKeyValueMessage;
                                
                            if(kvm != null && kvm.HasValue<EquipSlot>("Slot"))
                            {
                                HandleWeaponChange(kvm.GetValue<EquipSlot>("Slot"));
                            }
                            else
                            {
                                HandleWeaponChange(EquipSlot.None);
                            }
                                
                        }                        
                        break;
                }
            }
        }

        private void HandleView()
        {
            if (!(GameParams.DefaultPlayerView == PlayerViewType.PreferFirst || GameParams.DefaultPlayerView == PlayerViewType.PreferThird))
                return;

            if(MappedInput.GetButtonDown(DefaultControls.ChangeView)) 
            {
                //slow and stupid but it'll work for now

                GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
                GameObject fpCamera = CameraRoot.Find("FP Camera").gameObject;

                if(tpCamera.activeSelf)
                {
                    fpCamera.SetActive(true);
                    tpCamera.SetActive(false);
                    SetModelVisibility(false);
                    PushViewChangeMessage(PlayerViewType.ForceFirst);
                }
                else
                {
                    fpCamera.SetActive(false);
                    tpCamera.SetActive(true);
                    SetModelVisibility(true);
                    PushViewChangeMessage(PlayerViewType.ForceThird);
                }
            }
        }

        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use
            //TODO handle tooltips

            HUDScript.ClearTarget();

            int layerMask = LayerMask.GetMask("Default","ActorHitbox");

            Debug.DrawRay(CameraRoot.position, CameraRoot.transform.forward * MaxProbeDist);

            //we should actually do RaycastAll, cull based on 2D distance and separate 3D distance, and possibly handle occlusion

            //raycast all, go through the hits ignoring hits to self
            RaycastHit[] hits = Physics.RaycastAll(CameraRoot.transform.position, CameraRoot.transform.forward, MaxProbeDist * 2, layerMask, QueryTriggerInteraction.Collide);            
            if(hits != null && hits.Length > 0)
            {
                //GameObject nearestObject = null;
                InteractableComponent nearestInteractable = null;
                float nearestDist = float.MaxValue;
                foreach(RaycastHit hit in hits)
                {
                    //skip if it's further than nearestDist (occluded) or flatdist is further than MaxProbeDist (too far away)
                    if (hit.distance > nearestDist)
                        continue;

                    float fDist = CoreUtils.GetFlatVectorToTarget(transform.position, hit.point).magnitude;
                    if (fDist > MaxProbeDist)
                        continue;

                    //nearestObject = hit.collider.gameObject;

                    //if there's a PlayerController attached, we've hit ourselves
                    if (hit.collider.GetComponent<PlayerController>() != null)
                        continue;

                    //TODO pull a similar trick to see if we're pointing at an Actor?

                    //get the interactable component and hitbox component; if it doesn't have either then it's an obstacle
                    InteractableComponent ic = hit.collider.GetComponent<InteractableComponent>();
                    ActorHitboxComponent ahc = hit.collider.GetComponent<ActorHitboxComponent>();
                    if (ic == null && ahc == null)
                    {
                        //we null out our hit first since it's occluded by this one                        
                        nearestInteractable = null;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //it's just us lol
                    if (ahc != null && ahc.ParentController is PlayerController)
                        continue;                    
                    
                    //we have an interactablecomponent and we're not occluded
                    if(ic != null)
                    {
                        nearestInteractable = ic;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //if it doesn't meet any of those criteria then it's an occluder
                    nearestInteractable = null;
                    nearestDist = hit.distance;

                }

                //if(nearestObject != null)
                //    Debug.Log("Nearest: " + nearestObject.name);

                if (nearestInteractable != null && nearestInteractable.enabled)
                {
                    //Debug.Log("Detected: " + ic.Tooltip);
                    
                    HUDScript.SetTargetMessage(nearestInteractable.Tooltip);

                    //actual use
                    if (MappedInput.GetButtonDown(DefaultControls.Use))
                    {
                        nearestInteractable.OnActivate(this.gameObject);
                    }
                }
            }

        }

        private void SetModelVisibility(bool visible)
        {
            //fill renderer cache if empty
            if(ModelRendererCache == null || ModelRendererCache.Length == 0)
            {
                ModelRendererCache = ModelRoot.GetComponentsInChildren<Renderer>(true);
            }

            foreach(var r in ModelRendererCache)
            {
                if (visible)
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            if (RightViewModel != null)
                RightViewModel.SetVisibility(!visible);

            if (LeftViewModel != null)
                LeftViewModel.SetVisibility(!visible);
        }

        private void PushViewChangeMessage(PlayerViewType newView)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ViewType"] = newView;
            QdmsKeyValueMessage msg = new QdmsKeyValueMessage(dict, "PlayerChangeView");
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }
        

        //TODO handle crouching and fall damage
        protected void HandleMovement()
        {
            //really need to do something about these values
            IsMoving = false;
            IsRunning = false;
            IsGrounded = false;
            bool didJump = false;
            bool didChangeCrouch = false;
            float deadzone = 0.1f; //this really shouldn't be here
            float vmul = 7.5f; //mysterious magic number velocity multiplier
            float amul = 5.0f; //air move multiplier
            float lmul = 180f; //mostly logical look multiplier

            var playerState = GameState.Instance.PlayerRpgState;

            //looking is the same as long as we're in control
            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) != 0)
            {
                transform.Rotate(Vector3.up, lmul * ConfigState.Instance.LookSpeed * MappedInput.GetAxis(DefaultControls.LookX) * Time.deltaTime);
                if(Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) > deadzone)
                    IsMoving = true;
            }

            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookY)) != 0)
            {
                CameraRoot.transform.Rotate(Vector3.left, lmul * ConfigState.Instance.LookSpeed * MappedInput.GetAxis(DefaultControls.LookY) * Time.deltaTime);
            }

            //toggle crouch for now
            if(MappedInput.GetButtonDown(DefaultControls.Crouch))
            {
                IsCrouching = !IsCrouching;
                didChangeCrouch = true;
            }

            if (!Clipping)
            {
                //noclip mode: disable controller, kinematic rigidbody, use transform only
                CharController.enabled = false;
                CharRigidbody.isKinematic = true;

                Vector3 moveVector = Vector3.zero;

                if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
                {
                    moveVector += (CameraRoot.transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * vmul * Time.deltaTime);
                }

                if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
                {
                    moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * vmul * Time.deltaTime);
                }

                if (MappedInput.GetButton(DefaultControls.Sprint))
                {
                    moveVector *= 5.0f;
                    IsRunning = true;
                }
                    

                transform.Translate(moveVector, Space.World);
            }
            else
            {

                Vector3 moveVector = Vector3.zero;

                CharController.enabled = true;
                CharRigidbody.isKinematic = true;


                IsGrounded = CharController.isGrounded;
                if (IsGrounded)
                {
                    //grounded: controller enabled, kinematic rigidbody, use controller movement
                    
                    AirMoveVelocity = Vector3.zero;

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
                    {
                        moveVector += (transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * vmul * Time.deltaTime);
                        IsMoving = true;
                    }

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
                    {
                        moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * vmul * Time.deltaTime);
                        IsMoving = true;
                    }

                    //hacky sprinting
                    if(MappedInput.GetButton(DefaultControls.Sprint) && playerState.Energy > 0)
                    {
                        //TODO: this seems to be broken, and I don't know why     
                        IsRunning = true;
                        moveVector *= 2.0f;
                        playerState.Energy -= 10.0f * Time.deltaTime;
                    }
                    else if(!MappedInput.GetButton(DefaultControls.Sprint))
                    {
                        //oh, I think what happens is we recover one frame, then sprinting immediately kicks in the next
                        //need to hold button state and/or implement cooldown
                        playerState.Energy += 5.0f * Time.deltaTime;
                    }

                    //crouched movement
                    if(IsCrouching)
                    {
                        moveVector *= CrouchMoveScale;
                    }

                    if(moveVector.magnitude == 0)
                    {
                        playerState.Energy += 5.0f * Time.deltaTime;
                    }

                    playerState.Energy = Mathf.Min(playerState.Energy, playerState.DerivedStats.MaxEnergy);

                    //jump
                    if (MappedInput.GetButtonDown(DefaultControls.Jump))
                    {
                        AirMoveVelocity = (moveVector * 10.0f) + (transform.forward * 5.0f) + (transform.up * 7.5f); 

                        moveVector += (transform.forward * 0.1f) + (transform.up * 0.5f);

                        if (MappedInput.GetButton(DefaultControls.Sprint) && playerState.Energy > 0)
                            AirMoveVelocity += transform.forward * 1.0f;

                        IsMoving = true;
                        IsCrouching = false;
                        didJump = true;
                    }

                    moveVector += 0.6f * Physics.gravity * Time.deltaTime;

                }
                else
                {
                    //flying: controller enabled, non-kinematic rigidbody, use rigidbody movement
                    //except that didn't work...
                    //CharController.enabled = true;
                    //CharRigidbody.isKinematic = false;                    

                    IsCrouching = false; //because that would make little sense

                    AirMoveVelocity.x *= (0.9f * Time.deltaTime);
                    AirMoveVelocity.z *= (0.9f * Time.deltaTime);
                    AirMoveVelocity += Physics.gravity * 2.0f * Time.deltaTime;

                    moveVector += AirMoveVelocity * Time.deltaTime;

                    //air control
                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
                    {
                        moveVector += (transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * amul * Time.deltaTime);
                        IsMoving = true;
                    }

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
                    {
                        moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * amul * Time.deltaTime);
                        IsMoving = true;
                    }
                }

                //"gravity"
                //moveVector += 0.6f * Physics.gravity * Time.deltaTime; //fuck me! this is NOT how to do physics!

                CharController.Move(moveVector);
            }

            //TODO handle crouch collision changes
            //TODO figure out how to not set everything every time, only when there has been a *change*

            if(!CharControllerOriginalHeight.HasValue)
            {
                SetBaseScaleVars();
            }

            if(IsCrouching && CharControllerOriginalHeight.HasValue)
            {
                //set character controller, hitbox, camera root position
                CharController.height = CharControllerOriginalHeight.Value * CrouchYScale;
                CharController.center = new Vector3(CharController.center.x, CharController.height / 2f, CharController.center.z);
                Hitbox.height = HitboxOriginalHeight.Value * CrouchYScale;
                Hitbox.center = new Vector3(Hitbox.center.x, Hitbox.height / 2f, Hitbox.center.z);
                CameraRoot.localPosition = new Vector3(CameraRootOriginalLPos.Value.x, CameraRootOriginalLPos.Value.y * CrouchYScale, CameraRootOriginalLPos.Value.z);

                if (UseCrouchHack)
                {
                    ModelRoot.transform.localScale = Vector3.Scale(ModelOriginalScale.Value, new Vector3(1f, 0.66f, 1f));
                }
            }
            else if(CharControllerOriginalHeight.HasValue)
            {
                //restore character controller, hitbox, camera root position
                CharController.height = CharControllerOriginalHeight.Value;
                CharController.center = new Vector3(CharController.center.x, CharControllerOriginalYPos.Value, CharController.center.z);
                Hitbox.height = HitboxOriginalHeight.Value;
                Hitbox.center = new Vector3(Hitbox.center.x, HitboxOriginalYPos.Value, Hitbox.center.z);
                CameraRoot.localPosition = CameraRootOriginalLPos.Value;

                if (UseCrouchHack)
                {
                    ModelRoot.transform.localScale = ModelOriginalScale.Value;
                }
            }

            //handle animation (an absolute fucking shitshow here)
            if(didChangeCrouch && !UseCrouchHack)
            {
                IsAnimating = !IsMoving;
            }

            if (IsMoving)
            {
                if (!IsAnimating)
                {

                    //ac.Play("Run_Rifle_Foreward", 0);
                    if(IsCrouching && !UseCrouchHack)
                        AnimController.CrossFade("crouch_move", 0f);
                    else
                        AnimController.CrossFade("run", 0f);
                    IsAnimating = true;
                    //stepSound.Play();
                    if (RightViewModel != null)
                        RightViewModel.SetState(ViewModelState.Moving);
                    if (LeftViewModel != null)
                        LeftViewModel.SetState(ViewModelState.Moving);
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
                    if (RightViewModel != null)
                        RightViewModel.SetState(ViewModelState.Fixed);
                    if (LeftViewModel != null)
                        LeftViewModel.SetState(ViewModelState.Fixed);
                }
            }

            //handle sound
            if(IsGrounded && !didJump)
            {
                if(IsMoving)
                {
                    if (IsRunning && RunSound != null && !RunSound.isPlaying)
                        RunSound.Play();
                    else if (WalkSound != null && !WalkSound.isPlaying)
                        WalkSound.Play();
                }
                else
                {
                    if (WalkSound != null)
                        WalkSound.Pause();

                    if (RunSound != null)
                        RunSound.Pause();
                }
            }
            else
            {
                if (WalkSound != null)
                    WalkSound.Pause();

                if (RunSound != null)
                    RunSound.Pause();

                if (didJump && JumpSound != null)
                    JumpSound.Play();
            }


        }

        //handle weapons (very temporary)
        protected void HandleWeapons()
        {
            float oldTTN = TimeToNext;
            TimeToNext -= Time.deltaTime;
            if (TimeToNext > 0)
                return;

            //TODO reset reload time on weapon change, probably going to need to add messaging for that

            if(oldTTN > 0)
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReady"));

                if (RightViewModel != null)
                    RightViewModel.SetState(IsMoving ? ViewModelState.Moving : ViewModelState.Fixed);
                if (LeftViewModel != null)
                    LeftViewModel.SetState(IsMoving ? ViewModelState.Moving : ViewModelState.Fixed);
            }
                

            if(IsReloading)
            {
                FinishReload();
            }

            //Logic: 
            // -if there is only one weapon equipped, that weapon uses primary fire
            // -if one weapon is ranged and the other melee, the ranged is primary fire
            // -if both weapons are ranged or both melee, primary is the left weapon and secondary the right weapon
            //TBH this code is PFA
            if(AttackEnabled)
            {
                if (MappedInput.GetButtonDown(DefaultControls.Fire))
                {
                    bool leftEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.LeftWeapon);
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RightWeapon);
                    if (leftEquipped && rightEquipped)
                    {                        
                        var leftWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;

                        //if only one weapon is ranged, fire that one
                        if (leftWeaponModel is RangedWeaponItemModel && !(rightWeaponModel is RangedWeaponItemModel))
                            DoRangedAttack(EquipSlot.LeftWeapon);
                        else if (rightWeaponModel is RangedWeaponItemModel && !(leftWeaponModel is RangedWeaponItemModel))
                            DoRangedAttack(EquipSlot.RightWeapon);
                        //otherwise, fire the left weapon
                        else
                        {
                            if (leftWeaponModel is RangedWeaponItemModel)
                                DoRangedAttack(EquipSlot.LeftWeapon);
                            else
                                DoMeleeAttack(EquipSlot.LeftWeapon);
                        }
                        
                    }
                    else if(leftEquipped)
                    {
                        //fire left weapon
                        var weaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        if (weaponModel is RangedWeaponItemModel)
                            DoRangedAttack(EquipSlot.LeftWeapon);
                        else
                            DoMeleeAttack(EquipSlot.LeftWeapon);
                    }
                    else if(rightEquipped)
                    {
                        //fire right weapon
                        var weaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;
                        if (weaponModel is RangedWeaponItemModel)
                            DoRangedAttack(EquipSlot.RightWeapon);
                        else
                            DoMeleeAttack(EquipSlot.RightWeapon);
                    }                    
                }
                else if (MappedInput.GetButtonDown(DefaultControls.AltFire))
                {
                    bool leftEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.LeftWeapon);
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RightWeapon);
                    if (leftEquipped && rightEquipped)
                    {
                        var leftWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;

                        //if only one weapon is ranged, fire the other one
                        if (leftWeaponModel is RangedWeaponItemModel && !(rightWeaponModel is RangedWeaponItemModel))
                            DoMeleeAttack(EquipSlot.RightWeapon);
                        else if (rightWeaponModel is RangedWeaponItemModel && !(leftWeaponModel is RangedWeaponItemModel))
                            DoMeleeAttack(EquipSlot.LeftWeapon);
                        //otherwise, fire the right weapon
                        else
                        {
                            if (rightWeaponModel is RangedWeaponItemModel)
                                DoRangedAttack(EquipSlot.RightWeapon);
                            else
                                DoMeleeAttack(EquipSlot.RightWeapon);
                        }
                    }

                    //TODO eventually altfire
                }
                else if (MappedInput.GetButtonDown(DefaultControls.Reload))
                {
                    DoReload();
                }
                
            }

        }

        private void DoMeleeAttack(EquipSlot slot)
        {

            Debug.Log($"MeleeAttack {slot}");

            //punch
            LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");
            var rc = Physics.RaycastAll(ShootPoint.position, ShootPoint.forward, MeleeProbeDist, lm, QueryTriggerInteraction.Collide);

            //TODO handle 2D/3D probe distance

            //totally fucked!
            ITakeDamage ac = null;
            foreach (var r in rc)
            {
                var go = r.collider.gameObject;
                var ahgo = go.GetComponent<ActorHitboxComponent>();
                if (ahgo != null)
                {
                    ac = ahgo.ParentController as ITakeDamage; //this works as long as we don't go MP or do Voodoo Dolls
                    break;
                }
                var acgo = go.GetComponent<ActorController>();
                if (acgo != null)
                {
                    ac = acgo;
                    break;
                }
            }

            ActorHitInfo hitInfo = MeleeHitInfo;
            if (AttemptToUseStats)
            {
                if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon)
                    throw new ArgumentException("slot must refer to a weapon", nameof(slot));

                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {
                    MeleeWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel as MeleeWeaponItemModel;
                    if (wim != null)
                    {
                        TimeToNext = wim.Rate;
                        float calcDamage = RpgValues.GetMeleeDamage(GameState.Instance.PlayerRpgState, wim.Damage);
                        float calcDamagePierce = RpgValues.GetMeleeDamage(GameState.Instance.PlayerRpgState, wim.DamagePierce);
                        if (GameState.Instance.PlayerRpgState.Energy <= 0)
                        {
                            calcDamage *= 0.5f;
                            calcDamagePierce *= 0.5f;
                            TimeToNext += wim.Rate;
                        }
                        else
                            GameState.Instance.PlayerRpgState.Energy -= wim.EnergyCost;
                        hitInfo = new ActorHitInfo(calcDamage, calcDamagePierce, wim.DType, ActorBodyPart.Unspecified, this);

                    }
                    else
                    {
                        Debug.LogError($"Player can't do a melee attack because weapon in {slot.ToString()} is not a melee weapon!");
                    }
                    //TODO fists or something
                }
                else
                {
                    Debug.LogError($"Player can't do a melee attack because no {slot.ToString()} is equipped!");
                }
            }

            if (ac != null)
                ac.TakeDamage(hitInfo);

            if (slot == EquipSlot.RightWeapon && RightViewModel != null)
            {
                RightViewModel.SetState(ViewModelState.Firing);
            }
            else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
            {
                LeftViewModel.SetState(ViewModelState.Firing);
            }
            else if (MeleeEffect != null)
                Instantiate(MeleeEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
        }

        //this whole thing is a fucking mess that needs to be refactored
        private void DoRangedAttack(EquipSlot slot)
        {
            if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon)
                throw new ArgumentException("slot must refer to a weapon", nameof(slot));

            Debug.Log($"RangedAttack {slot}");

            //TODO default model for fallback instead of fixed values

            if (AttemptToUseStats)
            {

                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {

                    RangedWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel as RangedWeaponItemModel;
                    if (wim != null)
                    {
                        bool useAmmo = !(wim.AType == AmmoType.NoAmmo);
                        bool autoReload = wim.CheckFlag("AutoReload");

                        //ammo logic
                        if (useAmmo)
                        {
                            if (GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] == 0 && !IsReloading)
                            {
                                DoReload();
                                return;
                            }

                            GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] -= 1;
                        }

                        //bullet logic
                        GameObject bullet = null;

                        if (!string.IsNullOrEmpty(wim.Projectile))
                        {
                            var wimBulletPrefab = CoreUtils.LoadResource<GameObject>("Effects/" + wim.Projectile);
                            if (wimBulletPrefab != null)
                                bullet = Instantiate<GameObject>(wimBulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);

                        }

                        if (bullet == null)
                            bullet = Instantiate<GameObject>(BulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);

                        var bulletRigidbody = bullet.GetComponent<Rigidbody>();

                        //TODO factor in weapon skill, esp for bows
                        bullet.GetComponent<BulletScript>().HitInfo = new ActorHitInfo(wim.Damage, wim.DamagePierce, wim.DType, ActorBodyPart.Unspecified, this);
                        Vector3 fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-wim.Spread, wim.Spread), Vector3.right)
                            * (Quaternion.AngleAxis(UnityEngine.Random.Range(-wim.Spread, wim.Spread), Vector3.up)
                            * ShootPoint.forward.normalized);
                        bulletRigidbody.velocity = (fireVec * wim.Velocity);
                        TimeToNext = wim.FireRate;

                        GameObject fireEffect = null;

                        //TODO handle instantiate location (and variants?) in FPS/TPS mode?
                        if (slot == EquipSlot.RightWeapon && RightViewModel != null)
                        {
                            RightViewModel.SetState(ViewModelState.Firing);
                        }
                        else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
                        {
                            LeftViewModel.SetState(ViewModelState.Firing);
                        }
                        else if (!string.IsNullOrEmpty(wim.FireEffect))
                        {
                            var fireEffectPrefab = CoreUtils.LoadResource<GameObject>("Effects/" + wim.FireEffect);
                            if (fireEffectPrefab != null)
                                fireEffect = Instantiate(fireEffectPrefab, ShootPoint.position, ShootPoint.rotation, ShootPoint);
                        }

                        if (useAmmo && autoReload && GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] <= 0)
                        {
                            DoReload();
                        }

                    }
                    else
                    {
                        Debug.LogError("Can't find item model for ranged weapon!");
                    }
                    
                }
            }
            else
            {
                CDebug.LogEx("Ranged attack without model is deprecated!", LogLevel.Error, this);

                var bullet = Instantiate<GameObject>(BulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);
                var bulletRigidbody = bullet.GetComponent<Rigidbody>();

                bulletRigidbody.velocity = (ShootPoint.forward.normalized * BulletSpeed);
                bullet.GetComponent<BulletScript>().HitInfo = BulletHitInfo;
                TimeToNext = 1.0f;

                if (BulletFireEffect != null)
                    Instantiate(BulletFireEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);
            }

            

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
        }

        private void DoReload()
        {
            if (IsReloading) //I think we need this guard
                return;

            //TODO reload both

            reloadSide(EquipSlot.RightWeapon, RightViewModel);
            reloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsReloading = true;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloading"));

            void reloadSide(EquipSlot slot, ViewModelScript viewModel)
            {
                if(GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {
                    if(GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        //unreloadable condition
                        if (GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] == rwim.MagazineSize
                            || GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()) <= 0)
                        {
                            return;
                        }

                        if (viewModel != null)
                            viewModel.SetState(ViewModelState.Reloading);
                        else if(!string.IsNullOrEmpty(rwim.ReloadEffect))
                            AudioPlayer.Instance.PlaySound(rwim.ReloadEffect, SoundType.Sound, false);

                        TimeToNext = Math.Max(rwim.ReloadTime, TimeToNext); //we take the longest time
                    }
                }
                
            }

        }

        private void FinishReload()
        {

            finishReloadSide(EquipSlot.RightWeapon, RightViewModel);
            finishReloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsReloading = false;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloaded"));

            void finishReloadSide(EquipSlot slot, ViewModelScript viewModel)
            {
                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {
                    if (GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        int qty = Math.Min(rwim.MagazineSize, GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()));
                        GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] = qty;
                        GameState.Instance.PlayerRpgState.Inventory.RemoveItem(rwim.AType.ToString(), qty);

                        if (viewModel != null)
                        {
                            viewModel.SetState(IsMoving ? ViewModelState.Moving : ViewModelState.Fixed);
                        }
                    }
                }
            }

        }

        //this is confusing and bloated because everything is pretty much designed around equipping/unequipping weapons being the same scenario
        //but they're actually quite different
        private void HandleWeaponChange(EquipSlot slot)
        {
            //we should probably cache this at a higher level but it's probably not safe
            var player = GameState.Instance.PlayerRpgState;

            if (slot == EquipSlot.RightWeapon)
            {
                //handle equip/unequip melee weapon
                if (player.Equipped.ContainsKey(EquipSlot.RightWeapon) && player.Equipped[EquipSlot.RightWeapon] != null)
                {
                    //fixed to equip *right* weapon
                    Debug.Log("Equipped right weapon!");

                    WeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel as WeaponItemModel;
                    if (wim != null && !string.IsNullOrEmpty(wim.ViewModel))
                    {
                        var prefab = CoreUtils.LoadResource<GameObject>("WeaponViewModels/" + wim.ViewModel);
                        if(prefab != null)
                        {
                            var go = Instantiate<GameObject>(prefab, RightViewModelPoint);
                            RightViewModel = go.GetComponent<ViewModelScript>();
                            if (RightViewModel != null)
                                RightViewModel.SetState(IsMoving ? ViewModelState.Moving : ViewModelState.Fixed);
                        }
                        
                    }
                    
                }
                else
                {
                    //fixed to unequip *right* model
                    Debug.Log("Unequipped right weapon!");
                    if(RightViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(RightViewModelPoint.transform.GetChild(0).gameObject);                        
                    }
                    RightViewModel = null;
                }
            }
            else if(slot == EquipSlot.LeftWeapon)
            {
                IsReloading = false;
                TimeToNext = 0;

                //handle equip/unequip ranged weapon
                if(player.Equipped.ContainsKey(EquipSlot.LeftWeapon) && player.Equipped[EquipSlot.LeftWeapon] != null)
                {
                    //fixed to equip *left* model
                    Debug.Log("Equipped left weapon!");

                    WeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel as WeaponItemModel;
                    if (wim != null && !string.IsNullOrEmpty(wim.ViewModel))
                    {
                        var prefab = CoreUtils.LoadResource<GameObject>("WeaponViewModels/" + wim.ViewModel);
                        if (prefab != null)
                        {
                            var go = Instantiate<GameObject>(prefab, LeftViewModelPoint);
                            LeftViewModel = go.GetComponent<ViewModelScript>();
                            if (LeftViewModel != null)
                                LeftViewModel.SetState(IsMoving ? ViewModelState.Moving : ViewModelState.Fixed);
                        }

                    }
                }
                else
                {
                    //fixed to unequip *left* model
                    Debug.Log("Unequipped left weapon!");
                    if (LeftViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(LeftViewModelPoint.transform.GetChild(0).gameObject);                        
                    }
                    LeftViewModel = null;
                }
            }
        }

        public void TakeDamage(ActorHitInfo data)
        {
            if (MetaState.Instance.SessionFlags.Contains("GodMode"))
                return;

            CharacterModel playerModel = GameState.Instance.PlayerRpgState;

            //damage model is very stupid right now, we will make it better later
            float dt = playerModel.DerivedStats.DamageThreshold[(int)data.DType];
            float dr = playerModel.DerivedStats.DamageResistance[(int)data.DType];
            float damageTaken = RpgWorldUtils.CalculateDamage(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == ActorBodyPart.LeftArm || data.HitLocation == ActorBodyPart.LeftLeg || data.HitLocation == ActorBodyPart.RightArm || data.HitLocation == ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            if(damageTaken > 1)
            {
                if (PainSound != null && !PainSound.isPlaying)
                    PainSound.Play();
            }

            playerModel.Health -= damageTaken;

        }

    }
}
