using CommonCore.Messaging;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Script for a basic bullet that works with ActorHitboxComponent and ITakeDamage
    /// </summary>
    public class BulletScript : MonoBehaviour, IDamageOnHit
    {
        //TODO clean this up a bit

        private const float DefaultProbeDist = 10f;
        private const int BulletLayer = 9; //9=bullet, TODO find a better way of doing this

        public ActorHitInfo HitInfo;
        public float StayTime = 0;
        public float MaxDist = 10000f;

        public bool FiredByPlayer = false;

        private Rigidbody Rigidbody;
        private float Elapsed;

        void Start()
        {
            gameObject.layer = BulletLayer;
            Rigidbody = GetComponent<Rigidbody>();

            Update(); //seems legit
        }

        private void Update()
        {
            //maybe die
            if (StayTime > 0)
            {
                Elapsed += Time.deltaTime;

                if (Elapsed >= StayTime)
                {
                    //Debug.Log($"Destroying {name} at {Elapsed:F2}s/{StayTime:F2}");
                    Destroy(this.gameObject);
                }
            }

            if(transform.position.magnitude > MaxDist)
            {
                //Debug.Log($"Destroying {name} because it's really far away");
                Destroy(this.gameObject);
            }

            //raycast
            bool hasRigidbody = Rigidbody != null;
            Vector3 forward = hasRigidbody ? Rigidbody.velocity.normalized : transform.forward;
            float maxDistance = hasRigidbody ? Rigidbody.velocity.magnitude / 30f : DefaultProbeDist;
            //var hits = Physics.RaycastAll(transform.position, forward, maxDistance, RaycastLayerMask, QueryTriggerInteraction.Collide);

            //find closest hit
            //var (otherController, hitPoint, hitLocation, hitMaterial) = GetClosestHit(hits);
            var (otherController, hitPoint, hitLocation, hitMaterial) = WorldUtils.RaycastAttackHit(transform.position, forward, maxDistance, true, true, null);

            if (otherController != null && otherController != HitInfo.Originator)
            {
                //Debug.Log("Bullet hit " + otherController.name + " via raycast!");

                HandleCollision(otherController, hitLocation, hitMaterial, hitPoint);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            //Debug.Log("Bullet hit " + collision.transform.name);

            var ahc = collision.gameObject.GetComponent<IHitboxComponent>();
            if (ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            var otherController = collision.gameObject.GetComponent<BaseController>();
            if (otherController == null)
                otherController = collision.gameObject.GetComponentInParent<BaseController>();

            int hitMaterial = otherController?.HitMaterial ?? 0;

            //handle ColliderHitMaterial component
            var colliderHitMaterial = collision.gameObject.GetComponent<ColliderHitMaterial>();
            if (colliderHitMaterial != null)
                hitMaterial = colliderHitMaterial.Material;

            HandleCollision(otherController, 0, hitMaterial, null);
        }

        public void HandleCollision(BaseController otherController, int hitLocation, int hitmaterial, Vector3? positionOverride)
        {
            //Debug.Log($"{name} hit {otherController?.name}");

            if (gameObject == null)
                return; //don't double it up

            if (otherController != null)
            {

                if (otherController == HitInfo.Originator) //no friendly fire for now
                    return;

                if (positionOverride == null)
                    HitInfo.HitCoords = transform.position;
                else
                    HitInfo.HitCoords = positionOverride;

                if (hitLocation > 0)
                    HitInfo.HitLocation = hitLocation;

                if (otherController is ITakeDamage itd)
                {
                    itd.TakeDamage(HitInfo);

                    if (FiredByPlayer)
                        QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("PlayerHitTarget"));
                }
            }

            if(HitInfo.HitCoords == null)
                HitInfo.HitCoords = transform.position;

            if (hitmaterial > 0)
                HitInfo.HitMaterial = hitmaterial;

            HitPuffScript.SpawnHitPuff(HitInfo);

            Destroy(this.gameObject);
        }

    }
}