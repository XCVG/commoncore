using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.Experimental
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
                throw new KeyNotFoundException($"Couldn't find a texture for path \"{rPath}\"");

            if(Renderer != null)
            {
                Material rMat = Renderer.material;
                rMat.mainTexture = tex;
                if (SetEmissive)
                    rMat.SetTexture("_EmissionMap", tex);
            }

            if(RawImage != null)
            {
                RawImage.texture = tex;
            }
        }
    }
}