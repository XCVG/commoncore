using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Config
{

    //setting group structs
    //note that capitalization matches that in UnityEngine.QualitySettings

    /// <summary>
    /// Shadow quality struct
    /// </summary>
    public struct ShadowQuality
    {
        public UnityEngine.ShadowQuality shadows;
        public int shadowCascades;
        public ShadowResolution shadowResolution;
        public ShadowmaskMode shadowmaskMode;

        public static readonly IReadOnlyDictionary<QualityLevel, ShadowQuality> Presets = new Dictionary<QualityLevel, ShadowQuality>()
        {
            { QualityLevel.VeryLow, new ShadowQuality() { shadows = UnityEngine.ShadowQuality.Disable, shadowCascades = 1, shadowmaskMode = ShadowmaskMode.Shadowmask, shadowResolution = ShadowResolution.Low} },
            { QualityLevel.Low, new ShadowQuality() { shadows = UnityEngine.ShadowQuality.HardOnly, shadowCascades = 1, shadowmaskMode = ShadowmaskMode.Shadowmask, shadowResolution = ShadowResolution.Low} },
            { QualityLevel.Medium, new ShadowQuality() { shadows = UnityEngine.ShadowQuality.All, shadowCascades = 2, shadowmaskMode = ShadowmaskMode.DistanceShadowmask, shadowResolution = ShadowResolution.Medium} },
            { QualityLevel.High, new ShadowQuality() { shadows = UnityEngine.ShadowQuality.All, shadowCascades = 2, shadowmaskMode = ShadowmaskMode.DistanceShadowmask, shadowResolution = ShadowResolution.High} },
            { QualityLevel.Ultra, new ShadowQuality() { shadows = UnityEngine.ShadowQuality.All, shadowCascades = 4, shadowmaskMode = ShadowmaskMode.DistanceShadowmask, shadowResolution = ShadowResolution.VeryHigh} }
        };

    }

    /// <summary>
    /// Lighting quality struct
    /// </summary>
    public struct LightingQuality
    {
        public int pixelLightCount;
        public bool realtimeReflectionProbes;

        public static IReadOnlyDictionary<QualityLevel, LightingQuality> Presets = new Dictionary<QualityLevel, LightingQuality>()
        {
            { QualityLevel.VeryLow, new LightingQuality() { pixelLightCount = 0, realtimeReflectionProbes = false } },
            { QualityLevel.Low, new LightingQuality() { pixelLightCount = 1, realtimeReflectionProbes = false } },
            { QualityLevel.Medium, new LightingQuality() { pixelLightCount = 2, realtimeReflectionProbes = true } },
            { QualityLevel.High, new LightingQuality() { pixelLightCount = 3, realtimeReflectionProbes = true } },
            { QualityLevel.Ultra, new LightingQuality() { pixelLightCount = 4, realtimeReflectionProbes = true } }
        };
    }

    /// <summary>
    /// Mesh quality struct
    /// </summary>
    public struct MeshQuality
    {
        public BlendWeights blendWeights;
        public int maximumLODLevel;
        public float lodBias;

        public static IReadOnlyDictionary<QualityLevel, MeshQuality> Presets = new Dictionary<QualityLevel, MeshQuality>()
        {
            { QualityLevel.VeryLow, new MeshQuality() { blendWeights = BlendWeights.OneBone, lodBias = 0.3f, maximumLODLevel = 0 } },
            { QualityLevel.Low, new MeshQuality() { blendWeights = BlendWeights.TwoBones, lodBias = 0.7f, maximumLODLevel = 0 } },
            { QualityLevel.Medium, new MeshQuality() { blendWeights = BlendWeights.TwoBones, lodBias = 1.0f, maximumLODLevel = 0 } },
            { QualityLevel.High, new MeshQuality() { blendWeights = BlendWeights.FourBones, lodBias = 1.5f, maximumLODLevel = 0 } },
            { QualityLevel.Ultra, new MeshQuality() { blendWeights = BlendWeights.FourBones, lodBias = 2.0f, maximumLODLevel = 0 } },
        };
    }

    /// <summary>
    /// Rendering quality struct
    /// </summary>
    public struct RenderingQuality
    {
        public int particleRaycastBudget;
        public bool softParticles;
        public bool billboardsFaceCameraPosition;
        public bool softVegetation;

        public static IReadOnlyDictionary<QualityLevel, RenderingQuality> Presets = new Dictionary<QualityLevel, RenderingQuality>()
        {
            { QualityLevel.VeryLow, new RenderingQuality() { particleRaycastBudget = 4, billboardsFaceCameraPosition = false, softParticles = false, softVegetation = false } },
            { QualityLevel.Low, new RenderingQuality() { particleRaycastBudget = 64, billboardsFaceCameraPosition = false, softParticles = false, softVegetation = false } },
            { QualityLevel.Medium, new RenderingQuality() { particleRaycastBudget = 256, billboardsFaceCameraPosition = true, softParticles = false, softVegetation = true } },
            { QualityLevel.High, new RenderingQuality() { particleRaycastBudget = 1024, billboardsFaceCameraPosition = true, softParticles = true, softVegetation = true } },
            { QualityLevel.Ultra, new RenderingQuality() { particleRaycastBudget = 4096, billboardsFaceCameraPosition = true, softParticles = true, softVegetation = true } }
        };
    }

    /// <summary>
    /// Texture scale values
    /// </summary>
    public enum TextureScale
    {
        Full = 0, Half = 1, Quarter = 2, Eighth = 3 
    }

    /// <summary>
    /// Quality levels
    /// </summary>
    public enum QualityLevel
    {
        VeryLow, Low, Medium, High, Ultra
    }
}