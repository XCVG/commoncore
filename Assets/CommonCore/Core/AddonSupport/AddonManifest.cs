using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CommonCore
{

    /// <summary>
    /// Model class for an addon manifest
    /// </summary>
    public class AddonManifest
    {
        public string Name { get; set; } //package name
        public Version Version { get; set; } = new Version();

        //nice metadata
        public string Author { get; set; }
        public string Title { get; set; } //nice name
        public string Description { get; set; }

        public string MainAssembly { get; set; }

        public bool UseAssetBundlePaths { get; set; }
        public bool IgnoreSingleFileErrors { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }
    }
}