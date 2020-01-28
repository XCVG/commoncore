using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore.StringSub
{
    /// <summary>
    /// String subber for environment vars and paths
    /// </summary>
    /// <remarks>
    /// <para>Reserves the "env" selector</para>
    /// <para>Supports many paths like %Data% as well; look at MatchPatterns</para>
    /// </remarks>
    public class EnvironmentStringSubber : IStringSubber
    {

        public IEnumerable<string> MatchPatterns => new string[] { "env", "%Data%", "%GameFolder%", "%PersistentData%", "%StreamingAssets%", "%Screenshots%",
            "%AppData%", "%LocalAppData%", "%UserProfile%", "%MyDocuments%", "%MyPictures%", "%MyVideos%" };

        public string Substitute(string[] sequenceParts)
        {
            switch (sequenceParts[0])
            {
                case "%Data%":
                    return CoreParams.DataPath;
                case "%GameFolder%":
                    return CoreParams.GameFolderPath;
                case "%PersistentData%":
                    return CoreParams.PersistentDataPath;
                case "%LocalData%":
                    return CoreParams.LocalDataPath;
                case "%StreamingAssets%":
                    return CoreParams.StreamingAssetsPath;
                case "%Screenshots%":
                    return CoreParams.ScreenshotsPath;
                case "%AppData%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                case "%LocalAppData%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                case "%UserProfile%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                case "%MyDocuments%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                case "%MyPictures%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                case "%MyVideos%":
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                case "env":
                    switch (sequenceParts[1])
                    {
                        //TODO thread safe versions

                        case "CurrentScene":
                            return SceneManager.GetActiveScene().name; //TODO thread-safe versions, but we'd need to integrate that into core
                        case "Platform":
                            return Application.platform.ToString();
                        case "Identifier":
                            return Application.identifier;
                        case "ProductName":
                            return Application.productName;
                        case "CompanyName":
                            return Application.companyName;
                        case "Version":
                            return Application.version;
                        case "VersionName":
                            return CoreParams.GameVersionName;
                        case "UnityVersion":
                            return Application.unityVersion;
                        case "CoreVersion":
                            return CoreParams.VersionCode.ToString();
                        case "CoreVersionName":
                            return CoreParams.VersionName;
                        case "DeviceModel":
                            return SystemInfo.deviceModel;
                        case "DeviceName":
                            return SystemInfo.deviceName;
                        case "DeviceType":
                            return SystemInfo.deviceType.ToString();
                        case "GraphicsDeviceName":
                            return SystemInfo.graphicsDeviceName;
                        case "GraphicsDeviceType":
                            return SystemInfo.graphicsDeviceType.ToString();
                        case "OperatingSystem":
                            return SystemInfo.operatingSystem.ToString();
                        case "OperatingSystemFamily":
                            return SystemInfo.operatingSystemFamily.ToString();
                        case "ProcessorType":
                            return SystemInfo.processorType.ToString();
                        case "MemorySize":
                            return SystemInfo.systemMemorySize.ToString();
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new ArgumentException();
            }
        }
    }
}