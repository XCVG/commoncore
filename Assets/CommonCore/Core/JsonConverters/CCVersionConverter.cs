/*
Copyright(c) 2007 James Newton-King, 2021 Chris Leclair

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json;

namespace CCJsonConverters
{
    /// <summary>
    /// Converts a <see cref="Version"/> to an object and from a string or object
    /// </summary>
    public class VersionConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is Version v)
            {
                //writer.WriteValue(value.ToString());

                writer.WriteStartObject();
                if (serializer.TypeNameHandling != TypeNameHandling.None)
                {
                    writer.WritePropertyName("$type");
                    writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
                }
                writer.WritePropertyName("Major");
                writer.WriteValue(v.Major);
                writer.WritePropertyName("Minor");
                writer.WriteValue(v.Minor);
                writer.WritePropertyName("Build");
                writer.WriteValue(v.Build);
                writer.WritePropertyName("Revision");
                writer.WriteValue(v.Revision);
                writer.WriteEndObject();
            }
            else
            {
                throw new JsonSerializationException("Expected Version object value");
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                if (reader.TokenType == JsonToken.String)
                {
                    try
                    {
                        Version v = new Version((string)reader.Value);
                        return v;
                    }
                    catch (Exception ex)
                    {
                        throw new JsonSerializationException($"Error parsing version string: {reader.Value}", ex);
                    }
                }
                else if (reader.TokenType == JsonToken.StartObject)
                {
                    var data = ReadObject(reader);
                    return ParseVersion(data);
                }                
            }

            throw new JsonSerializationException($"Error parsing version: unknown representation");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Version);
        }

        //adapted from ExpandoObjectConverter, yes it's gross
        private IDictionary<string, string> ReadObject(JsonReader reader)
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading object");
                        }

                        string v = reader.Value.ToString();

                        dict[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return dict;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading object");
        }

        private Version ParseVersion(IDictionary<string, string> data)
        {
            int major = 0, minor = 0, build = -1, revision = -1;

            if (data.TryGetValue("Major", out string sMajor))
                major = int.Parse(sMajor);

            if (data.TryGetValue("Minor", out string sMinor))
                minor = int.Parse(sMinor);

            if (data.TryGetValue("Build", out string sBuild))
                build = int.Parse(sBuild);

            if (data.TryGetValue("Revision", out string sRevision))
                revision = int.Parse(sRevision);

            if (revision >= 0)
                return new Version(major, minor, build, revision);
            else if (build >= 0)
                return new Version(major, minor, build);
            else
                return new Version(major, minor);
        }
    }
}
