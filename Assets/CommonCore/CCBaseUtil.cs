using Newtonsoft.Json;
using System;
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
        public static T LoadResource<T>(string path) where T: UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        //same with this one
        public static T[] LoadResources<T>(string path) where T: UnityEngine.Object
        {
            return Resources.LoadAll<T>(path);
        }
        
        public static T LoadExternalJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            string text = File.ReadAllText(path);
            return LoadJson<T>(text);
        }

        public static T LoadJson<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, System.Object obj)
        {
            string json = SaveJson(obj);
            File.WriteAllText(path, json);
        }

        public static string SaveJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /*
         * Converts a string to an int or a float with correct type
         * (limitation: literally int or float, no long or double etc)
         */
        public static object StringToNumericAuto(string input)
        {
            //check if it is integer first
            int iResult;
            bool isInteger = int.TryParse(input, out iResult);
            if (isInteger)
                return iResult;

            //then check if it could be decimal
            float fResult;
            bool isFloat = float.TryParse(input, out fResult);
            if (isFloat)
                return fResult;

            //else return what we started with
            return input;
        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static Transform GetWorldRoot()
        {
            GameObject rootGo = GameObject.Find("WorldRoot");
            if (rootGo == null)
                return null;
            return rootGo.transform;
        }
    }
}