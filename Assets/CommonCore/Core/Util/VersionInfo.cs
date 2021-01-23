using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CommonCore
{
    /// <summary>
    /// Version info for this game including engine, framework, and game version
    /// </summary>
    public sealed class VersionInfo
    {
        [JsonProperty, JsonConverter(typeof(VersionConverter))]
        public Version GameVersion { get; private set; }
        [JsonProperty, JsonConverter(typeof(VersionConverter))]
        public Version FrameworkVersion { get; private set; }
        [JsonProperty, JsonConverter(typeof(VersionConverter))]
        public Version EngineVersion { get; private set; }

        public VersionInfo(Version gameVersion, Version frameworkVersion, Version engineVersion)
        {
            GameVersion = gameVersion;
            FrameworkVersion = frameworkVersion;
            EngineVersion = engineVersion;
        }

        public override int GetHashCode()
        {
            return GameVersion.GetHashCode() ^ FrameworkVersion.GetHashCode() ^ EngineVersion.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is VersionInfo vi)
                return this == vi;

            return base.Equals(obj);
        }

        public static bool operator == (VersionInfo a, VersionInfo b)
        {
            if (Object.ReferenceEquals(a, b))
                return true;

            if (((object)a == null) || ((object)b == null))            
                return false;            

            return a.GameVersion == b.GameVersion && a.FrameworkVersion == b.FrameworkVersion && a.EngineVersion == b.EngineVersion;
        }

        public static bool operator != (VersionInfo a, VersionInfo b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"Game: {GameVersion}, Framework: {FrameworkVersion}, Engine: {EngineVersion}";
        }
    }
}