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

namespace CommonCore.World
{
    public class PlayerController : BaseController
    {
        public bool AutoinitHud = true;

        [Header("Interactivity")]
        public bool PlayerInControl;
        public bool Clipping;
        public float PushFactor;

        public float MaxProbeDist;
        public float MaxUseDist;

        [Header("Components")]
        public WorldHUDController HUDScript;
        public CharacterController CharController;
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
        

        //TODO refactor, but I think that was in the cards from the beginning
        //TODO handle jumping, flying, noclip, crouching(?)
        protected void HandleMovement()
        {
            bool isMoving = false;
            float deadzone = 0.1f;
            float vmul = 10f;
            float rmul = 60f;
            float cmul = 0.5f * rmul;
            float lmul = 180f;
            
            if(Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveY)) > deadzone)
            {
                CharController.Move(transform.forward * MappedInput.GetAxis(DefaultControls.MoveY) * vmul * Time.deltaTime);
                isMoving = true;
            }

            if(Mathf.Abs(MappedInput.GetAxis(DefaultControls.MoveX)) > deadzone)
            {
                CharController.Move(transform.right * MappedInput.GetAxis(DefaultControls.MoveX) * vmul * Time.deltaTime);
                isMoving = true;
            }

            if(Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookX)) > deadzone)
            {
                transform.Rotate(Vector3.up, lmul * MappedInput.GetAxis(DefaultControls.LookX) * Time.deltaTime);
                isMoving = true;
            }

            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookY)) > deadzone)
            {
                CameraRoot.transform.Rotate(Vector3.left, lmul * MappedInput.GetAxis(DefaultControls.LookY) * Time.deltaTime);
            }

            //handle gravity
            if(Clipping)
            {
                CharController.Move(Physics.gravity * Time.deltaTime);
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
            if(MappedInput.GetButtonDown("Fire1") && ShootingEnabled)
            {
                //shoot
                var bullet = Instantiate<GameObject>(BulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);
                var bulletRigidbody = bullet.GetComponent<Rigidbody>();
                bulletRigidbody.velocity = (ShootPoint.forward.normalized * BulletSpeed);
                bullet.GetComponent<BulletScript>().HitInfo = BulletHitInfo;
                if (BulletFireEffect != null)
                    Instantiate(BulletFireEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);
                if (AttemptToUseStats)
                {
                    CDebug.LogEx("Weapon stats not implemented!", LogLevel.Warning, this);
                }
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
                if (ac != null)
                    ac.TakeDamage(MeleeHitInfo);

                if (MeleeEffect != null)
                    Instantiate(MeleeEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);
                
                if (AttemptToUseStats)
                {
                    CDebug.LogEx("Weapon stats not implemented!", LogLevel.Warning, this);
                }
            }
        }

        public void TakeDamage(ActorHitInfo data)
        {
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
