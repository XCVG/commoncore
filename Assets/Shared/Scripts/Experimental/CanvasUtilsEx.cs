using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Config;

namespace CommonCore.Experimental
{

    public static class CanvasUtilsEx
    {
        public static IEnumerator FadeInCanvas(CanvasGroup cg, float time, bool realtime = false)
        {
            for (float elapsed = 0; elapsed < time; elapsed += realtime ? Time.unscaledDeltaTime : Time.deltaTime)
            {
                cg.alpha = Mathf.Clamp(elapsed / time, 0, 1);
                yield return null;
            }

            cg.alpha = 1;
        }

        public static IEnumerator FadeOutCanvas(CanvasGroup cg, float time, bool realtime = false)
        {
            for (float elapsed = 0; elapsed < time; elapsed += realtime ? Time.unscaledDeltaTime : Time.deltaTime)
            {
                cg.alpha = Mathf.Clamp(1 - (elapsed / time), 0, 1);
                yield return null;
            }

            cg.alpha = 0;
        }

        public static void SetImageFromPath(RawImage image, string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                {
                    image.texture = null;
                    return;
                }

                if (!imagePath.StartsWith("/"))
                    imagePath = "DynamicTexture/" + imagePath;
                else
                    imagePath = imagePath.TrimStart('/');

                var texture = CoreUtils.LoadResource<Texture2D>(imagePath);

                if (texture == null)
                    Debug.LogWarning($"Couldn't find intro image {imagePath}");

                image.texture = texture;
            }
            catch(Exception e)
            {
                Debug.LogError($"[{nameof(CanvasUtilsEx)}.{nameof(SetImageFromPath)}] Failed to set image ({e.GetType().Name})");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);

            }
        }

    }
}