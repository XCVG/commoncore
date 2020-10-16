using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CommonCore.ResourceManagement
{

    /// <summary>
    /// Importer for sprite jassets
    /// </summary>
    public class SpriteAssetImporter : IResourceImporter
    {
        public bool CanLoadSync => true;

        public bool CanLoadResource(string path, ResourceLoadContext context)
        {
            if(Path.GetExtension(path).Equals(".jasset", StringComparison.OrdinalIgnoreCase))
            {
                JObject jo = JObject.Parse(File.ReadAllText(path));
                if (!jo.IsNullOrEmpty())
                {
                    string typeName = jo["$assetType"].ToString();
                    if (typeName == "Sprite")
                        return true;
                }
            }

            return false;
        }

        public Type GetResourceType(string path, ResourceLoadContext context)
        {
            return typeof(Sprite);
        }

        public object LoadResource(string path, Type target, ResourceLoadContext context)
        {
            SpriteData sd = ReadSpriteData(path);
            return LoadSprite(path, context, sd);
        }

        public static object LoadSprite(string path, ResourceLoadContext context, SpriteData sd)
        {
            string fullTexturePath = Path.Combine(Path.GetDirectoryName(path), sd.TexturePath);

            Texture2D texture;
            if (sd.TextureIsResource)
                texture = context.ResourceManager.GetResource<Texture2D>(path, false);
            else
                texture = (Texture2D)context.ResourceLoader.Load(fullTexturePath, new ResourceLoadContext() { AttemptingSyncLoad = true, ResourceLoader = context.ResourceLoader, ResourceManager = context.ResourceManager, ResourceType = typeof(Texture2D) });

            if (!sd.Rect.HasValue)
                sd.Rect = new Rect(0, 0, texture.width, texture.height);

            Sprite sprite = Sprite.Create(texture, sd.Rect.Value, sd.Pivot, sd.PixelsPerUnit, 0, SpriteMeshType.FullRect);

            return sprite;
        }

        public async Task<object> LoadResourceAsync(string path, Type target, ResourceLoadContext context)
        {
            SpriteData sd = ReadSpriteData(path);
            return await LoadSpriteAsync(path, context, sd);
        }

        public static async Task<object> LoadSpriteAsync(string path, ResourceLoadContext context, SpriteData sd)
        {
            string fullTexturePath = Path.Combine(Path.GetDirectoryName(path), sd.TexturePath);

            Texture2D texture;
            if (sd.TextureIsResource)
                texture = context.ResourceManager.GetResource<Texture2D>(path, false);
            else
                texture = (Texture2D)await context.ResourceLoader.LoadAsync(fullTexturePath, new ResourceLoadContext() { AttemptingSyncLoad = false, ResourceLoader = context.ResourceLoader, ResourceManager = context.ResourceManager, ResourceType = typeof(Texture2D) });

            if (!sd.Rect.HasValue)
                sd.Rect = new Rect(0, 0, texture.width, texture.height);

            await Task.Yield();

            Sprite sprite = Sprite.Create(texture, sd.Rect.Value, sd.Pivot, sd.PixelsPerUnit, 0, SpriteMeshType.FullRect);

            return sprite;
        }

        private SpriteData ReadSpriteData(string path)
        {
            JObject jo = JObject.Parse(File.ReadAllText(path));
            return SpriteData.FromJObject(jo);
        }

    }

    public class SpriteData
    {
        public string TexturePath;
        public bool TextureIsResource;
        public Rect? Rect;
        public Vector2 Pivot;
        public float PixelsPerUnit = 100f;

        public static SpriteData FromJObject(JObject jo)
        {
            SpriteData sd = new SpriteData();

            sd.TexturePath = jo["texturePath"].ToString();
            if (!jo["textureIsResource"].IsNullOrEmpty() && jo["textureIsResource"].ToObject<bool>())
                sd.TextureIsResource = true;
            if (!jo["rect"].IsNullOrEmpty())
                sd.Rect = jo["rect"].ToObject<Rect>(); //will this work?
            if (!jo["pivot"].IsNullOrEmpty())
                sd.Pivot = jo["pivot"].ToObject<Vector2>();
            if (!jo["pixelsPerUnit"].IsNullOrEmpty())
                sd.PixelsPerUnit = jo["pixelsPerUnit"].ToObject<float>();

            return sd;
        }
    }
}