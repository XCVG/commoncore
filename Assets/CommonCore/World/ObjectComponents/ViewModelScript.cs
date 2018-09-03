using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.World
{
    public enum ViewModelState
    {
        Fixed, Moving, Firing, Reloading
    }

    //attached to Weapon ViewModels
    public class ViewModelScript : MonoBehaviour
    {
        
        public ViewModelState State;
        public Vector3 Offsets;

        public Animator AnimController;
        private GameObject EffectObject;
        public GameObject EffectPrefab;
        public Transform EffectPoint;

        public AudioSource FireSound;
        public AudioSource ReloadSound;

        [Header("State Names")]
        public string FixedState;
        public string MovingState = "moving";
        public string FiringState = "firing";
        public string ReloadingState = "reloading";

        void Start()
        {
            TryGetDefaults();

            ApplyOffsets();
        }

        private void TryGetDefaults()
        {
            if (EffectPoint == null)
                EffectPoint = transform;

            if (AnimController == null)
                AnimController = GetComponent<Animator>();

            if (FireSound == null)
            {
                var t = transform.Find("FireSound");
                if (t != null)
                    FireSound = t.GetComponent<AudioSource>();
            }

            if (ReloadSound == null)
            {
                var t = transform.Find("ReloadSound");
                if (t != null)
                    ReloadSound = t.GetComponent<AudioSource>();
            }

        }

        private void ApplyOffsets()
        {
            transform.localPosition = Offsets;
        }


        void Update()
        {

        }

        public void SetState(ViewModelState newState)
        {
            if (State == newState)
                return;

            switch (newState)
            {
                case ViewModelState.Firing:
                    //spawn effect if exists
                    if(EffectPrefab != null)
                    {
                        EffectObject = Instantiate<GameObject>(EffectPrefab, EffectPoint);
                    }

                    //play fire animation if exists
                    if (AnimController != null)
                    {
                        if (!string.IsNullOrEmpty(FiringState))
                            AnimController.Play(FiringState);
                    }

                    //play sound if exists
                    if (FireSound != null)
                        FireSound.Play();
                    break;
                case ViewModelState.Reloading:
                    //play reload animation if exists
                    if (AnimController != null)
                    {
                        if (!string.IsNullOrEmpty(ReloadingState))
                            AnimController.Play(ReloadingState);
                    }
                    //play sound if exists
                    if (ReloadSound != null)
                        ReloadSound.Play();

                    break;
                case ViewModelState.Moving:
                    if(AnimController != null)
                    {
                        //play move animation if available, if not play idle/none
                        if (!string.IsNullOrEmpty(MovingState))
                            AnimController.Play(MovingState);
                        else if (!string.IsNullOrEmpty(FixedState))
                            AnimController.Play(FixedState);
                        else
                            AnimController.StopPlayback();
                    }

                    break;
                default:
                    if(AnimController != null)
                    {
                        //enter Fixed state, set anim accordingly
                        if (!string.IsNullOrEmpty(FixedState))
                            AnimController.Play(FixedState);
                        //stop anim if we don't have an actual fixed one
                        else
                            AnimController.StopPlayback();
                    }
                    break;
            }

            State = newState;

        }
    }
}