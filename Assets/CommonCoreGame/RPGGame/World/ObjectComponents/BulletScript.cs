using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Script for a basic bullet that works with ActorHitboxComponent and ITakeDamage
    /// </summary>
    public class BulletScript : MonoBehaviour
    {
        private const float DefaultProbeDist = 10f;
        private const int BulletLayer = 9; //9=bullet, TODO find a better way of doing this

        public ActorHitInfo HitInfo;
        public float StayTime = 0;
        public float MaxDist = 10000f;

        private LayerMask RaycastLayerMask;
        private Rigidbody Rigidbody;
        private float Elapsed;

        void Start()
        {
            gameObject.layer = BulletLayer;
            RaycastLayerMask = LayerMask.GetMask("Default", "ActorHitbox");
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
            var hits = Physics.RaycastAll(transform.position, forward, maxDistance, RaycastLayerMask, QueryTriggerInteraction.Collide);

            //find closest hit
            var (otherController, hitPoint) = GetClosestHit(hits);

            if(otherController != null && otherController != HitInfo.Originator)
            {
                //Debug.Log("Bullet hit " + otherController.name + " via raycast!");

                HandleCollision(otherController, hitPoint);
            }
        }

        /// <summary>
        /// Gets the closest eligible hit
        /// </summary>
        private (BaseController otherController, Vector3 hitPoint) GetClosestHit(IReadOnlyList<RaycastHit> hits)
        {
            //reject the easiest case
            if (hits.Count == 0)
                return (null, default);

            //we get the closest hit that is either:
            //-a trigger attached to an ActorHitboxComponent
            //-a non-trigger collider, regardless of what it is attached to

            RaycastHit closestHit = default;
            closestHit.distance = float.MaxValue;

            foreach(var hit in hits)
            {
                if(hit.distance < closestHit.distance)
                {
                    //reject other bullets
                    if (hit.collider.GetComponent<BulletScript>())
                        continue;

                    if(hit.collider.isTrigger) //if it's non-solid, it only counts if it's a hitbox
                    {
                        if (hit.collider.GetComponent<ActorHitboxComponent>() != null)
                            closestHit = hit;
                    }
                    else //if it's solid, closer always counts
                        closestHit = hit;
                }
            }

            //sentinel; we didn't find anything
            if (closestHit.distance == float.MaxValue)
                return (null, default);

            //try to find an actor hitbox
            var actorHitbox = closestHit.collider.GetComponent<ActorHitboxComponent>();
            if (actorHitbox != null)
                return (actorHitbox.ParentController, closestHit.point);

            //try to find a basecontroller
            var otherController = closestHit.collider.GetComponent<BaseController>();
            if (otherController == null)
                otherController = closestHit.collider.GetComponentInParent<BaseController>();
            return (otherController, closestHit.point);
        }

        void OnCollisionEnter(Collision collision)
        {
            //Debug.Log("Bullet hit " + collision.transform.name);

            var ahc = collision.gameObject.GetComponent<ActorHitboxComponent>();
            if (ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            var otherController = collision.gameObject.GetComponent<BaseController>();
            if (otherController == null)
                otherController = collision.gameObject.GetComponentInParent<BaseController>();

            HandleCollision(otherController, null);
        }

        public void HandleCollision(BaseController otherController, Vector3? positionOverride)
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

                if (otherController is ITakeDamage itd)
                    itd.TakeDamage(HitInfo);
            }

            SpawnPuff();

            Destroy(this.gameObject);
        }

        public void SpawnPuff()
        {
            if(!string.IsNullOrEmpty(HitInfo.HitPuff))
            {
                WorldUtils.SpawnEffect(HitInfo.HitPuff, HitInfo.HitCoords ?? transform.position, Vector3.zero, CoreUtils.GetWorldRoot());
            }
        }
    }
}