using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{

    /// <summary>
    /// Component that handles a static (non-animated) facing sprite
    /// </summary>
    public class StaticFacingSpriteComponent : FacingSpriteComponent
    {
        public FacingSpriteAsset Sprites = null;
        public bool Bright = false;

        protected override void Start()
        {
            base.Start();

            if(Sprites == null)
            {
                Debug.LogError($"{GetType().Name} on {name} has no sprites!");
                enabled = false;
                return;
            }
        }

        protected override void UpdateSprite(float facingAngle)
        {
            var (sprite, mirror) = Sprites.GetFacingSprite(facingAngle);
            SetSpriteOnRenderer(sprite, mirror, Bright);
        }
    }
}