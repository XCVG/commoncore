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

        /*
         * Converts a string to an int or a float with correct type
         * (double precision version: long or double)
         */
        public static object StringToNumericAutoDouble(string input)
        {
            //check if it is integer first
            long iResult;
            bool isInteger = long.TryParse(input, out iResult);
            if (isInteger)
                return iResult;

            //then check if it could be decimal
            double fResult;
            bool isFloat = double.TryParse(input, out fResult);
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

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }


        private static Transform WorldRoot;
        public static Transform GetWorldRoot() //TODO really ought to move this
        {
            if (WorldRoot == null)
            {
                GameObject rootGo = GameObject.Find("WorldRoot");
                if (rootGo == null)
                    return null;
                WorldRoot = rootGo.transform;
            }
            return WorldRoot;
        }

        public static Vector2 ToFlatVec(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }

        public static Vector3 ToSpaceVec(this Vector2 vec2)
        {
            return new Vector3(vec2.x, 0, vec2.y);
        }

        public static Vector3 GetFlatVectorToTarget(Vector3 pos, Vector3 target)
        {
            Vector3 dir = target - pos;
            return new Vector3(dir.x, 0, dir.z);
        }

        public static Vector2 GetRandomVector(Vector2 center, Vector2 extents)
        {
            return new Vector2(
                UnityEngine.Random.Range(-extents.x, extents.x) + center.x,
                UnityEngine.Random.Range(-extents.y, extents.y) + center.y
                );
        }

        public static float CalculateDamage(float Damage, float Pierce, float Threshold, float Resistance) //this is a dumb spot and we will move it later
        {
            float d1 = Damage * ((100f - Mathf.Min(Resistance, 99f)) / 100f);
            float dt = Mathf.Max(0, Threshold - Pierce);
            float d2 = Mathf.Max(d1 - dt, Damage * 0.1f);
            if (CCParams.UseRandomDamage)
                d2 *= UnityEngine.Random.Range(0.75f, 1.25f);
            return d2;
        }

    }
}