using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Input;
using CommonCore.UI;
using UnityEngine.EventSystems;

namespace CommonCore.World
{
    public class PlayerController : BaseController
    {
        public bool AutoinitHud = true;

        public bool PlayerInControl;
        public bool Clipping;
        public float PushFactor;

        public float MaxProbeDist;
        public float MaxUseDist;

        public WorldHUDController HUDScript;
        public CharacterController CharController;
        public Animator AnimController;
        public Transform CameraRoot;

        private bool isAnimating;

        // Use this for initialization
        public override void Start()
        {

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
        }

        //TODO: still unsure about the state system, but I'll likely rewrite this whole class
        //should be fixedupdate
        public override void Update()
        {


            if (Time.timeScale == 0)
                return;

            if (PlayerInControl)
            {
                HandleMovement();
                HandleInteraction();
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


        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use
            //TODO handle tooltips

            HUDScript.ClearTarget();

            int layerMask = LayerMask.GetMask("Default","InteractionHitbox");

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
                transform.Rotate(Vector3.up, rmul * MappedInput.GetAxis(DefaultControls.LookX) * Time.deltaTime);
                isMoving = true;
            }

            if (Mathf.Abs(MappedInput.GetAxis(DefaultControls.LookY)) > deadzone)
            {
                CameraRoot.transform.Rotate(Vector3.left, cmul * MappedInput.GetAxis(DefaultControls.LookY) * Time.deltaTime);
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
        
    }
}
