using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    public class ActorHitboxComponent : MonoBehaviour
    {
        public ActorController ParentController;
        public ActorBodyPart BodyPartOverride = ActorBodyPart.Unspecified;

        void Start()
        {
            if (ParentController == null)
                ParentController = GetComponentInParent<ActorController>();
        }

        //TODO take damage
        private void OnTriggerEnter(Collider other)
        {
            var bulletScript = other.GetComponent<BulletScript>();
            if(bulletScript != null)
            {
                var hi = bulletScript.HitInfo;
                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    hi.HitLocation = BodyPartOverride;
                ParentController.TakeDamage(hi);
                Destroy(other.gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnTriggerEnter(collision.collider);
        }
    }
}