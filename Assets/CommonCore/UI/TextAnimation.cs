using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    /// <summary>
    /// Text Animation class to support type-on
    /// We may make this abstract and build a hierarchy to support other animations in the future but the external interface (mostly) won't change 
    /// </summary>
    public class TextAnimation : MonoBehaviour
    {

        private enum TextAnimationType
        {
            TypeOn
        }

        private Text AttachedText;
        private TextAnimationType Type;
        private char[] Contents;
        private int ContentsIndex;
        private float Interval;
        private Coroutine CurrentCoroutine;

        private void Begin(TextAnimationType type, string contents, float time)
        {
            Type = type;
            Contents = contents.ToCharArray();
            ContentsIndex = 0;
            Interval = time / Contents.Length;
            AttachedText = GetComponent<Text>();
            AttachedText.text = string.Empty;
            CurrentCoroutine = StartCoroutine(TypeOnCoroutine());
        }

        private IEnumerator TypeOnCoroutine()
        {
            for(; ContentsIndex < Contents.Length; ContentsIndex++)
            {
                AttachedText.text = AttachedText.text + Contents[ContentsIndex];
                yield return new WaitForSecondsRealtime(Interval);
            }
        }

        /// <summary>
        /// Force the current text animation to complete
        /// </summary>
        public void Complete()
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);

            AttachedText.text = new string(Contents);

            Destroy(this);
        }

        /// <summary>
        /// Force the current text animation to abort (clears text)
        /// </summary>
        public void Abort()
        {
            if (CurrentCoroutine != null)
                StopCoroutine(CurrentCoroutine);

            AttachedText.text = string.Empty;

            Destroy(this);
        }

        /// <summary>
        /// Execute a "type on" animation on a text box
        /// </summary>
        /// <param name="text">The text field to apply the effect to</param>
        /// <param name="contents">The text to put in the field</param>
        /// <param name="time">How long to take to fill the text field</param>
        /// <returns>A TextAnimation object that can be completed, aborted, etc</returns>
        public static TextAnimation TypeOn(Text text, string contents, float time)
        {
            var animation = text.gameObject.AddComponent<TextAnimation>();

            animation.Begin(TextAnimationType.TypeOn, contents, time);

            return animation;
        }
    }
}