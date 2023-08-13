using CommonCore.ResourceManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.VersionControl;

/// <summary>
/// Generates a resource manifest after a build
/// </summary>
public class PreBuildGenerateResourceManifest : IPreprocessBuildWithReport //TODO run it at a more appropriate time?
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        try
        {
            var resourcesFolders = EditorResourceManifest.GetResourceFolders();

            var targetFolder = Application.streamingAssetsPath;

            List<string> directories = new List<string>();
            List<ResourceFolderModel> models = new List<ResourceFolderModel>();

            foreach (var resourcesFolder in resourcesFolders)
            {
                List<string> rDirectories = new List<string>();
                EditorResourceManifest.EnumerateDirectories(resourcesFolder, rDirectories);
                directories.AddRange(EditorResourceManifest.CleanDirectoryListing(resourcesFolder, rDirectories));
                foreach (var dir in rDirectories)
                {
                    models.Add(BuildFolderModel(resourcesFolder, dir));
                }
            }

            var fullModel = new
            {
                Folders = directories.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(d => d),
                FolderDetails = models
            };

            var targetFile = Path.Combine(targetFolder, "core_resources.json");
            File.WriteAllText(targetFile, JsonConvert.SerializeObject(fullModel, Formatting.Indented));

            Debug.Log($"[{nameof(PreBuildGenerateResourceManifest)}] Generated resource manifest at {targetFile}!");
        }
        catch (Exception e)
        {
            throw new UnityEditor.Build.BuildFailedException(e);
        }
    }

    private ResourceFolderModel BuildFolderModel(string basePath, string directoryPath)
    {
        string path = directoryPath.Substring(basePath.Length).Replace('\\', '/');
        var folderModel = new ResourceFolderModel() { Path = path, Items = new List<ResourceItemModel>() };

        foreach (var file in Directory.EnumerateFiles(directoryPath))
        {
            if (Path.GetExtension(file).Equals(".meta", StringComparison.OrdinalIgnoreCase))
                continue;
            var types = GetAllAssetTypes(file);
            foreach (var type in types)
            {
                folderModel.Items.Add(new ResourceItemModel() { Name = Path.GetFileNameWithoutExtension(file), Type = type });
            }            
        }

        return folderModel;
    }

    private string GetAssetType(string assetPath)
    {
        string shortenedPath = "Assets/" + assetPath.Substring(Application.dataPath.Length).Replace('\\', '/');
        try
        {
            return AssetDatabase.GetMainAssetTypeAtPath(shortenedPath).ToString();
        }
        catch (Exception)
        {
            return "";
        }

    }

    private IEnumerable<string> GetAllAssetTypes(string assetPath)
    {
        HashSet<string> types = new HashSet<string>();
        string shortenedPath = "Assets/" + assetPath.Substring(Application.dataPath.Length).Replace('\\', '/');
        try
        {
            string mainType = AssetDatabase.GetMainAssetTypeAtPath(shortenedPath).ToString();
            if(mainType == "UnityEngine.GameObject")
            {
                types.Add(mainType);
            }
            else
            {
                var assets = AssetDatabase.LoadAllAssetsAtPath(shortenedPath);
                foreach (var asset in assets)
                {
                    types.Add(asset.GetType().FullName);
                }
            }            
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error loading types for asset \"{assetPath}\"\n {e.GetType().Name}: {e.Message}");
        }
        return types;
    }

    private IEnumerable<ResourceFolderModel> MergeFolderModels(IEnumerable<ResourceFolderModel> models)
    {
        List<ResourceFolderModel> mergedModels = new List<ResourceFolderModel>();

        foreach (var model in models)
        {
            var existingModel = models.SingleOrDefault(m => m.Path == model.Path);
            if (existingModel != null)
            {
                existingModel.Items.AddRange(model.Items);
                existingModel.Items = existingModel.Items.GroupBy(x => x.Name).Select(g => g.First()).ToList();
            }
            else
            {
                mergedModels.Add(model);
            }
        }

        return mergedModels;
    }

    public class ResourceFolderModel
    {
        public string Path;
        public List<ResourceItemModel> Items;
    }

    public class ResourceItemModel
    {
        public string Name;
        public string Type;
    }
}