using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace CommonCore.Video
{

    /// <summary>
    /// Manages video files accessible to the game, including addons adding their own video paths
    /// </summary>
    /// <remarks>
    /// <para>Note that this is almost useless and just blindly gives you a path in WebGL</para>
    /// </remarks>
    public class VideoModule : CCModule
    {
        private static readonly IReadOnlyList<string> VideoExtensions = new string[] { "mp4", "webm" };

        private List<string> SearchPaths = new List<string>();
        private Dictionary<string, string> LookupCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public VideoModule()
        {
            SetupDefaultPaths();
        }

        public override void OnAddonLoaded(AddonLoadData data)
        {
            SearchPaths.AddRange(data.VideoPaths);
        }

        public override void OnAllAddonsLoaded()
        {
            Log($"{SearchPaths.Count} search paths \n{SearchPaths.ToNiceString()}");

            if (CoreParams.LoadPolicy == DataLoadPolicy.OnStart)
                PrecacheAllPaths();
        }

        public bool HasVideo(string videoName)
        {
            return !string.IsNullOrEmpty(GetPathForVideo(videoName));
        }

        public string GetPathForVideo(string videoName)
        {
            if (LookupCache.TryGetValue(videoName, out var path))
                return path;

            return GetAndCacheInternal(videoName);
        }

        private void SetupDefaultPaths()
        {
            SearchPaths.Add(Path.Combine(CoreParams.StreamingAssetsPath, "Video"));
            SearchPaths.Add(Path.Combine(CoreParams.GameFolderPath, "Video"));
        }        

        private void PrecacheAllPaths()
        {
            //enumerating directories won't work in WebGL, so don't try
#if UNITY_WEBGL
            return;
#endif

            foreach(string folder in SearchPaths)
            {
                if (!Directory.Exists(folder))
                    continue;

                var videoFiles = Directory.EnumerateFiles(folder)
                    .Where(f => VideoExtensions.Contains(Path.GetExtension(f).TrimStart('.'), StringComparer.OrdinalIgnoreCase));
                var videoFileEntries = videoFiles.ToDictionary(f => Path.GetFileNameWithoutExtension(f), f => f);
                LookupCache.AddRangeReplaceExisting(videoFileEntries);
            }
           
            Log($"Precached {LookupCache.Count} video file paths");
        }

        private string GetAndCacheInternal(string videoName)
        {
#if UNITY_WEBGL
            //try mp4 first in editor
            if(CoreParams.IsEditor)
            {
                string editorVideoPath = Path.Combine(Application.streamingAssetsPath, "Video", Path.GetFileNameWithoutExtension(videoName) + ".mp4");
                if(File.Exists(editorVideoPath))
                    return editorVideoPath;
            }

            //we do not check this first because it would be an expensive request in WebGL
            return Path.Combine(Application.streamingAssetsPath, "Video", Path.GetFileNameWithoutExtension(videoName) + ".webm");
#else
            string path = null;

            foreach(var folder in ((IEnumerable<string>)SearchPaths).Reverse()) //awkward cast to force LINQ call
            {
                if (!Directory.Exists(folder))
                    continue;

                path = Directory.EnumerateFiles(folder)
                    .Where(f => Path.GetFileNameWithoutExtension(f).Equals(videoName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault(); //should we prefer mp4 over webm (possibly depending on platform)?
                if (!string.IsNullOrEmpty(path))
                    break;
            }

            if (path != null && CoreParams.LoadPolicy != DataLoadPolicy.OnDemand)
                LookupCache[videoName] = path;

            return path;
#endif
        }
        
    }
}