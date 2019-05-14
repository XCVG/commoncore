using System;

namespace CommonCore
{

    /// <summary>
    /// When modules load their data
    /// </summary>
    /// <remarks>
    /// Note that it's up to the modules to actually implement the policy
    /// </remarks>
    public enum DataLoadPolicy
    {
        /// <summary>
        /// OnDemand in editor, OnStart in build
        /// </summary>
        Auto,
        /// <summary>
        /// Load data at the time it is needed
        /// </summary>
        OnDemand,
        /// <summary>
        /// Load data on game start
        /// </summary>
        OnStart,
        /// <summary>
        /// Load data as needed, and keep it in memory
        /// </summary>
        Cached
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