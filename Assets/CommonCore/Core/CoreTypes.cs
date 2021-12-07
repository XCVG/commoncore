using System;
using UnityEngine.Scripting;

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
    /// Which resource manager to use
    /// </summary>
    public enum ResourceManagerPolicy
    {
        /// <summary>
        /// Use legacy resource manager only
        /// </summary>
        UseLegacy,
        /// <summary>
        /// Use new resource manager only
        /// </summary>
        UseNew
    }

    /// <summary>
    /// How/when to start up CommonCore
    /// </summary>
    public enum StartupPolicy
    {
        /// <summary>
        /// Run startup before first scene load (like early versions of CommonCore)
        /// </summary>
        SynchronousEarly,
        /// <summary>
        /// Run startup synchronously, but after first scene load
        /// </summary>
        Synchronous,
        /// <summary>
        /// Run startup asynchronously
        /// </summary>
        Asynchronous
    }

    /// <summary>
    /// How UI theming should be handled
    /// </summary>
    public enum UIThemePolicy
    {
        /// <summary>
        /// Applies UI theme automatically
        /// </summary>
        Auto,
        /// <summary>
        /// Themes are not automatically applied
        /// </summary>
        ExplicitOnly,
        /// <summary>
        /// Theme engine is disabled entirely
        /// </summary>
        Disabled
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
    public class CommandAttribute : PreserveAttribute
    {
        public CommandAttribute()
        {

        }

        public string description { get; set; }
        public string alias { get; set; }
        public string className { get; set; }
        public bool useClassName { get; set; }
    }

    /// <summary>
    /// Defines possible scripting backends. Carbon-copy of UnityEditor.ScriptingImplementation
    /// </summary>
    public enum ScriptingImplementation
    {
        /// <summary>
        /// The standard Mono runtime (editor, standalone platforms)
        /// </summary>
        Mono2x = 0,

        /// <summary>
        /// Unity's CIL-to-C++ AOT compiled runtime (mobile, UWP, consoles?)
        /// </summary>
        IL2CPP = 1,

        /// <summary>
        /// Microsoft's .NET runtime (UWP, deprecated)
        /// </summary>
        WinRTDotNET = 2
    }    

    public class StartupFailedException : Exception
    {
        public StartupFailedException(Exception innerException): base("A fatal error occurred during CommonCore startup", innerException)
        {

        }
    }

    /// <summary>
    /// Defines an entity class as one that can be migrated
    /// </summary>
    public interface IMigratable
    {
        //migration system can actually tolerate a VersionInfo, though only GameVersion will be considered
        Version LastMigratedVersion { get; }
    }
    //TODO split this into IMigratable, IVersionMigratable, IVersionInfoMigratable. Maybe.
    
}