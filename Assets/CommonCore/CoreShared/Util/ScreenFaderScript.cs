using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore
{

    /// <summary>
    /// Screen fader script for use with ScreenFader utility class. I wouldn't use this on its own but it's up to you.
    /// </summary>
    public class ScreenFaderScript : MonoBehaviour
    {
        [SerializeField]
        private Canvas FadeCanvas = null;
        [SerializeField]
        private Image FadeImage = null;

        public bool Persist { get; private set; }

        private Coroutine CrossfadeCoroutine = null;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Aborts the current fade operation
        /// </summary>
        public void AbortFade()
        {
            //aborts the current fade if exists
            if (CrossfadeCoroutine != null)
                StopCoroutine(CrossfadeCoroutine);
        }

        /// <summary>
        /// Sets the screen fade to the specified color
        /// </summary>
        public void SetColor(Color color, bool abortCurrentFade, bool persist)
        {
            if (abortCurrentFade)
                AbortFade();

            Persist = persist;

            FadeImage.color = color;
        }

        /// <summary>
        /// Starts a fade operation, aborting the existing one if it is running
        /// </summary>
        public void Crossfade(Color? startColor, Color endColor, float duration, bool realTime, bool hideHud, bool persist)
        {
            //abort current fade if exists
            AbortFade();

            //set layer based on hideHud
            FadeCanvas.sortingOrder = hideHud ? 999 : 1;

            //set persist to persist
            Persist = persist;

            //set start color if desired
            if(startColor.HasValue)
            {
                FadeImage.color = startColor.Value;
            }

            //execute fade
            CrossfadeCoroutine = StartCoroutine(CoCrossfade(endColor, duration, realTime));
        }

        private IEnumerator CoCrossfade(Color endColor, float duration, bool realTime)
        {
            Color startColor = FadeImage.color;
            float elapsed = 0;
            while(elapsed < duration)
            {
                elapsed += realTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float ratio = elapsed / duration;
                Color newColor = Color.Lerp(startColor, endColor, ratio);
                FadeImage.color = newColor;
                yield return null;
            }

            yield return null;

            FadeImage.color = endColor;

            CrossfadeCoroutine = null;
        }


    }
}