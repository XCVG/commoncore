using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Newtonsoft.Json;
using UnityEditor;

/// <summary>
/// Generates a resource manifest after a build
/// </summary>
public class PostBuildGenerateResourceManifest : IPostprocessBuildWithReport //TODO run it at a more appropriate time?
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        try
        {
            var resourcesFolders = GetResourceFolders();
            var targetFolder = Path.Combine(Path.GetDirectoryName(report.summary.outputPath), $"{Application.productName}_Data", "StreamingAssets");

            List<string> directories = new List<string>();
            List<ResourceFolderModel> models = new List<ResourceFolderModel>();

            foreach (var resourcesFolder in resourcesFolders)
            {
                List<string> rDirectories = new List<string>();
                EnumerateDirectories(resourcesFolder, rDirectories);
                directories.AddRange(CleanDirectoryListing(resourcesFolder, rDirectories));
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
            //File.WriteAllText(targetFile, string.Join("\n", directories.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(d => d)));
            File.WriteAllText(targetFile, JsonConvert.SerializeObject(fullModel, Formatting.Indented));

            Debug.Log($"[{nameof(PostBuildGenerateResourceManifest)}] Generated resource manifest at {targetFile}!");
        }
        catch(Exception e)
        {
            throw new UnityEditor.Build.BuildFailedException(e);
        }
    }

    private IEnumerable<string> GetResourceFolders()
    {
        var basePath = Application.dataPath;
        List<string> resourceFolders = new List<string>();

        EnumerateResourceFolders(basePath, resourceFolders);

        return resourceFolders;
    }

    private void EnumerateResourceFolders(string baseDirectory, List<string> directories)
    {
        var dirs = Directory.EnumerateDirectories(baseDirectory).ToList();
        foreach(var dir in dirs)
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

    private void EnumerateDirectories(string baseDirectory, List<string> directories)
    {
        var d = Directory.EnumerateDirectories(baseDirectory).ToList();
        directories.AddRange(d);
        foreach(var dir in d)
        {
            EnumerateDirectories(dir, directories);
        }
    }

    private IEnumerable<string> CleanDirectoryListing(string basePath, IEnumerable<string> directories)
    {
        return directories.Select(d => d.Substring(basePath.Length).Replace('\\', '/'));
    }

    private ResourceFolderModel BuildFolderModel(string basePath, string directoryPath)
    {
        string path = directoryPath.Substring(basePath.Length).Replace('\\', '/');
        var folderModel = new ResourceFolderModel() { Path = path, Items = new List<ResourceItemModel>() };

        foreach(var file in Directory.EnumerateFiles(directoryPath))
        {
            if (Path.GetExtension(file).Equals(".meta", StringComparison.OrdinalIgnoreCase))
                continue;
            folderModel.Items.Add(new ResourceItemModel() { Name = Path.GetFileNameWithoutExtension(file), Type = GetAssetType(file) });
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
        catch(Exception e)
        {
            return "";
        }
        
    }

    private IEnumerable<ResourceFolderModel> MergeFolderModels(IEnumerable<ResourceFolderModel> models)
    {
        List<ResourceFolderModel> mergedModels = new List<ResourceFolderModel>();

        foreach(var model in models)
        {
            var existingModel = models.SingleOrDefault(m => m.Path == model.Path);
            if(existingModel != null)
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