using System;
using UnityEngine;

namespace CommonCore.World
{
    /// <summary>
    /// Asset representing a set of "facing" (Doom-style) sprites
    /// </summary>
    /// <remarks>
    /// <para>1, 4, or 8 sprites are valid</para>
    /// <para>1 sprite=Front</para>
    /// <para>4 sprites=Front/Left/Back/Right</para>
    /// <para>These correspond, in order, to Doom facings 1/2/3/4/5/6/7/8</para>
    /// <para>16 sprites is not supported</para>
    /// <para>To use mirrored sprites (eg A2A8) put the sprite in the first slot and enable the corresponding Mirror* option</para>
    /// </remarks>
    [CreateAssetMenu(fileName = "New Facing Sprite", menuName = "CCScriptableObjects/FacingSpriteAsset")]
    public class FacingSpriteAsset : ScriptableObject
    {
        public Sprite Front;
        public Sprite FrontLeft;
        public Sprite Left;
        public Sprite BackLeft;
        public Sprite Back;
        public Sprite BackRight;
        public Sprite Right;
        public Sprite FrontRight;

        public bool MirrorBackRight = false;
        public bool MirrorRight = false;
        public bool MirrorFrontRight = false;

        public (Sprite sprite, bool mirror) GetFacingSprite(SpriteFacing facing)
        {
            switch (facing)
            {
                case SpriteFacing.None:
                    return (Front, false);
                case SpriteFacing.Front:
                    return (Front, false);
                case SpriteFacing.FrontLeft:
                    return (FrontLeft, false);
                case SpriteFacing.Left:
                    return (Left, false);
                case SpriteFacing.BackLeft:
                    return (BackLeft, false);
                case SpriteFacing.Back:
                    return (Back, false);
                case SpriteFacing.BackRight:
                    return (BackRight, MirrorBackRight);
                case SpriteFacing.Right:
                    return (Right, MirrorRight);
                case SpriteFacing.FrontRight:
                    return (FrontRight, MirrorFrontRight);
                default:
                    throw new NotImplementedException();
            }
        }

        public (Sprite sprite, bool mirror) GetFacingSprite(float angle)
        {
            if (FrontLeft != null)
            {
                //assume we have all 8 angles
                if (angle > 337.5f || angle <= 22.5f)
                    return (Front, false);
                else if (angle > 22.5f && angle <= 67.5f)
                    return (FrontLeft, false);
                else if (angle > 67.5f && angle <= 112.5f)
                    return (Left, false);
                else if (angle > 112.5f && angle <= 157.5f)
                    return (BackLeft, false);
                else if (angle > 157.5f && angle <= 202.5f)
                    return (Back, false);
                else if (angle > 202.5f && angle <= 247.5f)
                    return (BackRight.Ref() ?? BackLeft, MirrorBackRight);
                else if (angle > 247.5f && angle <= 292.5f)
                    return (Right.Ref() ?? Left, MirrorRight);
                else if (angle > 292.5f && angle <= 337.5f)
                    return (FrontRight.Ref() ?? FrontLeft, MirrorFrontRight);
                else
                    throw new Exception();
            }
            else if (Left != null)
            {
                //assume we have all 4 angles
                if (angle > 337.5f || angle <= 45f)
                    return (Front, false);
                else if (angle > 45f && angle <= 135f)
                    return (Left, false);
                else if (angle > 135f && angle <= 225f)
                    return (Back, false);
                else if (angle > 225f && angle <= 315f)
                    return (Right.Ref() ?? Left, MirrorRight);
                else
                    throw new Exception();
            }
            else
            {
                return (Front, false);
            }

            throw new NotImplementedException();
        }
    }
}