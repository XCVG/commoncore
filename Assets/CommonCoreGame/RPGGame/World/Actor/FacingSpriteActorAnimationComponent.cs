using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.World;
using System;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Handles the animations for an Actor using 2D sprites
    /// </summary>
    public class FacingSpriteActorAnimationComponent : ActorAnimationComponentBase
    {
        [Header("Components"), SerializeField]
        private Renderer Attachment;

        [SerializeField, Header("Billboard Options"), Tooltip("If set, rotates by half turn for compatibility with default quad. Does NOT affect choice of facing sprite")]
        protected bool ReverseBillboard = true;
        [SerializeField, Tooltip("If set, will simulate being billboarded in x/y rather than just x")]
        protected bool BillboardBothAxes = false;
        [SerializeField, Tooltip("If unset, dead state will not use billboard logic")]
        protected bool BillboardCorpse = true;

        public FacingSpriteSizeMode SpriteSizeMode = default;
        [Tooltip("If >0, multiplies the sprite size by this value (exact effect depends on size mode)")]
        public float SpriteScale = 0;       

        [Header("Animation Options")]
        public bool Animate = true;
        public bool LoopIdle = true;
        public bool AutoTransition = true;        
        public float AnimationTimescale = 1;
        public bool AnimateInRealtime = false;

        //Idle, Dead, Dying, Walking, Running, Hurting, Talking, Shooting, Punching, Pickup
        [Header("Animations"), SerializeField]
        private SpriteFrame[] Idle = null;
        [SerializeField]
        private SpriteFrame[] Dead = null;
        [SerializeField]
        private SpriteFrame[] Dying = null;
        [SerializeField]
        private SpriteFrame[] Walking = null;
        [SerializeField]
        private SpriteFrame[] Running = null;
        [SerializeField]
        private SpriteFrame[] Hurting = null;
        [SerializeField]
        private SpriteFrame[] Talking = null;
        [SerializeField]
        private SpriteFrame[] Shooting = null;
        [SerializeField]
        private SpriteFrame[] Punching = null;

        [SerializeField]
        private SpriteFrameSequence[] ExtraAnimations = null;

        private bool LoopCurrentFrameSet = false;
        private SpriteFrame[] NextFrameSet = null;
        private int CurrentFrame = 0;
        private SpriteFrame[] CurrentFrameSet = null;
        private float TimeInFrame = 0;

        private Vector2 InitialRendererScale;

        protected override void Update()
        {
            base.Update();

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

            UpdateAnimation();
        }

        private void UpdateBillboard(Vector3 vecToTarget, Vector3 flatVecToTarget)
        {
            if(!BillboardCorpse && CurrentFrameSet == Dead)
            {
                Attachment.transform.localRotation = ReverseBillboard ? (Quaternion.identity * Quaternion.AngleAxis(180, Vector3.up)) : Quaternion.identity;
                return;
            }

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

        private void UpdateSprite(float facingAngle)
        {
            int numFrames = CurrentFrameSet.Length;

            if (CurrentFrameSet != null && numFrames > 0 && CurrentFrame <= numFrames)
            {
                int frameIndex = CurrentFrame == numFrames ? CurrentFrame - 1 : CurrentFrame; //this weirdness should allow sprite updates to work if animation is done

                var facingSprite = CurrentFrameSet[frameIndex].Sprite;

                if (facingSprite == null)
                    return;

                if (!BillboardCorpse && CurrentFrameSet == Dead)
                {
                    FacingSpriteUtils.SetSpriteOnQuad(Attachment, SpriteSizeMode, InitialRendererScale, SpriteScale, facingSprite.Front, false);
                }
                else
                {
                    var (sprite, mirror) = facingSprite.GetFacingSprite(facingAngle);
                    //SetSpriteOnRenderer(sprite, mirror);
                    FacingSpriteUtils.SetSpriteOnQuad(Attachment, SpriteSizeMode, InitialRendererScale, SpriteScale, sprite, mirror);
                }
            }
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

            if(CurrentFrame == CurrentFrameSet.Length) //we are at the end
            {
                if(LoopCurrentFrameSet)
                {
                    CurrentFrame = 0;
                }
                else if(AutoTransition && NextFrameSet != null)
                {
                    if (NextFrameSet == Idle && LoopIdle)
                        LoopCurrentFrameSet = true;

                    StartAnimationSequence(NextFrameSet);                    
                    NextFrameSet = null;
                }
            }

        }

        public override void SetAnimationForced(ActorAnimState state)
        {
            switch (state)
            {
                case ActorAnimState.Idle:
                    LoopCurrentFrameSet = true;
                    NextFrameSet = null;
                    StartAnimationSequence(Idle);
                    break;
                case ActorAnimState.Dead:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = null;
                    StartAnimationSequence(Dead);
                    break;
                case ActorAnimState.Dying:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = Dead;
                    StartAnimationSequence(Dying);
                    break;
                case ActorAnimState.Walking:
                    LoopCurrentFrameSet = true;
                    NextFrameSet = null;
                    StartAnimationSequence(Walking);
                    break;
                case ActorAnimState.Running:
                    LoopCurrentFrameSet = true;
                    NextFrameSet = null;
                    StartAnimationSequence(Running);
                    break;
                case ActorAnimState.Hurting:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = Idle;
                    StartAnimationSequence(Hurting);
                    break;
                case ActorAnimState.Talking:
                    LoopCurrentFrameSet = true;
                    NextFrameSet = null;
                    StartAnimationSequence(Talking);
                    break;
                case ActorAnimState.Shooting:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = Idle;
                    StartAnimationSequence(Shooting);
                    break;
                case ActorAnimState.Punching:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = Idle;
                    StartAnimationSequence(Punching);
                    break;
                default:
                    LoopCurrentFrameSet = false;
                    NextFrameSet = null;
                    StartAnimationSequence(null);
                    Debug.LogWarning($"Failed to set animation sequence {state} on {name} because it is not supported by {GetType().Name}!");
                    break;
            }

            if(CurrentFrameSet == null || CurrentFrameSet.Length == 0)
                Debug.LogWarning($"{GetType().Name} on {name} has no frames for state \"{state}\"!");

        }

        public override void SetAnimationForced(string stateName)
        {
            if(Enum.TryParse<ActorAnimState>(stateName, true, out var state))
            {
                //we thunk backwards compared to what ActorAnimationComponent does
                SetAnimationForced(state);
            }
            else
            {
                if(ExtraAnimations != null && ExtraAnimations.Length > 0)
                {
                    foreach(var animation in ExtraAnimations)
                    {
                        if(animation.Name.Equals(stateName, StringComparison.OrdinalIgnoreCase))
                        {
                            LoopCurrentFrameSet = animation.Loop;
                            NextFrameSet = animation.ReturnToIdle ? Idle : null;

                            StartAnimationSequence(animation.Frames);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to set animation sequence {stateName} on {name} because it could not be parsed and {GetType().Name} does not have it as an extra animation!");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Failed to set animation sequence {stateName} on {name} because it could not be parsed and {GetType().Name} has no extra animations!");
                }
            }
        }

        private void StartAnimationSequence(SpriteFrame[] frameSet)
        {
            CurrentFrame = 0;
            TimeInFrame = 0;
            CurrentFrameSet = frameSet;
        }

        [Serializable]
        public struct SpriteFrameSequence
        {
            public string Name;
            public SpriteFrame[] Frames;
            public bool Loop;
            public bool ReturnToIdle;
        }

        [Serializable]
        public struct SpriteFrame
        {
            public FacingSpriteAsset Sprite;
            public float Duration;
        }
    }
}