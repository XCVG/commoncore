using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.State;
using UnityEngine.SceneManagement;

namespace CommonCore.World
{
    [Serializable]
    public class EditorMapMarker
    {
        public string Id;
        public string NiceName;
        public string IconOverride;
        public Vector3 WorldPoint;
        public bool ForceShow;
    }

    public class Cartographer : MonoBehaviour
    {
        public Rect WorldBounds;
        public float WorldRotation;
        public bool ShowPlayer;

        [Header("Map Options")]
        public bool UseResourceFallback = true;
        public Texture2D Map;

        [Header("Markers")]
        public EditorMapMarker[] Markers;

        //transforms a world position into normalized (-1, 1) space based on world bounds
        public Vector2 NormalizeWorldPosition(Vector3 worldPos)
        {
            Vector2 worldPos2 = worldPos.ToFlatVec();
            Vector2 centerToPos = worldPos2 - new Vector2(WorldBounds.x, WorldBounds.y);
            centerToPos = Quaternion.Euler(0, 0, WorldRotation) * centerToPos;
            float xComponent = centerToPos.x / (WorldBounds.width / 2f);
            float yComponent = centerToPos.y / (WorldBounds.height / 2f);
            return new Vector2(xComponent, yComponent);
        }

        public bool WorldPositionInBounds(Vector3 worldPos)
        {
            return NormalizedPositionInBounds(NormalizeWorldPosition(worldPos));
        }

        public bool NormalizedPositionInBounds(Vector2 normalizedPos)
        {
            return (normalizedPos.x <= 1 && normalizedPos.x >= -1 && normalizedPos.y <= 1 && normalizedPos.y >= -1);
        }
        
        public Texture2D MapTexture
        {
            get
            {
                if (Map != null)
                {
                    return Map;
                }
                else if (UseResourceFallback)
                {
                    string mapPath = "Maps/" + SceneManager.GetActiveScene().name;
                    if (CCBaseUtil.CheckResource<Texture2D>(mapPath))
                    {
                        Texture2D mapTexture = CCBaseUtil.LoadResource<Texture2D>(mapPath);
                        return mapTexture;
                    }
                }

                return null;
            }
        }

        public static Cartographer Current
        {
            get
            {
                Transform worldRoot = CCBaseUtil.GetWorldRoot();
                if(worldRoot != null)
                {
                    Cartographer cartographer = worldRoot.GetComponent<Cartographer>();
                    if(cartographer != null)
                    {
                        return cartographer;
                    }

                }

                return null;
            }
        }
    }
}