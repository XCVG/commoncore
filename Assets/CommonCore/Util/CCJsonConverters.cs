/*
*            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
*                    Version 2, December 2004
*
* Copyright (C) 2018 Chris Leclair <chris@xcvgsystems.com>
*
* Everyone is permitted to copy and distribute verbatim or modified
* copies of this license document, and changing it is allowed as long
* as the name is changed.
*
*            DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE
*   TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION
*
*   0. You just DO WHAT THE FUCK YOU WANT TO.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CCJsonConverters
{

    public static class Defaults
    {
        public static IList<JsonConverter> Converters
        {
            get
            {
                return new List<JsonConverter>() { new Vector2Converter(), new Vector2IntConverter(),
                    new Vector3Converter(), new Vector3IntConverter(), new Vector4Converter(),
                    new QuaternionConverter(), new ColorConverter()};
            }
        }
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector2 result = default(Vector2);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = (float)reader.ReadAsDouble().Value;
                            break;
                        case "y":
                            result.y = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();            
            if(serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }                
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }

    public class Vector2IntConverter : JsonConverter<Vector2Int>
    {

        public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector2Int result = default(Vector2Int);

            while(reader.Read())
            {
                if(reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = reader.ReadAsInt32().Value;
                            break;
                        case "y":
                            result.y = reader.ReadAsInt32().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WriteEndObject();
        }
    }

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 result = default(Vector3);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = (float)reader.ReadAsDouble().Value;
                            break;
                        case "y":
                            result.y = (float)reader.ReadAsDouble().Value;
                            break;
                        case "z":
                            result.z = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }
    }

    public class Vector3IntConverter : JsonConverter<Vector3Int>
    {
        public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3Int result = default(Vector3Int);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = reader.ReadAsInt32().Value;
                            break;
                        case "y":
                            result.y = reader.ReadAsInt32().Value;
                            break;
                        case "z":
                            result.z = reader.ReadAsInt32().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }
    }

    public class Vector4Converter : JsonConverter<Vector4>
    {
        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector4 result = default(Vector4);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = (float)reader.ReadAsDouble().Value;
                            break;
                        case "y":
                            result.y = (float)reader.ReadAsDouble().Value;
                            break;
                        case "z":
                            result.z = (float)reader.ReadAsDouble().Value;
                            break;
                        case "w":
                            result.w = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }
    }

    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Quaternion result = default(Quaternion);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "x":
                            result.x = (float)reader.ReadAsDouble().Value;
                            break;
                        case "y":
                            result.y = (float)reader.ReadAsDouble().Value;
                            break;
                        case "z":
                            result.z = (float)reader.ReadAsDouble().Value;
                            break;
                        case "w":
                            result.w = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WritePropertyName("w");
            writer.WriteValue(value.w);
            writer.WriteEndObject();
        }
    }

    public class ColorConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Color result = default(Color);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "r":
                            result.r = (float)reader.ReadAsDouble().Value;
                            break;
                        case "g":
                            result.g = (float)reader.ReadAsDouble().Value;
                            break;
                        case "b":
                            result.b = (float)reader.ReadAsDouble().Value;
                            break;
                        case "a":
                            result.a = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("r");
            writer.WriteValue(value.r);
            writer.WritePropertyName("g");
            writer.WriteValue(value.g);
            writer.WritePropertyName("b");
            writer.WriteValue(value.b);
            writer.WritePropertyName("a");
            writer.WriteValue(value.a);
            writer.WriteEndObject();
        }
    }
}