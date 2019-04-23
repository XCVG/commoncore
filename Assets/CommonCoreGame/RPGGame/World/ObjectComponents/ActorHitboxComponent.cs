using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{

    //TODO move this into World
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

                if (ParentController is ITakeDamage)
                    ((ITakeDamage)ParentController).TakeDamage(hi);

                Destroy(other.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnTriggerEnter(collision.collider);
        }
    }
}