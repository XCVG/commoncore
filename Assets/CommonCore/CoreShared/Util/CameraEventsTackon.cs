using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /// <summary>
    /// Tack-on script for receiving camera events and propagating them through CameraEventsManager
    /// </summary>
    public class CameraEventsTackon : MonoBehaviour
    {
        private Camera AttachedCamera; 

        private void Start()
        {
            if (AttachedCamera == null)
                AttachedCamera = GetComponent<Camera>();
        }

        private void OnPreCull()
        {
            CameraEventsManager.ExecuteOnPreCull(AttachedCamera);
        }
    }
}