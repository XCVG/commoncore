using System;
using System.Collections.Generic;

namespace CommonCore.StringSub
{
    /// <summary>
    /// String subber for environment vars and paths
    /// </summary>
    public class EnvironmentStringSubber : IStringSubber
    {
        //TODO support system paths like appdata, some game and system config vars

        public IEnumerable<string> MatchPatterns => new string[] { "%PersistentData%", "%StreamingAssets%" };

        public string Substitute(string[] sequenceParts)
        {
            switch (sequenceParts[0])
            {
                case "%PersistentData%":
                    return CoreParams.PersistentDataPath;
                case "%StreamingAssets":
                    return CoreParams.StreamingAssetsPath;
                default:
                    throw new ArgumentException();
            }
        }
    }
}