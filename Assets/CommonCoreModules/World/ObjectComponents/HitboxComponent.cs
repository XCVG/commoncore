using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Generic hitbox component
    /// </summary>
    /// <remarks>
    /// <para>Attach to a child of an ITakeDamage Thing, with a collider</para>
    /// </remarks>
    public class HitboxComponent : MonoBehaviour, IHitboxComponent
    {
        [SerializeField]
        private BaseController ParentController = null;
        [SerializeField, Tooltip("This should correspond to a body part/hit location you have specified. Use 0 for no override")]
        private int HitLocationOverride = 0;
        [SerializeField, Tooltip("This should correspond to a hit material type you have specified. Use 0 for no override")]
        private int HitMaterialOverride = 0;
        [SerializeField, Tooltip("This will directly affect the amount of damage taken")]
        private float DamageMultiplier = 1;
        [SerializeField, Tooltip("Damage will be added to DamagePierce and zeroed. Use to simulate unarmored parts")]
        private bool AllDamageIsPierce = false;
        [SerializeField, Tooltip("Applies DamageMultiplier even if the hit has IgnoreHitLocation flag")]
        private bool AlwaysApplyMultiplier = true;

        //uses the parent controller's hit material if this doesn't have an override
        private int HitMaterial => HitMaterialOverride == 0 ? ParentController.HitMaterial : HitMaterialOverride;

        //IHitboxComponent implementation
        BaseController IHitboxComponent.ParentController => ParentController;
        int IHitboxComponent.HitLocationOverride => HitLocationOverride;
        int IHitboxComponent.HitMaterial => HitMaterial;
        float IHitboxComponent.DamageMultiplier => DamageMultiplier;
        bool IHitboxComponent.AllDamageIsPierce => AllDamageIsPierce;
        bool IHitboxComponent.AlwaysApplyMultiplier => AlwaysApplyMultiplier;

        void Start()
        {
            if (ParentController == null)
                ParentController = GetComponentInParent<BaseController>();

            if (ParentController == null)
            {
                Debug.LogError($"{gameObject.name} has {nameof(HitboxComponent)}, but is not attached to any CommonCore Object!");
                this.enabled = false;
                return;
            }

            if (!(ParentController is ITakeDamage))
            {
                Debug.LogError($"{ParentController.gameObject.name} has {nameof(HitboxComponent)}, but is not an {nameof(ITakeDamage)}!");
                this.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log(name + " hit by " + other.name);

            var bulletScript = other.GetComponent<IDamageOnHit>();
            if (bulletScript != null)
            {
                bulletScript.HandleCollision(ParentController, this, HitLocationOverride, HitMaterial, other.transform.position, DamageMultiplier, AllDamageIsPierce);
            }
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

                bulletScript.HandleCollision(ParentController, this, HitLocationOverride, HitMaterial, hitLocation, DamageMultiplier, AllDamageIsPierce);
            }
        }
    }
}