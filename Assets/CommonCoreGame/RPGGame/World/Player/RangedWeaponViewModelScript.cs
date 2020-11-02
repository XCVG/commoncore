using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{

    public class RangedWeaponViewModelScript : WeaponViewModelScript
    {
        [Header("Ranged View Model Options"), SerializeField, Tooltip("In ADS, will copy the relative position and rotation of this transform")]
        private Transform ADSOffsetPoint = null;
        [SerializeField]
        private Transform ModelTransform = null;
        [SerializeField]
        private Animator ModelAnimator = null;
        [SerializeField, Tooltip("If set, will reposition model to center point in ADS")]
        private bool CenterModelInAds = true;
        [SerializeField, Tooltip("Will have no effect if ADS enter/leave animations work")]
        private float ADSTransitionTime = 0.1f;
        [SerializeField]
        private bool HandleCrosshair = false;

        //separate into another component?
        [Header("Hand Animations")]
        public RangedWeaponViewModelAnimations TwoHandAnimations = default;
        public RangedWeaponViewModelAnimations OneHandAnimations = default;
        public RangedWeaponViewModelAnimations ADSAnimations = default;

        //TODO 1hand, ADS, etc

        //TODO sounds, effects, etc
        [Header("Sounds")]
        public AudioSource FireSound = null;
        public AudioSource ReloadSound = null;
        public AudioSource RaiseSound = null;
        public AudioSource LowerSound = null;

        [Header("Effects")]
        public ParticleSystem FireParticleSystem = null;
        public Light FireLight = null;
        public float FireLightDuration = 0;
        [Tooltip("The rotation of this transform will be used for the shell. The direction of its first child will be used as the ejection vector.")]
        public Transform ShellEjectPoint = null;        
        public string ShellPrefab = default;

        private PlayerWeaponComponent WeaponComponent { get { if (_weaponComponent == null) _weaponComponent = GetComponentInParent<PlayerWeaponComponent>(); return _weaponComponent; } }
        private PlayerWeaponComponent _weaponComponent; //cached

        private Coroutine LightFlashCoroutine;
        private Coroutine ADSTransitionCoroutine;
        private Vector3 ModelDefaultPosition; //in local space
        private Quaternion ModelDefaultRotation; //in local space
        private bool InADS;

        public override bool ViewHandlesCrosshair => HandleCrosshair;

        protected override void Start()
        {
            //WIP offsets/positioning
            ModelDefaultPosition = ModelTransform.localPosition;
            ModelDefaultRotation = ModelTransform.localRotation;
        }

        protected override void Update()
        {
            
        }

        public override void SetState(ViewModelState newState, ViewModelHandednessState handednessState, float timeScale)
        {
            if(ModelAnimator == null)
            {
                Debug.LogWarning($"{nameof(RangedWeaponViewModelScript)} on {name} has no {nameof(ModelAnimator)} attached!");
                return;
            }

            //TODO moving the model when entering and exiting ADS

            bool wasInADS = InADS;

            string prefix;
            switch (handednessState)
            {
                case ViewModelHandednessState.TwoHanded:
                    prefix = ""; //no prefix
                    InADS = false;
                    break;
                case ViewModelHandednessState.OneHanded:
                    prefix = "OneHand";
                    InADS = false;
                    break;
                case ViewModelHandednessState.ADS:
                    prefix = "ADS";
                    InADS = true;
                    break;
                default:
                    Debug.LogWarning($"Tried to put {name} into animation with handedness \"{handednessState}\" which is not supported for {nameof(RangedWeaponViewModelScript)}");
                    ModelAnimator.Play("Idle");
                    return;
            }

            //ADS<->non-ADS transitions; no support for animations yet
            if (CenterModelInAds)
            {
                if (wasInADS && !InADS)
                {
                    //move from ADS to not ADS
   
                    InterpolateADSTransition(ModelDefaultPosition, ModelDefaultRotation);
                }
                else if (!wasInADS && InADS)
                {
                    //move from not ADS to ADS
                    Vector3 posTargetWorld = WeaponComponent.CenterViewModelPoint.position;
                    Vector3 vecOffsetToOrigin = ModelTransform.localPosition - ADSOffsetPoint.localPosition;
                    ModelTransform.position = posTargetWorld + WeaponComponent.CenterViewModelPoint.TransformVector(ADSOffsetPoint.localPosition);
                    Vector3 targetPosition = ModelTransform.localPosition;
                    ModelTransform.localPosition = ModelDefaultPosition;

                    ModelTransform.rotation = WeaponComponent.CenterViewModelPoint.rotation;
                    Quaternion rotCenterPlusOffset = ModelTransform.localRotation * ADSOffsetPoint.localRotation;
                    ModelTransform.localRotation = rotCenterPlusOffset;
                    Quaternion targetRotation = ModelTransform.localRotation;
                    ModelTransform.localRotation = ModelDefaultRotation;

                    InterpolateADSTransition(targetPosition, targetRotation);
                }
            }

            ModelAnimator.speed = 1f / timeScale;

            switch (newState)
            {
                case ViewModelState.Idle:
                    ModelAnimator.Play(prefix + "Idle");
                    break;
                case ViewModelState.Raise:
                    ModelAnimator.Play(prefix + "Raise");
                    RaiseSound.Ref()?.Play();
                    break;
                case ViewModelState.Lower:
                    ModelAnimator.Play(prefix + "Lower");
                    LowerSound.Ref()?.Play();
                    break;
                case ViewModelState.Reload:
                    ModelAnimator.Play(prefix + "Reload");
                    if(ReloadSound != null)
                    {
                        ReloadSound.pitch = 1f / timeScale;
                        ReloadSound.Play();
                    }                    
                    break;
                case ViewModelState.Charge:
                    ModelAnimator.Play(prefix + "Charge");
                    break;
                case ViewModelState.Fire:
                    ModelAnimator.Play(prefix + "Fire");                    
                    FireSound.Ref()?.Play();
                    FireParticleSystem.Ref()?.Play();
                    FlashFireLight();
                    EjectShell();
                    break;
                case ViewModelState.Recock:
                    ModelAnimator.Play(prefix + "Recock");
                    break;
                default:
                    Debug.LogWarning($"Tried to put {name} into state {newState} which is not supported for {nameof(RangedWeaponViewModelScript)}");
                    ModelAnimator.Play("Idle");
                    break;
            }
        }

        public override (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handednessState)
        {
            RangedWeaponViewModelAnimations animSet; //TODO handle 2hand, 1hand, ADS

            switch (handednessState)
            {
                case ViewModelHandednessState.OneHanded:
                    animSet = OneHandAnimations;
                    break;
                case ViewModelHandednessState.TwoHanded:
                    animSet = TwoHandAnimations;
                    break;
                case ViewModelHandednessState.ADS:
                    animSet = ADSAnimations;
                    break;
                default:
                    Debug.LogWarning($"Tried to get animation for \"{handednessState}\" handedness from {name} which is not supported for {nameof(RangedWeaponViewModelScript)}");
                    return ("Idle", -1);
            }

            switch (newState)
            {
                case ViewModelState.Idle:
                    return string.IsNullOrEmpty(animSet.IdleAnim) ? ("Idle", -1) : (animSet.IdleAnim, -1);
                case ViewModelState.Raise:
                    return (animSet.RaiseAnim, animSet.RaiseAnimDuration);
                case ViewModelState.Lower:
                    return (animSet.LowerAnim, animSet.LowerAnimDuration);
                case ViewModelState.Reload:
                    return (animSet.ReloadAnim, animSet.ReloadAnimDuration);
                case ViewModelState.Charge:
                    return (animSet.ChargeAnim, animSet.ChargeAnimDuration);
                case ViewModelState.Fire:
                    return (animSet.FireAnim, animSet.FireAnimDuration);
                case ViewModelState.Recock:
                    return (animSet.RecockAnim, animSet.RecockAnimDuration);
                default:
                    Debug.LogWarning($"Tried to get hand state {newState} from {name} which is not supported for {nameof(RangedWeaponViewModelScript)}");
                    return ("Idle", -1);
            }
        }

        public override void SetVisibility(bool visible)
        {
            ModelAnimator.gameObject.SetActive(visible);
        }

        private void EjectShell()
        {
            if(ShellEjectPoint == null || ShellPrefab == null ||ShellEjectPoint.childCount == 0)
            {
                //can't eject shell
                return;
            }

            Transform shellDirTransform = ShellEjectPoint.GetChild(0);
            ShellEjectionComponent shellEjectionComponent = ShellEjectPoint.GetComponent<ShellEjectionComponent>();

            //var shell = Instantiate(ShellPrefab, ShellEjectPoint.position, ShellEjectPoint.rotation, CoreUtils.GetWorldRoot());
            var shell = WorldUtils.SpawnEffect(ShellPrefab, ShellEjectPoint.position, ShellEjectPoint.rotation.eulerAngles, CoreUtils.GetWorldRoot());

            if (shell == null)
                return;

            //shell parameters (use ShellEjectionComponent if available)
            float shellScale;
            float shellVelocity;
            float shellTorque;
            float shellRandomVelocity;
            float shellRandomTorque;

            if(shellEjectionComponent)
            {
                shellScale = shellEjectionComponent.ShellScale;
                shellVelocity = shellEjectionComponent.ShellVelocity;
                shellTorque = shellEjectionComponent.ShellTorque;
                shellRandomVelocity = shellEjectionComponent.ShellRandomVelocity;
                shellRandomTorque = shellEjectionComponent.ShellRandomTorque;
            }
            else
            {
                //legacy stupid hacky shit

                shellScale = shellDirTransform.localScale.x;
                shellVelocity = shellDirTransform.localScale.z;
                shellTorque = shellDirTransform.localScale.y;

                shellRandomVelocity = 0;
                shellRandomTorque = 0;
            }

            //scale the shell, make it move
            shell.transform.localScale = Vector3.one * shellScale;
            var shellRB = shell.GetComponent<Rigidbody>();
            if(shellRB != null)
            {
                Vector3 velocityDirection = shellDirTransform.forward;

                Vector3 playerVelocity = WeaponComponent.PlayerController.MovementComponent.Velocity;
                Vector3 randomVelocity = new Vector3(UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity, UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity, UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity);

                Vector3 velocity = velocityDirection * shellVelocity;
                shellRB.AddForce(velocity + playerVelocity + randomVelocity, ForceMode.VelocityChange);

                Vector3 randomTorque = new Vector3(UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque, UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque, UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque);

                shellRB.AddTorque(velocity * shellTorque, ForceMode.VelocityChange);
            }
        }

        private void FlashFireLight()
        {
            if (FireLight == null)
                return;

            FireLight.gameObject.SetActive(true);

            if (LightFlashCoroutine != null)
                StopCoroutine(LightFlashCoroutine);
            LightFlashCoroutine = StartCoroutine(FireLightCoroutine());
        }

        private IEnumerator FireLightCoroutine()
        {
            yield return new WaitForSeconds(FireLightDuration);
            FireLight.gameObject.SetActive(false);            
        }

        private void InterpolateADSTransition(Vector3 targetPosition, Quaternion targetRotation)
        {
            if(Mathf.Approximately(ADSTransitionTime, 0))
            {
                ModelTransform.localPosition = targetPosition;
                ModelTransform.localRotation = targetRotation;
                return;
            }

            if (ADSTransitionCoroutine != null)
                StopCoroutine(ADSTransitionCoroutine);

            ADSTransitionCoroutine = StartCoroutine(TransitionADSCoroutine(targetPosition, targetRotation));
        }

        private IEnumerator TransitionADSCoroutine(Vector3 targetPosition, Quaternion targetRotation)
        {
            Vector3 oldPosition = ModelTransform.localPosition;

            for(float f = 0; f <= ADSTransitionTime; )
            {
                Vector3 pos = Vector3.Lerp(oldPosition, targetPosition, f / ADSTransitionTime);

                ModelTransform.localPosition = pos;

                f += Time.deltaTime;
                yield return null;
            }

            ModelTransform.localPosition = targetPosition;
            ModelTransform.localRotation = targetRotation;
        }

        public bool HasADSEnterAnim => !string.IsNullOrEmpty(ADSAnimations.RaiseAnim);
        public bool HasADSExitAnim => !string.IsNullOrEmpty(ADSAnimations.LowerAnim);
        
    }

    [Serializable]
    public struct RangedWeaponViewModelAnimations
    {
        public string IdleAnim;

        public string ChargeAnim;
        public float ChargeAnimDuration;

        public string FireAnim;
        public float FireAnimDuration;

        public string ReloadAnim;
        public float ReloadAnimDuration;

        public string RecockAnim;
        public float RecockAnimDuration;

        public string RaiseAnim;
        public float RaiseAnimDuration;

        public string LowerAnim;
        public float LowerAnimDuration;
    }

}