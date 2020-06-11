using CommonCore.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.ObjectActions
{

    /// <summary>
    /// Base class for swing door and slide door specials
    /// </summary>
    public abstract class MovingDoorSpecial : ActionSpecial
    {
        [SerializeField, Header("Door Parameters"), Tooltip("Set to 0 to have the door immediately close, or -1 to have it stay open until manually closed")]
        protected float HoldTime = 5f;
        [SerializeField, Tooltip("If set, OpenSound will be used for Close as well")]
        protected bool UseSameSound = true;
        [SerializeField]
        protected bool LoopSound = false;
        [SerializeField]
        protected MovingDoorBlockedAction BlockedAction = MovingDoorBlockedAction.Continue;

        [SerializeField, Header("Door Effects")] //we prefer AudioSource over dynamic
        protected AudioSource OpenSoundSource = null;
        [SerializeField]
        protected string OpenSoundName = null;
        [SerializeField]
        protected AudioSource CloseSoundSource = null;
        [SerializeField]
        protected string CloseSoundName = null;

        private bool Locked;
        private Coroutine DoorSequenceCoroutine = null;
        protected bool DoorOpen = false;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            ToggleDoor();

            if (!Repeatable)
                Locked = true;
        }

        private void ToggleDoor()
        {
            //start sequence, handle "open wait close" case
            if(DoorOpen)
            {
                //door is open or opening, cancel sequence and start close sequence
                if (DoorSequenceCoroutine != null)
                    StopCoroutine(DoorSequenceCoroutine);

                DoorOpen = false;
                DoorSequenceCoroutine = StartCoroutine(CoCloseDoor());
            }
            else
            {
                //door is closed or closing, cancel sequence and start open sequence
                if (DoorSequenceCoroutine != null)
                    StopCoroutine(DoorSequenceCoroutine);

                DoorOpen = true;
                DoorSequenceCoroutine = StartCoroutine(CoOpenWaitCloseSequence());
            }
        }

        private IEnumerator CoOpenWaitCloseSequence()
        {
            DoorOpen = true;
            yield return CoOpenDoor();

            if(HoldTime > 0)
                yield return new WaitForSeconds(HoldTime);

            if (HoldTime >= 0)
            {
                DoorOpen = false;
                yield return CoCloseDoor();
            }

            DoorSequenceCoroutine = null;
        }

        //if "force" is set, override the current sequence and/or ignore "Locked" state

        /// <summary>
        /// Opens the door and runs its normal sequence
        /// </summary>
        public void OpenWaitClose(bool force)
        {
            if (!force && (Locked || DoorSequenceCoroutine != null))
                return;

            if (DoorSequenceCoroutine != null)
                StopCoroutine(DoorSequenceCoroutine);

            DoorOpen = true;
            DoorSequenceCoroutine = StartCoroutine(CoOpenWaitCloseSequence());
        }

        /// <summary>
        /// Opens this door
        /// </summary>
        public void Open(bool force)
        {
            if (!force && (Locked || DoorSequenceCoroutine != null))
                return;

            if (DoorSequenceCoroutine != null)
                StopCoroutine(DoorSequenceCoroutine);

            DoorOpen = true;
            DoorSequenceCoroutine = StartCoroutine(CoOpenDoor());
        }

        /// <summary>
        /// Closes this door
        /// </summary>
        public void Close(bool force)
        {
            if (!force && (Locked || DoorSequenceCoroutine != null))
                return;

            if (DoorSequenceCoroutine != null)
                StopCoroutine(DoorSequenceCoroutine);

            DoorOpen = false;
            DoorSequenceCoroutine = StartCoroutine(CoCloseDoor());
        }

        //sound functions need to be called by CoOpenDoor and CoCloseDoor implementations
        protected void PlayOpenSound()
        {
            StopSounds();

            if (OpenSoundSource != null)
            {
                OpenSoundSource.loop = LoopSound;
                OpenSoundSource.Play();
            }
            else if(!string.IsNullOrEmpty(OpenSoundName))
            {
                AudioPlayer.Instance.PlaySoundPositional(OpenSoundName, SoundType.Sound, false, transform.position);
            }
        }

        protected void PlayCloseSound()
        {
            StopSounds();

            if (CloseSoundSource != null)
            {
                CloseSoundSource.loop = LoopSound;
                CloseSoundSource.Play();
            }
            else if (!string.IsNullOrEmpty(CloseSoundName))
            {
                AudioPlayer.Instance.PlaySoundPositional(CloseSoundName, SoundType.Sound, false, transform.position);
            }
            else if(UseSameSound)
            {
                if (OpenSoundSource != null)
                {
                    OpenSoundSource.loop = LoopSound;
                    OpenSoundSource.Play();
                }
                else if (!string.IsNullOrEmpty(OpenSoundName))
                {
                    AudioPlayer.Instance.PlaySoundPositional(OpenSoundName, SoundType.Sound, false, transform.position);
                }
            }
        }

        protected void StopSounds()
        {
            OpenSoundSource.Ref()?.Stop();
            CloseSoundSource.Ref()?.Stop();

            if (LoopSound && (string.IsNullOrEmpty(OpenSoundName) || string.IsNullOrEmpty(CloseSoundName)))
                Debug.LogWarning($"{GetType().Name} on {gameObject.name} is set to loop sounds but uses dynamic sounds (looping dynamic sounds not supported)");
        }

        //these handle the actual open/close action
        protected abstract IEnumerator CoOpenDoor();
        protected abstract IEnumerator CoCloseDoor();


    }

    [Serializable]
    public enum MovingDoorBlockedAction
    {
        Continue, Pause, Reverse
    }
}