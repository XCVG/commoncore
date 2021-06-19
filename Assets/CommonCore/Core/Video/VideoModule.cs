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
        }
        
    }
}