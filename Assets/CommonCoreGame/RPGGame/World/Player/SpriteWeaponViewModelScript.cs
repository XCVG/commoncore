using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.RpgGame.World
{

    public class SpriteWeaponViewModelScript : WeaponViewModelScript, IWeaponViewModelCanHaveADSAnimations
    {
        [SerializeField, Header("Components")]
        private Image WeaponImage = null;
        [SerializeField]
        private Transform MovebobNode = null;

        [SerializeField, Header("Options")]
        private bool AllowADS = false;
        [SerializeField]
        private bool AllowTimescale = true;
        [SerializeField]
        private bool LoopIdle = true;        
        [SerializeField]
        private bool AutoTransition = true; //automatically go from fire->idle, raise->idle, reload->idle, etc
        [SerializeField]
        private bool HandleCrosshair = false;
        [SerializeField]
        private ViewModelWaitForLockTime EffectWaitForLockTime = ViewModelWaitForLockTime.Unspecified;
        [SerializeField]
        private bool EjectShellOnRecock = false;
        [SerializeField, Tooltip("If set, binds the canvas used by this weapon to the view model camera (note that this is actually faked)")]
        private bool BindCanvasToCamera = true;

        [SerializeField, Header("Lighting Options")]
        private bool ApplyReportedLighting = true;
        [SerializeField]
        private float ReportedLightingBias = 1.0f;

        [SerializeField, Header("Movebob Options")]
        private bool AllowMovebob = true;
        [SerializeField]
        private bool CopyMovebob = true;
        [SerializeField]
        private float MovebobYOffset = 0;
        [SerializeField]
        private float MovebobMultiplier = 0.1f;

        //Idle, Raise, Lower, Block, Reload, Charge, Fire, Recock
        [SerializeField, Header("Normal Frames")]
        private WeaponFrame[] Idle = null;
        [SerializeField]
        private WeaponFrame[] Raise = null;
        [SerializeField]
        private WeaponFrame[] Lower = null;
        [SerializeField]
        private WeaponFrame[] Reload = null;
        [SerializeField]
        private WeaponFrame[] Recock = null;
        [SerializeField]
        private WeaponFrame[] Fire = null;

        [SerializeField, Header("ADS Frames")]
        private WeaponFrame[] ADSIdle = null;
        [SerializeField]
        private WeaponFrame[] ADSRaise = null;
        [SerializeField]
        private WeaponFrame[] ADSLower = null;
        [SerializeField]
        private WeaponFrame[] ADSFire = null;
        [SerializeField]
        private WeaponFrame[] ADSRecock = null;

        [SerializeField, Header("Sounds")]
        private AudioSource FireSound = null;
        [SerializeField]
        private bool RepeatFireSound = false;
        [SerializeField]
        private AudioSource ReloadSound = null;
        [SerializeField]
        private AudioSource RecockSound = null;
        [SerializeField]
        private AudioSource RaiseSound = null;
        [SerializeField]
        private AudioSource LowerSound = null;
        [SerializeField]
        private AudioSource ADSRaiseSound = null;
        [SerializeField]
        private AudioSource ADSLowerSound = null;

        [SerializeField, Header("Effects"), Tooltip("The rotation of this transform will be used for the shell. The direction of its first child will be used as the ejection vector.")]
        private Transform ShellEjectPoint = null;
        [SerializeField]
        private string ShellPrefab = default;
        [SerializeField]
        private Transform FireEffectPoint = null;
        [SerializeField]
        private string FireEffectPrefab = null;
        [SerializeField]
        private Transform RecockEffectPoint = null;
        [SerializeField]
        private string RecockEffectPrefab = null;

        [SerializeField, Header("Reload Effects"), Tooltip("The rotation of this transform will be used for the magazine. The direction of its first child will be used as the ejection vector.")]
        private Transform MagazineEjectPoint = null;
        [SerializeField]
        private string MagazinePrefab = default;
        [SerializeField]
        private float MagazineEjectDelay = 0;
        [SerializeField]
        private Transform ReloadEffectPoint = null;
        [SerializeField]
        private string ReloadEffectPrefab = null;

        [SerializeField, Header("Crosshair")]
        private GameObject NormalCrosshair;
        [SerializeField]
        private GameObject AdsCrosshair;
        [SerializeField, Tooltip("If set, will always show ADS crosshair even if game and weapon settings have crosshair disabled")]
        private bool ForceAdsCrosshair = true;
        [SerializeField, Tooltip("If set, will fall back to normal crosshair in ADS mode if ADS crosshair is not available")]
        private bool FallbackToNormalCrosshair;

        //state
        private WeaponFrame[] CurrentFrameSet;
        private int CurrentFrameIndex;
        private float Timescale;
        private float TimeInFrame;

        private bool MovebobCriticalError;
        private bool Fullbright;

        private Canvas WeaponImageCanvas;
        private Camera AttachedCamera;

        private CrosshairState CrosshairState;

        private Coroutine EffectDelayedCoroutine;

        private QdmsMessageInterface MessageInterface;

        public override bool ViewHandlesCrosshair => HandleCrosshair;

        public bool HasADSEnterAnim => ADSRaise != null && ADSRaise.Length > 0;

        public bool HasADSExitAnim => ADSLower != null && ADSLower.Length > 0;

        protected override void Start()
        {
            //throw new System.NotImplementedException();
        }

        public override void Init(ViewModelOptions options)
        {
            base.Init(options);

            MessageInterface = new QdmsMessageInterface(this);
            MessageInterface.SubscribeReceiver(HandleMessage);

            WeaponImageCanvas = WeaponImage.canvas;
            CrosshairState = ConfigState.Instance.GetGameplayConfig().Crosshair;

            if (BindCanvasToCamera)
            {
                BindToCamera();
            }
                  
        }

        protected override void Update()
        {
            HandleCameraAttachment();
            HandleAnimation();
            HandleMovebob();
            HandleLighting();
            HandleCrosshairUpdate();
        }

        private void HandleMessage(QdmsMessage message)
        {
            if(message is QdmsFlagMessage flagMessage && flagMessage.Flag == "ConfigChanged")
                CrosshairState = ConfigState.Instance.GetGameplayConfig().Crosshair;
        }

        private void HandleCameraAttachment()
        {
            //this fakes the canvas being attached to the camera for state etc

            if(BindCanvasToCamera && AttachedCamera != null)
            {
                bool cameraActive = AttachedCamera.isActiveAndEnabled;
                if (cameraActive && !WeaponImageCanvas.enabled)
                {
                    WeaponImageCanvas.enabled = true;
                }
                else if(!cameraActive && WeaponImageCanvas.enabled)
                {
                    WeaponImageCanvas.enabled = false;
                }
            }
        }

        private void HandleAnimation()
        {
            if(WeaponImage == null || CurrentFrameSet == null || CurrentFrameIndex >= CurrentFrameSet.Length)
            {
                //fail. Do nothing.
                return;
            }

            TimeInFrame += Time.deltaTime / Timescale;

            WeaponFrame currentFrame = CurrentFrameSet[CurrentFrameIndex];

            if (TimeInFrame > currentFrame.Duration)
            {
                //advance to the next frame
                CurrentFrameIndex++;
                TimeInFrame = 0;
                if (CurrentFrameIndex == CurrentFrameSet.Length)
                {
                    //we were on the last frame and are now past the end
                    if(LoopIdle && (CurrentFrameSet == Idle || CurrentFrameSet == ADSIdle)) //loop
                    {
                        CurrentFrameIndex = 0;
                    }
                    else if(AutoTransition && CurrentFrameSet == Fire || CurrentFrameSet == Raise || CurrentFrameSet == Reload || CurrentFrameSet == ADSLower) //autotransition (non-ADS)
                    {
                        CurrentFrameSet = Idle;
                        CurrentFrameIndex = 0;
                    }
                    else if(AutoTransition && CurrentFrameSet == ADSFire || CurrentFrameSet == ADSRaise) //autotransition (ADS)
                    {
                        CurrentFrameSet = ADSIdle;
                        CurrentFrameIndex = 0;
                    }
                    else
                    {
                        return; //stop
                    }
                }

                SetImage(CurrentFrameSet, CurrentFrameIndex);
            }


        }

        private void HandleMovebob()
        {
            if (!Options.UseMovebob || !AllowMovebob || MovebobCriticalError)
                return;

            if(CopyMovebob)
            {
                if(Options.MovebobComponent == null || MovebobNode == null)
                {
                    Debug.LogWarning($"Movebob can't work without MovebobComponent and MovebobNode in {nameof(SpriteWeaponViewModelScript)}!");
                    MovebobCriticalError = true;
                    return;
                }

                Vector2 scaledOffset = Options.MovebobComponent.transform.localPosition * MovebobMultiplier;
                MovebobNode.localPosition = new Vector3(scaledOffset.x, scaledOffset.y + MovebobYOffset, MovebobNode.localPosition.z);
            }
            else
            {
                //explicit movebob handling?
                Debug.LogWarning($"Explicit movebob handling is not yet implemented in {nameof(SpriteWeaponViewModelScript)}!");
                MovebobCriticalError = true;
            }
        }

        private void HandleLighting()
        {
            if (!ApplyReportedLighting || Fullbright)
                return;

            var reporter = Options.WeaponComponent.Ref()?.PlayerController.Ref()?.LightReporter;
            if (reporter != null)
            {
                var c = reporter.Light;
                c *= ReportedLightingBias;
                c.a = WeaponImage.color.a;
                WeaponImage.color = c;
            }
        }

        private void HandleCrosshairUpdate()
        {
            if (!HandleCrosshair)
                return;

            bool inAds = (CurrentFrameSet == ADSFire || CurrentFrameSet == ADSIdle || CurrentFrameSet == ADSRecock);

            bool showNormalCrosshair = false, showAdsCrosshair = false;

            if (inAds && (ForceAdsCrosshair || CrosshairState == CrosshairState.Always || (CrosshairState == CrosshairState.Auto && Options.AdsCrosshair)))
            {
                showAdsCrosshair = true;
            }
            else if(!inAds && CrosshairState != CrosshairState.Never && (Options.UseCrosshair || CrosshairState == CrosshairState.Always))
            {
                showNormalCrosshair = true;
            }

            if(showAdsCrosshair && AdsCrosshair == null && FallbackToNormalCrosshair)
            {
                showAdsCrosshair = false;
                showNormalCrosshair = true;
            }

            if(AdsCrosshair != null)
            {
                if (showAdsCrosshair && !AdsCrosshair.activeSelf)
                {
                    AdsCrosshair.SetActive(true);
                }
                else if(!showAdsCrosshair && AdsCrosshair.activeSelf)
                {
                    AdsCrosshair.SetActive(false);
                }
            }

            if (NormalCrosshair != null)
            {
                if (showNormalCrosshair && !NormalCrosshair.activeSelf)
                {
                    NormalCrosshair.SetActive(true);
                }
                else if (!showNormalCrosshair && NormalCrosshair.activeSelf)
                {
                    NormalCrosshair.SetActive(false);
                }
            }


        }

        public override (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handedness)
        {
            return (HandsHidden, -1); //always hide hands
        }

        public override void SetState(ViewModelState newState, ViewModelHandednessState handedness, float timeScale)
        {
            if(handedness == ViewModelHandednessState.ADS && AllowADS)
            {
                switch (newState)
                {
                    case ViewModelState.Idle:
                        CurrentFrameSet = ADSIdle;
                        break;
                    case ViewModelState.Raise:
                        CurrentFrameSet = ADSRaise;
                        ADSRaiseSound.Ref()?.Play();
                        break;
                    case ViewModelState.Lower:
                        CurrentFrameSet = ADSLower;
                        ADSLowerSound.Ref()?.Play();
                        break;
                    case ViewModelState.Fire:
                        CurrentFrameSet = ADSFire;
                        PlayFireEffects();
                        break;
                    case ViewModelState.Recock:
                        CurrentFrameSet = ADSRecock;
                        PlayRecockEffects();
                        break;
                    default:
                        Debug.LogWarning($"Tried to put {name} into state {newState} which is not supported for {nameof(SpriteWeaponViewModelScript)}");
                        break;
                }
            }
            else
            {
                if (handedness == ViewModelHandednessState.ADS)
                    Debug.LogWarning($"Tried to set {nameof(SpriteWeaponViewModelScript)} to ADS frames which is disabled on {name}");

                switch (newState)
                {
                    case ViewModelState.Idle:
                        CurrentFrameSet = Idle;
                        break;
                    case ViewModelState.Raise:
                        CurrentFrameSet = Raise;
                        RaiseSound.Ref()?.Play();
                        break;
                    case ViewModelState.Lower:
                        CurrentFrameSet = Lower;
                        LowerSound.Ref()?.Play();
                        break;
                    case ViewModelState.Reload:
                        CurrentFrameSet = Reload;
                        ReloadSound.Ref()?.Play();
                        PlayReloadEffects();
                        break;
                    case ViewModelState.Fire:
                        CurrentFrameSet = Fire;
                        PlayFireEffects();
                        break;
                    case ViewModelState.Recock:
                        CurrentFrameSet = Recock;
                        PlayRecockEffects();
                        break;
                    default:
                        Debug.LogWarning($"Tried to put {name} into state {newState} which is not supported for {nameof(SpriteWeaponViewModelScript)}");
                        break;
                }
            }

            CurrentFrameIndex = 0;
            Timescale = AllowTimescale ? timeScale : 1;
            TimeInFrame = 0;

            SetImage(CurrentFrameSet, CurrentFrameIndex);
        }

        public override void SetVisibility(bool visible)
        {
            WeaponImage.gameObject.SetActive(visible);
        }

        private void PlayFireEffects()
        {
            if (Options.LockTime > 0 && ((Options.EffectWaitsForLockTime && EffectWaitForLockTime != ViewModelWaitForLockTime.Never) || EffectWaitForLockTime == ViewModelWaitForLockTime.Always))
            {
                EffectDelayedCoroutine = StartCoroutine(CoDelayedEffect(Options.LockTime, playFireEffects));
            }
            else
            {
                playFireEffects();
            }

            void playFireEffects()
            {
                if(FireSound != null)
                {
                    if(RepeatFireSound)
                    {
                        FireSound.PlayOneShot(FireSound.clip);
                    }
                    else
                    {
                        FireSound.Play();
                    }
                }
                
                if(!EjectShellOnRecock)
                    ViewModelUtils.EjectShell(ShellEjectPoint, ShellPrefab, Options.WeaponComponent);
                InstantiateFireEffect();
            }

        }

        private void PlayRecockEffects()
        {
            RecockSound.Ref()?.Play();

            if (!string.IsNullOrEmpty(RecockEffectPrefab))
            {
                var t = RecockEffectPoint.Ref() ?? transform;
                WorldUtils.SpawnEffect(RecockEffectPrefab, t.position, t.rotation, t, false);
            }

            if (EjectShellOnRecock)
                ViewModelUtils.EjectShell(ShellEjectPoint, ShellPrefab, Options.WeaponComponent);
        }

        private void PlayReloadEffects()
        {
            if (!string.IsNullOrEmpty(ReloadEffectPrefab))
            {
                var t = ReloadEffectPoint.Ref() ?? transform;
                WorldUtils.SpawnEffect(ReloadEffectPrefab, t.position, t.rotation, t, false);
            }

            if (!string.IsNullOrEmpty(MagazinePrefab))
            {
                if (MagazineEjectDelay > 0)
                {
                    StartCoroutine(CoDelayedEffect(MagazineEjectDelay, () => { ViewModelUtils.EjectShell(MagazineEjectPoint.Ref() ?? transform, MagazinePrefab, Options.WeaponComponent); }));
                }
                else
                {
                    ViewModelUtils.EjectShell(MagazineEjectPoint.Ref() ?? transform, MagazinePrefab, Options.WeaponComponent);
                }
            }

        }

        private void InstantiateFireEffect()
        {
            if (!string.IsNullOrEmpty(FireEffectPrefab))
            {
                var t = FireEffectPoint.Ref() ?? transform;
                WorldUtils.SpawnEffect(FireEffectPrefab, t.position, t.rotation, t, false);
            }
        }

        private void SetImage(WeaponFrame[] frameSet, int frameIndex)
        {
            if (WeaponImage == null)
            {
                Debug.LogError($"Can't set image on {name} because the image component is missing or not assigned!");
                return;
            }

            if (frameSet == null || frameSet.Length <= frameIndex || frameIndex < 0)
            {
                Debug.LogWarning($"Can't set image on {name} because frame set doesn't have index {frameIndex}!"); //not a great error message but oh well
                return;
            }

            var frame = frameSet[frameIndex];
            var sprite = frame.Sprite;
            if(sprite == null)
            {
                Debug.LogWarning($"Can't set image on {name} because frame struct exists but sprite is null!");
                return;
            }

            WeaponImage.sprite = sprite;

            float spriteScale = (100f / sprite.pixelsPerUnit);
            float spriteWidth = sprite.texture.width * spriteScale;
            float spriteHeight = sprite.texture.height * spriteScale;

            //sprite.pivot is actually an offset from the bottom-left in pixels
            float spriteXOffset = -((sprite.pivot.x * spriteScale) - (spriteWidth / 2f));
            float spriteYOffset = -((sprite.pivot.y * spriteScale) - (spriteHeight / 2f));

            WeaponImage.rectTransform.sizeDelta = new Vector2(spriteWidth, spriteHeight);
            WeaponImage.rectTransform.anchoredPosition = new Vector2(spriteXOffset, spriteYOffset);

            Fullbright = frame.Bright;
            if (Fullbright && ApplyReportedLighting)
            {
                var c = Color.white;
                c.a = WeaponImage.color.a;
                WeaponImage.color = c;
            }
            else
            {
                HandleLighting();
            }

        }

        private IEnumerator CoDelayedEffect(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action();
        }

        private void BindToCamera()
        {
            var cameraRoot = Options.WeaponComponent.PlayerController.CameraRoot;
            var viewModelCamera = cameraRoot.Find("ViewBobNode").FindDeepChild("FP Camera").GetChild(0).GetComponent<Camera>();

            AttachedCamera = viewModelCamera;
        }


        [Serializable]
        public struct WeaponFrame
        {
            public Sprite Sprite;
            public float Duration;
            public bool Bright;
        }
        
    }
}