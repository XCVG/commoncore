using CommonCore.Audio;
using CommonCore.Scripting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /// <summary>
    /// Utility class for fading music in and out
    /// </summary>
    /// <remarks>
    /// Music fades are implicitly cleared at some points of execution
    /// </remarks>
    public static class MusicFader
    {
        private static MusicFaderScript MusicFaderScript = null;

        public static void FadeTo(MusicSlot slot, float volume, float duration, bool realTime = false, bool persist = false)
        {
            InitFaderObjectIfNotExists();
            MusicFaderScript.StartFade(slot, volume, duration, realTime, persist);
        }

        public static void FadeIn(MusicSlot slot, float volume, float duration, bool realTime = false, bool persist = false)
        {
            AudioPlayer.Instance.SetMusicVolume(0, slot);
            FadeTo(slot, volume, duration, realTime, persist);
        }

        public static void FadeOut(MusicSlot slot, float duration, bool realTime = false, bool persist = false)
        {
            FadeTo(slot, 0, duration, realTime, persist);
        }        

        public static void ClearFade(MusicSlot slot)
        {
            if(MusicFaderScript != null)
                MusicFaderScript.AbortFade(slot);
        }

        public static void ClearAllFades()
        {
            if (MusicFaderScript != null)
                MusicFaderScript.AbortAllFades();
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnSceneUnload)]
        private static void HandleSceneChanged()
        {
            if (MusicFaderScript != null)
                MusicFaderScript.AbortNonPersistentFades();
        }

        [CCScript, CCScriptHook(AllowExplicitCalls = false, Hook = ScriptHook.OnGameEnd)]
        private static void HandleGameEnd()
        {
            if (MusicFaderScript != null)
                MusicFaderScript.AbortAllFades();
        }

        private static void InitFaderObjectIfNotExists()
        {
            if(MusicFaderScript == null)
            {
                GameObject go = new GameObject("MusicFader");
                MusicFaderScript = go.AddComponent<MusicFaderScript>();
            }
        }
    }
}