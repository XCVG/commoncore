
using CommonCore.ResourceManagement;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace CommonCore
{

    /// <summary>
    /// Load data for an addon (TODO)
    /// </summary>
    public class AddonLoadData
    {
        public IReadOnlyList<Assembly> LoadedAssemblies { get; private set; }
        public IReadOnlyDictionary<string, ResourceHandle> LoadedResources { get; private set; }

        public AddonLoadData(IEnumerable<Assembly> assemblies, IDictionary<string, ResourceHandle> resources)
        {
            LoadedAssemblies = assemblies.ToImmutableArray();
            LoadedResources = resources.ToImmutableDictionary();
        }
    }

    /// <summary>
    /// Context data for addon load
    /// </summary>
    public class AddonLoadContext
    {
        public string Path { get; set; }

        public bool AbortOnSingleFileFailure { get; set; } = true;

        public AddonManifest Manifest { get; set; }
        public Assembly MainAssembly { get; set; }
        public Type AddonBaseType { get; set; }
        public AddonBase AddonBase { get; set; }

        public List<Assembly> LoadedAssemblies { get; private set; } = new List<Assembly>();
        public Dictionary<string, ResourceHandle> LoadedResources { get; private set; } = new Dictionary<string, ResourceHandle>();

        //references
        public AddonManager AddonManager { get; set; }
        public Action<AddonLoadData> OnLoadedCallback { get; set; }

        public ResourcePriority ResourcePriority { get; set; }
        public string MountPathOverride { get; set; } = null;

        public string MountPath => string.IsNullOrEmpty(MountPathOverride) ? $"Addons/{Manifest.Name}" : MountPathOverride;

        
    }
}