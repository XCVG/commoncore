using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.DebugLog;

namespace CommonCore.Audio
{

    public enum SoundType
    {
        Any, Sound, Voice, Music, Root
    }

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

            Debug.Log("Audio module loaded!");
        }

        public AudioClip GetSound(string name, SoundType sType)
        {
            AudioClip clip = null;

            if (sType == SoundType.Any)
            {
                //attempt to load in order
                clip = CCBaseUtil.LoadResource<AudioClip>("DynamicSound/" + name);
                if(clip == null)
                    clip = CCBaseUtil.LoadResource<AudioClip>("Voice/" + name);
                if(clip == null)
                    clip = CCBaseUtil.LoadResource<AudioClip>("DynamicMusic/" + name);
                if (clip == null)
                    clip = CCBaseUtil.LoadResource<AudioClip>(name);
            }
            else
            {
                switch (sType)
                {
                    case SoundType.Sound:
                        clip = CCBaseUtil.LoadResource<AudioClip>("DynamicSound/" + name);
                        break;
                    case SoundType.Voice:
                        clip = CCBaseUtil.LoadResource<AudioClip>("Voice/" + name);
                        break;
                    case SoundType.Music:
                        clip = CCBaseUtil.LoadResource<AudioClip>("DynamicMusic/" + name);
                        break;
                    case SoundType.Root:
                        clip = CCBaseUtil.LoadResource<AudioClip>(name);
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