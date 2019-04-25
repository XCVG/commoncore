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
        Any, Sound, Voice, Music, Root
    }

    /// <summary>
    /// Audio module handles music player and provides some functionality for getting sounds
    /// </summary>
    public class AudioModule : CCModule
    {
        //private static AudioModule Instance;

        public AudioModule()
        {
            //TODO: load on start if policy permits

            //create AudioPlayer object
            GameObject audioObject = new GameObject("AudioPlayer");
            AudioPlayer audioPlayer = audioObject.AddComponent<AudioPlayer>();
            audioPlayer.SetModule(this);

        }

        public AudioClip GetSound(string name, SoundType sType)
        {
            AudioClip clip = null;

            if (sType == SoundType.Any)
            {
                //attempt to load in order
                clip = CoreUtils.LoadResource<AudioClip>("DynamicSound/" + name);
                if(clip == null)
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
                }

            }

            if (clip == null)
            {
                CDebug.LogEx(string.Format("Couldn't find sound {0} in category {1}", name, sType.ToString()), LogLevel.Verbose, this);
            }

            return clip;
        }

    }
}