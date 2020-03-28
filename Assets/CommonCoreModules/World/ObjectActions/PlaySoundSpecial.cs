using UnityEngine;
using System.Collections;
using System;

namespace CommonCore.ObjectActions
{

    public class PlaySoundSpecial : ActionSpecial
    {
        public AudioSource sound;

        private bool Locked;

        private void Start()
        {
            if(sound == null)
            {
                sound = GetComponent<AudioSource>();
                if(sound == null)
                {
                    Debug.LogWarning(string.Format("PlaySoundSpecial on {0} has no sound!", gameObject.name));
                }
            }
            
        }

        public override void Execute(ActionInvokerData data)
        {

            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            sound.Play();

            if (!Repeatable)
                Locked = true;
        }
    }
}