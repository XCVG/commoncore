using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommonCore.ResourceManagement
{
    /// <summary>
    /// Model and handling for Resource Manifest
    /// </summary>
    public class ResourceManifest
    {
        private List<string> Folders;

        internal static ResourceManifest Load()
        {
            try
            {
                if (CoreParams.IsEditor) //why a runtime check? so we can get intellisense on the non-editor parts, it's really that petty
                {
                    //load from actual folders
#if UNITY_EDITOR
                    var resourcesFolders = EditorResourceManifest.GetResourceFolders();
                    List<string> directories = new List<string>();

                    foreach (var resourcesFolder in resourcesFolders)
                    {
                        List<string> rDirectories = new List<string>();
                        EditorResourceManifest.EnumerateDirectories(resourcesFolder, rDirectories);
                        directories.AddRange(EditorResourceManifest.CleanDirectoryListing(resourcesFolder, rDirectories));
                    }

                    var folders = directories.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(d => d).ToList();
                    return new ResourceManifest() { Folders = folders };
#else
                    throw new NotSupportedException();
#endif
                }
                else
                {
                    //attempt to load manifest
                    string path = Path.Combine(CoreParams.StreamingAssetsPath, "core_resources.json");
                    JObject mObject = JObject.Parse(File.ReadAllText(path));
                    JArray mArray = mObject["Folders"] as JArray;
                    var folders = mArray.ToObject<List<string>>();

                    return new ResourceManifest() { Folders = folders };
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"[ResourceManifest] Failed to load resource manifest ({e.GetType().Name})");
                Debug.LogException(e);
                return null;
            }
            
        }

        public IEnumerable<string> EnumerateFolders() => Folders.ToArray();

        public IEnumerable<string> GetFoldersInFolder(string basePath)
        {
            string pathCleaned = "/" + basePath.Replace('\\', '/').Trim('/', '\\');
            int numSeparators = pathCleaned.CountChar('/');
            return Folders.Where(f => f.StartsWith(pathCleaned, StringComparison.OrdinalIgnoreCase) && f.CountChar('/') == numSeparators + 1);
        }

        public IEnumerable<string> GetFoldersRecursive(string basePath)
        {
            string pathCleaned = "/" + basePath.Replace('\\', '/').Trim('/', '\\');
            return Folders.Where(f => f.StartsWith(pathCleaned, StringComparison.OrdinalIgnoreCase));
        }

    }

    //also used by PostBuildGenerateResourceManifest
#if UNITY_EDITOR
    public static class EditorResourceManifest
    {
        public static IEnumerable<string> GetResourceFolders()
        {
            var basePath = Application.dataPath;
            List<string> resourceFolders = new List<string>();

            EnumerateResourceFolders(basePath, resourceFolders);

            return resourceFolders;
        }

        private static void EnumerateResourceFolders(string baseDirectory, List<string> directories)
        {
            var dirs = Directory.EnumerateDirectories(baseDirectory).ToList();
            foreach (var dir in dirs)
            {
                var dirName = Path.GetFileName(dir.TrimEnd('/', '\\'));

                if (dirName.Equals("Resources", StringComparison.OrdinalIgnoreCase))
                {
                    directories.Add(dir);
                }
                else
                {
                    EnumerateResourceFolders(dir, directories);
                }
            }
        }

        public static void EnumerateDirectories(string baseDirectory, List<string> directories)
        {
            var d = Directory.EnumerateDirectories(baseDirectory).ToList();
            directories.AddRange(d);
            foreach (var dir in d)
            {
                EnumerateDirectories(dir, directories);
            }
        }

        public static IEnumerable<string> CleanDirectoryListing(string basePath, IEnumerable<string> directories)
        {
            return directories.Select(d => d.Substring(basePath.Length).Replace('\\', '/'));
        }
    }
#endif

}