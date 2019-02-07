using System;
using UnityEngine;

namespace CommonCore.Config
{

    //setting group structs
    //note that capitalization matches that in UnityEngine.QualitySettings

    public struct ShadowQuality
    {
        public UnityEngine.ShadowQuality shadows;
        public int shadowCascades;
        public float shadowDistance;
        public ShadowResolution shadowResolution;
    }

    public struct LightingQuality
    {
        public int pixelLightCount;
        public bool realtimeReflectionProbes;
        public int particleRaycastBudget;
        public bool softParticles;
    }

    public struct TextureQuality
    {
        public int masterTextureLimit;
        public AnisotropicFiltering anisotropicFiltering;
        public bool billboardsFaceCameraPosition;
        public bool softVegetation;
        public float lodBias;
    }

    public enum QualityLevel
    {
        Low, Medium, High, Ultra
    }
}