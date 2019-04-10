/*
Copyright (c) 2018-2019 Chris Leclair https://www.xcvgsystems.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Custom converters for Unity types and Newtonsoft Json.NET
/// </summary>
/// <remarks>
/// <para>It is very aggressive about including $type value because there's no consistent way to tell when that's needed.</para>
/// <para>Refer to the class documentation for details, but this should cover most of Unity's basic types.</para>
/// <para>The serialization format is mostly compatible with Wanzyee Studio's Json.NET Converters and probably with parentElement, LLC's JSON.NET For Unity. It should at least deserialize output from those.</para>
/// </remarks>
namespace CCJsonConverters
{

    /// <summary>
    /// Default configurations for the converters
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// A list containing all the converters in the package
        /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Vector2"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Vector2Int"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Vector3"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Vector3Int"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Vector4"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Quaternion"/>
    /// </summary>
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

    /// <summary>
    /// Converter for <see cref="Color"/>
    /// </summary>
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