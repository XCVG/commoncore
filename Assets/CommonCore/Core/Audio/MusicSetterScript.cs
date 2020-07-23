using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Audio;

namespace CommonCore.Audio
{

    /// <summary>
    /// Sets the background music on start
    /// </summary>
    public class MusicSetterScript : MonoBehaviour
    {
        [SerializeField]
        private MusicSlot MusicSlot = default;
        [SerializeField]
        private string Music = null;
        [SerializeField]
        private float Volume = 1.0f;
        [SerializeField]
        private bool Loop = true;


        void Start()
        {
            AudioPlayer.Instance.SetMusic(Music, MusicSlot, Volume, Loop, false);
            AudioPlayer.Instance.StartMusic(MusicSlot);
        }

    }
}