using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Helper methods for manipulating things in Unity scenes
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

        /// <summary>
        /// Destroys all children of a transform
        /// </summary>
        public static void DestroyAllChildren(this Transform root)
        {
            foreach (Transform t in root)
            {
                UnityEngine.Object.Destroy(t.gameObject);
            }
        }

        /// <summary>
        /// Find a child by name, recursively
        /// </summary>
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            var result = aParent.Find(aName);
            if (result != null)
                return result;
            foreach (Transform child in aParent)
            {
                result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Finds all game objects with a given name. No, I don't know what it's for either.
        /// </summary>
        public static List<GameObject> FindAllGameObjects(string name)
        {
            var goList = new List<GameObject>();

            foreach (GameObject go in GameObject.FindObjectsOfType(typeof(GameObject)))
            {
                if (go.name == name)
                    goList.Add(go);
            }

            return goList;
        }

    }
}
