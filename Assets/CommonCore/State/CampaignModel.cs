using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace CommonCore.State
{

    public class CampaignModel
    {
        [JsonProperty]
        public Dictionary<string, object> Data { get; private set; } //generic data store
        [JsonProperty]
        public List<string> Flags { get; private set; } //fast and easy flag store
        [JsonProperty]
        public Dictionary<string, int> Quests { get; private set; }

        public CampaignModel()
        {
            Data = new Dictionary<string, object>();
            Flags = new List<string>();
            Quests = new Dictionary<string, int>();
        }
    }
}