using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Scripting;


namespace CommonCore
{
    /// <summary>
    /// Utility class for fading the screen in and out
    /// </summary>
    /// <remarks>
    /// Screen fades are implicitly cleared at some points of execution
    /// </remarks>
    public static class ScreenFader
    {
        private const string ScreenFaderPrefabPath = "UI/ScreenFader";
        private static ScreenFaderScript ScreenFaderScript = null;

        /// <summary>
        /// Fade from no fade or existing fade to target color
        /// </summary>
        public static void FadeTo(Color endColor, float duration, bool realTime = true, bool hideHud = true, bool persist = false)
        {
            ResetFaderObject();
            ScreenFaderScript.Crossfade(null, endColor, duration, realTime, hideHud, persist);
        }

        /// <summary>
        /// Fade from specified color or existing color to no fade
        /// </summary>
        public static void FadeFrom(Color? startColor, float duration, bool realTime = true, bool hideHud = true, bool persist = false)
        {
            ResetFaderObject();
            ScreenFaderScript.Crossfade(startColor, new Color(0, 0, 0, 0), duration, realTime, hideHud, persist);
        }

        /// <summary>
        /// Fade from one color to another
        /// </summary>
        public static void Crossfade(Color startColor, Color endColor, float duration, bool realTime = true, bool hideHud = true, bool persist = false)
        {
            ResetFaderObject();
            ScreenFaderScript.Crossfade(startColor, endColor, duration, realTime, hideHud, persist);
        }

        /// <summary>
        /// Clear the current fade
        /// </summary>
        public static void ClearFade()
        {
            if (ScreenFaderScript != null)
                UnityEngine.Object.Destroy(ScreenFaderScript.gameObject);
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnSceneUnload)]
        private static void HandleSceneChanged()
        {
            //clear non-persistent fade
            if (ScreenFaderScript != null && !ScreenFaderScript.Persist)
                UnityEngine.Object.Destroy(ScreenFaderScript.gameObject);
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnGameEnd)]
        private static void HandleGameEnd()
        {
            //clear all fade
            if (ScreenFaderScript != null)
                UnityEngine.Object.Destroy(ScreenFaderScript.gameObject);
        }

        private static void ResetFaderObject()
        {
            if (ScreenFaderScript != null)
            {
                ScreenFaderScript.AbortFade();
            }
            else
            {
                GameObject screenFader = UnityEngine.Object.Instantiate(CoreUtils.LoadResource<GameObject>(ScreenFaderPrefabPath));
                ScreenFaderScript = screenFader.GetComponent<ScreenFaderScript>();
            }
        }
    }
}