using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * CommonCore Base Utilities class
     * Includes common/general utility functions that don't fit within a module
     */
    public static class CCBaseUtil
    {
        public static T LoadResource<T>(string path) where T: Object
        {
            return Resources.Load<T>(path);
        }
    }
}