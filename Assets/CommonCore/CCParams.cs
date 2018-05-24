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
        //version info
        public const int VersionCode = 1; //will change to major/minor/patch struct soon
        public const string VersionName = "Arroyo";

        //baseline config settings
        public const bool AutoInit = true;
        public const bool AutoloadModules = true;

        //automatic environment params
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
    }
}