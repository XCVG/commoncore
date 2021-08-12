using System;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Base class for the script that controls a hit puff and allows choosing a variant
    /// </summary>
    public abstract class HitPuffScript : MonoBehaviour
    {
        public abstract void ActivateVariant(int hitMaterial);

        /// <summary>
        /// Spawns a hit puff based on an ActorHitInfo
        /// </summary>
        public static GameObject SpawnHitPuff(ActorHitInfo hitInfo)
        {
            if (!hitInfo.HitCoords.HasValue) //can't spawn if we don't know where to spawn
                return null;

            return SpawnHitPuff(hitInfo.HitPuff, hitInfo.HitCoords.Value, hitInfo.HitMaterial);
        }

        /// <summary>
        /// Spawns a hit puff, setting position and variant
        /// </summary>
        public static GameObject SpawnHitPuff(string effect, Vector3 position, int hitMaterial)
        {
            GameObject puff = null;

            try
            {
                if (!string.IsNullOrEmpty(effect))
                {
                    puff = WorldUtils.SpawnEffect(effect, position, Vector3.zero, CoreUtils.GetWorldRoot(), false);

                    if (puff != null)
                    {
                        var puffScript = puff.GetComponent<HitPuffScript>();

                        if (puffScript != null)
                        {
                            puffScript.ActivateVariant(hitMaterial);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to spawn HitPuff!");
                Debug.LogException(e);
            }

            return puff;
        }
    }
}