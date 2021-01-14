using CommonCore.LockPause;
using CommonCore.Messaging;
using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    /// <summary>
    /// Component for an actor to play audio clips on actions
    /// </summary>
    public class ActorAudioComponent : MonoBehaviour
    {
        [SerializeField, Header("Sounds")]
        private AudioSource WalkSound = null;
        [SerializeField]
        private AudioSource RunSound = null;
        [SerializeField]
        private AudioSource AlertSound = null;
        [SerializeField]
        private AudioSource PainSound = null;
        [SerializeField]
        private AudioSource DeathSound = null;
        [SerializeField]
        private AudioSource ExtremeDeathSound = null;

        public void StartWalkSound()
        {
            if (RunSound != null && RunSound.isPlaying)
                RunSound.loop = false;

            if(WalkSound != null)
            {
                if (!WalkSound.isPlaying)
                    WalkSound.Play();

                WalkSound.loop = true;
            }
        }

        public void StartRunSound()
        {
            if (RunSound == null)
                StartWalkSound();
            else if (WalkSound.isPlaying && !RunSound.isPlaying)
            {
                WalkSound.loop = false;
                RunSound.loop = true;
                RunSound.Play();
            }
            else if(RunSound.isPlaying)
            {
                WalkSound.loop = false; //probably not needed
                RunSound.loop = true;
            }
        }

        public void StopMoveSound()
        {
            if (WalkSound != null && WalkSound.isPlaying)
                WalkSound.loop = false;

            if (RunSound != null && RunSound.isPlaying)
                RunSound.loop = false;
        }

        public void PlayAlertSound()
        {
            if (AlertSound != null && !AlertSound.isPlaying)
                AlertSound.Play();
        }

        public void PlayPainSound()
        {
            if (PainSound != null)
                PainSound.Play();
        }

        public void StopLivingSounds()
        {
            WalkSound.Ref()?.Stop();
            RunSound.Ref()?.Stop();
            AlertSound.Ref()?.Stop();
            PainSound.Ref()?.Stop();
        }

        public void PlayDeathSound()
        {
            if (DeathSound != null)
                DeathSound.Play();
        }

        public void PlayExtremeDeathSound()
        {
            if (ExtremeDeathSound != null)
                ExtremeDeathSound.Play();
        }
    }
}