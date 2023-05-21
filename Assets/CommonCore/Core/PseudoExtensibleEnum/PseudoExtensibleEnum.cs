using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PseudoExtensibleEnum
{
    /// <summary>
    /// Defines an enum as able to be pseudo-extended
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class PseudoExtensibleAttribute : Attribute
    {

    }

    /// <summary>
    /// Defines an enum as a pseudo-extension to a base enum
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class PseudoExtendAttribute : Attribute
    {
        public Type BaseType { get; private set; }

        public PseudoExtendAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }

    /// <summary>
    /// Utility methods for manipulating pseudo-extensible enums
    /// </summary>
    public static class PxEnum
    {
        public static PseudoExtensibleEnumContext CurrentContext { get; internal set; }

        /// <summary>
        /// Creates or recreates the current PseudoExtensibleEnumContext
        /// </summary>
        public static void RecreateCurrentContext()
        {
            CurrentContext = new PseudoExtensibleEnumContext();
        }

        //similar API to Enum static methods

        /// <summary>
        /// Retrieves the name of the constant in the specified enumeration or its pseudo-extensions that has the specified value.
        /// </summary>
        public static string GetName(Type enumType, object value)
        {
            ThrowIfTypeInvalid(enumType);

            var baseResult = Enum.GetName(enumType, value);
            if(baseResult != null)
                return baseResult;

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach(var eType in extensions)
            {
                var eResult = Enum.GetName(eType, value);
                if (eResult != null)
                    return eResult;
            }

            return null;
        }

        /// <summary>
        /// Retrieves an array of the names of the constants in a specified enumeration and its pseudo-extensions.
        /// </summary>
        public static string[] GetNames(Type enumType)
        {
            ThrowIfTypeInvalid(enumType);

            HashSet<string> names = new HashSet<string>();

            names.UnionWith(Enum.GetNames(enumType));

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach (var eType in extensions)
            {
                names.UnionWith(Enum.GetNames(eType));
            }

            return names.ToArray();
        }

        /// <summary>
        /// Retrieves an array of the values of the constants in a specified enumeration and its pseudo-extensions.
        /// </summary>
        public static Array GetValues(Type enumType)
        {
            ThrowIfTypeInvalid(enumType);

            var underlyingType = Enum.GetUnderlyingType(enumType);
            IList values = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(underlyingType));

            var baseValues = Enum.GetValues(enumType);
            for(int i = 0; i < baseValues.Length; i++)
            {
                values.Add(baseValues.GetValue(i));
            }

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach (var eType in extensions)
            {
                var eValues = Enum.GetValues(eType);
                for (int i = 0; i < eValues.Length; i++)
                {
                    values.Add(eValues.GetValue(i));
                }
            }

            Array valuesArray = Array.CreateInstance(underlyingType, values.Count);
            for(int i = 0; i < values.Count; i++)
            {
                valuesArray.SetValue(values[i], i); 
            }

            return valuesArray;
        }

        /// <summary>
        /// Retrieves a value-name collection of the constants in a specified enumeration and its pseudo-extensions.
        /// </summary>
        /// <remarks>
        /// Intended for the custom property drawer in CommonCore
        /// </remarks>
        public static List<KeyValuePair<long, string>> GetValueNameCollection(Type enumType, bool useContext)
        {
            ThrowIfTypeInvalid(enumType);

            var underlyingType = Enum.GetUnderlyingType(enumType);
            List<KeyValuePair<long, string>> result = new List<KeyValuePair<long, string>>();

            var baseValues = Enum.GetValues(enumType);
            for (int i = 0; i < baseValues.Length; i++)
            {
                object val = baseValues.GetValue(i);
                result.Add(new KeyValuePair<long, string>(Convert.ToInt64(val), Enum.GetName(enumType, val)));
            }

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach (var eType in extensions)
            {
                var eValues = Enum.GetValues(eType);
                for (int i = 0; i < eValues.Length; i++)
                {
                    object val = eValues.GetValue(i);
                    result.Add(new KeyValuePair<long, string>(Convert.ToInt64(val), Enum.GetName(eType, val)));
                }
            }

            return result;
        }

        /// <summary>
        /// Returns a Boolean telling whether a given integral value, or its name as a string, exists in a specified enumeration or its pseudo-extensions.
        /// </summary>
        public static bool IsDefined(Type enumType, object value)
        {
            ThrowIfTypeInvalid(enumType);

            if (Enum.IsDefined(enumType, value))
                return true;

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach (var eType in extensions)
            {
                if (Enum.IsDefined(eType, value))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static object Parse(Type enumType, string value)
        {
            return Parse(enumType, value, true);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static object Parse(Type enumType, string value, bool ignoreCase)
        {
            if(TryParseInternal(enumType, value, ignoreCase, out object result))
            {
                return result;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            return TryParse(value, true, out result);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static bool TryParse<TEnum>(string value, bool ignoreCase, out TEnum result) where TEnum : struct
        {
            if(TryParseInternal(typeof(TEnum), value, ignoreCase, out object rawResult))
            {
                result = (TEnum)rawResult;
                return true;
            }

            result = default;
            return false;
        }

        //the "missing methods" from the base enum API

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static T Parse<T>(string value)
        {
            return (T)Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static T Parse<T>(string value, bool ignoreCase)
        {
            if (TryParseInternal(typeof(T), value, ignoreCase, out object result))
            {
                return (T)result;
            }

            throw new ArgumentException();
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static bool TryParse(Type enumType, string value, out object result)
        {
            return TryParse(enumType, value, true, out result);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent enumerated object of either the base or pseudo-extended type.
        /// </summary>
        public static bool TryParse(Type enumType, string value, bool ignoreCase, out object result)
        {
            if (TryParseInternal(enumType, value, ignoreCase, out object rawResult))
            {
                result = rawResult;
                return true;
            }

            result = GetDefaultForType(enumType);
            return false;
        }


        private static bool TryParseInternal(Type enumType, string value, bool ignoreCase, out object result)
        {
            ThrowIfTypeInvalid(enumType);

            if(TryParseSingleTypeInternal(enumType, value, ignoreCase, out result))
            {
                return true;
            }

            var extensions = GetPseudoExtensionsToEnum(enumType);
            foreach (var eType in extensions)
            {
                if (TryParseSingleTypeInternal(eType, value, ignoreCase, out result))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryParseSingleTypeInternal(Type enumType, string value, bool ignoreCase, out object result)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var names = Enum.GetNames(enumType);
            foreach(var name in names)
            {
                if(name.Equals(value, comparison))
                {
                    result = Enum.Parse(enumType, name);
                    return true;
                }
            }

            result = default;
            return false;
        }

        private static void ThrowIfTypeInvalid(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException($"{enumType.Name} is not an enum");

            //if(enumType.GetCustomAttribute<PseudoExtensibleAttribute>() == null)
            //    throw new ArgumentException($"{enumType.Name} is not a psuedo-extensible enum");
        }

        private static Type[] GetPseudoExtensionsToEnum(Type baseType, bool useContext = false)
        {
            if (baseType.GetCustomAttribute<PseudoExtensibleAttribute>() == null)
            {
                return new Type[] { };
            }

            if(useContext && CurrentContext != null)
            {
                return CurrentContext.GetPseudoExtensionsToEnum(baseType);
            }

#if UNITY_2017_1_OR_NEWER
            var allExtendTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !(a.FullName.StartsWith("Unity") || a.FullName.StartsWith("System") || a.FullName.StartsWith("netstandard") ||
                            a.FullName.StartsWith("mscorlib") || a.FullName.StartsWith("mono", StringComparison.OrdinalIgnoreCase) ||
                            a.FullName.StartsWith("Boo") || a.FullName.StartsWith("I18N")))
                .SelectMany((assembly) => assembly.GetTypes())
                .Where(t => t.IsDefined(typeof(PseudoExtendAttribute)))
                .Where(t => t.GetCustomAttribute<PseudoExtendAttribute>().BaseType == baseType);
#else
            var allExtendTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsDefined(typeof(PseudoExtendAttribute)))
                .Where(t => t.GetCustomAttribute<PseudoExtendAttribute>().BaseType == baseType);
#endif
            return allExtendTypes.ToArray();
        }

        private static object GetDefaultForType(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}