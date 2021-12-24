using CommonCore.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using System;
using CommonCore.World;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Experimental save screenshot provider
    /// </summary>
    public static class GetSaveThumbnailEx
    {
        [CCScript(ClassName = "Save", Name = "GetSaveThumbnail")]
        private static byte[] GetSaveThumbnail(ScriptExecutionContext context)
        {
            Camera bCamera = WorldUtils.GetActiveCamera();

            var go = new GameObject("TempSaveCamera", typeof(Camera));
            var camera = go.GetComponent<Camera>();

            RenderTexture rt = new RenderTexture(128, 64, 24);
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);

            camera.CopyFrom(bCamera);
            camera.targetTexture = rt;
            camera.aspect = rt.width / rt.height;
            camera.Render();

            var oldTarget = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            camera.targetTexture = null;
            RenderTexture.active = oldTarget;

            byte[] data = tex.EncodeToPNG();

            UnityEngine.Object.Destroy(rt);
            UnityEngine.Object.Destroy(tex);
            UnityEngine.Object.Destroy(go);

            return data;
        }
    }
}


