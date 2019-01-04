using System;
using UnityEngine;

namespace CommonCore
{

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