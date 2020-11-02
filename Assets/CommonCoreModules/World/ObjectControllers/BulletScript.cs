using CommonCore.Messaging;
using CommonCore.World;
using CommonCore.ObjectActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.World
{
    /// <summary>
    /// Script for a basic bullet that works with ActorHitboxComponent and ITakeDamage
    /// </summary>
    public class BulletScript : MonoBehaviour, IDamageOnHit
    {
        //TODO clean this up a bit

        private const float DefaultProbeDist = 10f;
        //private const int BulletLayer = 9; //9=bullet, TODO find a better way of doing this

        [Header("Mechanics")]
        public ActorHitInfo HitInfo;
        public string HitPuffOverride;
        public ActionSpecialEvent HitSpecial;
        public bool FiredByPlayer = false;

        [Header("Dynamics")]
        public float StayTime = 0;
        [FormerlySerializedAs("MaxDist"), Tooltip("Maximum distance from world origin")]
        public float MaxDistFromOrigin = 10000f;
        public float MaxDistToTravel = 0;
        public float FakeGravity = 0;
        public Vector3 Origin;       
        public bool EnableRaycasting = true;
        public bool EnableCollision = true;

        private Rigidbody Rigidbody;
        private float Elapsed;


        void Start()
        {
            gameObject.layer = LayerMask.NameToLayer("Bullet");
            Rigidbody = GetComponent<Rigidbody>();

            Origin = transform.position;

            Update(); //seems legit
        }

        private void FixedUpdate()
        {
            //fake gravity
            if (FakeGravity > 0)
            {
                Rigidbody.AddForce(Physics.gravity * FakeGravity * Time.fixedDeltaTime, ForceMode.Acceleration);
            }

            
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

            //distance travel check
            if(MaxDistToTravel > 0)
            {
                float distance = (Origin - transform.position).magnitude;
                if(distance > MaxDistToTravel)
                {
                    Destroy(this.gameObject);
                }
            }

            if (MaxDistFromOrigin >= 0 && transform.position.magnitude > MaxDistFromOrigin)
            {
                //Debug.Log($"Destroying {name} because it's really far away");
                Destroy(this.gameObject);
            }

            if (EnableRaycasting)
            {

                //raycast
                bool hasRigidbody = Rigidbody != null;
                Vector3 forward = hasRigidbody ? Rigidbody.velocity.normalized : transform.forward;
                float maxDistance = hasRigidbody ? Rigidbody.velocity.magnitude / 30f : DefaultProbeDist;
                //var hits = Physics.RaycastAll(transform.position, forward, maxDistance, RaycastLayerMask, QueryTriggerInteraction.Collide);

                //find closest hit
                //var (otherController, hitPoint, hitLocation, hitMaterial) = GetClosestHit(hits);
                var hit = WorldUtils.RaycastAttackHit(transform.position, forward, maxDistance, true, true, null);

                if (hit.Controller != null && hit.Controller != HitInfo.Originator)
                {
                    //Debug.Log("Bullet hit " + hit.Controller.name + " via raycast!");

                    float damageMultiplier;
                    bool allDamageIsPierce;
                    if (hit.Hitbox != null)
                    {
                        //Debug.Log((hit.Hitbox as MonoBehaviour)?.name);
                        damageMultiplier = hit.Hitbox.DamageMultiplier;
                        allDamageIsPierce = hit.Hitbox.AllDamageIsPierce;
                    }
                    else
                    {
                        damageMultiplier = 1;
                        allDamageIsPierce = false;
                    }

                    HandleHit(hit.Controller, hit.Hitbox, hit.HitLocation, hit.HitMaterial, hit.HitPoint, damageMultiplier, allDamageIsPierce);
                }
                else if(hit.Controller == null && hit.HitCollider != null)
                {
                    var hitMaterial = hit.HitMaterial;
                    var colliderHitMaterial = hit.HitCollider.gameObject.GetComponent<ColliderHitMaterial>();
                    if (colliderHitMaterial != null)
                        hitMaterial = colliderHitMaterial.Material;

                    HandleHit(null, null, hit.HitLocation, hitMaterial, hit.HitPoint, 1, false);
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!EnableCollision)
                return;

            var ahc = other.GetComponent<IHitboxComponent>();
            if (ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            HandleCollisionHit(other.gameObject, null);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!EnableCollision)
                return;

            //Debug.Log("Bullet hit " + collision.transform.name);

            var ahc = collision.gameObject.GetComponent<IHitboxComponent>();
            if (ahc != null)
            {
                //we'll let the other component handle the collision...

                return; //...but we won't destroy this one 
            }

            Vector3? positionOverride = null;
            if (collision.contactCount > 0)
                positionOverride = collision.GetContact(0).point;

            HandleCollisionHit(collision.gameObject, positionOverride);
        }

        private void HandleCollisionHit(GameObject otherObject, Vector3? positionOverride)
        {
            var otherController = otherObject.GetComponent<BaseController>();
            if (otherController == null)
                otherController = otherObject.GetComponentInParent<BaseController>();

            int hitMaterial = otherController?.HitMaterial ?? 0;

            //handle ColliderHitMaterial component
            var colliderHitMaterial = otherObject.GetComponent<ColliderHitMaterial>();
            if (colliderHitMaterial != null)
                hitMaterial = colliderHitMaterial.Material;

            //Debug.Log($"Contact points: {collision.contactCount} | First contact point: {(collision.contactCount > 0 ? collision.GetContact(0).point.ToString("F2") : null)} ");

            HandleHit(otherController, null, 0, hitMaterial, positionOverride, 1, false);
        }

        private void HandleHit(BaseController otherController, IHitboxComponent hitbox, int hitLocation, int hitmaterial, Vector3? positionOverride, float damageMultiplier, bool allDamageIsPierce)
        {
            //Debug.Log($"HandleHit called ({otherController})");

            if (gameObject == null)
                return; //don't double it up

            if(HitInfo.HitFlags.HasFlag(BuiltinHitFlags.IgnoreHitLocation))
            {
                if (!(hitbox != null && hitbox.AlwaysApplyMultiplier))
                    damageMultiplier = 1;
            }

            if (otherController != null)
            {

                if (otherController == HitInfo.Originator) //no friendly fire for now
                    return;

                if (positionOverride == null)
                    HitInfo.HitCoords = transform.position;
                else
                    HitInfo.HitCoords = positionOverride;

                HitInfo.Damage *= damageMultiplier;
                HitInfo.DamagePierce *= damageMultiplier;

                if (allDamageIsPierce)
                {
                    HitInfo.DamagePierce += HitInfo.Damage;
                    HitInfo.Damage = 0;
                }

                if (hitLocation > 0)
                    HitInfo.HitLocation = hitLocation;

                if (otherController is ITakeDamage itd)
                {
                    itd.TakeDamage(HitInfo);

                    if (FiredByPlayer)
                        QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("PlayerHitTarget"));
                }
            }

            if (HitInfo.HitCoords == null)
            {
                if (positionOverride == null)
                    HitInfo.HitCoords = transform.position;
                else
                    HitInfo.HitCoords = positionOverride;
                //Debug.Log($"HitCoords set to {HitInfo.HitCoords}");
            }

            if (hitmaterial > 0)
                HitInfo.HitMaterial = hitmaterial;

            if (!string.IsNullOrEmpty(HitPuffOverride))
                HitPuffScript.SpawnHitPuff(HitPuffOverride, HitInfo.HitCoords.Value, HitInfo.HitMaterial);
            else
                HitPuffScript.SpawnHitPuff(HitInfo);

            if (HitSpecial != null)
                HitSpecial.Invoke(new ActionInvokerData() { Activator = HitInfo.Originator });

            Destroy(this.gameObject);
        }

        void IDamageOnHit.HandleCollision(BaseController otherController, IHitboxComponent hitbox, int hitLocation, int hitmaterial, Vector3? positionOverride, float damageMultiplier, bool allDamageIsPierce)
        {
            //Debug.Log($"{name} hit {otherController?.name}");

            if (!EnableCollision)
                return;

            HandleHit(otherController, hitbox, hitLocation, hitmaterial, positionOverride, damageMultiplier, allDamageIsPierce);

        }

    }
}