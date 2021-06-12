using CommonCore.Config;
using CommonCore.DebugLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using CommonCore.ResourceManagement;

namespace CommonCore
{

    /// <summary>
    /// Manages addons and StreamingAssets
    /// </summary>
    public class AddonManager
    {

        private Dictionary<string, string> FoundAddons = null; //package name:path

        private Dictionary<string, AddonLoadContext> LoadedAddons = new Dictionary<string, AddonLoadContext>();

        private HashSet<string> LoadedScenes = new HashSet<string>();


        //addon paths
        private string StreamingAddonsPath => Path.Combine(CoreParams.StreamingAssetsPath, "Addons");
        private string InstallAddonsPath => Path.Combine(CoreParams.GameFolderPath, "Addons");
        private string LocalAddonsPath => Path.Combine(CoreParams.LocalDataPath, "Addons");
        private string RoamingAddonsPath => Path.Combine(CoreParams.PersistentDataPath, "Addons");


        public AddonManager()
        {

            //var manifestExample = new AddonManifest();
            //DebugUtils.JsonWrite(manifestExample, "AddonManifest");
        }

        public AddonBase GetAddonBase(string addonName)
        {
            if(LoadedAddons.TryGetValue(addonName, out var ctx))
            {
                return ctx.AddonBase;
            }

            return null;
        }

        public T GetAddonBase<T>(string addonName) where T : AddonBase
        {
            if (LoadedAddons.TryGetValue(addonName, out var ctx))
            {
                return (T)ctx.AddonBase;
            }

            return null;
        }

        public IEnumerable<string> EnumerateAddonScenePaths()
        {
            return LoadedScenes.ToArray();
        }

        public IEnumerable<Assembly> EnumerateAddonAssemblies()
        {
            return LoadedAddons.Select(kvp => kvp.Value.LoadedAssemblies).SelectMany(x => x);
        }

        public bool CanLoadAddons => CoreParams.ScriptingBackend == ScriptingImplementation.Mono2x && CoreParams.Platform != RuntimePlatform.WebGLPlayer;

        public void WarnOnSyncLoad()
        {
            if(CoreParams.LoadAddons && ConfigState.Instance.LoadAddons)
                Debug.LogWarning($"[AddonManager] Can't load addons during synchronous startup");
        }

        public void WarnIfUnsupported()
        {
            if(!CanLoadAddons)
                Debug.LogWarning($"[AddonManager] Can't load addons on this platform");
        }

        public async Task LoadStreamingAssetsAsync(Action<AddonLoadData> onLoadedMethod)
        {
            if (!(CoreParams.LoadAddons && CanLoadAddons))
                return;

            var context = new AddonLoadContext();
            //set various things in context
            context.Path = CoreParams.StreamingAssetsPath;
            context.AddonManager = this;
            context.MountPathOverride = "Streaming";
            context.ResourcePriority = ResourcePriority.Streaming;
            context.AbortOnSingleFileFailure = false;

            await LoadResourcesFromPathAsync(context);
            await LoadAssembliesAsync(context);

            onLoadedMethod(new AddonLoadData(context.LoadedAssemblies, context.LoadedResources));

        }

        public async Task LoadAddonsAsync(Action<AddonLoadData> onLoadedMethod)
        {
            if (!(CoreParams.LoadAddons && ConfigState.Instance.LoadAddons && CanLoadAddons))
                return;

            if (ConfigState.Instance.AddonsToLoad == null || ConfigState.Instance.AddonsToLoad.Count == 0)
            {
                Debug.Log("[AddonManager] Load order is empty, no addons will be loaded");
                return;
            }

            LogAddonsInLoadOrder();

            await ScanForAddonsAsync();

            //log found addons, then addons that will be loaded
            LogFoundAddons();

            List<string> addonsToLoad = GetAddonsToLoad();
            if(addonsToLoad == null || addonsToLoad.Count == 0)
            {
                Debug.Log("[AddonManager] Could not find any addons specified in load order, no addons will be loaded");
                return;
            }

            LogAddonsToLoad(addonsToLoad);

            foreach(string addon in addonsToLoad)
            {
                try
                {
                    string path = FoundAddons[addon];
                    await LoadAddonAsync(path, onLoadedMethod);
                    //GC.Collect();
                }
                catch(Exception e)
                {
                    Debug.LogError($"[AddonManager] Failed to load addon {addon} ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                }
            }
        }

        private async Task LoadAddonAsync(string path, Action<AddonLoadData> onLoadedMethod)
        {
            AddonLoadContext context = new AddonLoadContext();

            context.Path = path;
            context.ResourcePriority = ResourcePriority.Addon;

            context.AddonManager = this;
            context.OnLoadedCallback = onLoadedMethod;

            context.Manifest = ReadAddonManifest(path);

            if (context.Manifest.IgnoreSingleFileErrors)
                context.AbortOnSingleFileFailure = false;

            LoadMainAssembly(context);
            CreateAddonBase(context);

            await context.AddonBase.LoadAddonAsync(context);

            //if we get here, loading has succeeded
            LoadedAddons.Add(context.Manifest.Name, context);
        }

        private void LoadMainAssembly(AddonLoadContext context)
        {
            if (!string.IsNullOrEmpty(context.Manifest.MainAssembly)) //we do allow addons with no assembly
            {
                string dllPath = Path.Combine(context.Path, "managed", context.Manifest.MainAssembly + ".dll");
                var assembly = Assembly.LoadFile(dllPath);
                context.MainAssembly = assembly;
                context.LoadedAssemblies.Add(assembly);

                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.Log($"[AddonManager] Loaded assembly {assembly.FullName}");
            }
        }

        private void CreateAddonBase(AddonLoadContext context)
        {
            if (context.MainAssembly != null)
            {
                var types = context.MainAssembly.GetTypes();

                //scan for addonbase derived type, instantiate
                var abType = types.FirstOrDefault(t => t.IsSubclassOf(typeof(AddonBase)));

                if (abType != null)
                    context.AddonBaseType = abType;
                else
                    context.AddonBaseType = typeof(AddonBase); //if there is no addonbase derived type just use addonbase
            }
            else
                context.AddonBaseType = typeof(AddonBase);

            context.AddonBase = (AddonBase)Activator.CreateInstance(context.AddonBaseType);
        }

        public async Task LoadAssembliesAsync(AddonLoadContext context)
        {
            string dirPath = Path.Combine(context.Path, "managed");
            if(Directory.Exists(dirPath))
            {
                var dirEnumerable = Directory.EnumerateFiles(dirPath);
                foreach(var file in dirEnumerable)
                {
                    try
                    {
                        if (!Path.GetFileNameWithoutExtension(file).Equals(context.Manifest.MainAssembly) && Path.GetExtension(file).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                        {
                            //okay to load assembly!
                            var assembly = Assembly.LoadFile(file);
                            context.LoadedAssemblies.Add(assembly);

                            if (ConfigState.Instance.UseVerboseLogging)
                                Debug.Log($"[AddonManager] Loaded assembly {assembly.FullName}");
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"[AddonManager] Failed to load assembly \"{file}\" ({e.GetType().Name})");
                        if (ConfigState.Instance.UseVerboseLogging)
                            Debug.LogException(e);
                        if (context.AbortOnSingleFileFailure)
                            throw e;
                    }

                    await Task.Yield(); //don't try to load every dll in one frame
                }
            }
        }

        //TODO resilience, or should we just throw?

        public async Task LoadResourcesFromPathAsync(AddonLoadContext context)
        {
            //load elocal, then expand
            //load assetbundles, then loose files

            string elocalTargetPath = context.MountPath;
            string elocalPath = Path.Combine(context.Path, "elocal");
            string elocalBundlePath = Path.Combine(context.Path, "elocal.assetbundle");
            await tryLoadResourceFromAssetBundleAsync(elocalBundlePath, elocalTargetPath);
            await LoadResourcesInFolderRecurseAsync(context, elocalPath, elocalTargetPath);

            string expandTargetPath = ""; //I think this is actually correct
            string expandPath = Path.Combine(context.Path, "expand");
            string expandBundlePath = Path.Combine(context.Path, "expand.assetbundle");
            await tryLoadResourceFromAssetBundleAsync(expandBundlePath, expandTargetPath);
            await LoadResourcesInFolderRecurseAsync(context, expandPath, expandTargetPath);

            async Task tryLoadResourceFromAssetBundleAsync(string bundlePath, string targetPath)
            {
                try
                {
                    await LoadResourcesFromAssetBundleAsync(context, bundlePath, targetPath);
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to load assetbundle \"{bundlePath}\" ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                    if (context.AbortOnSingleFileFailure)
                        throw e;
                }
            }

        }

        public void RegisterLoadedScenes(AddonLoadContext context)
        {
            LoadedScenes.UnionWith(context.LoadedScenes);
        }

        private async Task LoadResourcesInFolderRecurseAsync(AddonLoadContext context, string folderPath, string targetPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            //Debug.Log($"load things in \"{folderPath}\" to \"{targetPath}\"");

            //handle both assetbundles and loose resources

            var files = Directory.EnumerateFiles(folderPath);
            foreach(var file in files)
            {
                string fileTargetPath = getTargetPath(file);

                try
                {
                    if (Path.GetExtension(file).Equals(".assetbundle", StringComparison.OrdinalIgnoreCase))
                        await LoadResourcesFromAssetBundleAsync(context, file, fileTargetPath);
                    else
                    {
                        var rh = await CCBase.ResourceManager.AddResourceFromFileAsync(fileTargetPath, file, context.ResourcePriority);
                        context.LoadedResources.Add(fileTargetPath, rh);
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"Failed to load \"{file}\" ({e.GetType().Name})");
                    if (ConfigState.Instance.UseVerboseLogging)
                        Debug.LogException(e);
                    if (context.AbortOnSingleFileFailure)
                        throw e;
                }
            }

            var subdirs = Directory.EnumerateDirectories(folderPath);
            foreach(var subdir in subdirs)
            {
                string subdirTargetPath = getTargetPath(subdir);

                await LoadResourcesInFolderRecurseAsync(context, subdir, subdirTargetPath);
            }

            string getTargetPath(string objectPath)
            {
                string fullBasePath = Path.GetFullPath(folderPath);
                string fullObjectPath = Path.GetFullPath(objectPath);
                string partialObjectPath = fullObjectPath.Substring(fullBasePath.Length); //watch the off-by-ones!
                string newTargetPath = targetPath + partialObjectPath;

                if (newTargetPath.StartsWith("/", StringComparison.OrdinalIgnoreCase) || newTargetPath.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
                    newTargetPath = newTargetPath.Substring(1);

                if (Path.HasExtension(newTargetPath))
                    newTargetPath = Path.ChangeExtension(newTargetPath, null);

                newTargetPath = newTargetPath.Replace('\\', '/');

                //Debug.Log($"{targetPath} + {partialObjectPath} = {newTargetPath}");

                return newTargetPath;
            }
        }

        private async Task LoadResourcesFromAssetBundleAsync(AddonLoadContext context, string bundlePath, string targetPath)
        {
            if (!File.Exists(bundlePath))
                return;

            if(ConfigState.Instance.UseVerboseLogging)
                Debug.Log($"loading assetbundle \"{bundlePath}\" to \"{targetPath}\"");

            //load from AssetBundles
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePath);
            while (!bundleLoadRequest.isDone)
                await Task.Yield(); //ugly but should work

            var assetBundle = bundleLoadRequest.assetBundle;
            if (assetBundle == null)
            {
                Debug.LogError($"[AddonManager] failed to load assetbundle \"{bundlePath}\"");
                throw new FileLoadException("Failed to load assetbundle"); //TODO change this to a more appropriate exception
            }

            var scenes = assetBundle.GetAllScenePaths();
            if(ConfigState.Instance.UseVerboseLogging && scenes != null && scenes.Length > 0)                
                Debug.Log(scenes.ToNiceString());

            if(scenes != null && scenes.Length > 0)
            {
                context.LoadedScenes.AddRange(scenes);                
            }
            
            var names = assetBundle.GetAllAssetNames();
            if (ConfigState.Instance.UseVerboseLogging && names != null && names.Length > 0)
                Debug.Log(names.ToNiceString());

            if (names != null && names.Length > 0)
            {
                bool useAssetBundlePaths = false;
                if (context.Manifest != null && context.Manifest.UseAssetBundlePaths)
                {
                    Debug.LogWarning($"[AddonManager] Addon {context.Manifest.Name} is set to use full asset bundle paths, which is considered experimental!");
                    //throw new NotImplementedException("Full asset bundle paths are not yet supported"); //we can actually add this with minimal difficulty

                    //so we'll just use the full paths from the asset bundle minus the "assets/" part and tack that on after the targetPrefix
                    //which will mount elocal.assetbundle at Addons/<package name> and expand.assetbundle at the root
                    //and really you should either use two pathed assetbundles or a lot of flat assetbundles, not multiple pathed assetbundles

                    useAssetBundlePaths = true;
                }

                string targetPrefix = targetPath.Replace('\\', '/');
                if (!targetPrefix.EndsWith("/", StringComparison.Ordinal))
                    targetPrefix = targetPrefix + "/";

                foreach (var name in names)
                {
                    string objectName = useAssetBundlePaths ? GetObjectPartialPathFromBundledName(name) : GetObjectNameFromBundledName(name);
                    string objectTargetPath = targetPrefix + objectName;

                    var rh = await CCBase.ResourceManager.AddResourceFromAssetBundleAsync(objectTargetPath, name, assetBundle, context.ResourcePriority);

                    context.LoadedResources.Add(objectTargetPath, rh);
                }
            }
        }

        public string GetObjectNameFromBundledName(string bundledName)
        {
            return Path.GetFileNameWithoutExtension(bundledName); //I _think_ this should work
        }

        public string GetObjectPartialPathFromBundledName(string bundledName)
        {
            if(bundledName.StartsWith("assets/", StringComparison.InvariantCultureIgnoreCase))
            {
                return bundledName.Substring(7); //probably off-by-one
            }

            return bundledName;
        }

        //scan the addon paths for addons
        private async Task ScanForAddonsAsync()
        {
            FoundAddons = new Dictionary<string, string>();

            await Task.Run(() =>
            {
                enumerateAddonsInFolder(StreamingAddonsPath);
                enumerateAddonsInFolder(InstallAddonsPath);
                enumerateAddonsInFolder(LocalAddonsPath);
                enumerateAddonsInFolder(RoamingAddonsPath);
            });

            void enumerateAddonsInFolder(string folder)
            {
                if(!Directory.Exists(folder))
                {
                    Debug.LogWarning($"[AddonManager] No addon directory found at {folder}");
                    return;
                }

                var dEnumerable = Directory.EnumerateDirectories(folder);

                foreach(string directoryPath in dEnumerable)
                {
                    try
                    {
                        var manifest = ReadAddonManifest(directoryPath);
                        if (string.IsNullOrEmpty(manifest.Name))
                            throw new KeyNotFoundException("Couldn't find a package name in package manifest");

                        if(FoundAddons.ContainsKey(manifest.Name))                        
                            Debug.LogWarning($"[AddonManager] Duplicate addon packages found at \"{FoundAddons[manifest.Name]}\" and \"{directoryPath}\"");

                        FoundAddons[manifest.Name] = directoryPath;

                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"[AddonManager] Failed to load metadata for addon in \"{directoryPath}\" ({e.GetType().Name})");
                        if(ConfigState.Instance.UseVerboseLogging)
                            Debug.LogException(e);
                    }
                }
            }
        }

        private List<string> GetAddonsToLoad()
        {
            //check the load order against available addons

            List<string> addonsToLoad = new List<string>();

            foreach(string addon in ConfigState.Instance.AddonsToLoad)
            {
                if (FoundAddons.ContainsKey(addon))
                    addonsToLoad.Add(addon);
            }

            return addonsToLoad;
        }

        private void LogAddonsInLoadOrder()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[AddonManager] Addons in load order:");
            foreach (var addon in ConfigState.Instance.AddonsToLoad)
            {
                sb.AppendFormat("\t{0}\n", addon);
            }

            Debug.Log(sb.ToString());
        }

        private void LogFoundAddons()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[AddonManager] Addons found:");
            foreach(var kvp in FoundAddons)
            {
                sb.AppendFormat("\t{0} ({1})\n", kvp.Key, kvp.Value);
            }

            Debug.Log(sb.ToString());
        }

        private void LogAddonsToLoad(List<string> addons)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("[AddonManager] Addons that will be loaded:");
            foreach(var addon in addons)
            {
                sb.AppendFormat("\t{0}\n", addon);
            }

            Debug.Log(sb.ToString());
        }

        private AddonManifest ReadAddonManifest(string path)
        {
            string pathToManifest = Path.Combine(path, "manifest.json");
            if (!File.Exists(pathToManifest))
                throw new FileNotFoundException("Couldn't find manifest.json!", pathToManifest);

            var fileText = File.ReadAllText(pathToManifest);
            AddonManifest manifest = CoreUtils.LoadJson<AddonManifest>(fileText);
            return manifest;
        }


    }
}
