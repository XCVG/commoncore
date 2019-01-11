using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * CommonCore Parameters class
     * Includes common parameters, version info, etc
     */
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
        public static readonly string PreferredCommandConsole = "SickDevConsoleImplementation";

        //*****additional config settings
        public static readonly bool UseVerboseLogging = true;
        public static readonly float DelayedEventPollInterval = 1.0f;
        public static readonly bool UseAggressiveLookups = true;

        //*****game config settings
        public static readonly string InitialScene = "World_Ext_TestIsland";
        public static readonly bool UseCustomLeveling = true;
        public static readonly bool UseDerivedSkills = true;
        public static readonly PlayerViewType DefaultPlayerView = PlayerViewType.PreferFirst;
        public static readonly bool UseRandomDamage = true;
        public static readonly bool AutoQuestNotifications = true;

        //*****automatic environment params
        public static bool IsDebug
        {
            get
            {
                return Debug.isDebugBuild; //may change to PDC (#define DEVELOPMENT_BUILD)
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
        public static string PersistentDataPath
        {
            get
            {
                return Application.persistentDataPath; //TODO thread-safe variants of this and StreamingAssets (and others?)
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
    }


}