using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.Messaging;
using CommonCore.Config;


namespace CommonCore.Audio
{


    /// <summary>
    /// Generalized audio player to handle music and oneshot sound effects
    /// </summary>
    /// <remarks>
    /// <para>will eventually handle overridable channels and use an object pool, but not now</para>
    /// </remarks>
    public class AudioPlayer : MonoBehaviour
    {
        //TODO global music enable

        /// <summary>
        /// Represents a sound that is playing or enqueued to play
        /// </summary>
        public class SoundInfo //watch that value->reference semantic change break everything
        {
            public AudioClip Clip;
            public AudioSource Source;
            public bool Retain;
        }

        /// <summary>
        /// Represents the music that is playing in a slot
        /// </summary>
        private class MusicInfo //we actually want reference semantics for this (I think)
        {
            public AudioClip Clip;
            public string Name;
            public float Volume;
            public bool Loop;
            public bool Retain;
            public bool Playing;
            public float? Time;
        }

        private const float CleanupInterval = 2.5f;

        public static AudioPlayer Instance;

        public bool MusicEnabled { get; private set; } = true; //not happy with the handling of this tbh

        private AudioModule Module;
        private QdmsMessageInterface MessageInterface;
        private List<SoundInfo> PlayingSounds;

        //user audio component handling
        private List<UserMusicComponent> UserMusicComponents = new List<UserMusicComponent>();
        private UserMusicComponent CurrentUserMusicComponent;

        //handle multiple slots
        private MusicSlot? CurrentMusicSlot = null;
        private Dictionary<MusicSlot, MusicInfo> CurrentMusics = new Dictionary<MusicSlot, MusicInfo>();
        private Dictionary<MusicSlot, AudioSource> MusicPlayers = new Dictionary<MusicSlot, AudioSource>();

        private float TimeElapsed;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
            Instance = this;
        }

        internal void SetModule(AudioModule module)
        {
            if (Module == null)
                Module = module;
        }

        private void Start()
        {
            MessageInterface = new QdmsMessageInterface(gameObject);

            //initialize music players
            foreach (MusicSlot slot in Enum.GetValues(typeof(MusicSlot)))
            {
                GameObject mpObject = new GameObject($"MusicPlayer_{(int)slot}");
                mpObject.transform.parent = transform;
                AudioSource mpSource = mpObject.AddComponent<AudioSource>();
                mpSource.spatialBlend = 0;
                mpSource.ignoreListenerPause = true;
                mpSource.ignoreListenerVolume = true;
                MusicPlayers[slot] = mpSource;
                //mpSource.volume = Config.ConfigState.Instance.MusicVolume;
            }

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
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

            //run cleanup periodically
            TimeElapsed += Time.deltaTime;
            if (TimeElapsed >= CleanupInterval)
            {
                for (int i = PlayingSounds.Count - 1; i >= 0; i--)
                {
                    var s = PlayingSounds[i];
                    if (s.Source == null || !s.Source.isPlaying)
                    {
                        Destroy(s.Source.gameObject);
                        PlayingSounds.RemoveAt(i);
                    }
                }

                TimeElapsed = 0;
            }

            //report time if applicable
            if (MusicEnabled && CurrentUserMusicComponent != null && CurrentMusicSlot == MusicSlot.User && MusicPlayers[MusicSlot.User].isPlaying && MusicPlayers[MusicSlot.User].clip != null)
                CurrentUserMusicComponent.ReportTime(MusicPlayers[MusicSlot.User].time);

            //handle track ended
            if (MusicEnabled && CurrentUserMusicComponent != null && CurrentMusicSlot == MusicSlot.User &&
                !MusicPlayers[MusicSlot.User].isPlaying && MusicPlayers[MusicSlot.User].clip != null && MusicPlayers[MusicSlot.User].timeSamples == MusicPlayers[MusicSlot.User].clip.samples)
            {
                CurrentUserMusicComponent.SignalTrackEnded();
            }
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is QdmsFlagMessage)
            {
                //TODO allow playing sounds by message

                QdmsFlagMessage flagMessage = (QdmsFlagMessage)message;
                switch (flagMessage.Flag)
                {
                    case "ConfigChanged":
                        if (CurrentMusicSlot != null)
                        {
                            foreach (var umc in UserMusicComponents)
                            {
                                umc.SignalAudioRestarted();
                            }

                            if (CurrentMusics.ContainsKey(CurrentMusicSlot.Value)) //anti-break, though we shouldn't hit this...
                            {
                                MusicPlayers[CurrentMusicSlot.Value].volume = Mathf.Clamp(CurrentMusics[CurrentMusicSlot.Value].Volume * ConfigState.Instance.MusicVolume, 0, 1);
                                if (CurrentMusics[CurrentMusicSlot.Value].Playing) //needed if the audio system is totally reset
                                    MusicPlayers[CurrentMusicSlot.Value].Play(); //this doesn't work with user music
                            }


                        }
                        break;
                    case "EnableMusic": //not entirely happy with this method
                        MusicEnabled = true;
                        HandleMusicChanged();
                        break;
                    case "DisableMusic":
                        MusicEnabled = false;
                        HandleMusicChanged();
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

            //handle unretain for all slots
            foreach (var slot in CurrentMusics.Keys.ToArray())
            {
                if (!CurrentMusics[slot].Retain)
                    CurrentMusics.Remove(slot);
            }
            HandleMusicChanged();
        }

        public void HandleGameEnd()
        {
            //disable/release user music component
            SelectUserMusicComponent(null);
            return;
        }

        private void HandleMusicChanged()
        {
            if (!MusicEnabled)
            {
                foreach (var slot in MusicPlayers.Keys)
                {
                    MusicPlayers[slot].Pause();
                }

                return;
            }

            //find the highest slot that should be playing, stop the other ones and play that one

            MusicSlot? highestPlayingSlot = GetHighestPlayingMusicSlot();

            //stop all other slots
            foreach (var slot in MusicPlayers.Keys)
            {
                if (!highestPlayingSlot.HasValue || slot != highestPlayingSlot.Value || !CurrentMusics[highestPlayingSlot.Value].Playing)
                    MusicPlayers[slot].Pause();
            }

            if (highestPlayingSlot.HasValue && CurrentMusics[highestPlayingSlot.Value].Playing)
            {
                CurrentMusicSlot = highestPlayingSlot.Value;

                var musicInfo = CurrentMusics[highestPlayingSlot.Value];
                var audioSource = MusicPlayers[highestPlayingSlot.Value];

                //if it's a different song, stop and set clip
                if (musicInfo.Clip != audioSource.clip)
                {
                    audioSource.Stop();
                    audioSource.clip = musicInfo.Clip;
                }

                if (audioSource.clip != null)
                {

                    //set parameters
                    audioSource.loop = musicInfo.Loop;
                    audioSource.volume = Mathf.Clamp(musicInfo.Volume * ConfigState.Instance.MusicVolume, 0, 1);

                    //also seek the music if requested
                    if (musicInfo.Time.HasValue)
                    {
                        audioSource.time = musicInfo.Time.Value;
                        musicInfo.Time = null;
                    }

                    //play if it is not playing
                    if (!audioSource.isPlaying)
                        audioSource.Play();


                }
            }

        }

        private MusicSlot? GetHighestPlayingMusicSlot()
        {
            MusicSlot? highestPlayingSlot = null;

            foreach (var slot in CurrentMusics.Keys.OrderByDescending(x => x))
            {
                var musicInfo = CurrentMusics[slot];
                if (musicInfo.Playing || (slot == MusicSlot.User && CurrentUserMusicComponent != null)) //hack: User slot is always considered playing if enabled
                {
                    highestPlayingSlot = slot;
                    break;
                }
            }

            return highestPlayingSlot;
        }

        //USER AUDIO COMPONENT HANDLING

        public void RegisterUserMusicComponent(UserMusicComponent uac)
        {
            Type uacType = uac.GetType();

            if (UserMusicComponents.Find(x => x.GetType() == uacType) != null)
            {
                Debug.LogError($"Can't register UserMusicComponent of type {uacType} because an instance is already registered!");
                throw new InvalidOperationException();
            }

            UserMusicComponents.Add(uac);
            Debug.Log("Registered " + uac.GetType().Name);
        }


        public void UnregisterUserMusicComponent(UserMusicComponent uac)
        {
            if (!UserMusicComponents.Remove(uac))
                throw new IndexOutOfRangeException();
        }

        public void SelectUserMusicComponent(string component)
        {
            //lookup/find
            UserMusicComponent uac = null;

            if (!string.IsNullOrEmpty(component))
            {
                uac = UserMusicComponents.Find(x => x.GetType().Name.Equals(component, StringComparison.Ordinal));

                if (uac == null)
                {
                    Debug.LogError($"Can't switch to UserMusicComponent of type {component} because it doesn't exist");
                    throw new IndexOutOfRangeException();
                }
            }

            if (CurrentUserMusicComponent != null)
            {
                if (CurrentMusics.ContainsKey(MusicSlot.User))
                {
                    AudioClip clip = CurrentMusics[MusicSlot.User].Clip;
                    CurrentMusics.Remove(MusicSlot.User);

                    //HandleMusicChanged();
                    MusicPlayers[MusicSlot.User].Stop();
                    MusicPlayers[MusicSlot.User].clip = null;

                    if (clip != null)
                        CurrentUserMusicComponent.ReportClipReleased(clip);
                }

                CurrentUserMusicComponent.Enabled = false;
            }

            CurrentUserMusicComponent = uac;

            if (uac != null)
            {
                CurrentUserMusicComponent.Enabled = true;
            }
            else
            {
                HandleMusicChanged();
            }


        }

        public string GetCurrentUserMusicComponent()
        {
            return CurrentUserMusicComponent?.GetType().Name ?? null;
        }

        public IEnumerable<UserMusicComponent> GetUserMusicComponents()
        {
            return UserMusicComponents;
        }

        //plumbing for console commands

        /// <summary>
        /// Clears all playing sounds, regardless of completion or retention status
        /// </summary>
        public void ClearAllSounds()
        {
            for (int i = PlayingSounds.Count - 1; i >= 0; i--)
            {
                var s = PlayingSounds[i];
                Destroy(s.Source.gameObject);
                PlayingSounds.RemoveAt(i);
            }
        }

        /// <summary>
        /// Clears all playing music, regardless of completion or retention status
        /// </summary>
        public void ClearAllMusic()
        {
            //explicitly release user clip if applicable
            if (CurrentMusics.ContainsKey(MusicSlot.User))
            {
                MusicPlayers[MusicSlot.User].Stop();
                var clip = MusicPlayers[MusicSlot.User].clip;
                MusicPlayers[MusicSlot.User].clip = null;
                CurrentUserMusicComponent.ReportClipReleased(clip);
            }

            //clear music entries
            CurrentMusics.Clear();

            //stop and unload all musics
            foreach (var musicSource in MusicPlayers.Values)
            {
                musicSource.Stop();
                musicSource.clip = null;
            }
        }

        //TODO a lot more functionality:
        //  returning some kind of reference
        //  manipulating sounds via reference
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
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to play sound {e.GetType().Name}");
            }
        }

        /// <summary>
        /// Plays a UI sound effect (ambient, retained)
        /// </summary>
        public void PlayUISound(AudioClip clip)
        {
            try
            {
                PlaySoundEx(clip, true, true, false, false, 1.0f, Vector3.zero);
            }
            catch (Exception e)
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
        /// Plays a sound effect
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        public void PlaySound(AudioClip clip, bool retain)
        {
            try
            {
                PlaySoundEx(clip, retain, false, false, false, 1.0f, Vector3.zero);
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
        /// Plays a sound effect at a certain position
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        /// <param name="position">Where in the world to play the sound effect</param>
        public void PlaySoundPositional(AudioClip clip, bool retain, Vector3 position)
        {
            try
            {
                PlaySoundEx(clip, retain, false, false, true, 1.0f, position);
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
        public SoundInfo PlaySoundEx(string sound, SoundType type, bool retain, bool ignorePause, bool loop, bool positional, float volume, Vector3 position)
        {
            //get clip
            var clip = Module.GetSound(sound, type);
            if (clip == null)
            {
                Debug.LogError("Can't play sound \"" + sound + "\" because it doesn't exist!");
                throw new InvalidOperationException();
            }

            return PlaySoundEx(clip, retain, ignorePause, loop, positional, volume, position);
        }

        /// <summary>
        /// Plays an audio clip with many available options
        /// </summary>
        /// <param name="clip">The audio clip to play</param>
        /// <param name="retain">Whether to retain the sound on scene transition</param>
        /// <param name="ignorePause">Whether to ignore listener/game pause</param>
        /// <param name="loop">Whether to loop the sound</param>
        /// <param name="positional">Whether to play the sound positionally or ambiently</param>
        /// <param name="volume">The volume to play the sound at</param>
        /// <param name="position">The position to play the sound at (if it is positional)</param>
        /// <returns>A struct that defines the playing sound</returns>
        public SoundInfo PlaySoundEx(AudioClip clip, bool retain, bool ignorePause, bool loop, bool positional, float volume, Vector3 position)
        {
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
        /// Plays music into a specified slot.
        /// </summary>
        /// <param name="sound">The music file to play</param>
        /// <param name="slot">The slot to play music in</param>
        /// <param name="volume">The volume to play the music at (will be multiplied with global music volume)</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
        /// <remarks>
        /// <para>Equivalent to calling SetMusic followed by StartMusic</para>
        /// </remarks>
        public void PlayMusic(string sound, MusicSlot slot, float volume, bool loop, bool retain)
        {
            SetMusic(sound, slot, volume, loop, retain);
            StartMusic(slot);
        }

        /// <summary>
        /// Plays music into a specified slot.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="slot">The slot to play music in</param>
        /// <param name="volume">The volume to play the music at (will be multiplied with global music volume)</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
        /// <remarks>
        /// <para>Equivalent to calling SetMusic followed by StartMusic</para>
        /// </remarks>
        public void PlayMusic(AudioClip clip, MusicSlot slot, float volume, bool loop, bool retain)
        {
            SetMusic(clip, slot, volume, loop, retain);
            StartMusic(slot);
        }

        /// <summary>
        /// Sets the music in a slot to the specified audio.
        /// </summary>
        /// <param name="sound">The music file to play</param>
        /// <param name="slot">The slot to play music in</param>
        /// <param name="volume">The volume to play the music at (will be multiplied with global music volume)</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
        /// <remarks>
        /// <para>No longer effects playback state</para>
        /// </remarks>
        public void SetMusic(string sound, MusicSlot slot, float volume, bool loop, bool retain)
        {
            var clip = Module.GetSound(sound, SoundType.Music);

            if (clip == null)
            {
                Debug.LogError("Can't find music " + sound);
                return;
            }

            SetMusic(clip, sound, slot, volume, loop, retain);
        }

        /// <summary>
        /// Sets the music in a slot to the specified audio.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="slot">The slot to play music in</param>
        /// <param name="volume">The volume to play the music at (will be multiplied with global music volume)</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
        /// <remarks>
        /// <para>No longer effects playback state</para>
        /// </remarks>
        public void SetMusic(AudioClip clip, MusicSlot slot, float volume, bool loop, bool retain) => SetMusic(clip, clip.Ref()?.name, slot, volume, loop, retain);

        /// <summary>
        /// Sets the music in a slot to the specified audio.
        /// </summary>
        /// <param name="clip">The music clip to play</param>
        /// <param name="name">The name of the music clip</param>
        /// <param name="slot">The slot to play music in</param>
        /// <param name="volume">The volume to play the music at (will be multiplied with global music volume)</param>
        /// <param name="loop">Whether to loop the music</param>
        /// <param name="retain">Whether to retain the music across scene loads</param>
        /// <remarks>
        /// <para>No longer effects playback state</para>
        /// </remarks>
        public void SetMusic(AudioClip clip, string name, MusicSlot slot, float volume, bool loop, bool retain)
        {
            bool wasPlaying = false;
            if (CurrentMusics.ContainsKey(slot))
                wasPlaying = CurrentMusics[slot].Playing;

            //release old clip from user music slot
            if (slot == MusicSlot.User && CurrentMusics.ContainsKey(slot) && CurrentMusics[slot].Clip != null && CurrentMusics[slot].Clip != clip)
            {
                MusicPlayers[MusicSlot.User].Stop();
                var oldClip = MusicPlayers[MusicSlot.User].clip;
                MusicPlayers[MusicSlot.User].clip = null;
                CurrentUserMusicComponent.ReportClipReleased(oldClip);
            }

            var musicInfo = new MusicInfo() { Clip = clip, Name = name, Volume = volume, Loop = loop, Retain = retain, Playing = wasPlaying, Time = 0 };
            CurrentMusics[slot] = musicInfo;

            HandleMusicChanged();
        }

        /// <summary>
        /// Sets the music in a slot to the specified audio, keeping everything else the same
        /// </summary>
        /// <remarks>Intended to be used with UserAudioComponent</remarks>
        public void SetMusicClip(AudioClip clip, MusicSlot slot)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                //release old clip from user music slot
                if (slot == MusicSlot.User && CurrentMusics.ContainsKey(slot) && CurrentMusics[slot].Clip != null && CurrentMusics[slot].Clip != clip)
                {
                    MusicPlayers[MusicSlot.User].Stop();
                    var oldClip = MusicPlayers[MusicSlot.User].clip;
                    MusicPlayers[MusicSlot.User].clip = null;
                    CurrentUserMusicComponent.ReportClipReleased(oldClip);
                }

                CurrentMusics[slot].Clip = clip;
                HandleMusicChanged();
            }
            else
            {
                SetMusic(clip, slot, 1f, true, false);
            }
        }

        /// <summary>
        /// Sets the looping state of a music slot
        /// </summary>
        public void SetMusicLooping(bool looping, MusicSlot slot)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                CurrentMusics[slot].Loop = looping;
                HandleMusicChanged();
            }
        }

        /// <summary>
        /// Sets the volume of a music slot
        /// </summary>
        public void SetMusicVolume(float volume, MusicSlot slot)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                CurrentMusics[slot].Volume = volume;
                HandleMusicChanged();
            }
        }

        /// <summary>
        /// Stops and clears the currently set music in a slot
        /// </summary>
        public void ClearMusic(MusicSlot slot)
        {
            CurrentMusics.Remove(slot);
            HandleMusicChanged();
        }

        /// <summary>
        /// Plays the currently set music in a slot
        /// </summary>
        public void StartMusic(MusicSlot slot) => StartMusic(slot, true);

        /// <summary>
        /// Plays the currently set music in a slot
        /// </summary>
        public void StartMusic(MusicSlot slot, bool restart)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                CurrentMusics[slot].Playing = true;
                if (restart)
                    CurrentMusics[slot].Time = 0;
                HandleMusicChanged();
            }
            else
            {
                Debug.LogWarning($"Tried to start music in slot {slot.ToString()} but none exists!");
            }
        }

        /// <summary>
        /// Stops the currently set music in a slot
        /// </summary>
        public void StopMusic(MusicSlot slot)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                CurrentMusics[slot].Playing = false;
                HandleMusicChanged();
            }
            else
            {
                Debug.LogWarning($"Tried to stop music in slot {slot.ToString()} but none exists!");
            }
        }

        /// <summary>
        /// Seeks the currently set music in a slot
        /// </summary>
        public void SeekMusic(MusicSlot slot, float time)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                CurrentMusics[slot].Time = time;
                HandleMusicChanged();
            }
            else
            {
                Debug.LogWarning($"Tried to seek music in slot {slot.ToString()} but none exists!");
            }
        }

        /// <summary>
        /// Gets the name of the current music in the slot, if it exists
        /// </summary>
        public string GetMusicName(MusicSlot slot)
        {
            if (CurrentMusics.ContainsKey(slot))
            {
                return CurrentMusics[slot].Name;
            }

            return null;
        }

        /// <summary>
        /// Gets the volume of the current music in the slot, if it exists
        /// </summary>
        public float? GetMusicVolume(MusicSlot slot)
        {
            if(CurrentMusics.ContainsKey(slot))
            {
                return CurrentMusics[slot].Volume;
            }

            return null;
        }

        /// <summary>
        /// Checks if a slot is currently playing
        /// </summary>
        public bool IsMusicPlaying(MusicSlot slot)
        {
            var highestPlayingMusic = GetHighestPlayingMusicSlot();
            return highestPlayingMusic != null && highestPlayingMusic.Value == slot;
        }

        /// <summary>
        /// Checks if a slot has music and is set to play
        /// </summary>
        public bool IsMusicSetToPlay(MusicSlot slot)
        {
            return MusicHasClip(slot) && CurrentMusics[slot].Playing;

        }

        /// <summary>
        /// Checks if a music slot has something in it
        /// </summary>
        public bool MusicHasClip(MusicSlot slot)
        {
            if(CurrentMusics.ContainsKey(slot))
            {
                if (slot == MusicSlot.User || CurrentMusics[slot].Playing)
                    return true;
            }

            return false;
        }


    }
}