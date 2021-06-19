using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

namespace CommonCore.Audio
{
    /// <summary>
    /// Sound types/namespaces
    /// </summary>
    public enum SoundType
    {
        Any, Sound, Voice, Music, Root, UI
    }

    /// <summary>
    /// Audio module handles music player and provides some functionality for getting sounds
    /// </summary>
    public class AudioModule : CCModule
    {
        public AudioPlayer AudioPlayer { get; private set; }

        public AudioModule()
        {
            //TODO: load on start if policy permits

            //create AudioPlayer object
            GameObject audioObject = new GameObject("AudioPlayer");
            AudioPlayer = audioObject.AddComponent<AudioPlayer>();
            AudioPlayer.SetModule(this);

        }

        public override void OnGameEnd()
        {
            AudioPlayer.HandleGameEnd();
        }

        public AudioClip GetSound(string name, SoundType sType)
        {
            return GetSound(name, sType, false);
        }

        public AudioClip GetSound(string name, SoundType sType, bool suppressWarnings)
        {
            AudioClip clip = null;

            if (sType == SoundType.Any)
            {
                //attempt to load in order
                clip = CoreUtils.LoadResource<AudioClip>("DynamicSound/" + name);
                if (clip == null)
                    clip = CoreUtils.LoadResource<AudioClip>("UI/Sound/" + name);
                if (clip == null)
                    clip = CoreUtils.LoadResource<AudioClip>("Voice/" + name);
                if(clip == null)
                    clip = CoreUtils.LoadResource<AudioClip>("DynamicMusic/" + name);
                if (clip == null)
                    clip = CoreUtils.LoadResource<AudioClip>(name);
            }
            else
            {
                switch (sType)
                {
                    case SoundType.Sound:
                        clip = CoreUtils.LoadResource<AudioClip>("DynamicSound/" + name);
                        break;
                    case SoundType.Voice:
                        clip = CoreUtils.LoadResource<AudioClip>("Voice/" + name);
                        break;
                    case SoundType.Music:
                        clip = CoreUtils.LoadResource<AudioClip>("DynamicMusic/" + name);
                        break;
                    case SoundType.Root:
                        clip = CoreUtils.LoadResource<AudioClip>(name);
                        break;
                    case SoundType.UI:
                        clip = CoreUtils.LoadResource<AudioClip>("UI/Sound/" + name);
                        break;
                }

            }

            if (clip == null && !suppressWarnings)
            {
                CDebug.LogEx(string.Format("Couldn't find sound {0} in category {1}", name, sType.ToString()), LogLevel.Verbose, this);
            }

            return clip;
        }

    }
}