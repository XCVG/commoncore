using CommonCore.Config;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WaveLoader;

namespace CommonCore.ResourceManagement
{

    /// <summary>
    /// Loader for files->resources
    /// </summary>
    /// <remarks>Crude and ugly and will probably be redone 4 times</remarks>
    public class ResourceLoader
    {

        private List<IResourceImporter> Importers = new List<IResourceImporter>();

        public ResourceLoader()
        {
            //register builtin importers
            RegisterImporter(new SpriteAssetImporter());
            RegisterImporter(new UnityWebRequestAssetImporter());
        }

        public void RegisterImporter(IResourceImporter importer)
        {
            Importers.Insert(0, importer);
            if (ConfigState.Instance.UseVerboseLogging)
                Debug.Log($"[ResourceLoader] registered resource importer \"{importer.GetType().Name}\"");
        }        

        /// <summary>
        /// Determines the resource type from file type
        /// </summary>
        /// <returns>The type to be used for a resourcehandle, or null on failure</returns>
        public Type DetermineResourceType(string path, ResourceLoadContext context)
        {
            foreach(var importer in Importers)
            {
                try
                {
                    if(importer.CanLoadResource(path, context) && (!context.AttemptingSyncLoad || importer.CanLoadSync))
                    {
                        context.ResourceImporter = importer;
                        return importer.GetResourceType(path, context); //note that malicious or malformed importers can scribble context
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"[ResourceLoader] Exception in asset importer {importer.GetType().Name}");
                    Debug.LogException(e);
                }
            }

            string extension = Path.GetExtension(path);
            
            if(!string.IsNullOrEmpty(extension))
            {
                extension = extension.TrimStart('.').ToLower(CultureInfo.InvariantCulture);
                switch (extension)
                {
                    case "txt":
                    case "htm":
                    case "html":
                    case "xml":
                    case "json":
                    case "csv":
                    case "yaml":
                    case "fnt":
                        return typeof(TextAsset);
                    case "jpg":
                    case "jpeg":
                    case "png":
                    case "tga":
                    case "bmp":
                        return typeof(Texture2D);
                    case "wav":
                    case "wave":
                    case "mp3":
                    case "ogg":
                    case "oga":
                    case "m4a":
                    case "flac":
                        return typeof(AudioClip);
                }
            }

            throw new NoImporterFoundException(context);
        }

        public object Load(string path, ResourceLoadContext context)
        {
            context.AttemptingSyncLoad = true;
            context.ResourceLoader = this;

            //determine type
            context.ResourceType = DetermineResourceType(path, context);

            //attempt to load with importers
            if(context.ResourceImporter != null)
            {
                try
                {
                    
                    return context.ResourceImporter.LoadResource(path, context.ResourceType, context);
                    
                }
                catch (Exception e)
                {
                    if(ConfigState.Instance.UseVerboseLogging)
                    {
                        Debug.LogError($"[ResourceLoader] Exception in asset importer {context.ResourceImporter.GetType().Name}");
                        Debug.LogException(e);
                    }
                    throw new ImporterFailedException(context, e);
                }
            }

            //attempt to load builtin
            try
            {
                return LoadFromFileBuiltin(path, context.ResourceType);
            }
            catch (Exception e)
            {
                if (ConfigState.Instance.UseVerboseLogging)
                {
                    Debug.LogError($"[ResourceLoader] Exception in builtin asset importer");
                    Debug.LogException(e);
                }
                throw new ImporterFailedException(context, e);
            }

        }

        public async Task<object> LoadAsync(string path, ResourceLoadContext context)
        {
            context.AttemptingSyncLoad = false;
            context.ResourceLoader = this;

            //determine type
            context.ResourceType = DetermineResourceType(path, context);

            //attempt to load with importers
            if (context.ResourceImporter != null)
            {
                try
                {

                    return await context.ResourceImporter.LoadResourceAsync(path, context.ResourceType, context);

                }
                catch (Exception e)
                {
                    if (ConfigState.Instance.UseVerboseLogging)
                    {
                        Debug.LogError($"[ResourceLoader] Exception in asset importer {context.ResourceImporter.GetType().Name}");
                        Debug.LogException(e);
                    }
                    throw new ImporterFailedException(context, e);
                }
            }

            //attempt to load builtin            
            try
            {
                return await LoadFromFileBuiltinAsync(path, context.ResourceType);
            }
            catch (Exception e)
            {
                if (ConfigState.Instance.UseVerboseLogging)
                {
                    Debug.LogError($"[ResourceLoader] Exception in builtin asset importer");
                    Debug.LogException(e);
                }
                throw new ImporterFailedException(context, e);
            }

        }

        //replacement for LoadFromFile, probably
        public object LoadTyped(string path, ResourceLoadContext context)
        {
            //context may include type and importer information, use that if possible

            context.AttemptingSyncLoad = true;
            context.ResourceLoader = this;

            if (context.ResourceImporter == null && context.ResourceType != null)
                Debug.LogWarning($"[ResourceLoader] LoadTyped was given a context with type but no importer"); //should this be a warning?

            if(context.ResourceImporter != null && !context.ResourceImporter.CanLoadSync)
            {
                Debug.LogWarning($"[ResourceLoader] LoadTyped was given an importer that can't load synchronously!");
                context.ResourceImporter = null;
            }

            if(context.ResourceImporter == null)
            {
                context.ResourceType = DetermineResourceType(path, context); //this will also get our importer
            }
            else if(context.ResourceType == null)
            {
                context.ResourceType = context.ResourceImporter.GetResourceType(path, context);
            }

            //attempt to load with importers
            if (context.ResourceImporter != null)
            {
                try
                {

                    return context.ResourceImporter.LoadResource(path, context.ResourceType, context);

                }
                catch (Exception e)
                {
                    if (ConfigState.Instance.UseVerboseLogging)
                    {
                        Debug.LogError($"[ResourceLoader] Exception in asset importer {context.ResourceImporter.GetType().Name}");
                        Debug.LogException(e);
                    }
                    throw new ImporterFailedException(context, e);
                }
            }

            //attempt to load builtin
            try
            {
                return LoadFromFileBuiltin(path, context.ResourceType);
            }
            catch (Exception e)
            {
                if (ConfigState.Instance.UseVerboseLogging)
                {
                    Debug.LogError($"[ResourceLoader] Exception in builtin asset importer");
                    Debug.LogException(e);
                }
                throw new ImporterFailedException(context, e);
            }            
        }

        //replacement for LoadFromFile, probably
        public async Task<object> LoadTypedAsync(string path, ResourceLoadContext context)
        {
            //context may include type and importer information, use that if possible

            context.AttemptingSyncLoad = false;
            context.ResourceLoader = this;

            if (context.ResourceImporter == null && context.ResourceType != null)
                Debug.LogWarning($"[ResourceLoader] LoadTyped was given a context with type but no importer"); //should this be a warning?

            if (context.ResourceImporter == null)
            {
                context.ResourceType = DetermineResourceType(path, context); //this will also get our importer
            }
            else if (context.ResourceType == null)
            {
                context.ResourceType = context.ResourceImporter.GetResourceType(path, context);
            }

            //attempt to load with importers
            if (context.ResourceImporter != null)
            {
                try
                {

                    return await context.ResourceImporter.LoadResourceAsync(path, context.ResourceType, context);

                }
                catch (Exception e)
                {
                    if (ConfigState.Instance.UseVerboseLogging)
                    {
                        Debug.LogError($"[ResourceLoader] Exception in asset importer {context.ResourceImporter.GetType().Name}");
                        Debug.LogException(e);
                    }
                    throw new ImporterFailedException(context, e);
                }
            }

            //attempt to load builtin
            try
            {
                return await LoadFromFileBuiltinAsync(path, context.ResourceType);
            }
            catch (Exception e)
            {
                if (ConfigState.Instance.UseVerboseLogging)
                {
                    Debug.LogError($"[ResourceLoader] Exception in builtin asset importer");
                    Debug.LogException(e);
                }
                throw new ImporterFailedException(context, e);
            }            
        }

        //basic builtin loaders

        private static async Task<object> LoadFromFileBuiltinAsync(string path, Type type)
        {
            byte[] bytes = null;
            await Task.Run(() =>
            {
                bytes = File.ReadAllBytes(path);
            });            

            var resource = LoadFromBytesBuiltin(bytes, type);

            if (resource is UnityEngine.Object obj)
                obj.name = Path.GetFileNameWithoutExtension(path);

            return resource;
        }

        private static object LoadFromFileBuiltin(string path, Type type)
        {
            byte[] bytes = null;
            bytes = File.ReadAllBytes(path);

            var resource = LoadFromBytesBuiltin(bytes, type);

            if (resource is UnityEngine.Object obj)
                obj.name = Path.GetFileNameWithoutExtension(path);

            return resource;
        }

        private static object LoadFromBytesBuiltin(byte[] data, Type type)
        {
            if(type == typeof(TextAsset))
            {
                return LoadTextAsset(data);
            }
            else if(type == typeof(Texture) || type == typeof(Texture2D))
            {
                return LoadTexture(data);
            }
            else if(type == typeof(AudioClip))
            {
                return LoadAudio(data);
            }

            return null;
        }

        private static TextAsset LoadTextAsset(byte[] data)
        {
            string str = Encoding.UTF8.GetString(data);
            return new TextAsset(str);
        }

        private static Texture2D LoadTexture(byte[] data)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            return tex;
        }

        private static AudioClip LoadAudio(byte[] data)
        {
            //TODO handling for things that aren't WAV
            return WaveFile.Load(data, false).ToAudioClip();
        }
    }
}
