using System;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Base class for components that handle facing sprites
    /// </summary>
    public abstract class FacingSpriteComponent : MonoBehaviour
    {
        [SerializeField, Tooltip("If not set, the attached object will be used")]
        protected Renderer Attachment = null;        
        [SerializeField, Tooltip("If set, rotates by half turn for compatibility with default quad. Does NOT affect choice of facing sprite")]
        protected bool ReverseBillboard = true;
        [SerializeField, Tooltip("If set, will simulate being billboarded in x/y rather than just x")]
        protected bool BillboardBothAxes = false;

        public FacingSpriteSizeMode SpriteSizeMode = default;
        [Tooltip("If >0, multiplies the sprite size by this value (exact effect depends on size mode)")]
        public float SpriteScale = 0;

        protected Vector2 InitialRendererScale;

        protected virtual void Start()
        {
            if (Attachment == null)
                Attachment = GetComponent<Renderer>();

            if (Attachment == null)
            {
                Debug.LogError($"{GetType().Name} on {name} can't find renderer!");
                enabled = false;
                return;
            }

            InitialRendererScale = Attachment.transform.localScale;
        }

        protected virtual void Update()
        {
            var targetTransform = FacingSpriteCameraCache.CameraTransform;
            Vector3 vecToTarget = targetTransform.position - Attachment.transform.position;
            Vector3 flatVecToTarget = new Vector3(vecToTarget.x, 0, vecToTarget.z).normalized;

            UpdateBillboard(vecToTarget, flatVecToTarget);

            Vector3 flatForwardVec = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
            Vector3 baseVecToTarget = targetTransform.position - transform.position;
            Vector3 flatBaseVecToTarget = new Vector3(baseVecToTarget.x, 0, baseVecToTarget.z).normalized;
            var quatFacing = Quaternion.FromToRotation(flatVecToTarget, flatForwardVec);
            float angle = quatFacing.eulerAngles.y;

            UpdateSprite(angle);

        }

        protected virtual void UpdateBillboard(Vector3 vecToTarget, Vector3 flatVecToTarget)
        {
            Quaternion quatToTarget;
            if (BillboardBothAxes)
            {
                Vector3 normVecToTarget = vecToTarget.normalized;
                quatToTarget = Quaternion.LookRotation(vecToTarget);
            }
            else
            {
                quatToTarget = Quaternion.LookRotation(flatVecToTarget);
            }

            //set rotation like billboard
            if (ReverseBillboard)
                quatToTarget *= Quaternion.AngleAxis(180, Vector3.up);

            Attachment.transform.rotation = quatToTarget;
        }

        protected abstract void UpdateSprite(float facingAngle);
        

        protected void SetSpriteOnRenderer(Sprite sprite, bool mirror)
        {
            //thunk!
            FacingSpriteUtils.SetSpriteOnQuad(Attachment, SpriteSizeMode, InitialRendererScale, SpriteScale, sprite, mirror);
        }

        protected virtual void LateUpdate()
        {
            FacingSpriteCameraCache.ResetCameraTransform();
        }
    }
}