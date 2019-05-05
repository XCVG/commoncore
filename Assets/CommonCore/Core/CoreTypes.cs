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

    /// <summary>
    /// Persistent data path to use on Windows platform
    /// </summary>
    /// <remarks>Because the default is stupid</remarks>
    public enum WindowsPersistentDataPath
    {
        /// <summary>
        /// Passes through Unity persistentDataPath
        /// </summary>
        UnityDefault,
        /// <summary>
        /// Uses Local instead of LocalLow, which matches the intended semantics better according to Microsoft's documentation
        /// </summary>
        Corrected,
        /// <summary>
        /// Uses Roaming instead of LocalLow, which is probably the more appropriate location
        /// </summary>
        Roaming,
        //SavedGames,
        /// <summary>
        /// Uses Documents folder directly (ie Documents/DefaultCompany/ExampleGame)
        /// </summary>
        Documents,
        /// <summary>
        /// Uses Documents/My Games, which is not officially recommended but commonly used
        /// </summary>
        MyGames
    }

    /// <summary>
    /// Custom log level enum
    /// </summary>
    public enum LogLevel
    {
        Error, Warning, Message, Verbose
    }

    /// <summary>
    /// Console command attribute, syntactically compatible with SickDev.CommandSystem
    /// </summary>
    public class CommandAttribute : Attribute
    {
        public CommandAttribute()
        {

        }

        public string description { get; set; }
        public string alias { get; set; }
        public string className { get; set; }
        public bool useClassName { get; set; }
    }
}