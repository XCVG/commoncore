using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;


namespace CommonCore
{

    /// <summary>
    /// Utilities for type conversion, coersion, introspection and a few other things
    /// </summary>
    /// <remarks>
    /// <para>Really kind of a dumping ground if I'm going to be honest</para>
    /// </remarks>
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
        /// Checks if this JToken is null or empty
        /// </summary>
        /// <remarks>
        /// <para>Based on https://stackoverflow.com/questions/24066400/checking-for-empty-null-jtoken-in-a-jobject </para>
        /// </remarks>
        public static bool IsNullOrEmpty(this JToken token)
        {
            return 
               (token == null) ||
               (token.Type == JTokenType.Null) ||
               (token.Type == JTokenType.Undefined) ||
               (token.Type == JTokenType.Array && !token.HasValues) ||
               (token.Type == JTokenType.Object && !token.HasValues) ||
               (token.Type == JTokenType.String && string.IsNullOrEmpty(token.ToString()));
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
        /// Compares two values of arbitrary numeric type
        /// </summary>
        /// <returns>-1 if a less than b, 0 if a equals b, 1 if a greater than b</returns>
        public static int CompareNumericValues(object a, object b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            //convert if possible, check if converstions worked

            if (a is string)
            {
                a = StringToNumericAutoDouble((string)a);
                if (a is string)
                    throw new ArgumentException($"\"{a}\" cannot be parsed to a numeric type!", nameof(a));
            }

            if (!a.GetType().IsNumericType())
                throw new ArgumentException($"\"{a}\" is not a numeric type!", nameof(a));

            if (b is string)
            {
                b = StringToNumericAutoDouble((string)b);
                if (b is string)
                    throw new ArgumentException($"\"{b}\" cannot be parsed to a numeric type!", nameof(b));
            }

            if (!b.GetType().IsNumericType())
                throw new ArgumentException($"\"{b}\" is not a numeric type!", nameof(b));

            //compare as decimal, double or long depending on type
            if (a is decimal || b is decimal)
            {
                decimal da = (decimal)Convert.ChangeType(a, typeof(decimal));
                decimal db = (decimal)Convert.ChangeType(b, typeof(decimal));

                return da.CompareTo(db);
            }
            else if (a is double || a is float || b is double || b is float)
            {
                double da = (double)Convert.ChangeType(a, typeof(double));
                double db = (double)Convert.ChangeType(b, typeof(double));

                return da.CompareTo(db);
            }
            else
            {
                long la = (long)Convert.ChangeType(a, typeof(long));
                long lb = (long)Convert.ChangeType(b, typeof(long));

                return la.CompareTo(lb);
            }
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

        /// <summary>
        /// Converts a string to Title Case
        /// </summary>
        /// <remarks>Some limitations may apply</remarks>
        public static string ToTitleCase(this string src)
        {
            if(!src.Contains(" "))
            {
                //simpler single-word handling
                string lcString = src.Substring(1, src.Length-1).ToLower(CultureInfo.InvariantCulture);
                string firstCharString = char.ToUpper(src[0], CultureInfo.InvariantCulture).ToString();
                return firstCharString + lcString;
            }
            else
            {
                //just call the library for now
                return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(src);
            }
        }

        /// <summary>
        /// Converts a string to Sentence case
        /// </summary>
        /// <remarks>
        /// <para>Some limitations may apply</para>
        /// <para>Based on https://stackoverflow.com/a/3141467</para>
        /// </remarks>
        public static string ToSentenceCase(this string src)
        {
            string lowerCase = src.ToLower();
            Regex regex = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            string result = regex.Replace(lowerCase, s => s.Value.ToUpper());
            return result;
        }
    }
}