using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Attaches to an object with a collider to set its hit material for hitpuffs
    /// </summary>
    public class ColliderHitMaterial : MonoBehaviour
    {
        [SerializeField]
        private int HitMaterial = 0;

        /// <summary>
        /// The hit material
        /// </summary>
        public int Material => HitMaterial;
    }
}