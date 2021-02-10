using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Copies some files from the source folder after a build (for readmes etc)
/// </summary>
public class PostBuildFileCopy : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        try
        {
            var projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
            var targetFolder = Path.GetDirectoryName(report.summary.outputPath); //may break on non-windows?
            var iniPath = Path.Combine(projectFolder, "filecopy.ini");
            if (File.Exists(iniPath))
            {
                List<string> filePaths = new List<string>();
                var lines = File.ReadAllLines(iniPath);

                foreach(var line in lines)
                {
                    if (line.TrimStart().StartsWith("#"))
                        continue;

                    var path = Path.Combine(projectFolder, line.Trim());
                    if(Directory.Exists(path))
                    {
                        CopyFilesRecursively(new DirectoryInfo(path), new DirectoryInfo(Path.Combine(targetFolder, line.Trim())), filePaths);
                    }
                    else if(File.Exists(path))
                    {
                        File.Copy(path, Path.Combine(targetFolder, Path.GetFileName(path)), true);
                        filePaths.Add(path);
                    }
                }

                Debug.Log($"[PostBuildFileCopy] Copied {filePaths.Count} files \n{string.Join(",", filePaths)}");
            }
            else
            {
                Debug.Log("[PostBuildFileCopy] No filecopy.ini found!");
            }
        }
        catch(Exception e)
        {
            Debug.LogError($"[PostBuildFileCopy] Fatal Error {e.GetType().Name}");
            Debug.LogException(e);
        }
    }

    //based on https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
    private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, ICollection<string> copiedPathsCollection = null)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
        {
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), copiedPathsCollection);
        }
        foreach (FileInfo file in source.GetFiles())
        {
            file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            copiedPathsCollection?.Add(file.Name);
        }
    }
}
