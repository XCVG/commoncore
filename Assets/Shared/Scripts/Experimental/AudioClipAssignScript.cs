using CommonCore.Audio;
using UnityEngine;

namespace CommonCore.Experimental
{

    /// <summary>
    /// Assigns an audio clip from a resource
    /// </summary>
    public class AudioClipAssignScript : MonoBehaviour
    {
        [SerializeField, Tooltip("If not set, will try to use an audio source on this object")]
        private AudioSource AudioSource = null;
        [SerializeField]
        private string Sound = null;
        [SerializeField]
        private SoundType SoundType = SoundType.Any;
        [SerializeField, Tooltip("If set, will clear the audio clip of the target audio source")]
        private bool ClearIfNotFound = false;
        [SerializeField]
        private bool PlayAfterSet = false;

        private void Start()
        {
            if (AudioSource == null)
                AudioSource = GetComponent<AudioSource>();

            if (AudioSource == null)
                return;

            var sound = CCBase.GetModule<AudioModule>().GetSound(Sound, SoundType);

            if(sound != null)
            {
                AudioSource.clip = sound;
                if (PlayAfterSet)
                    AudioSource.Play();
            }
            else
            {
                if (ClearIfNotFound)
                    AudioSource.clip = null;
            }
        }
    }
}