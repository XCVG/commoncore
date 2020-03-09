using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Caches the main camera for billboard sprites so we don't call expensive GetActiveCamera repeatedly throughout the frame
    /// </summary>
    public static class FacingSpriteCameraCache
    {
        public static Transform CameraTransform
        {
            get
            {
                if (_CameraTransform == null)
                {
                    _CameraTransform = WorldUtils.GetActiveCamera().Ref()?.transform;
                }

                return _CameraTransform;
            }
        }

        private static Transform _CameraTransform;

        public static void ResetCameraTransform()
        {
            _CameraTransform = null;
        }

    }
}