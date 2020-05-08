using CommonCore.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{

    /// <summary>
    /// Music fader script for use with MusicFader utility class. I wouldn't use this on its own but it's up to you.
    /// </summary>
    public class MusicFaderScript : MonoBehaviour
    {

        private Dictionary<MusicSlot, FadeInfo> ActiveFades = new Dictionary<MusicSlot, FadeInfo>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void AbortFade(MusicSlot slot)
        {
            if(ActiveFades.TryGetValue(slot, out var fade))
            {
                if (fade.Coroutine != null)
                    StopCoroutine(fade.Coroutine);

                ActiveFades.Remove(slot);
            }
        }

        public void AbortNonPersistentFades()
        {
            HashSet<MusicSlot> removedSlots = new HashSet<MusicSlot>();

            foreach(var kvp in ActiveFades)
            {
                var fade = kvp.Value;

                if(!fade.Persist)
                {
                    if (fade.Coroutine != null)
                        StopCoroutine(fade.Coroutine);

                    removedSlots.Add(kvp.Key);
                }
            }

            foreach(var slot in removedSlots)
            {
                ActiveFades.Remove(slot);
            }
        }

        public void AbortAllFades()
        {
            foreach(var fade in ActiveFades.Values)
            {
                if (fade.Coroutine != null)
                    StopCoroutine(fade.Coroutine);
            }

            ActiveFades.Clear();
        }

        public void StartFade(MusicSlot slot, float volume, float duration, bool realTime, bool persist)
        {
            AbortFade(slot);


            Coroutine fadeCoroutine = StartCoroutine(CoFade(slot, volume, duration, realTime));
            ActiveFades[slot] = new FadeInfo() { Coroutine = fadeCoroutine, Persist = persist };
        }

        private IEnumerator CoFade(MusicSlot slot, float endVolume, float duration, bool realTime)
        {
            float? sv = AudioPlayer.Instance.GetMusicVolume(slot);
            if (!sv.HasValue)
                yield break;
            float startVolume = sv.Value;

            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += realTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float ratio = elapsed / duration;
                float volume = Mathf.Lerp(startVolume, endVolume, ratio);
                AudioPlayer.Instance.SetMusicVolume(volume, slot);
                yield return null;
            }

            yield return null;

            AudioPlayer.Instance.SetMusicVolume(endVolume, slot);

            ActiveFades[slot].Coroutine = null;
        }

        private class FadeInfo
        {
            public Coroutine Coroutine;
            public bool Persist;
        }
    }
}