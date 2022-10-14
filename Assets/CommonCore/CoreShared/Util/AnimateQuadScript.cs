using System;
using UnityEngine;

namespace CommonCore.Util
{

    /// <summary>
    /// Animates a texture on a quad
    /// </summary>

    public class AnimateQuadScript : MonoBehaviour
    {

        [SerializeField]
        private Renderer Attachment = null;

        public TextureFrame[] Frames = null;

        [Header("Animation Options")] //these are deliberately public
        public bool Animate = true;
        public float AnimationTimescale = 1;
        public bool LoopAnimation = true;
        public bool AnimateInRealtime = false;

        [Header("Quad Options")]
        public bool SetEmissive = false;


        private int CurrentFrame = 0;
        private float TimeInFrame = 0;
        private bool FrameChanged = false;

        private void Start()
        {
            if (Attachment == null)
                Attachment = GetComponent<Renderer>();
            if (Attachment == null)
                Attachment = GetComponentInChildren<Renderer>(); //should this allow inactive?
            if(Attachment == null)
            {
                Debug.LogError($"[{nameof(AnimateQuadScript)}] has no attachment!");
                enabled = false;
            }

            SetTexture(Frames[0].Texture);
        }

        private void Update()
        {
            UpdateSprite();

            UpdateAnimation();
        }

        private void UpdateSprite()
        {
            if (!FrameChanged || Frames == null || CurrentFrame >= Frames.Length)
                return;

            var tex = Frames[CurrentFrame].Texture;

            if (tex == null)
                return;

            SetTexture(tex);

            FrameChanged = false;
        }

        private void UpdateAnimation()
        {
            if (!Animate || AnimationTimescale == 0 || CurrentFrame >= Frames.Length)
                return;

            TimeInFrame += (AnimateInRealtime ? Time.unscaledDeltaTime : Time.deltaTime) * AnimationTimescale;

            if (TimeInFrame > Frames[CurrentFrame].Duration)
            {
                TimeInFrame = 0;
                CurrentFrame++;
                FrameChanged = true;
            }

            if (CurrentFrame == Frames.Length && LoopAnimation)
                CurrentFrame = 0;

        }

        private void SetTexture(Texture2D tex)
        {
            var material = Attachment.material;

            material.mainTexture = tex;

            if (SetEmissive)
                material.SetTexture("_EmissionMap", tex);
        }

        [Serializable]
        public struct TextureFrame
        {
            public Texture2D Texture;
            public float Duration;
        }
    }
}