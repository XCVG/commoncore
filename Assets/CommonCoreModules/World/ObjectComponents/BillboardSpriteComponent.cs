using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Script that "billboards" a 2D renderer by rotating it to face the player
    /// </summary>
    public class BillboardSpriteComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("If not set, the attached object will be used")]
        protected Transform Attachment = null;
        [SerializeField, Tooltip("If set, rotates by half turn for compatibility with default quad")]
        protected bool Reverse = true;
        [SerializeField, Tooltip("If set, will simulate being billboarded in x/y rather than just x")]
        protected bool BillboardBothAxes = false;

        protected virtual void Start()
        {
            if (Attachment == null)
                Attachment = transform;
        }

        protected virtual void Update()
        {
            var targetTransform = FacingSpriteCameraCache.CameraTransform;

            Quaternion quatToTarget;
            Vector3 vecToTarget = targetTransform.position - Attachment.position;
            if (BillboardBothAxes)
            {
                Vector3 normVecToTarget = vecToTarget.normalized;
                quatToTarget = Quaternion.LookRotation(vecToTarget);
            }
            else
            {
                Vector3 flatVecToTarget = new Vector3(vecToTarget.x, 0, vecToTarget.z).normalized;
                quatToTarget = Quaternion.LookRotation(flatVecToTarget);
            }
            
            if (Reverse)
                quatToTarget *= Quaternion.AngleAxis(180, Vector3.up);
            Attachment.rotation = quatToTarget;
        }

        private void LateUpdate()
        {
            FacingSpriteCameraCache.ResetCameraTransform();
        }
    }
}