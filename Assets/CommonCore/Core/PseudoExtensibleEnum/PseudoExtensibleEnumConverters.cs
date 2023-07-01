using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PseudoExtensibleEnum
{
    /// <summary>
    /// Converter for pseudo-extensible enums. Can convert from numeric or string representations of either the base or pseudo-extended enums, and can convert to either enum or backing type. Will always serialize to numeric values.
    /// </summary>
    public class PxEnumConverter : JsonConverter
    {
        private Type BaseEnumType;
        private bool IgnoreCase = true;
        private bool TreatUnknownAsNull = false;

        public PxEnumConverter()
        {

        }

        public PxEnumConverter(Type baseEnumType)
        {
            BaseEnumType = baseEnumType;
        }

        public PxEnumConverter(Type baseEnumType, bool ignoreCase)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
        }

        public PxEnumConverter(Type baseEnumType, bool ignoreCase, bool treatUnknownAsNull)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
            TreatUnknownAsNull = treatUnknownAsNull;
        }

        public override bool CanRead => true;
        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string sValue = reader.Value as string;
            long? lValue = reader.Value as long?;

            Type nullableObjectType = Nullable.GetUnderlyingType(objectType);

            if (sValue == null && lValue == null)
            {
                if (objectType.IsValueType && nullableObjectType == null)
                    throw new JsonSerializationException($"Error converting value {{null}} to type '{objectType.Name}'. Path '{reader.Path}'");

                return null;
            }

            var type = BaseEnumType ?? nullableObjectType ?? objectType;

            object partiallyParsedValue;
            if(sValue != null)
            {
                //value is a string
                if(PxEnum.TryParse(type, sValue, IgnoreCase, out partiallyParsedValue))
                {
                    //nop; we continue with partiallyParsedValue later
                }
                else
                {
                    if (TreatUnknownAsNull)
                    {
                        if (serializer.NullValueHandling == NullValueHandling.Include)
                            return objectType.IsValueType ? Activator.CreateInstance(type) : null;
                        else
                            return existingValue;
                    }
                    else
                    {
                        throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'. Unable to parse string '{sValue}' to value.");
                    }                        
                }
            }
            else
            {
                //value is numeric
                Type backingType = BaseEnumType != null ? Enum.GetUnderlyingType(BaseEnumType) : (nullableObjectType ?? objectType);
                partiallyParsedValue = Convert.ChangeType(lValue.Value, backingType);
            }

            if(nullableObjectType != null)
            {
                if (nullableObjectType.IsEnum)
                    return Enum.ToObject(nullableObjectType, partiallyParsedValue);
                return Convert.ChangeType(partiallyParsedValue, nullableObjectType);
            }

            if (objectType.IsEnum)
                return Enum.ToObject(objectType, partiallyParsedValue);

            return Convert.ChangeType(partiallyParsedValue, objectType);            
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteToken(JsonToken.Integer, value);
        }
    }

    /// <summary>
    /// Converters for arrays of pseudo-extensible enums. Can convert from numeric or string representations of either the base or pseudo-extended enums. Can convert to arrays, lists, and sets. Will always serialize to arrays of numeric values.
    /// </summary>
    public class PxEnumArrayConverter : JsonConverter
    {
        private Type BaseEnumType;
        private bool IgnoreCase = true;
        private bool SkipUnknownValues = false;

        public PxEnumArrayConverter()
        {

        }

        public PxEnumArrayConverter(Type baseEnumType)
        {
            BaseEnumType = baseEnumType;
        }

        public PxEnumArrayConverter(Type baseEnumType, bool ignoreCase)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
        }

        public PxEnumArrayConverter(Type baseEnumType, bool ignoreCase, bool skipUnknownValues)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
            SkipUnknownValues = skipUnknownValues;
        }

        public override bool CanConvert(Type objectType)
        {
            if(objectType.IsArray)
            {
                var baseType = objectType.GetElementType();
                if (baseType.IsEnum || PxEnumConverterUtils.IsIntegralType(baseType))
                    return true;
            }
            else
            {
                if(objectType.IsGenericType)
                {
                    if(objectType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>) || t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>) || t.GetGenericTypeDefinition() == typeof(ISet<>)))                    
                        return true;
                    
                }
            }

            return false;
        }

        public override object ReadJson(JsonReader reader, Type collectionType, object existingValue, JsonSerializer serializer)
        {
            Type objectType = typeof(object);
            Type collectionBaseType = objectType;
            if(collectionType.IsGenericType)
            {
                objectType = collectionType.GetGenericArguments()[0];
                collectionBaseType = collectionType.GetGenericTypeDefinition();
            }

            Type nullableObjectType = Nullable.GetUnderlyingType(objectType);
            Type baseObjectType = nullableObjectType ?? objectType;

            var enumType = BaseEnumType ?? nullableObjectType ?? objectType;

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'");
            }
            reader.Read();

            List<object> rawItems = new List<object>();            
            while(reader.TokenType != JsonToken.EndArray)
            {                
                rawItems.Add(reader.Value);
                reader.Read();
            }

            List<object> parsedItems = new List<object>();
            foreach(var rawItem in rawItems)
            {
                if(rawItem == null)
                {
                    parsedItems.Add(null);
                    continue;
                }

                //check if numeric or string, try to convert with appropriate path
                if(rawItem is string s)
                {
                    if(PxEnum.TryParse(enumType, s, out object enumValue))
                    {
                        if (objectType.IsEnum)
                        {
                            enumValue = Enum.ToObject(objectType, enumValue);
                        }
                        else
                        {
                            enumValue = Convert.ChangeType(enumValue, objectType);
                        }
                        parsedItems.Add(enumValue);
                        
                    }
                    else
                    {
                        if (SkipUnknownValues)
                            continue;
                        throw new JsonSerializationException($"Error converting value {rawItem} to type '{objectType.Name}'. Path '{reader.Path}'. Unable to parse string '{s}' to value.");
                    }
                }
                else if(PxEnumConverterUtils.IsIntegralType(rawItem.GetType()))
                {
                    object enumValue;
                    if (objectType.IsEnum)
                    {
                        enumValue = Enum.ToObject(objectType, rawItem);
                    }
                    else
                    {
                        enumValue = Convert.ChangeType(rawItem, objectType);
                    }
                    parsedItems.Add(enumValue);
                }
                else
                {
                    throw new JsonSerializationException($"Error converting value {rawItem} to type '{objectType.Name}'. Path '{reader.Path}'");
                }
            }

            if(collectionBaseType.IsArray)
            {
                var array = Array.CreateInstance(collectionBaseType.GetElementType(), parsedItems.Count);
                for (int i = 0; i < array.Length; i++)
                {
                    array.SetValue(parsedItems[i], i);
                }
                return array;
            }
            else if(collectionBaseType == typeof(IList<>) || collectionBaseType == typeof(IReadOnlyList<>) || collectionBaseType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>) || t.GetGenericTypeDefinition() == typeof(IReadOnlyList<>)))
            {
                var constructedListType = typeof(List<>).MakeGenericType(objectType);
                IList list = (IList)Activator.CreateInstance(constructedListType);
                for(int i = 0; i < parsedItems.Count; i++)
                {
                    list.Add(parsedItems[i]);
                }
                return list;
            }
            else if(collectionBaseType == typeof(ISet<>) || collectionBaseType.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ISet<>)))
            {
                var constructedSetType = typeof(HashSet<>).MakeGenericType(objectType);
                var addMethod = constructedSetType.GetMethod("Add");
                object set = Activator.CreateInstance(constructedSetType);                
                for (int i = 0; i < parsedItems.Count; i++)
                {
                    addMethod.Invoke(set, new object[] { parsedItems[i] });
                }
                return set;
            }

            throw new JsonSerializationException($"Error converting value {parsedItems} to type '{objectType.Name}'. Path '{reader.Path}'");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            var enumerable = value as IEnumerable;
            foreach(object item in enumerable)
            {
                writer.WriteToken(JsonToken.Integer, item);
            }
            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// Converters for objects with keys that are pseudo-extensible enums. Can convert from numeric or string representations of either the base or pseudo-extended enums. Always returns a Dictionary. Will always serialize to numeric values as string keys.
    /// </summary>
    public class PxEnumObjectConverter : JsonConverter
    {
        private Type BaseEnumType;
        private bool IgnoreCase = true;
        private bool SkipUnknownKeys = false;

        public PxEnumObjectConverter()
        {

        }

        public PxEnumObjectConverter(Type baseEnumType)
        {
            BaseEnumType = baseEnumType;
        }

        public PxEnumObjectConverter(Type baseEnumType, bool ignoreCase)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
        }

        public PxEnumObjectConverter(Type baseEnumType, bool ignoreCase, bool skipUnknownKeys)
        {
            BaseEnumType = baseEnumType;
            IgnoreCase = ignoreCase;
            SkipUnknownKeys = skipUnknownKeys;
        }

        public override bool CanConvert(Type objectType)
        {
            if(!objectType.IsGenericType)
            {
                return false;
            }

            var interfaces = objectType.GetInterfaces();
            foreach (var @interface in interfaces)
            {
                if(@interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var elementType = @interface.GetGenericArguments()[0];
                    if(elementType.IsGenericType && elementType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                    {
                        var keyType = elementType.GetGenericArguments()[0];
                        var keyUnderlyingType = Nullable.GetUnderlyingType(keyType); //is this even possible?
                        keyType = keyUnderlyingType ?? keyType;
                        if(keyType.IsEnum || PxEnumConverterUtils.IsIntegralType(keyType))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'");
            }

            if(!objectType.IsGenericType)
            {
                throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'");
            }

            var keyType = objectType.GetGenericArguments()[0];
            var nullableKeyType = Nullable.GetUnderlyingType(keyType);
            var enumType = BaseEnumType ?? nullableKeyType ?? keyType;
            var effectiveKeyType = nullableKeyType ?? keyType;
            var parseKeyType = effectiveKeyType;

            if (effectiveKeyType.IsEnum)
            {
                parseKeyType = Enum.GetUnderlyingType(effectiveKeyType);
            }

            var valueType = objectType.GetGenericArguments()[1];
            var constructedDictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            
            var jObject = JObject.Load(reader);
            IDictionary dictionary = (IDictionary)Activator.CreateInstance(constructedDictionaryType);

            foreach(var item in jObject)
            {
                object key = null;
                if(long.TryParse(item.Key, out long lResult))
                {
                    key = Convert.ChangeType(lResult, parseKeyType);
                }
                else if(ulong.TryParse(item.Key, out ulong uResult))
                {
                    key = Convert.ChangeType(uResult, parseKeyType);
                }
                else
                {
                    if (!PxEnum.TryParse(enumType, item.Key, out key) && SkipUnknownKeys)
                    {
                        continue;
                    }
                }

                if(key == null)
                {
                    throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'. Unable to parse key '{item.Key}'");
                }

                if(effectiveKeyType.IsEnum)
                {
                    key = Enum.ToObject(effectiveKeyType, key);
                }
                else
                {
                    key = Convert.ChangeType(key, effectiveKeyType);
                }

                if(dictionary.Contains(key))
                    throw new JsonSerializationException($"Error converting value {reader.Value} to type '{objectType.Name}'. Path '{reader.Path}'. Duplicate keys. Enum type or enum extension is probably malformed with multiple names resolving to the same underlying value ({key})");

                dictionary.Add(key, item.Value.ToObject(valueType, serializer));
            }

            return dictionary;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if(value == null)
            {
                writer.WriteNull();
                return;
            }

            serializer.Serialize(writer, value);
            return;
        }
    }

    /// <summary>
    /// Helper methods for internal use only.
    /// </summary>
    internal static class PxEnumConverterUtils
    {
        public static bool IsIntegralType(Type type)
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
    }
}