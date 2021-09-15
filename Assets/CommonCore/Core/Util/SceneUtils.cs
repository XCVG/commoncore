using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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
        /// Find all children by name, recursively
        /// </summary>
        public static List<Transform> FindDeepChildren(this Transform aParent, string aName)
        {
            List<Transform> list = new List<Transform>();
            findDeepChildren(aName, aParent, list);
            return list;

            void findDeepChildren(string name, Transform t, List<Transform> lst)
            {
                if (t.name == name)
                    lst.Add(t);
                foreach (Transform c in t)
                    findDeepChildren(name, c, lst);
            }
        }

        /// <summary>
        /// Finds all game objects with a given name. No, I don't know what it's for either.
        /// </summary>
        /// <remarks>
        /// <para>Obsolete, consider <see cref="FindDeepChildren(Transform, string)"/> instead</para>
        /// </remarks>
        [Obsolete("Consider FindDeepChildren instead")]
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

        /// <summary>
        /// Adds a listener to an EventTrigger
        /// </summary>
        public static void AddListener(this EventTrigger trigger, EventTriggerType eventType, Action<BaseEventData> action)
        {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = eventType;
            entry.callback.AddListener(new UnityAction<BaseEventData>(action));
            trigger.triggers.Add(entry);
        }

    }
}
