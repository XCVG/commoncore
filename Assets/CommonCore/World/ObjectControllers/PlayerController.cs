using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public class PlayerController : ActorController
    {

        public bool PlayerInControl;
        public bool Clipping;
        public float PushFactor;

        public float MaxProbeDist;
        public float MaxUseDist;

        //public HUDController HUDScript;
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

            /*
            if(!HUDScript)
            {
                HUDScript = SceneUtils.GetHUDController();
            }
            */

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
                //HandleMovement();
                //HandleInteraction();
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

        /*
        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use

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
                    if(ic != null)
                    {
                        //Debug.Log("Detected: " + ic.Tooltip);
                        HUDScript.SetTargetMessage(ic.Tooltip);

                        //TODO actual use
                        if(cInput.GetKeyDown("Use"))
                        {
                            ic.OnActivate(this.gameObject);
                        }
                    }
                }

            }
        }
        */

        /*
        protected void HandleMovement()
        {
            bool isMoving = false;
            float deadzone = 0.1f;
            float vmul = 10f;
            float rmul = 25f;
            float cmul = 0.5f * rmul;

            //TODO: rewrite this to use new input system, and probably literally everything else
            
            if(Mathf.Abs(cInput.GetAxis("MoveY")) > deadzone)
            {
                CharController.Move(transform.forward * cInput.GetAxis("MoveY") * vmul * Time.deltaTime);
                isMoving = true;
            }

            if(Mathf.Abs(cInput.GetAxis("MoveX")) > deadzone)
            {
                CharController.Move(transform.right * cInput.GetAxis("MoveX") * vmul * Time.deltaTime);
                isMoving = true;
            }

            if(Mathf.Abs(cInput.GetAxis("LookX")) > deadzone)
            {
                transform.Rotate(Vector3.up, rmul * cInput.GetAxis("LookX") * Time.deltaTime);
                isMoving = true;
            }

            if (Mathf.Abs(cInput.GetAxis("LookY")) > deadzone)
            {
                CameraRoot.transform.Rotate(Vector3.left, cmul * cInput.GetAxis("LookY") * Time.deltaTime);
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
                    AnimController.CrossFade("Run_Rifle_Foreward", 0f);
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
        */
    }
}
