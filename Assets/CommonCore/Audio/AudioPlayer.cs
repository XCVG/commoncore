using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.Messaging;


namespace CommonCore.Audio
{
    public struct SoundInfo
    {
        public AudioClip Clip;
        public AudioSource Source;
        public bool Retain;
    }

    //generalized audio player to handle music and oneshot sound effects
    //will eventually handle overridable channels and use an object pool, but not now
    public class AudioPlayer : MonoBehaviour
    {
        private const float GCInterval = 2.5f;

        public static AudioPlayer Instance;

        private AudioModule Module;
        private QdmsMessageInterface MessageInterface;
        private List<SoundInfo> PlayingSounds;

        private AudioSource MusicPlayer;
        private bool MusicRetain;

        private float TimeElapsed;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        internal void SetModule(AudioModule module)
        {
            if (Module == null)
                Module = module;
        }

        private void Start()
        {
            Instance = this;
            MessageInterface = new QdmsMessageInterface(gameObject);

            //initialize music player
            GameObject mpObject = new GameObject("MusicPlayer");
            mpObject.transform.parent = transform;
            AudioSource mpSource = mpObject.AddComponent<AudioSource>();
            mpSource.spatialBlend = 0;
            mpSource.ignoreListenerPause = true;
            MusicPlayer = mpSource;
            //mpSource.volume = Config.ConfigState.Instance.MusicVolume;

            //initialize sound list
            PlayingSounds = new List<SoundInfo>();
        }

        private void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void Update()
        {
            //TODO message bus integration: pop audio messages


            //run cleanup periodically
            TimeElapsed += Time.deltaTime;
            if(TimeElapsed >= GCInterval)
            {
                for(int i = PlayingSounds.Count-1; i >= 0; i--)
                {
                    var s = PlayingSounds[i];
                    if(s.Source == null || !s.Source.isPlaying)
                    {
                        Destroy(s.Source.gameObject);
                        PlayingSounds.RemoveAt(i);
                    }
                }

                TimeElapsed = 0;
            }
        }

        void OnSceneUnloaded(Scene current)
        {
            for (int i = PlayingSounds.Count - 1; i >= 0; i--)
            {
                var s = PlayingSounds[i];
                if (!s.Retain || s.Source == null || !s.Source.isPlaying)
                {
                    Destroy(s.Source.gameObject);
                    PlayingSounds.RemoveAt(i);
                }
            }

            if(!MusicRetain)
            {
                MusicPlayer.Stop();
            }
        }
               

        //TODO a lot more functionality:
        //  returning some kind of reference
        //  manipulating sounds via reference
        //  "override" music
        //  ambients sound(s)
        //  fixed sound channels

        public void PlayUISound(string sound)
        {
            PlaySoundEx(sound, SoundType.Sound, true, true, false, false, 1.0f, Vector3.zero);
        }

        //play a sound from a category, and maybe retain it on scene change
        public void PlaySound(string sound, SoundType type, bool retain)
        {
            PlaySoundEx(sound, type, retain, false, false, false, 1.0f, Vector3.zero);
        }

        public void PlaySoundPositional(string sound, SoundType type, bool retain, Vector3 position)
        {
            PlaySoundEx(sound, type, retain, false, false, true, 1.0f, position);
        }

        private SoundInfo PlaySoundEx(string sound, SoundType type, bool retain, bool ignorePause, bool loop, bool positional, float volume, Vector3 position)
        {
            //get clip
            var clip = Module.GetSound(sound, type);
            if (clip == null)
            {
                Debug.LogWarning("Can't play sound " + sound);
                throw new InvalidOperationException();
            }

            //generate object
            GameObject spObject = new GameObject("SoundPlayer");
            spObject.transform.parent = transform;
            spObject.transform.position = position;
            AudioSource spSource = spObject.AddComponent<AudioSource>();
            spSource.spatialBlend = 0;

            //set params
            spSource.clip = clip;
            spSource.spatialBlend = positional ? 1.0f : 0f;
            spSource.loop = loop;
            spSource.time = 0;
            spSource.volume = volume;
            spSource.ignoreListenerPause = ignorePause;
            spSource.Play();

            //set record
            var soundInfo = new SoundInfo() { Clip = clip, Source = spSource, Retain = retain };
            PlayingSounds.Add(soundInfo);
            return soundInfo;
        }

        public void SetMusic(string sound, bool loop, bool retain)
        {
            var clip = Module.GetSound(sound, SoundType.Music);

            if (clip == null)
            {
                Debug.LogWarning("Can't play music " + sound);
                return;
            }

            MusicPlayer.Stop();
            MusicPlayer.clip = clip;

            MusicRetain = retain;
            MusicPlayer.loop = loop;
            MusicPlayer.time = 0;
            MusicPlayer.Play();
        }

        public void StartMusic()
        {
            MusicPlayer.Play();
        }

        public void StopMusic()
        {
            MusicPlayer.Stop();
        }


    }
}