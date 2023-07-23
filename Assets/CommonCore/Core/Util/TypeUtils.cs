using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PseudoExtensibleEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
        /// Converts a JToken to a primitive value based on JToken type, returns null if conversion is not possible
        /// </summary>
        /// <remarks>
        /// Note that it will return long or int based on size, but will always return float and not double
        /// </remarks>
        public static object ToValueAuto(this JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Integer:
                    long lValue = token.ToObject<long>();
                    if (lValue <= int.MaxValue && lValue >= int.MinValue)
                        return (int)lValue;
                    return lValue;
                case JTokenType.Float:
                    return token.ToObject<float>();
                case JTokenType.String:
                    return token.ToString();
                case JTokenType.Boolean:
                    return token.ToObject<bool>();
                case JTokenType.Date:
                    return token.ToObject<DateTime>();
                case JTokenType.Bytes:
                    return token.ToObject<byte[]>();
                default:
                    return null;
            }
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
        /// Checks if the Type is an "integer" type
        /// </summary>
        public static bool IsIntegerType(this Type type)
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
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the Type is castable from one type to another (includes user-defined operators)
        /// </summary>
        /// <remarks>Based on https://stackoverflow.com/questions/2119441/check-if-types-are-castable-subclasses</remarks>
        public static bool IsCastableFrom(this Type toType, Type fromType)
        {
            return toType.IsAssignableFrom(fromType) || HasUserDefinedConversion(fromType, toType);
        }

        /// <summary>
        /// Checks if there is an implicit or explicit conversion between fromType and toType
        /// </summary>
        /// <remarks>
        /// Based on an answer to https://stackoverflow.com/questions/32025201/how-can-i-determine-if-an-implicit-cast-exists-in-c/32025393#32025393
        /// </remarks>
        public static bool HasUserDefinedConversion(Type fromType, Type toType)
        {
            return
                fromType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => (mi.Name == "op_Implicit" || mi.Name == "op_Explicit") && mi.ReturnType == toType)
                .Any(mi =>
                {
                    ParameterInfo pi = mi.GetParameters().FirstOrDefault();
                    return pi != null && pi.ParameterType == fromType;
                })
                ||
                toType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => (mi.Name == "op_Implicit" || mi.Name == "op_Explicit") && mi.ReturnType == toType)
                .Any(mi =>
                {
                    ParameterInfo pi = mi.GetParameters().FirstOrDefault();
                    return pi != null && pi.ParameterType == fromType;
                });

        }

        /// <summary>
        /// Gets a user-defined conversion method between fromType and toType if it exists, or null if it doesn't
        /// </summary>
        /// <remarks>
        /// Based on an answer to https://stackoverflow.com/questions/32025201/how-can-i-determine-if-an-implicit-cast-exists-in-c/32025393#32025393
        /// </remarks>
        public static MethodInfo GetUserDefinedConversion(Type fromType, Type toType)
        {
            MethodInfo conversionMethod = null;

            fromType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => (mi.Name == "op_Implicit" || mi.Name == "op_Explicit") && mi.ReturnType == toType)
                .Where(mi =>
                {
                    ParameterInfo pi = mi.GetParameters().FirstOrDefault();
                    return pi != null && pi.ParameterType == fromType;
                })?.FirstOrDefault();

            if(conversionMethod == null)
            {
                toType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(mi => (mi.Name == "op_Implicit" || mi.Name == "op_Explicit") && mi.ReturnType == toType)
                .Where(mi =>
                {
                    ParameterInfo pi = mi.GetParameters().FirstOrDefault();
                    return pi != null && pi.ParameterType == fromType;
                })?.FirstOrDefault();
            }

            return conversionMethod;
        }

        /// <summary>
        /// Coerces a value of some type into a value of the target type. User defined conversions are used if they exist
        /// </summary>
        public static object CoerceValue(object value, Type targetType) => CoerceValue(value, targetType, true);

        /// <summary>
        /// Coerces a value of some type into a value of the target type
        /// </summary>
        public static object CoerceValue(object value, Type targetType, bool allowUserDefinedConversions)
        {
            Type valueType = value?.GetType();

            Type nullableType = Nullable.GetUnderlyingType(targetType);
            if (nullableType != null)
            {
                targetType = nullableType;
                if (value == null || targetType.IsAssignableFrom(valueType))
                    return value;
            }

            if (value == null)
            {
                if (targetType.IsValueType)
                    return Activator.CreateInstance(targetType);
                else
                    return null;
            }          

            if (targetType.IsAssignableFrom(valueType))
                return value;

            if (allowUserDefinedConversions)
            {
                MethodInfo conversionMethod = GetUserDefinedConversion(valueType, targetType); //should we only allow implicit conversions?
                if (conversionMethod != null)
                {
                    return conversionMethod.Invoke(null, new object[] { value });
                }
            }

            if (targetType.IsEnum && targetType.GetCustomAttribute<PseudoExtensibleAttribute>() != null && (value is string || valueType.IsIntegerType()))
            {
                return PxEnum.Parse(targetType, value.ToString(), true);
            }

            if (targetType.IsEnum && value is string stringValue)
            {
                return Enum.Parse(targetType, stringValue, true); //ignore case to taste
            }

            if(targetType.IsEnum && valueType.IsIntegerType())
            {
                return Enum.ToObject(targetType, value);
            }

            return Convert.ChangeType(value, targetType); //note that this will attempt to parse
        }

        /// <summary>
        /// Coerces a value of some type into a value of the target type. User defined conversions are used if they exist
        /// </summary>
        public static T CoerceValue<T>(object value)
        {
            return (T)CoerceValue(value, typeof(T));
        }

        /// <summary>
        /// Coerces a value of some type into a value of the target type
        /// </summary>
        public static T CoerceValue<T>(object value, bool allowUserDefinedConversions)
        {
            return (T)CoerceValue(value, typeof(T), allowUserDefinedConversions);
        }

        /// <summary>
        /// Gets the default value of a type
        /// </summary>
        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);

            return null;
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
            if (input.IndexOf('.') < 0)
            {
                int iResult;
                bool isInteger = int.TryParse(input, out iResult);
                if (isInteger)
                    return iResult;
            }

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
            if (input.IndexOf('.') < 0)
            {
                long iResult;
                bool isInteger = long.TryParse(input, out iResult);
                if (isInteger)
                    return iResult;
            }

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
        /// Adds two values dynamically, optionally coercing to the type of value0 first
        /// </summary>
        /// <remarks>Very, very different codepath for AOT</remarks>
        public static object AddValuesDynamic(object value0, object value1, bool coerceFirst)
        {
            if(coerceFirst)
            {
                value1 = CoerceValue(value1, value0.GetType());
            }

#if ENABLE_IL2CPP || !NET_4_6

            Type value0Type = value0.GetType();
            Type value1Type = value1.GetType();
            //may break in null edge cases but that's probably okay

            if(value0Type == typeof(string) || value1Type == typeof(string))
            {
                //if one of them is a string, handle as string
                return value0.ToString() + value1.ToString();
            }
            else if(value0Type.IsNumericType() && value1Type.IsNumericType())
            {
                //handle numeric types
                if(value0Type == typeof(decimal) || value1Type == typeof(decimal))
                {
                    return (decimal)value0 + (decimal)value1;
                }
                else if(value0Type == typeof(double) || value1Type == typeof(double))
                {
                    return (double)value0 + (double)value1;
                }
                else if (value0Type == typeof(float) || value1Type == typeof(float))
                {
                    return (float)value0 + (float)value1;
                }
                else if(value0Type == typeof(long) || value1Type == typeof(long))
                {
                    return (long)value0 + (long)value1;
                }
                else if (value0Type == typeof(int) || value1Type == typeof(int))
                {
                    return (int)value0 + (int)value1;
                }
                else
                {
                    //add as int and truncate
                    return Convert.ChangeType((int)value0 + (int)value1, value0Type);
                }
            }
            else
            {
                throw new NotSupportedException($"Can't add {value0Type.Name} and {value1Type.Name}");
            }
            //it would be possible to get a little more flexibility via reflection (calling overloaded operator+) but probably not worth it
#else
            return (dynamic)value0 + (dynamic)value1;
#endif

        }

        //these BREAK HORRIBLY if the backing type is not int

        /// <summary>
        /// Gets a flags-enum value from a collection of enum values
        /// </summary>
        public static T FlagsFromCollection<T>(IEnumerable<T> collection) where T : struct, Enum
        {
            if(CoreParams.IsDebug)
            {
                if (typeof(T).GetCustomAttribute<FlagsAttribute>() == null)
                    Debug.LogWarning($"Enum type \"{typeof(T).Name}\" appears to not be a flags enum");
            }

            if (collection == null)
                return default;

            int flags = 0;

            foreach (T flag in collection)
                flags |= (int)(object)flag;

            return (T)(object)flags;
        }

        /// <summary>
        /// Gets a flags-enum value from a collection of strings
        /// </summary>
        public static T FlagsFromCollection<T>(IEnumerable<string> collection) where T : struct, Enum
        {
            if (CoreParams.IsDebug)
            {
                if (typeof(T).GetCustomAttribute<FlagsAttribute>() == null)
                    Debug.LogWarning($"Enum type \"{typeof(T).Name}\" appears to not be a flags enum");
            }

            if (collection == null)
                return default;

            int flags = 0;

            foreach (string flagString in collection)
            {
                flags |= (int)Enum.Parse(typeof(T), flagString);
            }

            return (T)(object)flags;
        }

        /// <summary>
        /// Converts in a bitwise way from one integral type to another, truncating or zero-extending as needed
        /// </summary>
        /// <remarks></remarks>
        public static object ConvertBits(object source, Type targetType)
        {
            //should work https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/numeric-conversions

            //we want the source to be unsigned to avoid sign-extension
            ulong sourceBits;

            //here we cast signed to unsigned of the same width to avoid sign extension when going to 64 bits
            switch (Type.GetTypeCode(source.GetType()))
            {
                case TypeCode.Boolean:
                    throw new NotImplementedException(); //it's doable but I haven't done it yet
                case TypeCode.Char:
                    sourceBits = unchecked((ulong)(ushort)(short)source);
                    break;
                case TypeCode.SByte:
                    sourceBits = unchecked((ulong)(byte)source);
                    break;
                case TypeCode.Int16:
                    sourceBits = unchecked((ulong)(ushort)source);
                    break;
                case TypeCode.Int32:
                    sourceBits = unchecked((ulong)(uint)source);
                    break;
                default:
                    sourceBits = unchecked((ulong)source);
                    break;
            }

            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Boolean:
                    throw new NotImplementedException(); //it's doable but I haven't done it yet
                case TypeCode.Char:
                    return unchecked((char)(short)sourceBits);
                case TypeCode.Byte:
                    return unchecked((byte)sourceBits);
                case TypeCode.SByte:
                    return unchecked((sbyte)sourceBits);
                case TypeCode.UInt16:
                    return unchecked((ushort)sourceBits);
                case TypeCode.UInt32:
                    return unchecked((uint)sourceBits);
                case TypeCode.UInt64:
                    return sourceBits;
                case TypeCode.Int16:
                    return unchecked((short)sourceBits);
                case TypeCode.Int32:
                    return unchecked((int)sourceBits);
                case TypeCode.Int64:
                    return unchecked((long)sourceBits);
                default:
                    throw new ArgumentException();
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

            if (minor < 0)
                minor = 0;

            if (revision >= 0)
                return new Version(major, minor, build, revision);
            else if (build >= 0)
                return new Version(major, minor, build);
            else
                return new Version(major, minor);

            int parseSingleSegment(string segment)
            {
                if (string.IsNullOrEmpty(segment))
                    return -1;

                return int.Parse(segment);
            }
        }

        /// <summary>
        /// Populates a static object with properties deserialzed from JSON
        /// </summary>
        /// <remarks>Based on an answer to https://stackoverflow.com/questions/50340801/deserializing-a-json-file-into-a-static-class-in-c-sharp</remarks>
        public static void PopulateStaticObject(Type type, string json, JsonSerializerSettings settings)
        {
            var source = JsonConvert.DeserializeObject<JToken>(json, settings);

            var destinationProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (JProperty prop in source)
            {
                var destinationProp = destinationProperties
                    .SingleOrDefault(p => p.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                if (destinationProp == null)
                    continue;
                var value = ((JValue)prop.Value).Value;

                destinationProp.SetValue(null, CoerceValue(value, destinationProp.PropertyType));
            }

            var destinationFields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (JProperty prop in source)
            {
                var destinationField = destinationFields
                    .SingleOrDefault(p => p.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                if (destinationField == null)
                    continue;
                var value = ((JValue)prop.Value).Value;

                destinationField.SetValue(null, CoerceValue(value, destinationField.FieldType));
            }
        }

        /// <summary>
        /// Populates an object (existing or new) from a dictionary of data
        /// </summary>
        public static object PopulateObjectFromDictionary(Type type, IReadOnlyDictionary<string, object> dictionary, object originalObject = null, BindingFlags? bindingFlags = null)
        {
            var bFlags = bindingFlags ?? (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (originalObject == null)
            {
                originalObject = Activator.CreateInstance(type);
            }

            var properties = type.GetProperties(bFlags);
            foreach(var prop in properties)
            {
                if(dictionary.TryGetValue(prop.Name, out var val))
                {
                    var cVal = CoerceValue(val, prop.PropertyType);
                    prop.SetValue(originalObject, cVal);
                }                
            }

            var fields = type.GetFields(bFlags);
            foreach (var field in fields)
            {
                if (dictionary.TryGetValue(field.Name, out var val))
                {
                    var cVal = CoerceValue(val, field.FieldType);
                    field.SetValue(originalObject, cVal);
                }
            }

            return originalObject;
        }

        /// <summary>
        /// Populates an object (existing or new) from a dictionary of data
        /// </summary>
        public static T PopulateObjectFromDictionary<T>(IReadOnlyDictionary<string, object> dictionary, T originalObject = default(T), BindingFlags? bindingFlags = null)
        {
            return (T)PopulateObjectFromDictionary(typeof(T), dictionary, originalObject, bindingFlags);
        }

        /// <summary>
        /// Creates a Delegate from a MethodInfo
        /// </summary>
        /// <param name="methodInfo">The MethodInfo representing the method to make a delegate for</param>
        /// <returns>A Delegate created from the MethodInfo</returns>
        /// <remarks>Currently only supports static methods</remarks>
        public static Delegate CreateDelegate(MethodInfo methodInfo)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = Expression.GetActionType;
            }
            else
            {
                getType = Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            throw new ArgumentException("Method must be static!", "methodInfo");
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