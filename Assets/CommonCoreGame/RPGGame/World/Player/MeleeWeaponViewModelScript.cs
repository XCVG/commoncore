using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    public class MeleeWeaponViewModelScript : WeaponViewModelScript
    {
        [Header("Melee View Model Options"), SerializeField]
        private MeleeWeaponViewModelMode Mode = MeleeWeaponViewModelMode.None;
        [SerializeField]
        private bool ShowHands = false;

        [Header("Hand Animations"), SerializeField, Tooltip("Will be ignored if ShowHands is false")]
        public MeleeWeaponViewModelAnimations HandAnimations = default;

        [Header("Glue To Bone Options"), SerializeField, Tooltip("It is strongly recommended that you use either offset point or explicit offsets, not both")]
        private Transform OffsetPoint = null;
        [SerializeField]
        private Vector3 OffsetTranslation = Vector3.zero;
        [SerializeField]
        private Vector3 OffsetRotation = Vector3.zero;
        //TODO? option for which bone to glue to (?)

        [Header("Components"), SerializeField]
        private Animator ModelAnimator = null;
        [SerializeField]
        private Transform ModelRoot = null;

        [Header("Sounds"), SerializeField]
        private AudioSource AttackSound = null;
        [SerializeField]
        private AudioSource RaiseSound = null;
        [SerializeField]
        private AudioSource LowerSound = null;

        private PlayerWeaponComponent WeaponComponent { get { if (_weaponComponent == null) _weaponComponent = GetComponentInParent<PlayerWeaponComponent>(); return _weaponComponent; } }
        private PlayerWeaponComponent _weaponComponent; //cached

        private WeaponHandModelScript HandModel { get { if (_handModel == null) _handModel = WeaponComponent.HandModel; return _handModel; } }
        private WeaponHandModelScript _handModel;

        protected override void Start()
        {
           // throw new NotImplementedException();

            if(Mode == MeleeWeaponViewModelMode.GlueToBone && !ShowHands)
            {
                Debug.LogWarning("GlueToBone mode without showing hands will result in undefined behavior!");
            }

            if(Mode == MeleeWeaponViewModelMode.GlueToBone && ModelAnimator != null && ModelAnimator.enabled == true)
            {
                Debug.LogWarning("GlueToBone mode will not work if the ModelAnimator is controlling root position!");
            }

            if(Mode == MeleeWeaponViewModelMode.GlueAndAnimate && ModelAnimator.transform == ModelRoot)
            {
                Debug.LogWarning("GlueAndAnimate mode may not work properly if the model root is controlled by an animator!");
            }

            Update(); //hack to glue immediately
        }

        protected override void Update()
        {
           // throw new NotImplementedException();

            if(Mode == MeleeWeaponViewModelMode.GlueToBone || Mode == MeleeWeaponViewModelMode.GlueAndAnimate)
            {
                //blindly copy the transform for now
                //Debug.Log(".");
                var (newPos, newRot) = HandModel.HandBoneTransform;
                //Debug.Log($"{newPos} | {newRot}");

                //basically, we want to glue the hand to the offset point if we have one
                Vector3 offset = Vector3.zero;
                Quaternion offsetRotation = Quaternion.identity;

                if (OffsetPoint)
                {
                    offset = -OffsetPoint.parent.TransformVector(OffsetPoint.localPosition);
                    offsetRotation = Quaternion.Inverse(OffsetPoint.localRotation); //OffsetPoint.localRotation * Quaternion.AngleAxis(180, Vector3.up);
                }

                ModelRoot.position = newPos + offset + OffsetTranslation;
                ModelRoot.rotation = newRot * offsetRotation;
            }
        }

        public override (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handedness)
        {           
            if (!ShowHands)
                return ("Hidden", -1f);

            switch (newState)
            {
                case ViewModelState.Idle:
                    return string.IsNullOrEmpty(HandAnimations.IdleAnim) ? ("Idle", -1) : (HandAnimations.IdleAnim, -1);
                case ViewModelState.Raise:
                    return (HandAnimations.RaiseAnim, HandAnimations.RaiseAnimDuration);
                case ViewModelState.Lower:
                    return (HandAnimations.LowerAnim, HandAnimations.LowerAnimDuration);
                case ViewModelState.Block:
                    throw new NotImplementedException();
                case ViewModelState.Charge:
                    throw new NotImplementedException();
                case ViewModelState.Fire:
                    return (HandAnimations.AttackAnim, HandAnimations.AttackAnimDuration);
                default:
                    Debug.LogWarning($"Tried to get hand state {newState} from {name} which is not supported for {nameof(MeleeWeaponViewModelScript)}");
                    return ("Idle", -1);
            }
        }

        public override void SetState(ViewModelState newState, ViewModelHandednessState handedness, float timeScale)
        {
            if(Mode == MeleeWeaponViewModelMode.ExplicitAnimation || Mode == MeleeWeaponViewModelMode.GlueAndAnimate)
            {
                ModelAnimator.speed =  1f / timeScale;

                switch (newState)
                {
                    case ViewModelState.Idle:
                        ModelAnimator.Play("Idle");
                        break;
                    case ViewModelState.Raise:
                        ModelAnimator.Play("Raise");
                        break;
                    case ViewModelState.Lower:
                        ModelAnimator.Play("Lower");
                        break;
                    case ViewModelState.Block:
                        //TODO
                        break;
                    case ViewModelState.Charge:
                        //TODO
                        break;
                    case ViewModelState.Fire:
                        ModelAnimator.Play("Attack");
                        break;
                    default:
                        Debug.LogWarning($"Tried to put {name} into state {newState} which is not supported for {nameof(MeleeWeaponViewModelScript)}");
                        ModelAnimator.Play("Idle");
                        break;
                }
            }

            //we don't animate in other modes but we still play sounds
            switch (newState)
            {
                case ViewModelState.Raise: //TODO will probably have to rethink raise and lower sounds
                    RaiseSound.Ref()?.Play();
                    break;
                case ViewModelState.Lower:
                    LowerSound.Ref()?.Play();
                    break;
                case ViewModelState.Fire:
                    AttackSound.Ref()?.Play();
                    break;
            }
        }

        public override void SetVisibility(bool visible)
        {
            ModelRoot.gameObject.SetActive(visible);
        }

        
    }

    public enum MeleeWeaponViewModelMode
    {
        None, GlueToBone, ExplicitAnimation, GlueAndAnimate
    }

    [Serializable]
    public struct MeleeWeaponViewModelAnimations
    {
        public string IdleAnim;

        public string AttackAnim;
        public float AttackAnimDuration;

        public string RaiseAnim;
        public float RaiseAnimDuration;

        public string LowerAnim;
        public float LowerAnimDuration;
    }
}