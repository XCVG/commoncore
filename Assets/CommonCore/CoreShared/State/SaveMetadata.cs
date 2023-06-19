using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    /// <summary>
    /// Object representing metadata stored with a save
    /// </summary>
    public class SaveMetadata
    {
        public string NiceName { get; set; }
        public string CampaignIdentifier { get; set; }
        public string Location { get; set; }
        public string LocationRaw { get; set; }

        public string CompanyName { get; set; }
        public string GameName { get; set; }

        public VersionInfo GameVersion { get; set; }

        public byte[] ThumbnailImage { get; set; }

        //new in 5.x
        public DateTime? CampaignStartDate { get; set; }
        public DateTime? LastSaveDate { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JToken> AdditionalData { get; set; }

    }
}

