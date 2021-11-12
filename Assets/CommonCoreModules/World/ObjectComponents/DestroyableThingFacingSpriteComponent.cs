using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Component that allows DestroyableThing to use FacingSprite animation
    /// </summary>
    public class DestroyableThingFacingSpriteComponent : FacingSpriteComponent
    {
        public bool InvertBrightFlag = false;

        [Header("Animation Options")]
        public bool Animate = true;
        public bool LoopIdle = true;
        //public bool AutoTransition = true; //auto-transition is implicit for destroyable things
        public float AnimationTimescale = 1;
        public bool AnimateInRealtime = false;

        [Header("Animations"), SerializeField]
        private SpriteFrame[] Idle = null;
        [SerializeField]
        private SpriteFrame[] Dead = null;
        [SerializeField]
        private SpriteFrame[] Dying = null;
        [SerializeField]
        private SpriteFrame[] Hurting = null;

        private int CurrentFrame = 0;
        private SpriteFrame[] CurrentFrameSet = null;
        private float TimeInFrame = 0;

        protected override void Start()
        {
            base.Start();

            if (CurrentFrameSet == null)
                SetState(DestroyableThingState.Idle);
        }

        protected override void Update()
        {
            base.Update();

            UpdateAnimation();
        }

        protected override void UpdateSprite(float facingAngle)
        {
            int numFrames = CurrentFrameSet.Length;

            if (CurrentFrameSet == null || numFrames == 0 || CurrentFrame > numFrames)
                return;

            int currentFrame = CurrentFrame == numFrames ? CurrentFrame - 1 : CurrentFrame;
            var facingSprite = CurrentFrameSet[currentFrame].Sprite;

            if (facingSprite == null)
                return;

            var (sprite, mirror) = facingSprite.GetFacingSprite(facingAngle);
            SetSpriteOnRenderer(sprite, mirror, CurrentFrameSet[currentFrame].Bright ^ InvertBrightFlag);
        }

        private void UpdateAnimation()
        {
            if (!Animate || AnimationTimescale == 0 || CurrentFrame >= CurrentFrameSet.Length)
                return;

            TimeInFrame += (AnimateInRealtime ? Time.unscaledDeltaTime : Time.deltaTime) * AnimationTimescale;

            if (TimeInFrame > CurrentFrameSet[CurrentFrame].Duration)
            {
                TimeInFrame = 0;
                CurrentFrame++;
            }

            if (CurrentFrame == CurrentFrameSet.Length) //we are at the end
            {
                if (CurrentFrameSet == Idle && LoopIdle)
                {
                    CurrentFrame = 0;
                }
                else if (CurrentFrameSet == Hurting)
                {
                    CurrentFrameSet = Idle;
                    CurrentFrame = 0;
                }
                else if(CurrentFrameSet == Dying)
                {
                    CurrentFrameSet = Dead;
                    CurrentFrame = 0;
                }
            }
        }

        public void SetState(DestroyableThingState state)
        {
            switch (state)
            {
                case DestroyableThingState.Idle:
                    CurrentFrame = 0;
                    TimeInFrame = 0;
                    CurrentFrameSet = Idle;
                    break;
                case DestroyableThingState.Hurting:
                    CurrentFrame = 0;
                    TimeInFrame = 0;
                    CurrentFrameSet = Hurting;
                    break;
                case DestroyableThingState.Dying:
                    CurrentFrame = 0;
                    TimeInFrame = 0;
                    CurrentFrameSet = Dying;
                    break;
                case DestroyableThingState.Dead:
                    CurrentFrame = 0;
                    TimeInFrame = 0;
                    CurrentFrameSet = Dead;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [Serializable]
        public struct SpriteFrame
        {
            public FacingSpriteAsset Sprite;
            public float Duration;
            public bool Bright;
        }
    }
}