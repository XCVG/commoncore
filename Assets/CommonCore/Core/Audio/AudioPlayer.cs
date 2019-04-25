using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.Messaging;
using CommonCore.Config;


namespace CommonCore.Audio
{

    /// <summary>
    /// Represents a sound that is playing or enqueued to play
    /// </summary>
    public struct SoundInfo
    {
        public AudioClip Clip;
        public AudioSource Source;
        public bool Retain;
    }

    /// <summary>
    /// Generalized audio player to handle music and oneshot sound effects
    /// </summary>
    /// <remarks>
    /// <para>will eventually handle overridable channels and use an object pool, but not now</para>
    /// </remarks>
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
        private bool MusicShouldBePlaying;

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
            mpSource.ignoreListenerVolume = true;
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
            //message bus integration
            while(MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

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

        private void HandleMessage(QdmsMessage message)
        {
            if(message is QdmsFlagMessage)
            {
                QdmsFlagMessage flagMessage = (QdmsFlagMessage)message;
                switch (flagMessage.Flag)
                {
                    case "ConfigChanged":
                        MusicPlayer.volume = ConfigState.Instance.MusicVolume;
                        if(MusicShouldBePlaying)
                            MusicPlayer.Play(); //needed if audio system is reset
                        break;
                    default:
                        break;
                }
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
                StopMusic();
            }
        }
               

        //TODO a lot more functionality:
        //  returning some kind of reference
        //  manipulating sounds via reference
        //  "override" music
        //  ambients sound(s)
        //  fixed sound channels
        //(some of this is coded in but not exposed)

        /// <summary>
        /// Plays a UI sound effect (ambient, retained)
        /// </summary>
        public void PlayUISound(string sound)
        {
            try
            {
                PlaySoundEx(sound, SoundType.Sound, true, true, false, false, 1.0f, Vector3.zero);
            }
            catch(Exception e)
            {
                Debug.LogWarning($"Failed to play sound {e.GetType().Name}");
            }
        }

        /// <summary>
        /// Plays a sound effect
        /// </summary>
        /// <param name="sound">The sound to play</param>
        /// <param name="type">The type of sound</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        public void PlaySound(string sound, SoundType type, bool retain)
        {
            try
            {
                PlaySoundEx(sound, type, retain, false, false, false, 1.0f, Vector3.zero);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to play sound {e.GetType().Name}");
            }
        }

        /// <summary>
        /// Plays a sound effect at a certain position
        /// </summary>
        /// <param name="sound">The sound to play</param>
        /// <param name="type">The type of sound</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        /// <param name="position">Where in the world to play the sound effect</param>
        public void PlaySoundPositional(string sound, SoundType type, bool retain, Vector3 position)
        {
            try
            {
                PlaySoundEx(sound, type, retain, false, false, true, 1.0f, position);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to play sound {e.GetType().Name}");
            }
        }

        /// <summary>
        /// Plays a sound with many available options
        /// </summary>
        /// <param name="sound">The sound to play</param>
        /// <param name="type">The type of sound</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        /// <param name="ignorePause">Whether to ignore listener/game pause</param>
        /// <param name="loop">Whether to loop the sound</param>
        /// <param name="positional">Whether to play the sound positionally or ambiently</param>
        /// <param name="volume">The volume to play the sound at</param>
        /// <param name="position">The position to play the sound at (if it is positional)</param>
        /// <returns>A struct that defines the playing sound</returns>
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

        //TODO more music options like "play, or continue playing if already playing"
        //things like fades and overrides would also be nice

        /// <summary>
        /// Sets the background music to a specified track and starts it
        /// </summary>
        /// <param name="sound">The music file to play</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
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
            MusicPlayer.volume = ConfigState.Instance.MusicVolume;
            MusicRetain = retain;
            MusicPlayer.loop = loop;            
            StartMusic();
        }

        /// <summary>
        /// Stops and clears the currently set background music
        /// </summary>
        public void ClearMusic()
        {
            MusicPlayer.Stop();
            MusicPlayer.clip = null;
            MusicShouldBePlaying = false;
        }

        /// <summary>
        /// Plays the currently set background music
        /// </summary>
        public void StartMusic()
        {
            MusicPlayer.time = 0;
            MusicPlayer.Play();
            MusicShouldBePlaying = true;
        }

        /// <summary>
        /// Stops the currently set background music
        /// </summary>
        public void StopMusic()
        {
            MusicPlayer.Stop();
            MusicShouldBePlaying = false;
        }


    }
}