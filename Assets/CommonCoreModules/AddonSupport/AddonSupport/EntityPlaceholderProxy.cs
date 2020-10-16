using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CommonCore.AddonSupport
{

    /// <summary>
    /// Proxy component that spawns an entity 
    /// </summary>
    public class EntityPlaceholderProxy : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField, ProxyField, Tooltip("The entity to spawn")]
        private string FormID = null;
        [SerializeField, ProxyField, Tooltip("TID, leave empty to use auto TID or placeholder's TID")]
        private string ThingID = null;
        [SerializeField, ProxyField, Tooltip("If set and TID is empty, uses this placeholder's TID instead of auto TID")]
        private bool UsePlaceholderTID = true;

        [SerializeField, ProxyField]
        private bool DestroyPlaceholder = true;
#pragma warning restore 0414

        //we actually just create an EntityPlaceholder

        private void Awake()
        {
            Type placeholderType = CCBase.BaseGameTypes
                    .Where(t => t.FullName == "CommonCore.World.EntityPlaceholder")
                    .Single();
            MonoBehaviour controller = (MonoBehaviour)gameObject.AddComponent(placeholderType);
            ProxyUtils.SetProxyFields(this, controller);
        }
    }
}