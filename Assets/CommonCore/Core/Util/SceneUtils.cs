using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CommonCore.Util
{

    /// <summary>
    /// Helper methods for manipulating 
    /// </summary>
    public static class SceneUtils
    {

        /// <summary>
        /// Gets the nearest hit from a collection of raycast hits
        /// </summary>
        public static RaycastHit GetNearestHit(this IEnumerable<RaycastHit> raycastHits)
        {
            float minDist = float.MaxValue;
            RaycastHit? closestHit = null;
            foreach(var hit in raycastHits)
            {
                if(hit.distance < minDist)
                {
                    closestHit = hit;
                    minDist = hit.distance;
                }
            }

            if (!closestHit.HasValue)
                throw new ArgumentNullException();

            return closestHit.Value;
        }

    }
}
