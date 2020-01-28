using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Script for a hit puff that activates a specific child for each supported hit material type
    /// </summary>
    public class HitPuffVariantScript : HitPuffScript
    {
        [SerializeField, Tooltip("The child object to activate if a better match isn't available")]
        private GameObject FallbackVariant;

        [SerializeField, Tooltip("Activation delay in seconds")]
        private float ActivateDelay;

        [SerializeField, Tooltip("Children to activate corresponding to a hit material. Indices should match the ones chosen for your game.")]
        private List<EditorHitPuffVariant> Variants; //blame Unity's halfassed serialization system for this not being a Dictionary

        public override void ActivateVariant(int hitMaterial)
        {
            if (ActivateDelay <= 0)
                FinishActivateVariant(hitMaterial);
            else
                StartCoroutine(ActivateVariantCoroutine(hitMaterial));
        }

        private IEnumerator ActivateVariantCoroutine(int hitMaterial)
        {
            yield return new WaitForSeconds(ActivateDelay);
            FinishActivateVariant(hitMaterial);
        }

        private void FinishActivateVariant(int hitMaterial)
        {
            //always use the fallback variant if there are no available variants (though why would you do that?)
            if (Variants == null || Variants.Count == 0)
            {
                FallbackVariant.SetActive(true);
                return;
            }

            GameObject foundVariant = null;
            foreach (var variant in Variants)
            {
                if (variant.HitMaterial == hitMaterial)
                {
                    foundVariant = variant.VariantChild;
                    break;
                }
            }

            if (foundVariant != null)
                foundVariant.SetActive(true);
            else
                FallbackVariant.SetActive(true);
        }

        [Serializable]
        private struct EditorHitPuffVariant
        {
            public int HitMaterial;
            public GameObject VariantChild;
        }
    }
}