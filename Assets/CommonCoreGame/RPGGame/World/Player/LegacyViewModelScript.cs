using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Weapon view model script for legacy animation sets (ie from before we separated out weapon components)
    /// </summary>
    public class LegacyViewModelScript : WeaponViewModelScript
    {
        
        public ViewModelState State;
        public Vector3 Offsets;

        public Animator AnimController;
        private GameObject EffectObject;
        public GameObject EffectPrefab;
        public Transform EffectPoint;
        public Transform ModelRoot;

        public AudioSource FireSound;
        public AudioSource ReloadSound;

        public bool UseMoveStateTransitions = false;

        [Header("State Names")]
        public string FixedState;
        public string MovingState = "moving";
        public string FiringState = "firing";
        public string ReloadingState = "reloading";

        protected override void Start()
        {
            TryGetDefaults();

            ApplyOffsets();
        }

        private void TryGetDefaults()
        {
            if (ModelRoot == null)
                ModelRoot = transform;

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


        public override void SetVisibility(bool visible)
        {
            ModelRoot.gameObject.SetActive(visible);
        }

        public override void SetState(ViewModelState newState, ViewModelHandednessState handednessState, float timeScale)
        {
            if (State == newState)
                return;

            switch (newState)
            {
                case ViewModelState.Fire:
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
                case ViewModelState.Reload:
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
                default:
                    if(AnimController != null)
                    {
                        if (UseMoveStateTransitions)
                        {
                            AnimController.SetBool("IsMoving", false);
                        }
                        else
                        {
                            //enter Fixed state, set anim accordingly
                            if (!string.IsNullOrEmpty(FixedState))
                                AnimController.Play(FixedState);
                            //stop anim if we don't have an actual fixed one
                            else
                                AnimController.StopPlayback();
                        }

                    }
                    break;
            }

            State = newState;

        }

        protected override void Update()
        {
            //nop nop nop for now
        }

        public override (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handednessState)
        {
            /*
            switch (newState)
            {
                case ViewModelState.Reload:
                    return ("LegacyReload", -1);
                case ViewModelState.Fire:
                    return ("LegacyFire", -1);
                default:
                    return ("LegacyIdle", -1);
            }
            */

            return ("Hidden", -1);
        }
    }
}