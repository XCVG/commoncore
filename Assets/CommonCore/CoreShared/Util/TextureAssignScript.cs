using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.Util
{

    /// <summary>
    /// Assigns a texture from a resource
    /// </summary>
    public class TextureAssignScript : MonoBehaviour
    {
        [SerializeField]
        private Renderer Renderer;
        [SerializeField]
        private RawImage RawImage;
        [SerializeField]
        private string ResourcePath;
        [SerializeField]
        private bool AutoPrefix = true;
        [SerializeField]
        private bool SetEmissive = false;

        private void Start()
        {
            string rPath = ResourcePath;
            if (AutoPrefix && !rPath.StartsWith("DynamicTexture"))
                rPath = "DynamicTexture/" + rPath.TrimStart('/');

            Texture tex = CoreUtils.LoadResource<Texture2D>(rPath);

            if (tex == null)
            {
                Debug.LogError($"TextureAssignScript on ${gameObject.name} couldn't find a texture for path \"{rPath}\"");
                return;
            }                

            if (Renderer == null)
                Renderer = GetComponent<Renderer>();
            
            if(Renderer == null && RawImage == null)
            {
                Debug.LogError($"TextureAssignScript on ${gameObject.name} couldn't find a renderer");
                return;
            }

            if(Renderer != null)
            {
                Material rMat = Renderer.material;
                rMat.mainTexture = tex;
                if (SetEmissive)
                    rMat.SetTexture("_EmissionMap", tex);
            }

            if (RawImage == null)
                RawImage = GetComponent<RawImage>();

            if (RawImage != null)
            {
                RawImage.texture = tex;
            }
        }
    }
}