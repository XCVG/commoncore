using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace CommonCore.XSMP
{

    /// <summary>
    /// Model class representing a playlist
    /// </summary>
    /// <remarks>
    /// Taken straight from XSMP source
    /// </remarks>
    public class Playlist //TODO visibility?
    {
        [JsonProperty(Required = Required.Always)]
        public string NiceName { get; set; }

        [JsonProperty]
        public string Description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public IList<string> Songs { get; private set; } = new List<string>(); //list of song hashes

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

    }
}