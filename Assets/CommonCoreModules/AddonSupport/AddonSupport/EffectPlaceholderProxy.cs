using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.AddonSupport
{

    /// <summary>
    /// Proxy component that spawns an effect 
    /// </summary>
    public class EffectPlaceholderProxy : MonoBehaviour
    {
        [SerializeField]
        private string EffectId = null;
        [SerializeField]
        private bool UseUniqueId = false;
        [SerializeField]
        private bool ParentToThis = false;

        //we actually don't proxy anything, we call SpawnEffect through ForwardedUtils

        private void Start()
        {
            ForwardedUtils.SpawnEffect(EffectId, transform.position, transform.rotation, ParentToThis ? transform : null, UseUniqueId);
        }
    }
}