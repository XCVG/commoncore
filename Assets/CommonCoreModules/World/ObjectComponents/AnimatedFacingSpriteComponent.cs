using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace CommonCore.World
{

    /// <summary>
    /// Component that handles animated facing sprite
    /// </summary>
    public class AnimatedFacingSpriteComponent : FacingSpriteComponent
    {
        public SpriteFrame[] Frames = null;

        [Header("Animation Options")] //these are deliberately public
        public bool Animate = true;
        public float AnimationTimescale = 1;
        public bool LoopAnimation = true;
        public bool AnimateInRealtime = false;

        private int CurrentFrame = 0;
        private float TimeInFrame;

        protected override void Start()
        {
            base.Start();

            if (Frames == null || Frames.Length == 0)
            {
                Debug.LogError($"{GetType().Name} on {name} has no sprites!");
                enabled = false;
                return;
            }
        }

        protected override void Update()
        {
            base.Update();

            UpdateAnimation();
        }

        protected override void UpdateSprite(float facingAngle)
        {
            int numFrames = Frames.Length;

            if (Frames == null || numFrames == 0 || CurrentFrame > numFrames) //this will make it fail to update sprites if animation has stopped
                return;

            int currentFrame = CurrentFrame == numFrames ? CurrentFrame - 1 : CurrentFrame;
            var facingSprite = Frames[currentFrame].Sprite;

            if (facingSprite == null)
                return;

            var (sprite, mirror) = facingSprite.GetFacingSprite(facingAngle);
            SetSpriteOnRenderer(sprite, mirror);
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
            }

            if (CurrentFrame == Frames.Length && LoopAnimation)
                CurrentFrame = 0;
            
        }

        [Serializable]
        public struct SpriteFrame
        {
            public FacingSpriteAsset Sprite;
            public float Duration;
        }
    }
}