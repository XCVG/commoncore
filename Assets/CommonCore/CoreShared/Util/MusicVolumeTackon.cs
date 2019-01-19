using CommonCore.Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tack-on script for applying global music volume to an AudioSource
/// </summary>
public class MusicVolumeTackon : MonoBehaviour
{
    [SerializeField, Tooltip("Check for changes to ConfigState after this tackon is started?")]
    private bool UseAggressiveConfigurationCheck = false;
    [SerializeField, Tooltip("Leave blank to attach to the AudioSource on this GameObject")]
    private AudioSource AttachedMusic;
    [SerializeField, Tooltip("Set to also apply IgnoreListenerPause")]
    private bool IgnoreListenerPause;

    private float? DefaultMusicVolume;

    private void Start()
    {
        if (AttachedMusic == null)
            AttachedMusic = GetComponent<AudioSource>();

        if (IgnoreListenerPause && AttachedMusic)
            AttachedMusic.ignoreListenerPause = true;

        ApplyMusicVolume();
    }

    private void Update()
    {
        //TODO move this to messaging once we have support for calling delegates in QdmsMessageInterface

        if (UseAggressiveConfigurationCheck)
            ApplyMusicVolume();
    }

    private void ApplyMusicVolume()
    {
        if(AttachedMusic == null)
        {
            Debug.LogError($"MusicVolumeTackon on {this.gameObject.name} has no attached AudioSource!");
            return;
        }

        if (DefaultMusicVolume == null)
            DefaultMusicVolume = AttachedMusic.volume;

        AttachedMusic.volume = DefaultMusicVolume.Value * ConfigState.Instance.MusicVolume;
    }


}
