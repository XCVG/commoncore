using System;
using UnityEngine;
using CommonCore.ResourceManagement;
using CommonCore.Scripting;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace CommonCore.World
{

    /// <summary>
    /// Imports FacingSpriteAssets from jasset files
    /// </summary>
    public class FacingSpriteAssetImporter : IResourceImporter
    {

        //injects importer
        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.AfterModulesLoaded)]
        private static void RegisterImporter()
        {
            CCBase.ResourceManager.ResourceLoader.RegisterImporter(new FacingSpriteAssetImporter());
        }

        public bool CanLoadSync => true;        

        public bool CanLoadResource(string path, ResourceLoadContext context)
        {
            if (Path.GetExtension(path).Equals(".jasset", StringComparison.OrdinalIgnoreCase))
            {
                JObject jo = JObject.Parse(File.ReadAllText(path));
                if (!jo.IsNullOrEmpty())
                {
                    string typeName = jo["$assetType"].ToString();
                    if (typeName == "FacingSpriteAsset")
                        return true;
                }
            }

            return false;
        }

        public Type GetResourceType(string path, ResourceLoadContext context) => typeof(FacingSpriteAsset);

        public object LoadResource(string path, Type target, ResourceLoadContext context)
        {
            JObject jo = JObject.Parse(File.ReadAllText(path));

            FacingSpriteAsset asset = ScriptableObject.CreateInstance<FacingSpriteAsset>();

            if (jo.ContainsKey("Front"))
                asset.Front = LoadSprite(path, (JObject)jo["Front"], context);
            if (jo.ContainsKey("FrontLeft"))
                asset.FrontLeft = LoadSprite(path, (JObject)jo["FrontLeft"], context);
            if (jo.ContainsKey("Left"))
                asset.Left = LoadSprite(path, (JObject)jo["Left"], context);
            if (jo.ContainsKey("BackLeft"))
                asset.BackLeft = LoadSprite(path, (JObject)jo["BackLeft"], context);
            if (jo.ContainsKey("Back"))
                asset.Back = LoadSprite(path, (JObject)jo["Back"], context);
            if (jo.ContainsKey("BackRight"))
                asset.BackRight = LoadSprite(path, (JObject)jo["BackRight"], context);
            if (jo.ContainsKey("Right"))
                asset.Right = LoadSprite(path, (JObject)jo["Right"], context);
            if (jo.ContainsKey("FrontRight"))
                asset.FrontRight = LoadSprite(path, (JObject)jo["FrontRight"], context);

            asset.name = Path.GetFileNameWithoutExtension(path);

            return asset;
        }

        public async Task<object> LoadResourceAsync(string path, Type target, ResourceLoadContext context)
        {
            JObject jo = JObject.Parse(File.ReadAllText(path));

            FacingSpriteAsset asset = ScriptableObject.CreateInstance<FacingSpriteAsset>();

            if (jo.ContainsKey("Front"))
                asset.Front = await LoadSpriteAsync(path, (JObject)jo["Front"], context);
            if (jo.ContainsKey("FrontLeft"))
                asset.FrontLeft = await LoadSpriteAsync(path, (JObject)jo["FrontLeft"], context);
            if (jo.ContainsKey("Left"))
                asset.Left = await LoadSpriteAsync(path, (JObject)jo["Left"], context);
            if (jo.ContainsKey("BackLeft"))
                asset.BackLeft = await LoadSpriteAsync(path, (JObject)jo["BackLeft"], context);
            if (jo.ContainsKey("Back"))
                asset.Back = await LoadSpriteAsync(path, (JObject)jo["Back"], context);
            if (jo.ContainsKey("BackRight"))
                asset.BackRight = await LoadSpriteAsync(path, (JObject)jo["BackRight"], context);
            if (jo.ContainsKey("Right"))
                asset.Right = await LoadSpriteAsync(path, (JObject)jo["Right"], context);
            if (jo.ContainsKey("FrontRight"))
                asset.FrontRight = await LoadSpriteAsync(path, (JObject)jo["FrontRight"], context);

            asset.name = Path.GetFileNameWithoutExtension(path);

            return asset;
        }

        private Sprite LoadSprite(string basePath, JObject spriteNode, ResourceLoadContext context)
        {
            if(spriteNode.ContainsKey("texturePath"))
            {
                //sprite node is an importable sprite object
                var sd = SpriteData.FromJObject(spriteNode);
                if (sd.TextureIsResource)
                    Debug.LogWarning("TextureIsResource will break in FacingSpriteAssetImporter, use resource:path instead");
                return (Sprite)SpriteAssetImporter.LoadSprite(basePath, context, sd);
            }
            else if(spriteNode.ContainsKey("spritePath"))
            {
                //sprite node references an asset on disk
                string path;
                if (spriteNode.ContainsKey("pathIsAbsolute") && spriteNode["pathIsAbsolute"].ToObject<bool>()) //this... isn't useful
                    path = spriteNode["spritePath"].ToString();
                else
                    path = Path.Combine(Path.GetDirectoryName(basePath), spriteNode["spritePath"].ToString());

                return (Sprite)context.ResourceLoader.Load(path, new ResourceLoadContext() { AttemptingSyncLoad = true, ResourceLoader = context.ResourceLoader, ResourceManager = context.ResourceManager });
            }
            else if(spriteNode.ContainsKey("resource"))
            {
                //sprite node references a resource
                string path;
                if (spriteNode.ContainsKey("pathIsAbsolute") && spriteNode["pathIsAbsolute"].ToObject<bool>())
                    path = spriteNode["resource"].ToString();
                else
                    path = Path.Combine(Path.GetDirectoryName(basePath), spriteNode["resource"].ToString());

                return context.ResourceManager.GetResource<Sprite>(path, false);
            }

            throw new NotSupportedException();
        }

        private async Task<Sprite> LoadSpriteAsync(string basePath, JObject spriteNode, ResourceLoadContext context)
        {
            if (spriteNode.ContainsKey("texturePath"))
            {
                //sprite node is an importable sprite object
                var sd = SpriteData.FromJObject(spriteNode);
                if (sd.TextureIsResource)
                    Debug.LogWarning("TextureIsResource will break in FacingSpriteAssetImporter, use resource:path instead");
                return (Sprite)await SpriteAssetImporter.LoadSpriteAsync(basePath, context, sd);
            }
            else if (spriteNode.ContainsKey("spritePath"))
            {
                //sprite node references an asset on disk
                string path;
                if (spriteNode.ContainsKey("pathIsAbsolute") && spriteNode["pathIsAbsolute"].ToObject<bool>())
                    path = spriteNode["spritePath"].ToString();
                else
                    path = Path.Combine(Path.GetDirectoryName(basePath), spriteNode["spritePath"].ToString());

                return (Sprite)await context.ResourceLoader.LoadAsync(path, new ResourceLoadContext() { AttemptingSyncLoad = false, ResourceLoader = context.ResourceLoader, ResourceManager = context.ResourceManager });
            }
            else if (spriteNode.ContainsKey("resource"))
            {
                //sprite node references a resource
                string path;
                if (spriteNode.ContainsKey("pathIsAbsolute") && spriteNode["pathIsAbsolute"].ToObject<bool>())
                    path = spriteNode["resource"].ToString();
                else
                    path = Path.Combine(Path.GetDirectoryName(basePath), spriteNode["resource"].ToString());

                return context.ResourceManager.GetResource<Sprite>(path, false);
            }

            throw new NotSupportedException();
        }
    }
}