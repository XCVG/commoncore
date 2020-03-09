using System;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Script that "billboards" a 2D renderer by rotating it to face the player, supports animation
    /// </summary>
    /// <remarks>Note that this imposes more requirements such as requiring a renderer, similar to FacingSprite system</remarks>
    public class AnimatedBillboardSpriteComponent : BillboardSpriteComponent
    {
        [SerializeField]
        private Renderer Renderer;

        public SpriteFrame[] Frames = null;
        public FacingSpriteSizeMode SpriteSizeMode = default;
        [Tooltip("If >0, multiplies the sprite size by this value (exact effect depends on size mode)")]
        public float SpriteScale = 0;

        [Header("Animation Options")] //these are deliberately public
        public bool Animate = true;
        public float AnimationTimescale = 1;
        public bool LoopAnimation = true;
        public bool AnimateInRealtime = false;

        private int CurrentFrame = 0;
        private float TimeInFrame = 0;
        private bool FrameChanged = false;
        private Vector2 InitialRendererScale;

        protected override void Start()
        {
            base.Start();

            if (Renderer == null)
                Renderer = Attachment.GetComponent<Renderer>();

            if (Renderer == null)
            {
                Debug.LogError($"{GetType().Name} on {name} has no Renderer!");
                enabled = false;
                return;
            }

            FrameChanged = true;

            InitialRendererScale = Renderer.transform.localScale;
        }

        protected override void Update()
        {
            base.Update();

            UpdateSprite();

            UpdateAnimation();
        }

        private void UpdateSprite()
        {
            if (!FrameChanged || Frames == null || CurrentFrame >= Frames.Length)
                return;

            var sprite = Frames[CurrentFrame].Sprite;

            if (sprite == null)
                return;

            FacingSpriteUtils.SetSpriteOnQuad(Renderer, SpriteSizeMode, InitialRendererScale, SpriteScale, sprite, false);

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

        [Serializable]
        public struct SpriteFrame
        {
            public Sprite Sprite;
            public float Duration;
        }
    }
}