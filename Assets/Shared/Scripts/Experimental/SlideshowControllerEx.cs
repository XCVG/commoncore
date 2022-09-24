using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.Experimental
{

    public class SlideshowControllerEx : MonoBehaviour
    {
        private Canvas RootCanvas = null;
        private AspectRatioFitter AspectRatioFitter = null;
        private Image BackgroundImage = null;
        private Image SlideshowImage = null;

        private static SlideshowControllerEx _Instance;

        public static SlideshowControllerEx GetInstance()
        {
            if (_Instance == null)
            {
                var go = new GameObject("SlideshowExperiment");
                go.transform.SetParent(CoreUtils.GetUIRoot());
                _Instance = go.AddComponent<SlideshowControllerEx>();
                _Instance.Initialize();
            }

            return _Instance;
        }

        public static void ClearInstance()
        {
            if (_Instance != null)
            {
                Destroy(_Instance.gameObject);
                _Instance = null;
            }
        }

        public bool HideHud
        {
            get
            {
                return RootCanvas.sortingOrder < 100;
            }
            set
            {
                RootCanvas.sortingOrder = value ? 110 : 1;
            }
        }

        public bool UseBackground
        {
            get
            {
                return BackgroundImage.gameObject.activeSelf;
            }
            set
            {
                BackgroundImage.gameObject.SetActive(value);
            }
        }

        public void ShowImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                Debug.LogWarning("Please use the ClearImage method instead of passing null/empty to ShowImage");
                ClearImage();
                return;
            }

            var sprite = TryLoadSprite(imageName);
            if (sprite == null)
            {
                Debug.LogError($"Failed to load sprite \"Sequences/{imageName}\"");
                SlideshowImage.sprite = null;
                SlideshowImage.color = Color.white;
                return;
            }

            AspectRatioFitter.aspectRatio = sprite.bounds.size.x / sprite.bounds.size.y;
            SlideshowImage.sprite = sprite;
            SlideshowImage.color = Color.white;
        }

        public void ClearImage()
        {
            SlideshowImage.sprite = null;
            SlideshowImage.color = new Color(0, 0, 0, 0);
        }

        private Sprite TryLoadSprite(string imageName)
        {
            var sprite = CoreUtils.LoadResource<Sprite>("Sequences/" + imageName);
            if(sprite == null)
                sprite = CoreUtils.LoadResource<Sprite>("Dialogue/char/" + imageName);
            if (sprite == null)
                sprite = CoreUtils.LoadResource<Sprite>("Dialogue/bg/" + imageName);
            return sprite;
        }

        private void Initialize()
        {
            RootCanvas = gameObject.AddComponent<Canvas>();
            //these need to be called in this specific order and I do not know why
            RootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            RootCanvas.gameObject.layer = 5;
            RootCanvas.sortingOrder = 1;

            var cs = gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920, 1080);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var bgObj = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(AspectRatioFitter));
            bgObj.transform.SetParent(RootCanvas.transform);

            var bgRT = (RectTransform)bgObj.transform;
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.pivot = new Vector2(0.5f, 0.5f);

            var bgArf = bgObj.GetComponent<AspectRatioFitter>(); //hacky way of doing things
            bgArf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            bgArf.aspectRatio = 16f / 9f;

            BackgroundImage = bgObj.GetComponent<Image>();
            BackgroundImage.sprite = null;
            BackgroundImage.color = Color.black;

            bgObj.SetActive(false);

            var imageObj = new GameObject("Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(AspectRatioFitter));
            imageObj.transform.SetParent(RootCanvas.transform);

            AspectRatioFitter = imageObj.GetComponent<AspectRatioFitter>();
            AspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            AspectRatioFitter.aspectRatio = 16f / 9f;

            var imageRT = (RectTransform)imageObj.transform;
            imageRT.anchorMin = Vector2.zero;
            imageRT.anchorMax = Vector2.one;
            imageRT.pivot = new Vector2(0.5f, 0.5f);

            SlideshowImage = imageObj.GetComponent<Image>();
            SlideshowImage.color = new Color(0, 0, 0, 0);

            //Debug.Log(RootCanvas.transform.position);
        }
    }
}