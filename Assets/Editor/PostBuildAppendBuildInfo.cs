using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Newtonsoft.Json;
using CommonCore;
using System.IO;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

public class PostBuildAppendBuildInfo : IPostprocessBuildWithReport
{
    public int callbackOrder => 10;

    public void OnPostprocessBuild(BuildReport report)
    {
#if UNITY_WSA
        var targetFolder = report.summary.outputPath;
#else
        var targetFolder = Path.GetDirectoryName(report.summary.outputPath);
#endif
        var targetPath = Path.Combine(targetFolder, "buildinfo.json");

        var buildInfo = new BuildInfo()
        {
            FrameworkVersion = CoreParams.VersionCode,
            FrameworkVersionName = CoreParams.VersionName,
            UnityVersion = TypeUtils.ParseVersion(Application.unityVersion),
            UnityVersionName = Application.unityVersion,
            GameVersion = TypeUtils.ParseVersion(Application.version),
            GameVersionName = CoreParams.GameVersionName,

            ProductName = Application.productName,
            CompanyName = Application.companyName,

            BuildDate = DateTimeOffset.Now,
            BuildPlatform = Application.platform
        };

        AppendGitInfoIfExists(buildInfo);

        var json = JsonConvert.SerializeObject(buildInfo, new JsonSerializerSettings()
        {
            Formatting = Formatting.Indented,
            Converters = new JsonConverter[] { new VersionConverter(), new StringEnumConverter() },
            NullValueHandling = NullValueHandling.Ignore
        });

        File.WriteAllText(targetPath, json);
    }

    private void AppendGitInfoIfExists(BuildInfo buildInfo)
    {
        bool isInRepo = InvokeGit("rev-parse --is-inside-work-tree") == "true";

        if (!isInRepo)
            return;

        string branchName = InvokeGit("rev-parse --abbrev-ref HEAD");
        string commitHash = InvokeGit("rev-parse HEAD");

        buildInfo.GitBranch = branchName;
        buildInfo.GitCommitHash = commitHash;
        
    }

    private string InvokeGit(string command)
    {
        try
        {
            var projectFolder = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));

            Process p = new Process();
            p.StartInfo.FileName = "git";
            p.StartInfo.Arguments = command;
            p.StartInfo.WorkingDirectory = projectFolder;

            p.StartInfo.UseShellExecute = false;

            p.StartInfo.RedirectStandardOutput = true;

            p.Start();

            string output = p.StandardOutput.ReadToEnd()?.Trim();

            p.WaitForExit();

            return output;
        }
        catch(Exception e)
        {
            UnityEngine.Debug.LogError($"[{nameof(PostBuildAppendBuildInfo)}] Error invoking Git ({e.GetType().Name})");
            UnityEngine.Debug.LogException(e);
            return null;
        }
    }

    public class BuildInfo
    {
        public Version FrameworkVersion;
        public string FrameworkVersionName;
        public Version UnityVersion;
        public string UnityVersionName;
        public Version GameVersion;
        public string GameVersionName;

        public string ProductName;
        public string CompanyName;

        public DateTimeOffset BuildDate;
        public RuntimePlatform BuildPlatform;

        public string GitBranch;
        public string GitCommitHash;

    }
}
