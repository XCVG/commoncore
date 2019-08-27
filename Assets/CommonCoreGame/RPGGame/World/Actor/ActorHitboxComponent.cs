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

            if(ParentController == null)
            {
                Debug.LogError($"{gameObject.name} has ActorHitboxComponent, but is not attached to any CommonCore Thing!");
                this.enabled = false;
                return;
            }

            if(!(ParentController is ITakeDamage))
            {
                Debug.LogError($"{ParentController.gameObject.name} has ActorHitboxComponent, but is not an ITakeDamage!");
                this.enabled = false;
            }
        }

        //take damage on hit
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(name + " hit by " + other.name);

            var bulletScript = other.GetComponent<BulletScript>();
            if(bulletScript != null && bulletScript.HitInfo.Originator != ParentController)
            {
                bulletScript.HitInfo.HitCoords = other.transform.position;

                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    bulletScript.HitInfo.HitLocation = BodyPartOverride;

                bulletScript.HandleCollision(ParentController, null);

            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(name + " hit by " + collision.collider.name);

            var bulletScript = collision.collider.GetComponent<BulletScript>();
            if (bulletScript != null && bulletScript.HitInfo.Originator != ParentController)
            {
                if (collision.contactCount > 0)
                    bulletScript.HitInfo.HitCoords = collision.GetContact(0).point;
                else
                    bulletScript.HitInfo.HitCoords = collision.transform.position;

                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    bulletScript.HitInfo.HitLocation = BodyPartOverride;

                bulletScript.HandleCollision(ParentController, null);

            }
        }
    }
}