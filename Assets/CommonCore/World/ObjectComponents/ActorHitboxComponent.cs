using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

namespace CommonCore.World
{

    public class ActorHitboxComponent : MonoBehaviour
    {
        public BaseController ParentController;
        public ActorBodyPart BodyPartOverride = ActorBodyPart.Unspecified;

        void Start()
        {
            if (ParentController == null)
                ParentController = GetComponentInParent<BaseController>();

            if(!(ParentController is PlayerController || ParentController is ActorController))
            {
                CDebug.LogEx(string.Format("{0} has ActorHitboxComponent, but is not an Actor or Player!", ParentController.gameObject.name), LogLevel.Error, this);
                this.enabled = false;
            }
        }

        //take damage on hit
        private void OnTriggerEnter(Collider other)
        {
            var bulletScript = other.GetComponent<BulletScript>();
            if(bulletScript != null)
            {
                var hi = bulletScript.HitInfo;
                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    hi.HitLocation = BodyPartOverride;

                //this is where dynamics would kick ass
                //we could use an interface but that makes Too Much Sense (and also Unity hates them)
                if(ParentController is ActorController)
                    ((ActorController)ParentController).TakeDamage(hi);
                else if (ParentController is PlayerController)
                    ((PlayerController)ParentController).TakeDamage(hi);

                Destroy(other.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnTriggerEnter(collision.collider);
        }
    }
}