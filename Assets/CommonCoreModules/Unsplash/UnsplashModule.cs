using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace CommonCore.Unsplash
{

    /// <summary>
    /// Module for presenting an "unsplash" screen when the game is exited
    /// </summary>
    /// <remarks>
    /// Currently only works on Windows, oh well
    /// </remarks>
    public class UnsplashModule : CCModule
    {
        /// <summary>
        /// The unsplash exe name- change this to what you need
        /// </summary>
        private const string UnsplashExecutableName = "Unsplash.exe";

        private string UnsplashExecutablePath;

        public UnsplashModule()
        {
            FindExecutable();

            if(!string.IsNullOrEmpty(UnsplashExecutablePath))
                Log($"Found unsplash executable at {UnsplashExecutablePath}");
        }

        public override void Dispose()
        {
            base.Dispose();

            if (!string.IsNullOrEmpty(UnsplashExecutablePath))
                StartExecutable();
        }

        private void FindExecutable()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer)
                return;

            string path = Path.Combine(CoreParams.GameFolderPath, UnsplashExecutableName);
            if (File.Exists(path))
                UnsplashExecutablePath = path;
        }

        private void StartExecutable()
        {
            Process p = new Process();
            p.StartInfo.FileName = UnsplashExecutablePath;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(UnsplashExecutablePath);

            p.Start();
        }
    }
}