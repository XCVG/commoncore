using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace CommonCore
{

    /// <summary>
    /// Utilities for type conversion, coersion, introspection and a few other things
    /// </summary>
    public static class TypeUtils
    {

        /// <summary>
        /// Hack around Unity-fake-null
        /// </summary>
        public static object Ref(this object obj)
        {
            if (obj is UnityEngine.Object)
                return (UnityEngine.Object)obj == null ? null : obj;
            else
                return obj;
        }

        /// <summary>
        /// Hack around Unity-fake-null
        /// </summary>
        public static T Ref<T>(this T obj) where T : UnityEngine.Object
        {
            return obj == null ? null : obj;
        }

        /// <summary>
        /// Checks if the Type is a "numeric" type
        /// </summary>
        public static bool IsNumericType(this Type type)
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

        /// <summary>
        /// Converts a string to a target type, handling enums and other special cases
        /// </summary>
        public static object Parse(string value, Type parseType)
        {
            if (parseType.IsEnum)
                return Enum.Parse(parseType, value);

            return Convert.ChangeType(value, parseType);
        }

        /// <summary>
        /// Converts a string to an int or a float with correct type
        /// </summary>
        /// <remarks>
        /// Returns original string on failure.
        /// </remarks>
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


        /// <summary>
        /// Converts a string to an long or a double with correct type (double precision version)
        /// </summary>
        /// <remarks>
        /// Returns original string on failure.
        /// </remarks>
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

        /// <summary>
        /// Parses a string to a Version object
        /// </summary>
        public static Version ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return new Version();

            string[] segments = version.Split('.', ',', 'f', 'b', 'a', 'v');
            int major = 0, minor = 0, build = -1, revision = -1;

            if (segments.Length >= 1)
                major = parseSingleSegment(segments[0]);
            if (segments.Length >= 2)
                minor = parseSingleSegment(segments[1]);
            if (segments.Length >= 3)
                build = parseSingleSegment(segments[2]);
            if (segments.Length >= 4)
                revision = parseSingleSegment(segments[3]);

            if (revision > 0)
                return new Version(major, minor, build, revision);
            else if (build > 0)
                return new Version(major, minor, build);
            else
                return new Version(major, minor);

            int parseSingleSegment(string segment)
            {
                if (string.IsNullOrEmpty(segment))
                    return 0;

                return int.Parse(segment);
            }
        }
    }
}