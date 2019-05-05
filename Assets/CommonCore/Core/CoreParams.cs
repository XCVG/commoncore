using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// CommonCore Parameters- core config, versioning, paths, etc
    /// </summary>
    public static class CoreParams
    {
        //TODO move some of this into GameParams et al
        //TODO runtime overriding

        //*****system version info
        public static readonly Version VersionCode = new Version(2,0,0); //2.0.0
        public static readonly string VersionName = "Balmora"; //start with A, locations from RPGs
        public static string UnityVersion
        {
            get
            {
                return Application.unityVersion;
            }
        }

        //*****game version info
        public static readonly string GameVersionName = "Technological Preview 3";

        //*****basic config settings
        public static readonly bool AutoInit = true;
        public static readonly bool AutoloadModules = true;
        public static readonly string[] ExplicitModules = new string[] { "DebugModule", "QdmsMessageBus", "ConfigModule", "ConsoleModule" };
        private static readonly DataLoadPolicy LoadData = DataLoadPolicy.OnStart;
        public static readonly string PreferredCommandConsole = "BasicCommandConsoleImplementation";
        private static readonly WindowsPersistentDataPath PersistentDataPathWindows = WindowsPersistentDataPath.UnityDefault;

        //*****additional config settings
        public static readonly bool UseVerboseLogging = true;
        public static readonly float DelayedEventPollInterval = 1.0f;
        public static readonly bool UseAggressiveLookups = true;
        public static readonly bool UseDirectSceneTransitions = false;

        //*****game config settings (TODO move to GameParams)
        public static readonly string InitialScene = "World_Ext_TestIsland";
        public static readonly bool UseCustomLeveling = true;
        public static readonly bool UseDerivedSkills = true;
        public static readonly PlayerViewType DefaultPlayerView = PlayerViewType.PreferFirst;
        public static readonly bool UseRandomDamage = true;
        public static readonly bool AutoQuestNotifications = true;

        //*****path variables (some hackery to provide thread-safeish versions)
        public static string DataPath { get; private set; }
        public static string PersistentDataPath { get; private set; }
        public static string StreamingAssetsPath { get; private set; }

        //*****automatic environment params
        public static bool IsDebug
        {
            get
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsEditor
        {
            get
            {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static string SavePath
        {
            get
            {
                return PersistentDataPath + "/saves";
            }
        }

        public static DataLoadPolicy LoadPolicy
        {
            get
            {
                if (LoadData == DataLoadPolicy.Auto)
                {
#if UNITY_EDITOR
                    return DataLoadPolicy.OnDemand;
#else
                    return DataLoadPolicy.OnStart;
#endif
                }
                else
                    return LoadData;
            }
        }

        static CoreParams()
        {
            DataPath = Application.dataPath;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            switch (PersistentDataPathWindows)
            {
                case WindowsPersistentDataPath.Corrected:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Roaming:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.Documents:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Application.companyName, Application.productName);
                    break;
                case WindowsPersistentDataPath.MyGames:
                    PersistentDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", Application.companyName, Application.productName);
                    break;
                default:
                    PersistentDataPath = Application.persistentDataPath;
                    break;
            }
#else
            PersistentDataPath = Application.persistentDataPath;
#endif
            StreamingAssetsPath = Application.streamingAssetsPath;
        }
    }


}