using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.Audio
{

    /// <summary>
    /// A component loaded and handled by the AudioPlayer to play music into the User slot
    /// </summary>
    public abstract class UserMusicComponent
    {
        /// <summary>
        /// Enable/disable (select) this UserAudioComponent. Implement logic as necessary.
        /// </summary>
        public abstract bool Enabled { get; set; }

        /// <summary>
        /// Called by AudioPlayer as the clip is played. Use to implement counter, seekbar, etc
        /// </summary>
        public abstract void ReportTime(float time);

        /// <summary>
        /// Called by the AudioPlayer when a clip ends. In effect requests the next track.
        /// </summary>
        public abstract void SignalTrackEnded();
        
        /// <summary>
        /// Called by the AudioPlayer when it no longer needs a clip. You are then free to release it.
        /// </summary>
        public abstract void ReportClipReleased(AudioClip clip);

        /// <summary>
        /// Called by the AudioPlayer when the audio system is restarted. TBD what this entails
        /// </summary>
        public abstract void SignalAudioRestarted();

        /// <summary>
        /// A prefab for the control panel, will be added to the radio panel
        /// </summary>
        public abstract GameObject PanelPrefab { get; }

        /// <summary>
        /// A nice human-readable name to put on buttons etc
        /// </summary>
        public abstract string NiceName { get; }
    }
}