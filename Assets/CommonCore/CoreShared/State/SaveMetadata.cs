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
        //POCO for now, fancier later?

        public string NiceName { get; set; }
        public string CampaignIdentifier { get; set; }
        public string Location { get; set; }
        public string LocationRaw { get; set; }

        public string CompanyName { get; set; }
        public string GameName { get; set; }

        //TODO do we keep version info here?
        public VersionInfo GameVersion { get; set; }

        public byte[] ThumbnailImage { get; set; }

    }
}

