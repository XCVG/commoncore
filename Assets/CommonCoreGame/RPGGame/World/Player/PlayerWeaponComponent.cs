using CommonCore.Config;
using CommonCore.Input;
using CommonCore.LockPause;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CommonCore.World;
using CommonCore.Scripting;
using System.Linq;

namespace CommonCore.RpgGame.World
{
    public class PlayerWeaponComponent : MonoBehaviour
    {
        [Header("Components")]
        public PlayerController PlayerController;       
        public Transform LeftViewModelPoint;
        public Transform CenterViewModelPoint;
        public Transform RightViewModelPoint;
        public Transform WeaponBobNode;
        public Transform ShootPointNear;
        public Transform ShootPointFar;
        public WeaponMovebobComponent MovebobComponent;

        [SerializeField, Header("Hands")]
        private WeaponHandModelScript Hands;
        public WeaponHandModelScript HandModel => Hands;


        [SerializeField, Header("Recoil Shake")]
        private WeaponViewShakeScript ViewShakeScript;
        [SerializeField, Tooltip("Contribution of actual fire vector to recoil shake angle")]
        private float RecoilFireVecFactor = 0.2f;

        [SerializeField, Header("Fallback Weapon")]
        private WeaponViewModelScript FallbackViewModel = null;
        [SerializeField]
        private string FallbackItemModel = string.Empty;

        private MeleeWeaponItemModel FallbackWeapon => InventoryModel.GetModel(FallbackItemModel) as MeleeWeaponItemModel;

        [SerializeField, Header("Offhand Kick")] //TODO ought to move this into another component
        private bool OffhandKickEnabled = true;
        [SerializeField]
        private Animator OffhandKickAnimator = null;
        [SerializeField]
        private Transform OffhandKickPoint = null;
        [SerializeField]
        private float OffhandKickRange = 1.5f;
        [SerializeField]
        private float OffhandKickDamage = 10f; //TODO move to stats and stuff
        [SerializeField]
        private BuiltinHitFlags[] OffhandKickFlags;
        [SerializeField]
        private float OffhandKickDelay = 1f;
        [SerializeField]
        private float OffhandKickForce = 1000f;
        [SerializeField]
        private AudioSource OffhandKickSound = null;
        [SerializeField]
        private string OffhandKickPuff;      

        [Header("Params")]
        public float MeleeProbeDist = 3.0f;
        public float MeleeBoxCastSize = 0.5f;
        public float MeleeContactHitRadius = 0.5f;
        public float MeleeContactHitHeight = 1.5f;
        public float MeleeContactMaxAngle = 45f;
        public float ADSZoomFadeTime = 0.2f;

        [Header("Autoaim Options")]
        public float AutoaimWeakCastSize = 0.25f;
        public float AutoaimStrongCastSize = 1.0f;
        public float AutoaimCastRange = 100f;

        public WeaponViewModelScript LeftViewModel { get; private set; }
        public WeaponViewModelScript RightViewModel { get; private set; }

        private float TimeToNext;
        public bool IsReloading { get; private set; }
        public bool IsADS { get; private set; }

        public bool ShouldBeVisible { get; private set; }
        public bool IsVisible { get; private set; }

        private float AccumulatedSpread;
        private float AccumulatedRecoil;
        private bool DidJustFire;
        private int BurstCount;
        private float PendingRecockTime;
        private bool PendingADSExit;
        private WeaponTransitionState TransitionState;
        private float OldWeaponLowerTime; //need to save this because we lose the item model before transition
        private bool OldWeaponNoLowerAnimation;

        //pending firing (lock time)
        private Action DelayedFiringAction;
        private float DelayedFiringTimeRemaining;

        //offhand kick
        private float TimeToNextKick;

        //plan is to just bodge this for now, giving up on dual-wielding support, and add proper dual-wielding later

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {

            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            HandleDelayedFiring();
            HandleAccumulators();

            DidJustFire = false;

            HandleWeapons();
            HandleOffhandKick();

        }

        /// <summary>
        /// Sets the visibility of weapons and hands
        /// </summary>
        public void SetVisibility(bool visibility)
        {
            ShouldBeVisible = visibility;
            SetModelsVisibility(visibility);
        }

        //handles actually setting the visibility of weapons and hands
        private void SetModelsVisibility(bool visibility)
        {
            IsVisible = visibility;
            LeftViewModel.Ref()?.SetVisibility(visibility);
            RightViewModel.Ref()?.SetVisibility(visibility);
            Hands.Ref()?.SetVisibility(visibility);

            //force offhand kick to disappear
            if (!visibility)
                OffhandKickAnimator.Ref()?.gameObject.SetActive(false);
        }

        /// <summary>
        /// Requests an exit from ADS mode at the next available opportunity
        /// </summary>
        public void RequestADSExit()
        {
            if(IsADS)
                PendingADSExit = true;
        }

        /// <summary>
        /// Requests that the weapon be hidden at the next available opportunity
        /// </summary>
        public void RequestHideWeapon()
        {
            //this thunk should just work
            SetVisibility(false);
        }

        /// <summary>
        /// Handle delayed firing (ie lock time)
        /// </summary>
        protected void HandleDelayedFiring()
        {
            if (DelayedFiringAction == null)
                return;

            DelayedFiringTimeRemaining -= Time.deltaTime;
            if(DelayedFiringTimeRemaining <= 0)
            {
                DelayedFiringAction();

                DelayedFiringAction = null;
                DelayedFiringTimeRemaining = 0;
            }
        }

        /// <summary>
        /// Handle decay of accumulators (recoil/spread) every frame
        /// </summary>
        protected void HandleAccumulators()
        {
            if (DidJustFire || TimeToNext > 0)
                return;

            if (!GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon))
                return;

            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon]?.ItemModel;
            if(rightWeaponModel != null && rightWeaponModel is RangedWeaponItemModel rwim)
            {
                float decayMoveFactor = 1.0f;
                if(PlayerController.MovementComponent.IsMoving)
                {
                    decayMoveFactor = rwim.MovementRecoveryFactor;
                    if (rwim.CheckFlag(ItemFlag.WeaponProportionalMovement) && PlayerController.MovementComponent.IsRunning)
                        decayMoveFactor /= 2f;
                    if (PlayerController.MovementComponent.IsCrouching)
                        decayMoveFactor *= rwim.CrouchRecoveryFactor;
                }

                float decayRpgFactor = RpgValues.GetWeaponRecoveryFactor(GameState.Instance.PlayerRpgState, rwim);

                if(AccumulatedRecoil > 0)
                {
                    RangeEnvelope recoilEnvelope = IsADS ? rwim.ADSRecoil : rwim.Recoil;
                    AccumulatedRecoil = Mathf.Max(recoilEnvelope.Min, AccumulatedRecoil - (recoilEnvelope.Decay * decayMoveFactor * decayRpgFactor * Time.deltaTime));
                }

                if(AccumulatedSpread > 0)
                {
                    RangeEnvelope spreadEnvelope = IsADS ? rwim.ADSSpread : rwim.Spread;
                    AccumulatedSpread = Mathf.Max(spreadEnvelope.Min, AccumulatedSpread - (spreadEnvelope.Decay * decayMoveFactor * decayRpgFactor * Time.deltaTime));
                }
            }
        }

        /// <summary>
        /// Reset accumulators (recoil/spread) to 0
        /// </summary>
        protected void ResetAccumulators()
        {
            AccumulatedRecoil = 0;
            AccumulatedSpread = 0;
        }

        /// <summary>
        /// Rescales accumulators when entering or exiting ADS
        /// </summary>
        protected void RescaleAccumulators(bool enteringADS)
        {
            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon]?.ItemModel;
            if (rightWeaponModel != null && rightWeaponModel is RangedWeaponItemModel rwim)
            {
                float recoilRatio = AccumulatedRecoil / (enteringADS ? rwim.Recoil.Max : rwim.ADSRecoil.Max);
                if (float.IsNaN(recoilRatio))
                    recoilRatio = 0;
                AccumulatedRecoil = recoilRatio * (enteringADS ? rwim.ADSRecoil.Max : rwim.Recoil.Max);                

                float spreadRatio = AccumulatedSpread / (enteringADS ? rwim.Spread.Max : rwim.ADSSpread.Max);
                if (float.IsNaN(spreadRatio))
                    spreadRatio = 0;
                AccumulatedSpread = spreadRatio * (enteringADS ? rwim.ADSSpread.Max : rwim.Spread.Max);
            }
            else
            {
                ResetAccumulators();
            }
        }

        //handle weapons (very temporary)
        protected void HandleWeapons()
        {
            //handle hiding weapons (super janky)
            if(GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoWeapons))
            {
                if (IsVisible)
                    SetModelsVisibility(false);
            }
            else
            {
                if (ShouldBeVisible && !IsVisible)
                    SetModelsVisibility(true);
                else if (!ShouldBeVisible && IsVisible)
                    SetModelsVisibility(false);
            }

            if(PendingRecockTime > 0)
            {
                HandlePendingRecock();
            }

            //this is completely fuxxored for dual-wielding... actually everything basically assumes one action at a time...
            float oldTTN = TimeToNext;
            TimeToNext -= Time.deltaTime;
            if (TimeToNext > 0)
                return;

            //handle weapon lower and raise
            if(TransitionState != WeaponTransitionState.None)
            {
                //Debug.Log("hit transition state handling");

                if(TransitionState == WeaponTransitionState.Lowering) 
                {
                    //lowering is done at this point, raise the next weapon
                    if (GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon))
                    {
                        //swap the viewmodel, begin animation
                        ClearViewModel(EquipSlot.RightWeapon);
                        SetViewModel(EquipSlot.RightWeapon);
                        HandleCrosshair();

                        var wim = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel as WeaponItemModel;
                                                
                        if (RightViewModel != null)
                        {
                            if (wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower))
                            {
                                //nop; we will handle it ourselves, or not use it at all
                            }
                            else
                            {
                                RightViewModel.SetState(ViewModelState.Raise, ViewModelHandednessState.TwoHanded, 1);
                                Hands.SetState(ViewModelState.Raise, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                            }
                        }
                        else
                        {
                            Hands.SetState(ViewModelState.Idle, null, ViewModelHandednessState.TwoHanded, 1);
                        }
                        TransitionState = WeaponTransitionState.Raising;
                        TimeToNext = wim.RaiseTime;
                    }
                    else
                    {
                        ClearViewModel(EquipSlot.RightWeapon);

                        if (FallbackWeapon != null && FallbackViewModel != null)
                        {
                            TransitionState = WeaponTransitionState.Raising; //raise your fists!
                            Hands.SetState(ViewModelState.Raise, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);
                            TimeToNext = FallbackWeapon.RaiseTime;
                        }
                    }                 

                    return;
                }
                else if(TransitionState == WeaponTransitionState.Raising)
                {
                    //raising done, switch to idle
                    if (GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon))
                    {
                        var wim = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel as WeaponItemModel;

                        if (RightViewModel != null)
                        {
                            if (wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower))
                            {
                                //nop; we will handle it ourselves, or not use it at all
                            }
                            else
                            {
                                RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded, 1);
                                Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                            }
                        }
                        else
                            Hands.SetState(ViewModelState.Idle, null, ViewModelHandednessState.TwoHanded, 1);

                        OldWeaponLowerTime = wim.LowerTime;                        
                    }
                    else
                    {
                        if(FallbackViewModel != null)
                            Hands.SetState(ViewModelState.Idle, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);

                        OldWeaponLowerTime = FallbackWeapon?.LowerTime ?? 0;
                        OldWeaponNoLowerAnimation = false;
                    }
                    TransitionState = WeaponTransitionState.None;
                    return;
                }
                else
                {
                    Debug.LogWarning($"Invalid transition state \"{TransitionState.ToString()}\"");
                    TransitionState = WeaponTransitionState.None;
                    return;
                }
            }

            if (IsReloading)
            {
                FinishReload();
                TryRefire();
            }

            //refire and return-to-idle handling
            if (oldTTN > 0 && TransitionState == WeaponTransitionState.None)
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReady"));

                //TODO handle 1H/2H(?), default

                //note that TryRefire will actually fire the weapon if it succeeds
                if (RightViewModel != null && TryRefire())
                {
                    return;
                }
                else
                {
                    if (RightViewModel != null)
                        RightViewModel.SetState(ViewModelState.Idle, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, 1);
                    //else if (LeftViewModel != null)
                    //    LeftViewModel.SetState(ViewModelState.Idle, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, 1);
                    else if (FallbackViewModel != null && !GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.LeftWeapon))
                        Hands.SetState(ViewModelState.Idle, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);
                    else if (Hands != null)
                        Hands.SetState(ViewModelState.Idle, null, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, 1);
                }
            }

            //input handling
            if (PlayerController.AttackEnabled && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoAttack) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoWeapons) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen) && PlayerController.PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                bool rightEquipped = GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon);
                bool leftEquipped = GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.LeftWeapon);

                WeaponItemModel weaponModel = GameState.Instance.PlayerRpgState.Equipped.GetOrDefault((int)EquipSlot.RightWeapon, null)?.ItemModel as WeaponItemModel;

                if (weaponModel is DummyWeaponItemModel)
                {
                    //nop
                }
                else
                {

                    if (MappedInput.GetButtonDown(DefaultControls.Fire))
                    {
                        //-if left weapon equipped, fire that weapon, otherwise,
                        //-if fallback weapon exists, fire that weapon

                        if (rightEquipped)
                        {
                            BurstCount = 0;

                            //fire right weapon
                            if (weaponModel is RangedWeaponItemModel)
                                DoRangedAttack(EquipSlot.RightWeapon);
                            else
                                DoMeleeAttack(EquipSlot.RightWeapon);
                        }
                        else if (!string.IsNullOrEmpty(FallbackItemModel))
                        {
                            if (FallbackWeapon != null)
                            {
                                DoMeleeAttack(EquipSlot.None);
                            }
                            //else nop, no fallback defined
                        }
                    }
                    else if (MappedInput.GetButtonDown(DefaultControls.Reload))
                    {
                        DoReload();
                    }
                    else if (ConfigState.Instance.GetGameplayConfig().HoldAds && weaponModel != null && weaponModel.CheckFlag(ItemFlag.WeaponHasADS))
                    {
                        //handle ADS (hold variant)
                        if (!PendingADSExit && !IsReloading)
                        {
                            bool adsButtonHeld = MappedInput.GetButton(DefaultControls.AltFire);
                            if (adsButtonHeld && !IsADS && !PlayerController.MovementComponent.IsRunning)
                            {
                                ToggleADS(); //enter ADS
                            }
                            else if (!adsButtonHeld && IsADS)
                            {
                                ToggleADS(); //exit ADS
                            }
                        }
                    }
                    else if (MappedInput.GetButtonDown(DefaultControls.AltFire))
                    {
                        //handle ADS
                        //TODO eventually altfire

                        if (!PendingADSExit && !PlayerController.MovementComponent.IsRunning)
                        {
                            if (rightEquipped)
                            {
                                var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel;
                                if (rightWeaponModel.CheckFlag(ItemFlag.WeaponHasADS))
                                {
                                    ToggleADS();
                                }
                            }
                        }
                    }
                }

            }

            //sprint-ADS handling
            if(PendingADSExit)
            {
                if (IsADS)
                {
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey((int)EquipSlot.RightWeapon);
                    var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel;
                    if (rightEquipped && !(rightWeaponModel is DummyWeaponItemModel) && rightWeaponModel.CheckFlag(ItemFlag.WeaponHasADS))
                    {
                        ToggleADS();
                    }
                }

                PendingADSExit = false;
            }

        }

        private void HandlePendingRecock()
        {
            //handle recock
            if (TimeToNext <= PendingRecockTime)
            {
                CharacterModel player = GameState.Instance.PlayerRpgState;
                InventoryItemInstance wItem = player.Equipped[(int)EquipSlot.RightWeapon];
                RangedWeaponItemModel wim = wItem?.ItemModel as RangedWeaponItemModel;
                if (wim == null)
                {
                    Debug.LogWarning($"[{nameof(PlayerWeaponComponent)}] Pending recock exists but there is no weapon model!");
                    PendingRecockTime = 0;
                    return;
                }

                float rateRpgFactor = RpgValues.GetWeaponRateFactor(player, wim);

                if (RightViewModel != null)
                {
                    float timeScale = (wim.CheckFlag(ItemFlag.WeaponRecockIgnoreLevelledRate) || wim.CheckFlag(ItemFlag.WeaponUnscaledAnimations)) ? 1 : rateRpgFactor;
                    RightViewModel.SetState(ViewModelState.Recock, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, timeScale);
                    Hands.SetState(ViewModelState.Recock, RightViewModel, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, timeScale);
                }

                PendingRecockTime = 0;
            }
        }

        private bool TryRefire()
        {
            //Debug.Log("TryRefire");

            if (!PlayerController.AttackEnabled || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoAttack) || GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoWeapons) || GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen) || !PlayerController.PlayerInControl || LockPauseModule.IsInputLocked())
            {
                return false;
            }

            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel;
            
            if ((MappedInput.GetButton(DefaultControls.Fire) && rightWeaponModel.CheckFlag(ItemFlag.WeaponFullAuto)) 
                || (rightWeaponModel is RangedWeaponItemModel rm && BurstCount < rm.ShotsPerBurst && BurstCount > 0))
            {

                //ammo logic
                if (rightWeaponModel is RangedWeaponItemModel rwim && rwim.UseAmmo)
                {
                    if (rwim.UseMagazine && GameState.Instance.PlayerRpgState.AmmoInMagazine[(int)EquipSlot.RightWeapon] < rwim.AmmoPerShot)
                    {
                        return false;
                    }
                    if (!rwim.UseMagazine && GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()) < rwim.AmmoPerShot)
                    {
                        return false;
                    }
                }

                DoRangedAttack(EquipSlot.RightWeapon);
                return true;
            }

            return false;
        }

        private void ToggleADS()
        {
            if (IsADS)
            {
                IsADS = false;

                //we don't need to handle dual-wielding or 1H/2H
                //note that a pistol is 2-handed here; 1-handed means dual-wieldable and dual-wielded

                if (RightViewModel is IWeaponViewModelCanHaveADSAnimations rwvms && rwvms.HasADSExitAnim)
                {
                    RightViewModel.SetState(ViewModelState.Lower, ViewModelHandednessState.ADS, 1);
                    Hands.SetState(ViewModelState.Lower, RightViewModel, ViewModelHandednessState.ADS, 1);
                }
                else
                {
                    RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded, 1);
                    Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                }

                SetCameraZoom(1, ADSZoomFadeTime);
                RescaleAccumulators(false);
                HandleCrosshair();
            }
            else
            {
                IsADS = true;

                if(RightViewModel is IWeaponViewModelCanHaveADSAnimations rwvms && rwvms.HasADSEnterAnim)
                {
                    RightViewModel.SetState(ViewModelState.Raise, ViewModelHandednessState.ADS, 1);
                    Hands.SetState(ViewModelState.Raise, RightViewModel, ViewModelHandednessState.ADS, 1);
                }
                else
                {
                    RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.ADS, 1);
                    Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.ADS, 1);
                }

                if(GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon) && GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon].ItemModel is RangedWeaponItemModel rwim)
                {
                    SetCameraZoom(rwim.ADSZoomFactor, ADSZoomFadeTime);
                }

                RescaleAccumulators(true);
                HandleCrosshair();
            }

        }

        private void DoMeleeAttack(EquipSlot slot)
        {

            //Debug.Log($"MeleeAttack {slot}");

            InventoryItemInstance wItem = null;
            MeleeWeaponItemModel wim = null;

            CharacterModel player = GameState.Instance.PlayerRpgState;
            if (slot == EquipSlot.None)
            {
                //attempt to get fallback weapon
                wim = FallbackWeapon;
                if(wim == null)
                {
                    //we don't actually have a fallback weapon, nop out
                    return;
                }
               
            }
            else
            {
                wItem = player.Equipped.GetOrDefault((int)slot, null);
                wim = wItem?.ItemModel as MeleeWeaponItemModel;
            }

            

            if (wim != null)
            {
                float timeScale = 1;

                Transform shootPoint = wim.CheckFlag(ItemFlag.WeaponUseFarShootPoint) ? ShootPointFar : ShootPointNear;

                //Debug.Log(distance);

                //calculate all the RPG dice stuff
                float energyUsed = wim.EnergyCost * RpgValues.GetWeaponEnergyCostFactor(player, wim);
                float damageDifficultyFactor = ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerStrength;
                float rateRpgFactor = RpgValues.GetWeaponRateFactor(player, wim);
                if (!wim.CheckFlag(ItemFlag.WeaponUnscaledAnimations) && !wim.CheckFlag(ItemFlag.WeaponIgnoreLevelledRate))
                    timeScale = rateRpgFactor;

                TimeToNext = wim.Rate * (wim.CheckFlag(ItemFlag.WeaponIgnoreLevelledRate) ? 1f : rateRpgFactor);

                //damage calculations that take into account alot of things (randomization, rpg calcs, difficulty)
                bool useRandom = wim.DamageSpread > 0 && !wim.CheckFlag(ItemFlag.WeaponNeverRandomize);
                float randomizedDamage = useRandom ? Mathf.Max(Mathf.Min(1, wim.Damage), wim.Damage + UnityEngine.Random.Range(-wim.DamageSpread, wim.DamageSpread)) : wim.Damage;
                float calcDamage = RpgValues.GetWeaponDamageFactor(player, wim) * randomizedDamage * damageDifficultyFactor;
                bool useRandomPierce = wim.DamagePierceSpread > 0 && !wim.CheckFlag(ItemFlag.WeaponNeverRandomize);
                float randomizedPierce = useRandomPierce ? Mathf.Max(Mathf.Min(1, wim.DamagePierce), wim.DamagePierce + UnityEngine.Random.Range(-wim.DamagePierceSpread, wim.DamagePierceSpread)) : wim.DamagePierce;
                float calcDamagePierce = RpgValues.GetWeaponDamageFactor(player, wim) * randomizedPierce * damageDifficultyFactor;

                if (player.Energy <= energyUsed)
                {
                    //slow attack
                    player.Energy = 0;
                    calcDamage *= 0.5f;
                    calcDamagePierce *= 0.5f;
                    TimeToNext += wim.Rate;
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("RpgInsufficientEnergy"));
                }
                else
                    player.Energy -= energyUsed;

                bool harmFriendly = wim.HarmFriendly ?? GameParams.UseFriendlyFire;

                //cast!
                if (wim.CheckFlag(ItemFlag.MeleeWeaponDelayCasting) && wim.DamageDelay > 0)
                {
                    DelayedFiringAction = () => MeleeCastAndDealDamage(wim, shootPoint, calcDamage, calcDamagePierce, harmFriendly);
                    DelayedFiringTimeRemaining = wim.DamageDelay;
                }
                else
                {
                    MeleeCastAndDealDamage(wim, shootPoint, calcDamage, calcDamagePierce, harmFriendly);
                }                

                if (slot == EquipSlot.RightWeapon && RightViewModel != null)
                {
                    RightViewModel.SetState(ViewModelState.Fire, ViewModelHandednessState.TwoHanded, timeScale);
                    Hands.SetState(ViewModelState.Fire, RightViewModel, ViewModelHandednessState.TwoHanded, timeScale);
                }
                else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
                {
                    //not supported
                }
                else if (slot == EquipSlot.None)
                {
                    //we can only reach here if we found and are using a fallback
                    FallbackViewModel.Ref()?.SetState(ViewModelState.Fire, ViewModelHandednessState.TwoHanded, timeScale);
                    Hands.SetState(ViewModelState.Fire, FallbackViewModel, ViewModelHandednessState.TwoHanded, timeScale);
                }
                //else if (MeleeEffect != null)
                //    Instantiate(MeleeEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);

                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
                if(!string.IsNullOrEmpty(wim?.Scripts?.OnFire))
                    ScriptingModule.Call(wim.Scripts.OnFire, new ScriptExecutionContext() { Activator = PlayerController.gameObject, Caller = this }, wItem, wItem.ItemModel);
            }
            else
            {
                Debug.LogError($"Player can't do a melee attack because weapon in {slot.ToString()} is not a melee weapon!");
            }
            
        }

        private void MeleeCastAndDealDamage(MeleeWeaponItemModel wim, Transform shootPoint, float calcDamage, float calcDamagePierce, bool harmFriendly)
        {
            var physicsInfo = new HitPhysicsInfo() { Impulse = wim.Impulse };
            if (wim.CheckFlag(ItemFlag.MeleeWeaponPushNonEntities))
                physicsInfo.HitPhysicsFlags |= BuiltinHitPhysicsFlags.PushNonEntities;

            if (wim.CheckFlag(ItemFlag.MeleeWeaponAllowMultipleHits))
            {
                HashSet<BaseController> hitEntities = null;
                HashSet<Collider> hitNonEntities = null;

                //run codepath adapted from motherearth

                bool defer = !wim.CheckFlag(ItemFlag.MeleeWeaponDelayCasting) && wim.DamageDelay > 0;
                List<Action> deferredActions = defer ? new List<Action>() : null;
                List<HitInfo> nonActorHits = wim.CheckFlag(ItemFlag.MeleeWeaponHitNonDamageable) ? new List<HitInfo>() : null;
                var hits = WorldUtils.SpherecastAllAttackHits(shootPoint.position, shootPoint.forward, wim.CastRadius, Math.Max(MeleeProbeDist, wim.Reach * 1.5f), true, PlayerController, nonActorHits);

                if(wim.CheckFlag(ItemFlag.MeleeWeaponDistinctMultipleHits))
                {
                    //expensive.
                    var hitDic = new Dictionary<BaseController, HitInfo>();
                    foreach(var hit in hits)
                    {
                        if(hitDic.ContainsKey(hit.Controller))
                        {
                            var oldHit = hitDic[hit.Controller];
                            if ((hit.HitPoint - shootPoint.position).magnitude < (oldHit.HitPoint - shootPoint.position).magnitude)
                                hitDic[hit.Controller] = hit;
                        }
                        else
                        {
                            hitDic.Add(hit.Controller, hit);
                        }
                    }
                    hits = hitDic.Values.ToList();
                    hitEntities = new HashSet<BaseController>(hitDic.Keys);
                }

                if(hits != null)
                {
                    foreach (var hit in hits)
                    {
                        float distance = (hit.HitPoint - shootPoint.position).magnitude;

                        if (distance <= wim.Reach)
                        {
                            var hitInfo = new ActorHitInfo(calcDamage, calcDamagePierce, (int)wim.DType, (int)wim.Effector, harmFriendly, hit.HitLocation, hit.HitMaterial, PlayerController, PredefinedFaction.Player.ToString(), wim.HitPuff, hit.HitPoint, wim.GetHitFlags());

                            if (hit.Controller is ITakeDamage itd)
                            {
                                if (defer)
                                    deferredActions.Add(() => MeleeDealDamage(hitInfo, itd, physicsInfo, hit.Controller as IAmPushable, shootPoint.forward));
                                else
                                    MeleeDealDamage(hitInfo, itd, physicsInfo, hit.Controller as IAmPushable, shootPoint.forward);
                            }
                        }
                    }
                }                

                if(nonActorHits != null && nonActorHits.Count > 0)
                {
                    foreach(var naHit in nonActorHits)
                    {
                        float distance = (naHit.HitPoint - shootPoint.position).magnitude;

                        if(distance <= wim.Reach)
                        {
                            //TODO improve this later
                            if (defer)
                                deferredActions.Add(() => { HitPuffScript.SpawnHitPuff(string.IsNullOrEmpty(wim.EnvironmentHitPuff) ? wim.HitPuff : wim.EnvironmentHitPuff, naHit.HitPoint, naHit.HitCollider is TerrainCollider ? (int)HitMaterial.Dirt : 0); });
                            else
                                HitPuffScript.SpawnHitPuff(string.IsNullOrEmpty(wim.EnvironmentHitPuff) ? wim.HitPuff : wim.EnvironmentHitPuff, naHit.HitPoint, naHit.HitCollider is TerrainCollider ? (int)HitMaterial.Dirt : 0);
                        }                        
                    }

                    if (wim.CheckFlag(ItemFlag.MeleeWeaponDistinctMultipleHits))
                    {
                        hitNonEntities = new HashSet<Collider>(nonActorHits.Select(h => h.HitCollider).Distinct());
                    }
                }
                
                if(wim.CheckFlag(ItemFlag.MeleeWeaponUseContactHitHack))
                {
                    //TODO anything in arc close enough and within 90 degrees of forward
                    var colliders = Physics.OverlapCapsule(PlayerController.transform.position, PlayerController.transform.position + Vector3.up * MeleeContactHitHeight, MeleeContactHitRadius, WorldUtils.GetAttackLayerMask());

                    if (hitEntities != null)
                    {
                        //means MeleeWeaponDistinctMultipleHits flag is set, don't hit any entities we've already hit

                    }
                    else
                    {
                        //means we can hit entities we've already hit
                    }

                    if (hitNonEntities != null)
                    {
                        //means MeleeWeaponDistinctMultipleHits flag is set, don't hit any non-entities we've already hit

                    }
                    else
                    {
                        //means we can hit entities we've already hit
                    }

                    Debug.LogError($"[{nameof(MeleeCastAndDealDamage)}] ItemFlag.MeleeWeaponUseContactHitHack is not implemented when ItemFlag.MeleeWeaponDistinctMultipleHits is also set!");
                }

                if (defer)
                {
                    DelayedFiringAction = () =>
                    {
                        foreach (var dAction in deferredActions)
                            dAction();
                    };
                    DelayedFiringTimeRemaining = wim.DamageDelay;
                }
            }
            else
            {
                bool hitSingle = false;

                //run old codepath essentially unchanged
                {
                    var (otherController, hitPoint, hitLocation, hitMaterial) = wim.CheckFlag(ItemFlag.MeleeWeaponUsePreciseCasting) ?
                                    WorldUtils.RaycastAttackHit(shootPoint.position, shootPoint.forward, Math.Max(MeleeProbeDist, wim.Reach * 1.5f), true, true, PlayerController) :
                                    WorldUtils.SpherecastAttackHit(shootPoint.position, shootPoint.forward, MeleeBoxCastSize * 0.5f, Math.Max(MeleeProbeDist, wim.Reach * 1.5f), true, false, PlayerController);
                    float distance = (hitPoint - shootPoint.position).magnitude;                    

                    if (distance <= wim.Reach)
                    {
                        var hitInfo = new ActorHitInfo(calcDamage, calcDamagePierce, (int)wim.DType, (int)wim.Effector, harmFriendly, hitLocation, hitMaterial, PlayerController, PredefinedFaction.Player.ToString(), wim.HitPuff, hitPoint, wim.GetHitFlags());

                        if (otherController is ITakeDamage itd)
                        {
                            if (!wim.CheckFlag(ItemFlag.MeleeWeaponDelayCasting) && wim.DamageDelay > 0)
                            {
                                DelayedFiringAction = () => MeleeDealDamage(hitInfo, itd, physicsInfo, otherController as IAmPushable, shootPoint.forward);
                                DelayedFiringTimeRemaining = wim.DamageDelay;
                            }
                            else
                            {
                                MeleeDealDamage(hitInfo, itd, physicsInfo, otherController as IAmPushable, shootPoint.forward);
                            }
                            hitSingle = true;
                        }
                    }                    
                }

                if (!hitSingle && wim.CheckFlag(ItemFlag.MeleeWeaponUseContactHitHack))
                {
                    //hit closest to center within 90 degrees of forward
                    var colliders = Physics.OverlapCapsule(PlayerController.transform.position, PlayerController.transform.position + Vector3.up * MeleeContactHitHeight, MeleeContactHitRadius, WorldUtils.GetAttackLayerMask());

                    Collider bestCollider = null;
                    float smallestAngle = MeleeContactMaxAngle;
                    foreach(var collider in colliders)
                    {
                        Vector2 vecPlayerToCollider = (collider.transform.position - shootPoint.position).GetFlatVector();
                        Vector2 vecForward = shootPoint.forward.GetFlatVector();
                        float angle = Vector2.Angle(vecPlayerToCollider, vecForward);
                        if(angle < smallestAngle)
                        {
                            var hitbox = collider.GetComponent<IHitboxComponent>();
                            if(hitbox != null)
                            {
                                if (hitbox.ParentController is PlayerController)
                                    continue;

                                bestCollider = collider;
                                smallestAngle = angle;
                            }
                            var d = collider.GetComponent<ITakeDamage>();
                            if(d != null)
                            {
                                if (d is PlayerController)
                                    continue;

                                bestCollider = collider;
                                smallestAngle = angle;
                            }
                        }
                    }

                    if (bestCollider != null)
                    {
                        var hb = bestCollider.GetComponent<IHitboxComponent>();
                        var oc = bestCollider.GetComponent<BaseController>();
                        if (oc == null)
                            oc = bestCollider.GetComponentInParent<BaseController>();

                        var hitInfo = new ActorHitInfo(calcDamage, calcDamagePierce, (int)wim.DType, (int)wim.Effector, harmFriendly, hb?.HitLocationOverride ?? 0, hb?.HitMaterial ?? oc.HitMaterial, PlayerController, PredefinedFaction.Player.ToString(), wim.HitPuff, bestCollider.ClosestPoint(shootPoint.position), wim.GetHitFlags());

                        if (oc is ITakeDamage itd)
                        {
                            if (!wim.CheckFlag(ItemFlag.MeleeWeaponDelayCasting) && wim.DamageDelay > 0)
                            {
                                DelayedFiringAction = () => MeleeDealDamage(hitInfo, itd, physicsInfo, oc as IAmPushable, shootPoint.forward);
                                DelayedFiringTimeRemaining = wim.DamageDelay;
                            }
                            else
                            {
                                MeleeDealDamage(hitInfo, itd, physicsInfo, oc as IAmPushable, shootPoint.forward);
                            }
                            hitSingle = true;
                        }
                    }
                   
                }


                if (!hitSingle && wim.CheckFlag(ItemFlag.MeleeWeaponHitNonDamageable))
                {
                    if(Physics.Raycast(shootPoint.position, shootPoint.forward, out var naHit, Math.Max(MeleeProbeDist, wim.Reach * 1.5f), WorldUtils.GetAttackLayerMask(), QueryTriggerInteraction.Ignore))
                    {
                        if(naHit.distance <= wim.Reach)
                        {
                            if (!wim.CheckFlag(ItemFlag.MeleeWeaponDelayCasting) && wim.DamageDelay > 0)
                            {
                                DelayedFiringAction = () => {
                                    HitPuffScript.SpawnHitPuff(string.IsNullOrEmpty(wim.EnvironmentHitPuff) ? wim.HitPuff : wim.EnvironmentHitPuff, naHit.point, naHit.collider is TerrainCollider ? (int)HitMaterial.Dirt : 0);
                                    if (wim.Impulse > 0 && wim.CheckFlag(ItemFlag.MeleeWeaponPushNonEntities) && naHit.collider.attachedRigidbody != null)
                                        naHit.collider.attachedRigidbody.AddForce(wim.Impulse * shootPoint.forward, ForceMode.Impulse);
                                };
                                DelayedFiringTimeRemaining = wim.DamageDelay;
                            }
                            else
                            {
                                HitPuffScript.SpawnHitPuff(string.IsNullOrEmpty(wim.EnvironmentHitPuff) ? wim.HitPuff : wim.EnvironmentHitPuff, naHit.point, naHit.collider is TerrainCollider ? (int)HitMaterial.Dirt : 0);
                                if (wim.Impulse > 0 && wim.CheckFlag(ItemFlag.MeleeWeaponPushNonEntities) && naHit.collider.attachedRigidbody != null)
                                    naHit.collider.attachedRigidbody.AddForce(wim.Impulse * shootPoint.forward, ForceMode.Impulse);
                            }
                        }
                    }
                }

            }
            
        }

        private static void MeleeDealDamage(ActorHitInfo hitInfo, ITakeDamage itd, HitPhysicsInfo hitPhysicsInfo, IAmPushable iap, Vector3 impulseVector)
        {
            HitPuffScript.SpawnHitPuff(hitInfo);
            itd.TakeDamage(hitInfo);

            if(hitPhysicsInfo.Impulse > 0 && iap != null)
            {
                if (hitPhysicsInfo.HitPhysicsFlags.HasFlag(BuiltinHitPhysicsFlags.UseFlatPhysics))
                    impulseVector = impulseVector.GetFlatVector().GetSpaceVector();
                impulseVector = impulseVector.normalized;
                iap.Push(hitPhysicsInfo.Impulse * impulseVector);
            }
        }

        //this whole thing is a fucking mess that needs to be refactored
        private void DoRangedAttack(EquipSlot slot)
        {
            if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon)
                throw new ArgumentException("slot must refer to a weapon", nameof(slot));

            //Debug.Log($"RangedAttack {slot}");

            CharacterModel player = GameState.Instance.PlayerRpgState;

            if (player.Equipped.ContainsKey((int)slot))
            {
                InventoryItemInstance wItem = player.Equipped[(int)slot];
                RangedWeaponItemModel wim = wItem?.ItemModel as RangedWeaponItemModel;
                if (wim != null)
                {
                    bool useAmmo = wim.UseAmmo;
                    bool autoReload = wim.CheckFlag(ItemFlag.WeaponAutoReload);

                    bool harmFriendly = wim.HarmFriendly ?? GameParams.UseFriendlyFire;

                    //ammo logic
                    //TODO more burst logic?
                    if (useAmmo)
                    {
                        if (wim.UseMagazine)
                        {
                            if (player.AmmoInMagazine[(int)slot] < wim.AmmoPerShot && !IsReloading)
                            {
                                //I think this one actually works okay
                                DoReload();
                                return;
                            }

                            player.AmmoInMagazine[(int)slot] -= wim.AmmoPerShot;
                        }
                        else
                        {
                            if (player.Inventory.CountItem(wim.AType.ToString()) < wim.AmmoPerShot)
                                return;

                            player.Inventory.RemoveItem(wim.AType.ToString(), wim.AmmoPerShot);
                        }
                    }

                    Transform shootPoint = wim.CheckFlag(ItemFlag.WeaponUseFarShootPoint) ? ShootPointFar : ShootPointNear;

                    float damageRpgFactor = RpgValues.GetWeaponDamageFactor(player, wim);
                    float damageDifficultyFactor = ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerStrength;
                    bool useRandomDamage = wim.DamageSpread > 0 && !wim.CheckFlag(ItemFlag.WeaponNeverRandomize);
                    bool useRandomPierce = wim.DamagePierceSpread > 0 && !wim.CheckFlag(ItemFlag.WeaponNeverRandomize);
                    Vector3 rFireVec = shootPoint.forward.normalized;

                    //this GIANT ASS BLOCK OF CODE fires ONE bullet
                    //some logic is definitely redundant but eh
                    for(int bulletCount = 0; bulletCount < Math.Max(1, wim.ShotsPerBurst); bulletCount++)
                    {
                        float randomizedDamage = useRandomDamage ? Mathf.Max(Mathf.Min(1, wim.Damage), wim.Damage + UnityEngine.Random.Range(-wim.DamageSpread, wim.DamageSpread)) : wim.Damage;
                        float randomizedPierce = useRandomPierce ? Mathf.Max(Mathf.Min(1, wim.DamagePierce), wim.DamagePierce + UnityEngine.Random.Range(-wim.DamagePierceSpread, wim.DamagePierceSpread)) : wim.DamagePierce;

                        //Vector3 fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.right)
                        //    * (Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.up) * ShootPoint.forward.normalized);

                        float spreadRpgFactor = RpgValues.GetWeaponSpreadFactor(player, wim);

                        float spreadMoveFactor = 1f;
                        if (PlayerController.MovementComponent.IsMoving)
                        {
                            spreadMoveFactor = wim.MovementSpreadFactor;
                            if (wim.CheckFlag(ItemFlag.WeaponProportionalMovement) && PlayerController.MovementComponent.IsRunning)
                                spreadMoveFactor *= 2f;
                            if (PlayerController.MovementComponent.IsCrouching)
                                spreadMoveFactor *= wim.CrouchSpreadFactor;
                        }

                        Vector3 fireVec = CalculateFireVec(shootPoint, spreadRpgFactor, spreadMoveFactor, AccumulatedSpread, AccumulatedRecoil, out var intendedTarget);
                        rFireVec = fireVec;

                        var hitInfo = new ActorHitInfo(randomizedDamage * damageRpgFactor * damageDifficultyFactor, randomizedPierce * damageRpgFactor * damageDifficultyFactor, (int)wim.DType, (int)wim.Effector, harmFriendly, (int)ActorBodyPart.Unspecified, (int)DefaultHitMaterials.Unspecified, PlayerController, PredefinedFaction.Player.ToString(), wim.HitPuff, null, wim.GetHitFlags());

                        //now a function call!
                        if (wim.LockTime > 0)
                        {
                            float aRecoil = AccumulatedRecoil;
                            float aSpread = AccumulatedSpread;
                            DelayedFiringTimeRemaining = wim.LockTime;
                            DelayedFiringAction = () =>
                            {
                                Vector3 fVec = CalculateFireVec(shootPoint, spreadRpgFactor, spreadMoveFactor, aSpread, aRecoil, out var tgt); //need to recalculate because the player may move
                                SpawnBullet(wim, shootPoint, fVec, tgt, hitInfo);
                            };

                        }
                        else
                        {
                            SpawnBullet(wim, shootPoint, fireVec, intendedTarget, hitInfo);
                        }

                    }

                    //recoil accumulation
                    float accumulatorRpgFactor = RpgValues.GetWeaponInstabilityFactor(player, wim);
                    RangeEnvelope recoilEnvelope = IsADS ? wim.ADSRecoil : wim.Recoil;
                    AccumulatedRecoil = Mathf.Min(recoilEnvelope.Max, AccumulatedRecoil + (recoilEnvelope.Gain * accumulatorRpgFactor));
                    RangeEnvelope spreadEnvelope = IsADS ? wim.ADSSpread : wim.Spread;
                    AccumulatedSpread = Mathf.Min(spreadEnvelope.Max, AccumulatedSpread + (spreadEnvelope.Gain * accumulatorRpgFactor));

                    DidJustFire = true;
                    if(wim.ShotsPerBurst > 1)
                        BurstCount++;
                    if (BurstCount >= wim.ShotsPerBurst)
                        BurstCount = 0;
                    float rateRpgFactor = RpgValues.GetWeaponRateFactor(player, wim);
                    float fireInterval = BurstCount > 0 ? wim.BurstFireInterval : wim.FireInterval;
                    TimeToNext = (wim.CheckFlag(ItemFlag.WeaponIgnoreLevelledRate)) ? fireInterval : (fireInterval * rateRpgFactor);
                    if(wim.CheckFlag(ItemFlag.WeaponHasRecock) && (player.AmmoInMagazine[(int)slot] > 0 || !wim.CheckFlag(ItemFlag.WeaponRecockSkipOnEmpty)))
                    {
                        var oldTTN = TimeToNext;
                        TimeToNext += (wim.CheckFlag(ItemFlag.WeaponRecockIgnoreLevelledRate)) ? wim.RecockTime : wim.RecockTime * rateRpgFactor;
                        PendingRecockTime = Mathf.Max(0, TimeToNext - oldTTN);
                    }

                    //GameObject fireEffect = null;

                    //TODO handle instantiate location (and variants?) in FPS/TPS mode?

                    //pivot the screen with the recoil
                    if (wim.CheckFlag(ItemFlag.WeaponShake))
                    {
                        float recoilScale = ConfigState.Instance.GetGameplayConfig().RecoilEffectScale;
                        float wimRecoilScale = IsADS ? wim.ADSRecoilEffectScale : wim.RecoilEffectScale;

                        //factor in the actual fire vector, but only a little bit
                        Quaternion fireRotation = Quaternion.LookRotation(ViewShakeScript.transform.parent.InverseTransformDirection(rFireVec));
                        Quaternion scaledFireRotation = Quaternion.SlerpUnclamped(Quaternion.identity, fireRotation, RecoilFireVecFactor * recoilScale * wimRecoilScale);

                        //scaledFireRotation = Quaternion.identity;
       
                        Vector3 rawRecoilAngle = new Vector3(-(IsADS ? wim.ADSRecoilImpulse.Intensity : wim.RecoilImpulse.Intensity) * recoilScale * wimRecoilScale, 0, 0);
                        Quaternion recoilRotation = Quaternion.Euler(rawRecoilAngle);

                        Vector3 recoilAngle = (recoilRotation * scaledFireRotation).eulerAngles;

                        //Debug.Log(recoilAngle.ToString("F2"));

                        ViewShakeScript.Shake(recoilAngle, wim.RecoilImpulse.Time, wim.RecoilImpulse.Violence); //try that and see how terrible it looks

                    }
                    
                    //set viewmodel and hands state
                    if (slot == EquipSlot.RightWeapon && RightViewModel != null)
                    {
                        float timeScale = (wim.CheckFlag(ItemFlag.WeaponIgnoreLevelledRate) || wim.CheckFlag(ItemFlag.WeaponUnscaledAnimations)) ? 1 : rateRpgFactor;
                        RightViewModel.SetState(ViewModelState.Fire, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, timeScale);
                        Hands.SetState(ViewModelState.Fire, RightViewModel, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded, timeScale);
                    }
                    else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
                    {
                        Debug.LogWarning("Left weapon slot not supported!");
                    }

                    if (useAmmo && autoReload && wim.UseMagazine && player.AmmoInMagazine[(int)slot] <= 0)
                    {
                        DoReload();
                    }

                    if (!string.IsNullOrEmpty(wim?.Scripts?.OnFire))
                        ScriptingModule.Call(wim.Scripts.OnFire, new ScriptExecutionContext() { Activator = PlayerController.gameObject, Caller = this }, wItem, wItem.ItemModel);

                }
                else
                {
                    Debug.LogError("Can't find item model for ranged weapon!");
                }

            }


            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));

            
        }
        private Vector3 CalculateFireVec(Transform shootPoint, float spreadRpgFactor, float spreadMoveFactor, float accumulatedSpread, float accumulatedRecoil, out Transform intendedTarget)
        {
            Vector3 fireVec = shootPoint.forward.normalized;

            //bend bullets if autoaim is enabled
            var autoaim = ConfigState.Instance.GetGameplayConfig().AimAssist;
            intendedTarget = null;
            //if(autoaim != AimAssistState.Off)
            {
                //we "probe" to see if we would have hit anyway, and don't correct if we did
                var probeHit = WorldUtils.RaycastAttackHit(shootPoint.position, shootPoint.forward, AutoaimCastRange * 1.25f, true, false, PlayerController);
                if (probeHit.Controller == null && autoaim != AimAssistState.Off)
                {
                    float castSize = autoaim == AimAssistState.Strong ? AutoaimStrongCastSize : AutoaimWeakCastSize;
                    var autoaimHit = WorldUtils.SpherecastForAutoaim(shootPoint.position, shootPoint.forward, castSize * 0.5f, AutoaimCastRange, true, false, PlayerController);
                    if (autoaimHit.Controller != null)
                    {
                        fireVec = (autoaimHit.HitPoint - shootPoint.position).normalized;
                        intendedTarget = autoaimHit.Controller.transform;
                    }
                }
                else
                    intendedTarget = probeHit.Controller.Ref()?.transform;
            }

            //apply spread
            //we don't use accumulators directly because they may change between pulling the trigger and the gun going off (although they probably _shouldn't_)
            fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-accumulatedSpread, accumulatedSpread) * spreadMoveFactor * spreadRpgFactor, Vector3.up) * fireVec;
            fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-accumulatedSpread, accumulatedSpread) * spreadMoveFactor * spreadRpgFactor, transform.right) * fireVec;
            fireVec = Quaternion.AngleAxis(accumulatedRecoil * spreadMoveFactor * spreadRpgFactor, -transform.right) * fireVec; //iffy

            return fireVec;
        }

        private void SpawnBullet(RangedWeaponItemModel wim, Transform shootPoint, Vector3 fireVec, Transform intendedTarget, ActorHitInfo hitInfo)
        {
            GameObject bullet = null;
            if (!string.IsNullOrEmpty(wim.Projectile))
            {
                if (wim.CheckFlag(ItemFlag.WeaponProjectileIsEntity))
                    bullet = WorldUtils.SpawnEntity(wim.Projectile, null, shootPoint.position + (shootPoint.forward.normalized * 0.25f), shootPoint.rotation, null);
                else
                    bullet = WorldUtils.SpawnEffect(wim.Projectile, shootPoint.position + (shootPoint.forward.normalized * 0.25f), shootPoint.rotation.eulerAngles, transform.root, false);
            }

            var bulletRigidbody = bullet.GetComponent<Rigidbody>();
            var bulletScript = bullet.GetComponent<BulletScript>();

            bulletScript.HitInfo = hitInfo;
            //Debug.Log(wim.Effector);
            //Debug.Log($"damage: {bulletScript.HitInfo.Damage:F2} | pierce: {bulletScript.HitInfo.DamagePierce:F2}");
            bulletScript.FiredByPlayer = true;
            bulletScript.Target = intendedTarget;

            //apply projectile explosion, physics options

            if (wim.ProjectileData != null)
            {
                if(wim.ProjectileData.Gravity >= 0)
                {
                    bulletScript.FakeGravity = wim.ProjectileData.Gravity;
                    bulletRigidbody.useGravity = false;
                }                
            }

            if (wim.ExplosionData != null && !wim.CheckFlag(ItemFlag.WeaponAlwaysUseEffectExplosion))
            {
                var explosionComponent = bulletScript.GetComponent<BulletExplosionComponent>();
                if (explosionComponent != null)
                {
                    explosionComponent.Damage = wim.ExplosionData.Damage;
                    explosionComponent.Radius = wim.ExplosionData.Radius;
                    explosionComponent.UseFalloff = wim.ExplosionData.UseFalloff;
                    if (!string.IsNullOrEmpty(wim.ExplosionData.HitPuff))
                        explosionComponent.HitPuff = wim.ExplosionData.HitPuff;

                    explosionComponent.DetonateOnWorldHit = wim.ExplosionData.DetonateOnWorldHit;
                    explosionComponent.DetonateOnDespawn = wim.ExplosionData.DetonateOnDespawn;

                    explosionComponent.EnableProximityDetonation = wim.ExplosionData.EnableProximityDetonation;
                    explosionComponent.ProximityRadius = wim.ExplosionData.ProximityRadius;
                    explosionComponent.UseFactions = wim.ExplosionData.UseFactions;
                    explosionComponent.UseTangentHack = wim.ExplosionData.UseTangentHack;

                    explosionComponent.Impulse = wim.ExplosionData.Impulse;
                    explosionComponent.PushNonEntities = wim.ExplosionData.PushNonEntities;
                    explosionComponent.ImpulseFlatPhysics = wim.ExplosionData.ImpulseFlatPhysics;
                    explosionComponent.ImpulseUseFalloff = wim.ExplosionData.ImpulseUseFalloff;
                }
                else
                {
                    Debug.LogWarning($"Explosion data is specified for {wim.Name} but {wim.Projectile} does not have a {nameof(BulletExplosionComponent)}!");
                }
            }
            else if (!wim.CheckFlag(ItemFlag.WeaponAlwaysUseEffectExplosion))
            {
                var explosionComponent = bulletScript.GetComponent<BulletExplosionComponent>();
                if (explosionComponent != null)
                {
                    explosionComponent.enabled = false;
                }
            }

            if (wim.PhysicsData != null)
            {
                var physicsInfo = new HitPhysicsInfo() { Impulse = wim.PhysicsData.Impulse };
                if (wim.PhysicsData.PushNonEntities)
                    physicsInfo.HitPhysicsFlags |= BuiltinHitPhysicsFlags.PushNonEntities;
                if (wim.PhysicsData.UseFlatPhysics)
                    physicsInfo.HitPhysicsFlags |= BuiltinHitPhysicsFlags.UseFlatPhysics;

                bulletScript.PhysicsInfo = physicsInfo;
            }

            bulletRigidbody.velocity = (fireVec * wim.ProjectileVelocity);
        }

        private void DoReload()
        {
            if (IsReloading) //I think we need this guard
                return;

            reloadSide(EquipSlot.RightWeapon, RightViewModel);
            reloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsADS = false;
            SetCameraZoom(1, ADSZoomFadeTime);
            IsReloading = true;
            BurstCount = 0;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloading"));

            HandleCrosshair(); //needed I think

            void reloadSide(EquipSlot slot, WeaponViewModelScript viewModel)
            {
                CharacterModel player = GameState.Instance.PlayerRpgState;
                if (player.Equipped.ContainsKey((int)slot))
                {
                    if (player.Equipped[(int)slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        //unreloadable condition
                        if (!rwim.UseMagazine || player.AmmoInMagazine[(int)slot] == rwim.MagazineSize
                            || player.Inventory.CountItem(rwim.AType.ToString()) <= 0)
                        {
                            return;
                        }

                        float reloadRpgFactor = RpgValues.GetWeaponReloadFactor(player, rwim);

                        if (viewModel != null)
                        {
                            //TODO handle 1H/2H (or not)

                            float timeScale = rwim.CheckFlag(ItemFlag.WeaponUnscaledAnimations) ? 1 : reloadRpgFactor;

                            viewModel.SetState(ViewModelState.Reload, ViewModelHandednessState.TwoHanded, timeScale);
                            Hands.SetState(ViewModelState.Reload, viewModel, ViewModelHandednessState.TwoHanded, timeScale);
                        }
                        //else if(!string.IsNullOrEmpty(rwim.ReloadEffect))
                        //    AudioPlayer.Instance.PlaySound(rwim.ReloadEffect, SoundType.Sound, false);

                        TimeToNext = Math.Max(rwim.ReloadTime * reloadRpgFactor, TimeToNext); //we take the longest time

                        if (!string.IsNullOrEmpty(rwim?.Scripts?.OnReload))
                            ScriptingModule.Call(rwim.Scripts.OnReload, new ScriptExecutionContext() { Activator = PlayerController.gameObject, Caller = this }, player.Equipped[(int)slot], player.Equipped[(int)slot]?.ItemModel);
                    }
                }

            }

        }

        private void FinishReload()
        {

            finishReloadSide(EquipSlot.RightWeapon, RightViewModel);
            finishReloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsReloading = false;
            BurstCount = 0;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloaded"));

            void finishReloadSide(EquipSlot slot, WeaponViewModelScript viewModel)
            {
                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey((int)slot))
                {
                    if (GameState.Instance.PlayerRpgState.Equipped[(int)slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        int currentAmmo = GameState.Instance.PlayerRpgState.AmmoInMagazine[(int)slot];
                        int qty = Math.Min(rwim.MagazineSize - currentAmmo, GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()));
                        GameState.Instance.PlayerRpgState.AmmoInMagazine[(int)slot] = qty + currentAmmo;
                        GameState.Instance.PlayerRpgState.Inventory.RemoveItem(rwim.AType.ToString(), qty);

                        if (viewModel != null)
                        {
                            //TODO handle 1H/2H
                            viewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded, 1);
                            Hands.SetState(ViewModelState.Idle, viewModel, ViewModelHandednessState.TwoHanded, 1);
                        }
                    }
                }
            }

        }

        public void HandleWeaponChange(EquipSlot slot, bool skipAnimations)
        {
            if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon) //do not handle non-weapon equipment
                return;

            //we should probably cache this at a higher level but it's probably not safe
            var player = GameState.Instance.PlayerRpgState;

            //reset ADS and accumulators
            IsADS = false;
            ResetAccumulators(); //probably exploitable
            SetCameraZoom(1);

            if (slot == EquipSlot.RightWeapon)
            {
                //needed?
                IsReloading = false;
                PendingRecockTime = 0;
                TimeToNext = 0;
                BurstCount = 0;
                DelayedFiringAction = null; //should we?

                //handle equip/unequip melee weapon
                if (player.Equipped.ContainsKey((int)EquipSlot.RightWeapon) && player.Equipped[(int)EquipSlot.RightWeapon] != null && player.Equipped[(int)EquipSlot.RightWeapon].ItemModel is WeaponItemModel wim)
                {
                    //fixed to equip *right* weapon
                    Debug.Log("Equipped right weapon!");

                    if (TransitionState == WeaponTransitionState.Lowering) //guard. Do we need one for Raising as well?
                    {
                        //Debug.Log("lowering hack guard");
                        OldWeaponLowerTime = 0; //this forces it to clear the viewmodel 
                        //OldWeaponNoLowerAnimation = false;
                    }

                    //Debug.Log(OldWeaponLowerTime);

                    //handle lower and raise
                    if (OldWeaponLowerTime > 0 && !skipAnimations) //old lower time: need to lower the weapon
                    {
                        lowerWeapon();
                        TimeToNext = OldWeaponLowerTime;
                        TransitionState = WeaponTransitionState.Lowering;
                    }
                    else if(wim.RaiseTime > 0 && !skipAnimations) //no old lower time, but new raise time: raise the weapon
                    {
                        ClearViewModel(EquipSlot.RightWeapon);
                        SetViewModel(EquipSlot.RightWeapon);
                        HandleCrosshair();

                        if (RightViewModel != null)
                        {
                            if (wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower))
                            {
                                //nop; we will handle it ourselves, or not use it at all
                            }
                            else
                            {
                                RightViewModel.SetState(ViewModelState.Raise, ViewModelHandednessState.TwoHanded, 1);
                                Hands.SetState(ViewModelState.Raise, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                            }
                        }
                        else
                            Hands.SetState(ViewModelState.Idle, null, ViewModelHandednessState.TwoHanded, 1);

                        TimeToNext = wim.RaiseTime;
                        TransitionState = WeaponTransitionState.Raising;
                    }
                    else //no old lower, no new raise: immediately swap
                    {                        
                        ClearViewModel(EquipSlot.RightWeapon);
                        SetViewModel(EquipSlot.RightWeapon);
                        HandleCrosshair();
                        OldWeaponLowerTime = wim.LowerTime;
                        OldWeaponNoLowerAnimation = wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower);

                        if (RightViewModel != null)
                        {
                            if (wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower))
                            {
                                //nop; we will handle it ourselves, or not use it at all
                            }
                            else
                            {
                                RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded, 1);
                            }
                            //hands.setstate?
                        }

                        TransitionState = WeaponTransitionState.None;
                    }

                    OldWeaponNoLowerAnimation = wim is DummyWeaponItemModel && !wim.CheckFlag(ItemFlag.DummyWeaponUseViewModelRaiseLower);
                }
                else
                {

                    Debug.Log("Unequipped right weapon!");

                    if (TransitionState == WeaponTransitionState.Lowering)
                        TransitionState = WeaponTransitionState.None;

                    if (OldWeaponLowerTime > 0 && !skipAnimations) //need to lower the weapon
                    {
                        lowerWeapon();
                        TimeToNext = OldWeaponLowerTime;
                        TransitionState = WeaponTransitionState.Lowering;
                    }
                    else if (FallbackWeapon != null && FallbackWeapon.RaiseTime > 0 && !skipAnimations) //don't need to lower the weapon, do need to raise the new one
                    {
                        ClearViewModel(EquipSlot.RightWeapon);
                        Hands.SetState(ViewModelState.Raise, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);
                        TimeToNext = FallbackWeapon.RaiseTime;
                        TransitionState = WeaponTransitionState.Raising;
                    }
                    else
                    {
                        ClearViewModel(EquipSlot.RightWeapon);
                        TransitionState = WeaponTransitionState.None;
                    }

                    OldWeaponNoLowerAnimation = false;
                }
            }
            else if (slot == EquipSlot.LeftWeapon)
            {
                //only half works, natch

                IsReloading = false;
                PendingRecockTime = 0;
                TimeToNext = 0;

                //handle equip/unequip ranged weapon
                if (player.Equipped.ContainsKey((int)EquipSlot.LeftWeapon) && player.Equipped[(int)EquipSlot.LeftWeapon] != null)
                {
                    Debug.LogError("Left weapons are not actually supported!");
                }
                else
                {
                    //fixed to unequip *left* model
                    Debug.Log("Unequipped left weapon!");
                    ClearViewModel(EquipSlot.LeftWeapon);
                }
            }

            //HandleCrosshair();

            if (TransitionState == WeaponTransitionState.None)
            {
                if (RightViewModel != null)
                    Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                else if (player.IsEquipped((int)EquipSlot.RightWeapon))
                    Hands.SetState(ViewModelState.Idle, null, ViewModelHandednessState.TwoHanded, 1);
                else
                    Hands.SetState(ViewModelState.Idle, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);
            }

            void lowerWeapon()
            {
                if (GameState.Instance.PlayerRpgState.IsEquipped((int)EquipSlot.RightWeapon))
                {
                    if (RightViewModel != null)
                    {
                        //issues:
                        //we want to skip this if our old 
                        if (OldWeaponNoLowerAnimation)
                        {

                        }
                        else
                        {
                            RightViewModel.SetState(ViewModelState.Lower, ViewModelHandednessState.TwoHanded, 1);
                            Hands.SetState(ViewModelState.Lower, RightViewModel, ViewModelHandednessState.TwoHanded, 1);
                        }
                    }
                    else
                    {
                        Hands.SetState(ViewModelState.Lower, null, ViewModelHandednessState.TwoHanded, 1);
                    }
                }
                else if (FallbackViewModel != null)
                {
                    Hands.SetState(ViewModelState.Lower, FallbackViewModel, ViewModelHandednessState.TwoHanded, 1);
                }
            }
        }

        private void SetViewModel(EquipSlot slot)
        {
            if (slot != EquipSlot.RightWeapon)
                throw new NotImplementedException();

            InventoryItemInstance item = GameState.Instance.PlayerRpgState.Equipped[(int)EquipSlot.RightWeapon];
            WeaponItemModel wim = item.ItemModel as WeaponItemModel;
            if (wim != null && !string.IsNullOrEmpty(wim.ViewModel))
            {

                var prefab = CoreUtils.LoadResource<GameObject>("WeaponViewModels/" + wim.ViewModel);
                if (prefab != null)
                {
                    var vmo = new ViewModelOptions(item, slot, this, ViewShakeScript, MovebobComponent);
                    var go = Instantiate<GameObject>(prefab, RightViewModelPoint);                    
                    RightViewModel = go.GetComponent<WeaponViewModelScript>();
                    RightViewModel.Init(vmo);
                }
                else
                {
                    Debug.LogError($"Could not find weapon view model \"{wim.ViewModel}\"");
                }

            }
        }

        private void ClearViewModel(EquipSlot slot)
        {

            switch (slot)
            {
                case EquipSlot.LeftWeapon:
                    if (LeftViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(LeftViewModelPoint.transform.GetChild(0).gameObject);
                    }
                    LeftViewModel = null;
                    break;
                case EquipSlot.RightWeapon:
                    if (RightViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(RightViewModelPoint.transform.GetChild(0).gameObject);
                    }
                    RightViewModel = null;
                    break;
            }

        }

        private void SetCameraZoom(float zoomFactor, float fadeTime = 0)
        {
            if (zoomFactor <= 0)
                zoomFactor = 1;

            //because alot of shit can go wrong
            try
            {
                PlayerController.CameraZoomComponent.SetADSZoomFactor(zoomFactor, fadeTime);
            }
            catch(Exception e)
            {
                Debug.LogError($"Failed to set camera zoom ({e.GetType().Name})");
                Debug.LogException(e);
            }
        }

        private void HandleCrosshair()
        {
            if(RightViewModel != null && RightViewModel.ViewHandlesCrosshair)
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("HudDisableCrosshair"));
            }
            else if (GameState.Instance.PlayerRpgState.Equipped.TryGetValue((int)EquipSlot.RightWeapon, out var weaponItem))
            {
                WeaponItemModel rwim = weaponItem.ItemModel as WeaponItemModel;
                var gameplayConfig = ConfigState.Instance.GetGameplayConfig();
                if (rwim == null || (!(rwim.CheckFlag(ItemFlag.WeaponCrosshairInADS) || gameplayConfig.Crosshair == CrosshairState.Always) && IsADS) || (!rwim.CheckFlag(ItemFlag.WeaponUseCrosshair) && gameplayConfig.Crosshair == CrosshairState.Auto) || gameplayConfig.Crosshair == CrosshairState.Never) //probably fuxxored
                {
                    //DisableCrosshair();
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("HudDisableCrosshair"));
                }
                else
                {
                    //EnableCrosshair();
                    QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("HudEnableCrosshair"));
                }
            }
            else
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("HudDisableCrosshair"));
            }
        }

        private void HandleOffhandKick()
        {

            //kick timing
            if (TimeToNextKick > 0)
            {
                TimeToNextKick -= Time.deltaTime;
                if (TimeToNextKick <= 0) //disappear the model if we're not actively kicking
                {
                    OffhandKickAnimator.Ref()?.Play("Idle");
                    OffhandKickAnimator.Ref()?.gameObject.SetActive(false);
                }
            }

            if(TimeToNextKick <= 0 && PlayerController.AttackEnabled && OffhandKickEnabled && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoAttack) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoWeapons) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.TotallyFrozen))
            {
                if (MappedInput.GetButtonDown(DefaultControls.Offhand1))
                {
                    if (OffhandKickAnimator != null)
                    {
                        OffhandKickAnimator.gameObject.SetActive(true);
                        OffhandKickAnimator.Play("Kick");
                    }

                    OffhandKickSound.Ref()?.Play();

                    var player = GameState.Instance.PlayerRpgState;

                    //deal damage
                    //var (otherController, hitPoint, hitLocation, hitMaterial) = GetMeleeHitEx(OffhandKickPoint, OffhandKickRange);
                    var (otherController, hitPoint, hitLocation, hitMaterial) = WorldUtils.SpherecastAttackHit(OffhandKickPoint.position, OffhandKickPoint.forward, 0.25f, OffhandKickRange, true, false, PlayerController);

                    if (otherController is ITakeDamage itd)
                    {
                        float damageMultiplier = RpgValues.GetKickDamageFactor(player) * ConfigState.Instance.GetGameplayConfig().Difficulty.PlayerStrength; //from stats and difficulty
                        var hitInfo = new ActorHitInfo(OffhandKickDamage * damageMultiplier, 0, (int)DamageType.Impact, (int)DamageEffector.Melee, GameParams.UseFriendlyFire, (int)hitLocation, (int)hitMaterial, PlayerController, PredefinedFaction.Player.ToString(), OffhandKickPuff, hitPoint, TypeUtils.FlagsFromCollection(OffhandKickFlags));
                        itd.TakeDamage(hitInfo);
                        if(!string.IsNullOrEmpty(OffhandKickPuff))
                            HitPuffScript.SpawnHitPuff(hitInfo);                        
                    }
                    if(otherController is IAmPushable iap)
                    {
                        iap.Push(OffhandKickPoint.forward * OffhandKickForce * RpgValues.GetKickForceFactor(player));
                    }

                    //kick away kickable things
                    //if(Physics.Raycast(OffhandKickPoint.position, OffhandKickPoint.forward, out var hit, OffhandKickRange))
                    if(Physics.BoxCast(OffhandKickPoint.position, Vector3.one * 0.25f, OffhandKickPoint.forward, out var hit, Quaternion.identity, OffhandKickRange))
                    {
                        Rigidbody rb = hit.collider.attachedRigidbody;
                        var rbIap = rb.gameObject.GetComponent<IAmPushable>();
                        if(rb != null && rbIap == null)
                        {
                            rb.AddForce(OffhandKickPoint.forward * OffhandKickForce * RpgValues.GetKickForceFactor(player), ForceMode.Impulse);
                        }
                    }

                    TimeToNextKick = OffhandKickDelay * RpgValues.GetKickRateFactor(player);
                }
            }

        }

        private enum WeaponTransitionState
        {
            None, Lowering, Raising
        }

    }

    public enum ViewModelHandednessState
    {
        //two-handed is the "default"
        TwoHanded = 0, OneHanded, ADS
    }
}