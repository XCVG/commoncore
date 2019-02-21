using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Text;

namespace CommonCore.DebugLog
{
    /// <summary>
    /// Miscellaneous utilities to aid debugging
    /// </summary>
    public static class DebugUtils
    {
        private const string DebugPath = "debug";
        private const string DateFormat = "yyyy-MM-dd_HHmmss";

        /// <summary>
        /// Serializes an arbitrary object to a json string
        /// </summary>
        public static string JsonStringify(object o)
        {
            return JsonStringify(o, false);
        }

        /// <summary>
        /// Serializes an arbitrary object to a json string
        /// </summary>
        public static string JsonStringify(object o, bool ignoreErrors)
        {
            var settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = TypeNameHandling.All,
                Converters = CCJsonConverters.Defaults.Converters
            };
            if (ignoreErrors)
                settings.Error += (sender, args) => { args.ErrorContext.Handled = true; };
            return JsonConvert.SerializeObject(o, settings);
        }

        /// <summary>
        /// Serializes an arbitrary object to json, then writes it to a dated debug file
        /// </summary>
        public static void JsonWrite(object o, string name)
        {
            try
            {
                string fileName = DateTime.Now.ToString(DateFormat) + "_" + name + ".json";
                string filePath = Path.Combine(CoreParams.PersistentDataPath, DebugPath, fileName);

                string jsonData = JsonStringify(o, true);

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                File.WriteAllText(filePath, jsonData);
            }
            catch(Exception e)
            {
                CDebug.LogEx($"Failed to write object {o.Ref()?.GetType().Name} to file {name} ({e.GetType().Name})", LogLevel.Warning, null);
            }
        }

        /// <summary>
        /// Writes arbitrary text to a dated debug file
        /// </summary>
        public static void TextWrite(string s, string name)
        {
            try
            {
                string fileName = DateTime.Now.ToString(DateFormat) + "_" + name + ".txt";
                string filePath = Path.Combine(CoreParams.PersistentDataPath, DebugPath, fileName);

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                File.WriteAllText(filePath, s, Encoding.UTF8);
            }
            catch (Exception e)
            {
                CDebug.LogEx($"Failed to write text to file {name} ({e.GetType().Name})", LogLevel.Warning, null);
            }
        }

        /// <summary>
        /// Writes arbitrary binary data to a dated debug file
        /// </summary>
        public static void BinaryWrite(byte[] b, string name)
        {
            try
            {
                string fileName = DateTime.Now.ToString(DateFormat) + "_" + name + ".bin";
                string filePath = Path.Combine(CoreParams.PersistentDataPath, DebugPath, fileName);

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                File.WriteAllBytes(filePath, b);
            }
            catch (Exception e)
            {
                CDebug.LogEx($"Failed to write binary data to file {name} ({e.GetType().Name})", LogLevel.Warning, null);
            }
        }

    }
}