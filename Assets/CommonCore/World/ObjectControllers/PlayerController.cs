using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonCore.Input;
using CommonCore.UI;
using CommonCore.LockPause;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Rpg;
using CommonCore.Messaging;

namespace CommonCore.World
{
    public class PlayerController : BaseController
    {
        public bool AutoinitHud = true;

        [Header("Interactivity")]
        public bool PlayerInControl;
        public bool Clipping;
        public float PushFactor;

        private Vector3 AirMoveVelocity;

        public float MaxProbeDist;
        public float MaxUseDist;

        [Header("Components")]
        public WorldHUDController HUDScript;
        public CharacterController CharController;
        public Rigidbody CharRigidbody;
        public Animator AnimController;
        public Transform CameraRoot;

        [Header("Shooting")]
        public bool ShootingEnabled = true;
        public bool AttemptToUseStats = false;
        public GameObject BulletPrefab;
        public GameObject BulletFireEffect;
        public ActorHitInfo BulletHitInfo;
        public float BulletSpeed = 50.0f;
        public bool MeleeEnabled = true;
        public ActorHitInfo MeleeHitInfo;
        public float MeleeProbeDist = 1.5f;
        public GameObject MeleeEffect;
        public Transform ShootPoint;
        private float TimeToNext;

        private bool isAnimating;

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

            if(!AnimController)
            {
                AnimController = GetComponent<Animator>();
            }

            if(!HUDScript)
            {
                HUDScript = WorldHUDController.Current;
            }
            
            if(!HUDScript && AutoinitHud)
            {
                Instantiate<GameObject>(Resources.Load<GameObject>("UI/DefaultWorldHUD"));
                if (EventSystem.current == null)
                    Instantiate(Resources.Load("UI/DefaultEventSystem"));

                HUDScript = WorldHUDController.Current;
            }
            

            isAnimating = false;

            LockPauseModule.CaptureMouse = true;

            SetDefaultPlayerView();
        }

        //TODO: still unsure about the state system, but I'll likely rewrite this whole class
        //should be fixedupdate
        public override void Update()
        {


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

            switch (CCParams.DefaultPlayerView)
            {
                case PlayerViewType.PreferFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    break;
                case PlayerViewType.PreferThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    break;
                case PlayerViewType.ForceFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    break;
                case PlayerViewType.ForceThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    break;
                case PlayerViewType.ExplicitOther:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(false);
                    break;
            }
        }

        private void HandleView()
        {
            if (!(CCParams.DefaultPlayerView == PlayerViewType.PreferFirst || CCParams.DefaultPlayerView == PlayerViewType.PreferThird))
                return;

            if(MappedInput.GetButtonDown("ChangeView")) 
            {
                //slow and stupid but it'll work for now

                GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
                GameObject fpCamera = CameraRoot.Find("FP Camera").gameObject;

                if(tpCamera.activeSelf)
                {
                    fpCamera.SetActive(true);
                    tpCamera.SetActive(false);
                }
                else
                {
                    fpCamera.SetActive(false);
                    tpCamera.SetActive(true);

                }
            }
        }

        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use
            //TODO handle tooltips

            HUDScript.ClearTarget();

            int layerMask = LayerMask.GetMask("Default","ActorHitbox");

            RaycastHit probeHit;
            if(Physics.Raycast(CameraRoot.transform.position,CameraRoot.transform.forward,out probeHit,MaxProbeDist,layerMask,QueryTriggerInteraction.Collide))
            {
                // Debug.Log("Detected: " + probeHit.transform.gameObject.name);
                GameObject go = probeHit.transform.gameObject;
                if(go != null)
                {
                    var ic = go.GetComponent<InteractableComponent>();
                    if(ic != null && ic.enabled)
                    {
                        //Debug.Log("Detected: " + ic.Tooltip);
                        HUDScript.SetTargetMessage(ic.Tooltip);

                        //actual use
                        if(MappedInput.GetButtonDown("Use"))
                        {
                            ic.OnActivate(this.gameObject);
                        }
                    }
                }

            }
        }
        

        //TODO handle crouching and fall damage
        protected void HandleMovement()
        {
            //really need to do something about these values
            bool isMoving = false;
            float deadzone = 0.1f; //this really shouldn't be here
            float vmul = 10f; //mysterious magic number velocity multiplier
            float amul = 5.0f; //air move multiplier
            float lmul = 180f; //mostly logical look multiplier

            var playerState = GameState.Instance.PlayerRpgState;

            //looking is the same as long as we're in control
            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) > deadzone)
            {
                transform.Rotate(Vector3.up, lmul * MappedInput.GetAxis(DefaultControls.LookX) * Time.deltaTime);
                isMoving = true;
            }

            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookY)) > deadzone)
            {
                CameraRoot.transform.Rotate(Vector3.left, lmul * MappedInput.GetAxis(DefaultControls.LookY) * Time.deltaTime);
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
                    moveVector *= 2.0f;

                transform.Translate(moveVector, Space.World);
            }
            else
            {

                Vector3 moveVector = Vector3.zero;

                CharController.enabled = true;
                CharRigidbody.isKinematic = true;

                if (CharController.isGrounded)
                {
                    //grounded: controller enabled, kinematic rigidbody, use controller movement
                    
                    AirMoveVelocity = Vector3.zero;

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
                    {
                        moveVector += (transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * vmul * Time.deltaTime);
                        isMoving = true;
                    }

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
                    {
                        moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * vmul * Time.deltaTime);
                        isMoving = true;
                    }

                    //hacky sprinting
                    if(MappedInput.GetButton(DefaultControls.Sprint) && playerState.Energy > 0)
                    {
                        moveVector *= 1.5f;
                        playerState.Energy -= 10.0f * Time.deltaTime;
                    }
                    else
                    {
                        playerState.Energy += 5.0f * Time.deltaTime;
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

                        isMoving = true;
                    }

                    moveVector += 0.6f * Physics.gravity * Time.deltaTime;

                }
                else
                {
                    //flying: controller enabled, non-kinematic rigidbody, use rigidbody movement
                    //except that didn't work...
                    //CharController.enabled = true;
                    //CharRigidbody.isKinematic = false;                    

                    AirMoveVelocity.x *= (0.9f * Time.deltaTime);
                    AirMoveVelocity.z *= (0.9f * Time.deltaTime);
                    AirMoveVelocity += Physics.gravity * 2.0f * Time.deltaTime;

                    moveVector += AirMoveVelocity * Time.deltaTime;

                    //air control
                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
                    {
                        moveVector += (transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * amul * Time.deltaTime);
                        isMoving = true;
                    }

                    if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
                    {
                        moveVector += (transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * amul * Time.deltaTime);
                        isMoving = true;
                    }
                }

                //"gravity"
                //moveVector += 0.6f * Physics.gravity * Time.deltaTime; //fuck me! this is NOT how to do physics!

                CharController.Move(moveVector);
            }

            //handle animation
            if (isMoving)
            {
                if (!isAnimating)
                {

                    //ac.Play("Run_Rifle_Foreward", 0);
                    AnimController.CrossFade("Run", 0f);
                    isAnimating = true;
                    //stepSound.Play();
                }
            }
            else
            {
                if (isAnimating)
                {

                    //ac.Stop();
                    AnimController.CrossFade("Idle", 0f);
                    isAnimating = false;
                    //stepSound.Stop();
                }
            }


        }

        //handle weapons (very temporary)
        protected void HandleWeapons()
        {
            float oldTTN = TimeToNext;
            TimeToNext -= Time.deltaTime;
            if (TimeToNext > 0)
                return;

            if(oldTTN > 0)
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReady"));

            //TODO use ammo/magazine
            //TODO fire rate, spread, etc

            if (MappedInput.GetButtonDown("Fire1") && ShootingEnabled)
            {
                //shoot
                var bullet = Instantiate<GameObject>(BulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);
                var bulletRigidbody = bullet.GetComponent<Rigidbody>();
                
                bulletRigidbody.velocity = (ShootPoint.forward.normalized * BulletSpeed);                
                bullet.GetComponent<BulletScript>().HitInfo = BulletHitInfo;
                TimeToNext = 1.0f;
                if (AttemptToUseStats)
                {
                    if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RangedWeapon))
                    {
                        RangedWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RangedWeapon].ItemModel as RangedWeaponItemModel;
                        if (wim != null)
                        {
                            //TODO factor in weapon skill, esp for bows
                            bullet.GetComponent<BulletScript>().HitInfo = new ActorHitInfo(wim.Damage, wim.DamagePierce, wim.DType, ActorBodyPart.Unspecified, this);
                            Vector3 fireVec = Quaternion.AngleAxis(Random.Range(-wim.Spread, wim.Spread), Vector3.right)
                                * (Quaternion.AngleAxis(Random.Range(-wim.Spread, wim.Spread), Vector3.up)
                                * ShootPoint.forward.normalized);
                            bulletRigidbody.velocity = (fireVec * wim.Velocity);
                            TimeToNext = wim.FireRate;
                        }
                    }      
                }

                if (BulletFireEffect != null)
                    Instantiate(BulletFireEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);

                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
            }
            else if(MappedInput.GetButtonDown("Fire2") && MeleeEnabled)
            {
                //punch
                LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");
                var rc = Physics.RaycastAll(ShootPoint.position, ShootPoint.forward, MeleeProbeDist, lm, QueryTriggerInteraction.Collide);
                ActorController ac = null;
                foreach(var r in rc)
                {
                    var go = r.collider.gameObject;
                    var ahgo = go.GetComponent<ActorHitboxComponent>();
                    if(ahgo != null)
                    {
                        ac = (ActorController)ahgo.ParentController; //this works as long as we don't go MP or do Voodoo Dolls
                        break;
                    }
                    var acgo = go.GetComponent<ActorController>();
                    if(acgo != null)
                    {
                        ac = acgo;
                        break;
                    }
                }

                ActorHitInfo hitInfo = MeleeHitInfo;
                if (AttemptToUseStats)
                {
                    if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.MeleeWeapon))
                    {
                        MeleeWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.MeleeWeapon].ItemModel as MeleeWeaponItemModel;
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
                        //TODO fists or something
                    }
                }

                if (ac != null)
                    ac.TakeDamage(hitInfo);

                if (MeleeEffect != null)
                    Instantiate(MeleeEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);

                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
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
            float damageTaken = CCBaseUtil.CalculateDamage(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == ActorBodyPart.LeftArm || data.HitLocation == ActorBodyPart.LeftLeg || data.HitLocation == ActorBodyPart.RightArm || data.HitLocation == ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            playerModel.Health -= damageTaken;

        }

    }
}
