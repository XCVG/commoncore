using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    /// <summary>
    /// Placeholder weapon viewmodel script used to implement fists (animation for hands only)
    /// </summary>
    public class PlaceholderWeaponViewModelScript : WeaponViewModelScript
    {
        [Header("Hand Animations"), SerializeField]
        private string IdleAnimation;
        [SerializeField]
        private float IdleDuration;
        [SerializeField]
        private string FireAnimation;
        [SerializeField]
        private float FireDuration;
        [SerializeField]
        private string RaiseAnimation;
        [SerializeField]
        private float RaiseDuration;
        [SerializeField]
        private string LowerAnimation;
        [SerializeField]
        private float LowerDuration;

        [Header("Sounds"), SerializeField]
        private AudioSource FireSound;

        protected override void Start()
        {
            //nop
        }

        protected override void Update()
        {
            //nop
        }

        public override (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handedness)
        {
            switch (newState)
            {
                case ViewModelState.Idle:
                    return (IdleAnimation, IdleDuration);
                case ViewModelState.Raise:
                    return (RaiseAnimation, RaiseDuration);
                case ViewModelState.Lower:
                    return (LowerAnimation, LowerDuration);
                case ViewModelState.Fire:
                    return (FireAnimation, FireDuration);
                default:
                    Debug.LogWarning($"Tried to get hand state {newState} from {name} which is not supported for {nameof(PlaceholderWeaponViewModelScript)}");
                    return ("Idle", -1);
            }
        }

        public override void SetState(ViewModelState newState, ViewModelHandednessState handedness, float timeScale)
        {
            if (newState == ViewModelState.Fire)
                FireSound.Ref()?.Play();
        }

        public override void SetVisibility(bool visible)
        {
            //nop
        }

    }
}