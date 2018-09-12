using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * CommonCore Parameters class
     * Includes common parameters, version info, etc
     */
    public static class CCParams
    {

        //*****system version info
        public static readonly SemanticVersion VersionCode = new SemanticVersion(1,0,0); //1.0.0
        public static readonly string VersionName = "Arroyo"; //start with A, locations from RPGs
        public static string UnityVersion
        {
            get
            {
                return Application.unityVersion;
            }
        }

        //*****game version info
        public static readonly SemanticVersion GameVersion = new SemanticVersion(0,0,0);
        public static readonly string GameVersionName = "Mechanics Preview 1";

        //*****basic config settings
        public static readonly bool AutoInit = true;
        public static readonly bool AutoloadModules = true;
        private static readonly DataLoadPolicy LoadData = DataLoadPolicy.OnStart;

        //*****additional config settings
        public static readonly bool UseVerboseLogging = true;

        //*****game config settings
        public static readonly string InitialScene = "World_Ext_TestIsland";
        public static readonly bool UseCustomLeveling = true;
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
                return Application.persistentDataPath;
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