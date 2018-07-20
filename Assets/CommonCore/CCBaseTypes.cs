using UnityEngine;

namespace CommonCore
{

    /*
     * Just your basic Semantic Version struct
     * (immutable, just the way we like it, though that matters less for structs)
     */
    public struct SemanticVersion
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Patch;

        public SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }
    }

    /*
     * When do modules load data?
     * 
     * Auto:        OnDemand in editor, OnStart in build
     * OnDemand:    Load data as needed
     * OnStart:     Load data on game start
     * Cached:      Load data as needed, and keep in memory
     * 
     * Note that it's up to the modules to implement the policy
     */
    public enum DataLoadPolicy
    {
        Auto, OnDemand, OnStart, Cached
    }

    /*
     * Player view types, pretty self explanatory
     */
    public enum PlayerViewType
    {
        PreferFirst, PreferThird, ForceFirst, ForceThird, ExplicitOther
    }
}