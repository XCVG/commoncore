using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WaveLoader;

namespace CommonCore.ResourceManagement
{

    /// <summary>
    /// Loader for files->resources
    /// </summary>
    /// <remarks>Crude and ugly and will probably be redone 4 times</remarks>
    public static class ResourceLoader
    {

        /// <summary>
        /// Determines the resource type from file type
        /// </summary>
        /// <returns>The type to be used for a resourcehandle, or null on failure</returns>
        public static Type DetermineResourceType(string path)
        {
            string extension = Path.GetExtension(path);
            
            if(!string.IsNullOrEmpty(extension))
            {
                extension = extension.TrimStart('.').ToLower(CultureInfo.InvariantCulture);
                switch (extension)
                {
                    case "txt":
                    case "htm":
                    case "html":
                    case "xml":
                    case "json":
                    case "csv":
                    case "yaml":
                    case "fnt":
                        return typeof(TextAsset);
                    case "jpg":
                    case "jpeg":
                    case "png":
                    case "tga":
                    case "bmp":
                        return typeof(Texture2D);
                    case "wav":
                    case "wave":
                    case "mp3":
                    case "ogg":
                    case "oga":
                    case "m4a":
                    case "flac":
                        return typeof(AudioClip);
                }
            }

            return null;
        }

        public static T LoadFromFile<T>(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            T resource = LoadFromBytes<T>(bytes);

            if (resource is UnityEngine.Object obj)
                obj.name = Path.GetFileNameWithoutExtension(path);

            return resource;
        }

        public static async Task<T> LoadFromFileAsync<T>(string path)
        {
            byte[] bytes = null;
            await Task.Run(() => {
                bytes = File.ReadAllBytes(path);
            });

            T resource = await LoadFromBytesAsync<T>(bytes);
            if (resource is UnityEngine.Object obj)
                obj.name = Path.GetFileNameWithoutExtension(path);

            return resource;
        }

        public static T LoadFromBytes<T>(byte[] data)
        {
            return (T)LoadFromBytes(data, typeof(T));
        }

        public static async Task<T> LoadFromBytesAsync<T>(byte[] data)
        {
            //TODO actual async implementations

            await Task.Yield();
            return (T)LoadFromBytes(data, typeof(T));
        }

        private static object LoadFromBytes(byte[] data, Type type)
        {
            if(type == typeof(TextAsset))
            {
                return LoadTextAsset(data);
            }
            else if(type == typeof(Texture) || type == typeof(Texture2D))
            {
                return LoadTexture(data);
            }
            else if(type == typeof(AudioClip))
            {
                return LoadAudio(data);
            }

            return null;
        }

        private static TextAsset LoadTextAsset(byte[] data)
        {
            string str = Encoding.UTF8.GetString(data);
            return new TextAsset(str);
        }

        private static Texture2D LoadTexture(byte[] data)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            return tex;
        }

        private static AudioClip LoadAudio(byte[] data)
        {
            //TODO handling for things that aren't WAV
            return WaveFile.Load(data, false).ToAudioClip();
        }
    }
}
