using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.XSMP
{

    internal class XSMPConfig
    {
        [JsonProperty]
        internal string Hostname { get; set; } = "localhost";
        [JsonProperty]
        internal string Prefix { get; set; } = "api1";
        [JsonProperty]
        internal int Port { get; set; } = 1547;
        [JsonProperty]
        internal bool ServerAutostart { get; set; } = false;
        [JsonProperty]
        internal string ServerPath { get; set; } = null;
        [JsonProperty]
        internal float StatusUpdateInterval { get; set; } = 10.0f; //every 10 seconds, ping the server
        [JsonProperty]
        internal float MaxCachedClips { get; set; } = 4;
    }
}