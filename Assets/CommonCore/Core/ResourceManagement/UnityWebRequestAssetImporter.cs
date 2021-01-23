using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using CommonCore.Async;

namespace CommonCore.ResourceManagement
{
    /// <summary>
    /// AssetImporter using UnityWebRequest
    /// </summary>
    /// <remarks>
    /// <para>Just some very limited audio imports for now</para>
    /// </remarks>
    public class UnityWebRequestAssetImporter : IResourceImporter
    {
        public bool CanLoadSync => false;

        public bool CanLoadResource(string path, ResourceLoadContext context)
        {
            if(Path.GetExtension(path).Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public Type GetResourceType(string path, ResourceLoadContext context)
        {
            if (Path.GetExtension(path).Equals(".ogg", StringComparison.OrdinalIgnoreCase) || Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(AudioClip);
            }

            throw new NotSupportedException();
        }

        public object LoadResource(string path, Type target, ResourceLoadContext context)
        {
            throw new NotImplementedException();
        }

        public async Task<object> LoadResourceAsync(string path, Type target, ResourceLoadContext context)
        {
            var aType = AudioType.UNKNOWN;

            if (Path.GetExtension(path).Equals(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                aType = AudioType.OGGVORBIS;
            }
            else if (Path.GetExtension(path).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                aType = AudioType.MPEG;
            }

            using (var request = UnityWebRequestMultimedia.GetAudioClip($"file://{path}", aType))
            {
                var operation = request.SendWebRequest();

                //dumb AF but fine for now
                while(!operation.isDone)
                {
                    await Task.Delay(60);
                }

                if (request.isNetworkError || request.isHttpError)
                {
                    throw new Exception(request.error);
                }
                else
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                    clip.name = Path.GetFileNameWithoutExtension(path);
                    return clip;
                }
            }

            throw new NotImplementedException();
        }
    }
}