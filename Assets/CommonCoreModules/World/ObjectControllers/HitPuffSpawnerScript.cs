using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Script for a hit puff that spawns another effect
    /// </summary>
    public class HitPuffSpawnerScript : HitPuffScript
    {
        [SerializeField]
        private bool DestroyAfterSpawn = true;

        [SerializeField, Tooltip("The effect to spawn if a better match isn't available")]
        private string FallbackEffect;

        [SerializeField, Tooltip("Effects to spawn corresponding to a hit material. Indices should match the ones chosen for your game.")]
        private List<EditorHitPuffSpawnerVariant> Variants;

        public override void ActivateVariant(int hitMaterial)
        {
            if(Variants == null || Variants.Count == 0)
            {
                WorldUtils.SpawnEffect(FallbackEffect, transform.position, transform.eulerAngles, CoreUtils.GetWorldRoot());
                return;
            }

            string foundEffect = null;
            foreach (var variant in Variants)
            {
                if (variant.HitMaterial == hitMaterial)
                {
                    foundEffect = variant.Effect;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(foundEffect))
                WorldUtils.SpawnEffect(foundEffect, transform.position, transform.eulerAngles, CoreUtils.GetWorldRoot());
            else
                WorldUtils.SpawnEffect(FallbackEffect, transform.position, transform.eulerAngles, CoreUtils.GetWorldRoot());

            if (DestroyAfterSpawn)
                Destroy(this.gameObject);
        }

        [Serializable]
        private struct EditorHitPuffSpawnerVariant
        {
            public int HitMaterial;
            public string Effect;
        }
    }
}