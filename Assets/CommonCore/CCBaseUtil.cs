using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WanzyeeStudio;

namespace CommonCore
{
    /*
     * CommonCore Base Utilities class
     * Includes common/general utility functions that don't fit within a module
     */
    public static class CCBaseUtil
    {
        //this seems absolutely pointless but will make sense when eXPostFacto (mod support) is added
        public static T LoadResource<T>(string path) where T: Object
        {
            return Resources.Load<T>(path);
        }

        
        public static T LoadExternalJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, System.Object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(path, json);
        }
    }
}