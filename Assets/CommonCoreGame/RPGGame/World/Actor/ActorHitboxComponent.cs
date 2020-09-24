using CommonCore.World;
using System;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Hitbox component for an actor
    /// </summary>
    /// <remarks>
    /// <para>Attach this to a hitbox gameobject that is a child of an actor</para>
    /// <para>Mostly deprecated by CommonCore.World.HitboxComponent</para>
    /// </remarks>
    [Obsolete("Use HitboxComponent instead", false)]
    public class ActorHitboxComponent : MonoBehaviour, IHitboxComponent
    {
        public BaseController ParentController;
        public ActorBodyPart BodyPartOverride = ActorBodyPart.Unspecified;
        public HitMaterial HitMaterialOverride = HitMaterial.Unspecified;

        //uses the parent's hit material if this does not override the hit material
        private int HitMaterialResolved => HitMaterialOverride == 0 ? ParentController.HitMaterial : (int)HitMaterialOverride;

        //IHitboxComponent implementation
        BaseController IHitboxComponent.ParentController => ParentController;
        int IHitboxComponent.HitLocationOverride => (int)BodyPartOverride;
        int IHitboxComponent.HitMaterial => HitMaterialResolved;
        float IHitboxComponent.DamageMultiplier => 1;
        bool IHitboxComponent.AllDamageIsPierce => false;

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
            //Debug.Log(name + " hit by " + other.name);

            var bulletScript = other.GetComponent<IDamageOnHit>();
            if(bulletScript != null)
            {
                bulletScript.HandleCollision(ParentController, this, (int)BodyPartOverride, HitMaterialResolved, other.transform.position, ((IHitboxComponent)this).DamageMultiplier, ((IHitboxComponent)this).AllDamageIsPierce);
            }

            /*
            var bulletScript = other.GetComponent<BulletScript>();
            if(bulletScript != null && bulletScript.HitInfo.Originator != ParentController)
            {
                bulletScript.HitInfo.HitCoords = other.transform.position;

                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    bulletScript.HitInfo.HitLocation = (int)BodyPartOverride;

                bulletScript.HandleCollision(ParentController, null);

            }
            */
        }

        private void OnCollisionEnter(Collision collision)
        {
            //Debug.Log(name + " hit by " + collision.collider.name);

            var bulletScript = collision.collider.GetComponent<IDamageOnHit>();
            if (bulletScript != null)
            {
                Vector3 hitLocation;
                if (collision.contactCount > 0)
                    hitLocation = collision.GetContact(0).point;
                else
                    hitLocation = collision.transform.position;

                bulletScript.HandleCollision(ParentController, this, (int)BodyPartOverride, HitMaterialResolved, hitLocation, ((IHitboxComponent)this).DamageMultiplier, ((IHitboxComponent)this).AllDamageIsPierce);
            }

            /*
            var bulletScript = collision.collider.GetComponent<BulletScript>();
            if (bulletScript != null && bulletScript.HitInfo.Originator != ParentController)
            {
                if (collision.contactCount > 0)
                    bulletScript.HitInfo.HitCoords = collision.GetContact(0).point;
                else
                    bulletScript.HitInfo.HitCoords = collision.transform.position;

                if (BodyPartOverride != ActorBodyPart.Unspecified)
                    bulletScript.HitInfo.HitLocation = (int)BodyPartOverride;

                bulletScript.HandleCollision(ParentController, null);

            }
            */
        }
    }
}