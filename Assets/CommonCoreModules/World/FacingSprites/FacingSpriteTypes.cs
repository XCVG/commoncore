using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Enum representing the facing sprite positions/angles
    /// </summary>
    public enum SpriteFacing //these match Doom number and order
    {
        None, Front, FrontLeft, Left, BackLeft, Back, BackRight, Right, FrontRight
    }

    /// <summary>
    /// Enum representing the ways facing sprites can be scaled on the renderer
    /// </summary>
    public enum FacingSpriteSizeMode
    {
        //may also use this for billboard sprites

        /// <summary>
        /// Do not scale the renderer based on sprite size
        /// </summary>
        None,
        /// <summary>
        /// Scale the renderer based on the size of the sprite only
        /// </summary>
        SpriteSize,
        /// <summary>
        /// Scale the renderer so that the sprite remains in proportion and renderer height is maintained
        /// </summary>
        FitHeight,
        /// <summary>
        /// Scale the renderer so that the sprite remains in proportion and renderer width is maintained
        /// </summary>
        FitWidth
    }

    public static class FacingSpriteUtils
    {
        /// <summary>
        /// Gets the facing for a given angle
        /// </summary>
        public static SpriteFacing GetFacingForAngle(float angle)
        {
            //Not all that useful. We actually need to pass angle directly; we may not even need SpriteFacing

            if (angle > 337.5f || angle <= 22.5f)
                return SpriteFacing.Front;
            else if (angle > 22.5f && angle <= 67.5f)
                return SpriteFacing.FrontLeft;
            else if (angle > 67.5f && angle <= 112.5f)
                return SpriteFacing.Left;
            else if (angle > 112.5f && angle <= 157.5f)
                return SpriteFacing.BackLeft;
            else if (angle > 157.5f && angle <= 202.5f)
                return SpriteFacing.Back;
            else if (angle > 202.5f && angle <= 247.5f)
                return SpriteFacing.BackRight;
            else if (angle > 247.5f && angle <= 292.5f)
                return SpriteFacing.Right;
            else if (angle > 292.5f && angle <= 337.5f)
                return SpriteFacing.FrontRight;
            else
                throw new Exception();

        }

        /// <summary>
        /// Sets the sprite on a quad renderer and rescales/repositions based on offsets, scale, and scale options
        /// </summary>
        public static void SetSpriteOnQuad(Renderer renderer, FacingSpriteSizeMode spriteSizeMode, Vector2 initialRendererScale, float spriteScale, Sprite sprite, bool mirror)
        {
            var texture = sprite.Ref()?.texture;
            if (renderer.material.mainTexture != texture) //skip a bunch of stuff if we've already assigned the right texture
            {
                renderer.material.mainTexture = texture;

                Vector2 newScale = new Vector2(Mathf.Abs(initialRendererScale.x), Mathf.Abs(initialRendererScale.y));
                Vector2 newOffset = renderer.transform.localPosition;
                if (sprite != null)
                {
                    if (sprite.packed)
                        Debug.LogWarning($"Sprites attached to {renderer.name} are packed, which will yield unexpected results!");

                    switch (spriteSizeMode)
                    {
                        case FacingSpriteSizeMode.None:
                            //nop
                            break;
                        case FacingSpriteSizeMode.SpriteSize:
                            {
                                newScale = new Vector2(texture.width / sprite.pixelsPerUnit, texture.height / sprite.pixelsPerUnit);
                                if (spriteScale > 0)
                                    newScale *= spriteScale;
                                newOffset.x = (sprite.pivot.x - texture.width / 2) / sprite.pixelsPerUnit * spriteScale;
                                newOffset.y = (newScale.y / 2f) - sprite.pivot.y / sprite.pixelsPerUnit * spriteScale;
                            }
                            break;
                        case FacingSpriteSizeMode.FitHeight:
                            {
                                newScale = new Vector2(texture.width / sprite.pixelsPerUnit, texture.height / sprite.pixelsPerUnit);
                                float rescale = initialRendererScale.y / newScale.y;
                                newScale *= rescale;

                                newOffset.x = (sprite.pivot.x - texture.width / 2) / sprite.pixelsPerUnit * rescale;
                                newOffset.y = (newScale.y / 2f) - sprite.pivot.y / sprite.pixelsPerUnit * rescale;
                            }
                            break;
                        case FacingSpriteSizeMode.FitWidth:
                            {
                                newScale = new Vector2(texture.width / sprite.pixelsPerUnit, texture.height / sprite.pixelsPerUnit);
                                float rescale = initialRendererScale.x / newScale.x;
                                newScale *= rescale;

                                newOffset.x = (sprite.pivot.x - texture.width / 2) / sprite.pixelsPerUnit * rescale;
                                newOffset.y = (newScale.y / 2f) - sprite.pivot.y / sprite.pixelsPerUnit * rescale;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                renderer.transform.localScale = new Vector3(Mathf.Abs(newScale.x) * Mathf.Sign(initialRendererScale.x) * (mirror ? -1 : 1),
                    Mathf.Abs(newScale.y) * Mathf.Sign(initialRendererScale.y),
                    renderer.transform.localScale.z);

                renderer.transform.localPosition = new Vector3(newOffset.x, newOffset.y, renderer.transform.localPosition.z);
            }
        }


    }

}