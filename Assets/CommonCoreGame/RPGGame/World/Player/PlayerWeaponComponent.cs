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
        public Transform ShootPoint;

        [SerializeField, Header("Hands")]
        private WeaponHandModelScript Hands;

        [SerializeField, Header("Recoil Shake")]
        private WeaponViewShakeScript ViewShakeScript;
        [SerializeField]
        private float RecoilFireVecFactor = 0.2f;

        [SerializeField, Header("Offhand Kick")] //TODO ought to move this into another component
        private Animator OffhandKickAnimator = null;
        [SerializeField]
        private Transform OffhandKickPoint = null;
        [SerializeField]
        private float OffhandKickRange = 1.5f;
        [SerializeField]
        private float OffhandKickDamage = 10f; //TODO move to stats and stuff
        [SerializeField]
        private float OffhandKickDelay = 1f;
        [SerializeField]
        private float OffhandKickForce = 1000f;
        [SerializeField]
        private AudioSource OffhandKickSound = null;

        [Header("Params")]
        public float MeleeProbeDist = 1.5f;
        

        public WeaponViewModelScript LeftViewModel { get; private set; }
        public WeaponViewModelScript RightViewModel { get; private set; }
        private float TimeToNext;
        private bool IsReloading;
        public bool IsADS { get; private set; }

        //serialized for debug only
        [SerializeField]
        private float AccumulatedSpread;
        [SerializeField]
        private float AccumulatedRecoil;
        [SerializeField]
        private bool DidJustFire;
        [SerializeField]
        private bool PendingADSExit;

        //offhand kick
        private float TimeToNextKick;

        //plan is to just bodge this for now, giving up on dual-wielding support, and add proper dual-wielding later
        //we need to add that offhand kick BTW

        //WIP ADS state
        //TODO movebob
        //TODO more
        //TODO weapon raise/lower handling

        private bool IsDualWielded => throw new NotImplementedException(); //TODO implement this

        private void Start()
        {
            if (PlayerController == null)
                PlayerController = GetComponentInParent<PlayerController>();
        }

        private void Update()
        {

            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            //HandleDynamicMovement();

            HandleAccumulators();

            DidJustFire = false;

            //TODO handle player death
            if (PlayerController.PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleWeapons();
                HandleOffhandKick();
            }

            
        }

        /// <summary>
        /// Sets the visibility of weapons and hands
        /// </summary>
        public void SetVisibility(bool visibility)
        {
            LeftViewModel.Ref()?.SetVisibility(visibility);
            RightViewModel.Ref()?.SetVisibility(visibility);
            Hands.Ref()?.SetVisibility(visibility);

            //force offhand kick to disappear
            if(!visibility)
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

        //copied from PlayerController

        /// <summary>
        /// Handle decay of accumulators (recoil/spread) every frame
        /// </summary>
        protected void HandleAccumulators()
        {
            if (DidJustFire || TimeToNext > 0)
                return;

            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon]?.ItemModel;
            if(rightWeaponModel != null && rightWeaponModel is RangedWeaponItemModel rwim)
            {
                if(AccumulatedRecoil > 0)
                {
                    RangeEnvelope recoilEnvelope = IsADS ? rwim.ADSRecoil : rwim.Recoil;
                    AccumulatedRecoil = Mathf.Max(recoilEnvelope.Min, AccumulatedRecoil - (recoilEnvelope.Decay * Time.deltaTime));
                }

                if(AccumulatedSpread > 0)
                {
                    RangeEnvelope spreadEnvelope = IsADS ? rwim.ADSSpread : rwim.Spread;
                    AccumulatedSpread = Mathf.Max(spreadEnvelope.Min, AccumulatedSpread - (spreadEnvelope.Decay * Time.deltaTime));
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
            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon]?.ItemModel;
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
            //this is completely fuxxored for dual-wielding... actually everything basically assumes one action at a time...
            float oldTTN = TimeToNext;
            TimeToNext -= Time.deltaTime;
            if (TimeToNext > 0)
                return;

            //TODO reset reload time on weapon change, probably going to need to add messaging for that

            if (oldTTN > 0)
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReady"));

                //TODO handle 1H/2H(?), default

                if (RightViewModel != null && TryRefire()) //it'll break on dual-wielding
                {

                }
                else
                {
                    if (RightViewModel != null)
                        RightViewModel.SetState(ViewModelState.Idle, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                    else if (LeftViewModel != null)
                        LeftViewModel.SetState(ViewModelState.Idle, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                    else if (Hands != null)
                        Hands.SetState(ViewModelState.Idle, null, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                }
            }


            if (IsReloading)
            {
                FinishReload();
            }

            //Logic: 
            // -if there is only one weapon equipped, that weapon uses primary fire
            // -if one weapon is ranged and the other melee, the ranged is primary fire
            // -if both weapons are ranged or both melee, primary is the left weapon and secondary the right weapon
            //TBH this code is PFA
            if (PlayerController.AttackEnabled)
            {
                if (MappedInput.GetButtonDown(DefaultControls.Fire))
                {
                    bool leftEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.LeftWeapon);
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RightWeapon);
                    if (leftEquipped && rightEquipped)
                    {
                        var leftWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;

                        //if only one weapon is ranged, fire that one
                        if (leftWeaponModel is RangedWeaponItemModel && !(rightWeaponModel is RangedWeaponItemModel))
                            DoRangedAttack(EquipSlot.LeftWeapon);
                        else if (rightWeaponModel is RangedWeaponItemModel && !(leftWeaponModel is RangedWeaponItemModel))
                            DoRangedAttack(EquipSlot.RightWeapon);
                        //otherwise, fire the left weapon
                        else
                        {
                            if (leftWeaponModel is RangedWeaponItemModel)
                                DoRangedAttack(EquipSlot.LeftWeapon);
                            else
                                DoMeleeAttack(EquipSlot.LeftWeapon);
                        }

                    }
                    else if (leftEquipped)
                    {
                        //fire left weapon
                        var weaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        if (weaponModel is RangedWeaponItemModel)
                            DoRangedAttack(EquipSlot.LeftWeapon);
                        else
                            DoMeleeAttack(EquipSlot.LeftWeapon);
                    }
                    else if (rightEquipped)
                    {
                        //fire right weapon
                        var weaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;
                        if (weaponModel is RangedWeaponItemModel)
                            DoRangedAttack(EquipSlot.RightWeapon);
                        else
                            DoMeleeAttack(EquipSlot.RightWeapon);
                    }
                }
                else if (MappedInput.GetButtonDown(DefaultControls.AltFire))
                {
                    bool leftEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.LeftWeapon);
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RightWeapon);
                    if (leftEquipped && rightEquipped)
                    {
                        var leftWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel;
                        var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;

                        //if only one weapon is ranged, fire the other one
                        if (leftWeaponModel is RangedWeaponItemModel && !(rightWeaponModel is RangedWeaponItemModel))
                            DoMeleeAttack(EquipSlot.RightWeapon);
                        else if (rightWeaponModel is RangedWeaponItemModel && !(leftWeaponModel is RangedWeaponItemModel))
                            DoMeleeAttack(EquipSlot.LeftWeapon);
                        //otherwise, fire the right weapon
                        else
                        {
                            if (rightWeaponModel is RangedWeaponItemModel)
                                DoRangedAttack(EquipSlot.RightWeapon);
                            else
                                DoMeleeAttack(EquipSlot.RightWeapon);
                        }
                    }
                    else if(!PendingADSExit && !PlayerController.MovementComponent.IsRunning)
                    {
                        //WIP handle ADS
                        if(leftEquipped)
                        {
                            Debug.LogWarning("Trying to enter ADS with only a left weapon equipped!");
                        }
                        else if(rightEquipped)
                        {
                            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;
                            if (rightWeaponModel.CheckFlag(ItemFlag.WeaponHasADS))
                            {
                                ToggleADS();
                            }
                        }
                    }

                    //TODO eventually altfire
                }
                else if (MappedInput.GetButtonDown(DefaultControls.Reload))
                {
                    DoReload();
                }

            }

            //sprint-ADS handling
            if(PendingADSExit)
            {
                if (IsADS)
                {
                    bool rightEquipped = GameState.Instance.PlayerRpgState.Equipped.ContainsKey(EquipSlot.RightWeapon);
                    var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;
                    if (rightEquipped && rightWeaponModel.CheckFlag(ItemFlag.WeaponHasADS))
                    {
                        ToggleADS();
                    }
                }

                PendingADSExit = false;
            }

        }

        private bool TryRefire()
        {
            //I think there might be something wrong with this

            var rightWeaponModel = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel;
            
            if (MappedInput.GetButton(DefaultControls.Fire) && rightWeaponModel.CheckFlag(ItemFlag.WeaponFullAuto))
            {

                //ammo logic
                if (rightWeaponModel is RangedWeaponItemModel rwim && rwim.AType != AmmoType.NoAmmo)
                {
                    if (GameState.Instance.PlayerRpgState.AmmoInMagazine[EquipSlot.RightWeapon] <= 0)
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

                if (RightViewModel is RangedWeaponViewModelScript rwvms && rwvms.HasADSExitAnim)
                {
                    RightViewModel.SetState(ViewModelState.Raise, ViewModelHandednessState.ADS);
                    Hands.SetState(ViewModelState.Raise, RightViewModel, ViewModelHandednessState.ADS);
                }
                else
                {
                    RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded);
                    Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.TwoHanded);
                }

                RescaleAccumulators(false);
            }
            else
            {
                IsADS = true;

                if(RightViewModel is RangedWeaponViewModelScript rwvms && rwvms.HasADSEnterAnim)
                {
                    RightViewModel.SetState(ViewModelState.Raise, ViewModelHandednessState.ADS);
                    Hands.SetState(ViewModelState.Raise, RightViewModel, ViewModelHandednessState.ADS);
                }
                else
                {
                    RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.ADS);
                    Hands.SetState(ViewModelState.Idle, RightViewModel, ViewModelHandednessState.ADS);
                }

                RescaleAccumulators(true);
            }

        }

        private void DoMeleeAttack(EquipSlot slot)
        {

            Debug.Log($"MeleeAttack {slot}");

            //punch
            ITakeDamage ac = GetMeleeHit(ShootPoint, MeleeProbeDist);

            if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon)
                throw new ArgumentException("slot must refer to a weapon", nameof(slot));

            ActorHitInfo hitInfo = default;
            if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
            {
                MeleeWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel as MeleeWeaponItemModel;
                if (wim != null)
                {
                    TimeToNext = wim.Rate;
                    float calcDamage = RpgValues.GetMeleeDamage(GameState.Instance.PlayerRpgState, wim.Damage);
                    float calcDamagePierce = RpgValues.GetMeleeDamage(GameState.Instance.PlayerRpgState, wim.DamagePierce);
                    if (GameState.Instance.PlayerRpgState.Energy <= 0)
                    {
                        calcDamage *= 0.5f;
                        calcDamagePierce *= 0.5f;
                        TimeToNext += wim.Rate;
                    }
                    else
                        GameState.Instance.PlayerRpgState.Energy -= wim.EnergyCost;
                    hitInfo = new ActorHitInfo(calcDamage, calcDamagePierce, (int)wim.DType, (int)ActorBodyPart.Unspecified, (int)DefaultHitMaterials.Unspecified, PlayerController);

                }
                else
                {
                    Debug.LogError($"Player can't do a melee attack because weapon in {slot.ToString()} is not a melee weapon!");
                }
                //TODO fists or something
            }
            else
            {
                Debug.LogError($"Player can't do a melee attack because no {slot.ToString()} is equipped!");
            }


            if (ac != null)
                ac.TakeDamage(hitInfo);

            //TODO handle 1H/2H

            if (slot == EquipSlot.RightWeapon && RightViewModel != null)
            {
                RightViewModel.SetState(ViewModelState.Fire, ViewModelHandednessState.TwoHanded);
                Hands.SetState(ViewModelState.Fire, RightViewModel, ViewModelHandednessState.TwoHanded);
            }
            else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
            {
                LeftViewModel.SetState(ViewModelState.Fire, ViewModelHandednessState.TwoHanded);
                Hands.SetState(ViewModelState.Fire, LeftViewModel, ViewModelHandednessState.TwoHanded);
            }
            //else if (MeleeEffect != null)
            //    Instantiate(MeleeEffect, ShootPoint.position, ShootPoint.rotation, ShootPoint);

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
        }

        //this whole thing is a fucking mess that needs to be refactored
        private void DoRangedAttack(EquipSlot slot)
        {
            if (slot != EquipSlot.LeftWeapon && slot != EquipSlot.RightWeapon)
                throw new ArgumentException("slot must refer to a weapon", nameof(slot));

            //Debug.Log($"RangedAttack {slot}");

            //TODO default model for fallback instead of fixed values

            if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
            {

                RangedWeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel as RangedWeaponItemModel;
                if (wim != null)
                {
                    bool useAmmo = !(wim.AType == AmmoType.NoAmmo);
                    bool autoReload = wim.CheckFlag("AutoReload");

                    //ammo logic
                    if (useAmmo)
                    {
                        if (GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] == 0 && !IsReloading)
                        {
                            //breaks anims for some reason
                            DoReload();
                            return;
                        }

                        GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] -= 1;
                    }

                    //bullet logic
                    GameObject bullet = null;

                    if (!string.IsNullOrEmpty(wim.Projectile))
                    {
                        var wimBulletPrefab = CoreUtils.LoadResource<GameObject>("Effects/" + wim.Projectile);
                        if (wimBulletPrefab != null)
                            bullet = Instantiate<GameObject>(wimBulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);

                    }

                    /*
                    if (bullet == null)
                        bullet = Instantiate<GameObject>(BulletPrefab, ShootPoint.position + (ShootPoint.forward.normalized * 0.25f), ShootPoint.rotation, transform.root);
                    */

                    var bulletRigidbody = bullet.GetComponent<Rigidbody>();

                    //TODO factor in weapon skill, esp for bows

                    bullet.GetComponent<BulletScript>().HitInfo = new ActorHitInfo(wim.Damage, wim.DamagePierce, (int)wim.DType, (int)ActorBodyPart.Unspecified, (int)DefaultHitMaterials.Unspecified, PlayerController, wim.HitPuff, null);

                    //Vector3 fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.right)
                    //    * (Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.up) * ShootPoint.forward.normalized);

                    Vector3 fireVec = ShootPoint.forward.normalized;
                    fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.up) * fireVec;
                    fireVec = Quaternion.AngleAxis(UnityEngine.Random.Range(-AccumulatedSpread, AccumulatedSpread), Vector3.right) * fireVec;
                    fireVec = Quaternion.AngleAxis(AccumulatedRecoil, -transform.right) * fireVec; //iffy

                    bulletRigidbody.velocity = (fireVec * wim.ProjectileVelocity);

                    //recoil accumulation
                    RangeEnvelope recoilEnvelope = IsADS ? wim.ADSRecoil : wim.Recoil;
                    AccumulatedRecoil = Mathf.Min(recoilEnvelope.Max, AccumulatedRecoil + recoilEnvelope.Gain);
                    RangeEnvelope spreadEnvelope = IsADS ? wim.ADSSpread : wim.Spread;
                    AccumulatedSpread = Mathf.Min(spreadEnvelope.Max, AccumulatedSpread + spreadEnvelope.Gain);

                    DidJustFire = true;
                    TimeToNext = wim.FireInterval;

                    //GameObject fireEffect = null;

                    //TODO handle instantiate location (and variants?) in FPS/TPS mode?

                    //pivot the screen with the recoil
                    if (wim.CheckFlag(ItemFlag.WeaponShake))
                    {
                        //factor in the actual fire vector, but only a little bit
                        Quaternion fireRotation = Quaternion.LookRotation(ViewShakeScript.transform.parent.InverseTransformDirection(fireVec));
                        Quaternion scaledFireRotation = Quaternion.SlerpUnclamped(Quaternion.identity, fireRotation, RecoilFireVecFactor);                        
       
                        Vector3 rawRecoilAngle = new Vector3(-(IsADS ? wim.ADSRecoilImpulse.Intensity : wim.RecoilImpulse.Intensity), 0, 0);
                        Quaternion recoilRotation = Quaternion.Euler(rawRecoilAngle);

                        Vector3 recoilAngle = (recoilRotation * scaledFireRotation).eulerAngles;

                        ViewShakeScript.Shake(recoilAngle, wim.RecoilImpulse.Time, wim.RecoilImpulse.Violence); //try that and see how terrible it looks

                    }
                    
                    //set viewmodel and hands state
                    if (slot == EquipSlot.RightWeapon && RightViewModel != null)
                    {
                        RightViewModel.SetState(ViewModelState.Fire, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                        Hands.SetState(ViewModelState.Fire, RightViewModel, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                    }
                    else if (slot == EquipSlot.LeftWeapon && LeftViewModel != null)
                    {
                        LeftViewModel.SetState(ViewModelState.Fire, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                        Hands.SetState(ViewModelState.Fire, LeftViewModel, IsADS ? ViewModelHandednessState.ADS : ViewModelHandednessState.TwoHanded);
                    }

                    if (useAmmo && autoReload && GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] <= 0)
                    {
                        DoReload();
                    }

                }
                else
                {
                    Debug.LogError("Can't find item model for ranged weapon!");
                }

            }


            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepFired"));
        }

        private void DoReload()
        {
            if (IsReloading) //I think we need this guard
                return;

            //TODO reload both

            reloadSide(EquipSlot.RightWeapon, RightViewModel);
            reloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsADS = false;
            IsReloading = true;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloading"));

            void reloadSide(EquipSlot slot, WeaponViewModelScript viewModel)
            {
                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {
                    if (GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        //unreloadable condition
                        if (GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] == rwim.MagazineSize
                            || GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()) <= 0)
                        {
                            return;
                        }

                        if (viewModel != null)
                        {
                            //TODO handle 1H/2H

                            viewModel.SetState(ViewModelState.Reload, ViewModelHandednessState.TwoHanded);
                            Hands.SetState(ViewModelState.Reload, viewModel, ViewModelHandednessState.TwoHanded);
                        }
                        //else if(!string.IsNullOrEmpty(rwim.ReloadEffect))
                        //    AudioPlayer.Instance.PlaySound(rwim.ReloadEffect, SoundType.Sound, false);

                        TimeToNext = Math.Max(rwim.ReloadTime, TimeToNext); //we take the longest time
                    }
                }

            }

        }

        private void FinishReload()
        {

            finishReloadSide(EquipSlot.RightWeapon, RightViewModel);
            finishReloadSide(EquipSlot.LeftWeapon, LeftViewModel);

            IsReloading = false;

            QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("WepReloaded"));

            void finishReloadSide(EquipSlot slot, WeaponViewModelScript viewModel)
            {
                if (GameState.Instance.PlayerRpgState.Equipped.ContainsKey(slot))
                {
                    if (GameState.Instance.PlayerRpgState.Equipped[slot].ItemModel is RangedWeaponItemModel rwim)
                    {
                        int currentAmmo = GameState.Instance.PlayerRpgState.AmmoInMagazine[slot];
                        int qty = Math.Min(rwim.MagazineSize - currentAmmo, GameState.Instance.PlayerRpgState.Inventory.CountItem(rwim.AType.ToString()));
                        GameState.Instance.PlayerRpgState.AmmoInMagazine[slot] = qty + currentAmmo;
                        GameState.Instance.PlayerRpgState.Inventory.RemoveItem(rwim.AType.ToString(), qty);

                        if (viewModel != null)
                        {
                            //TODO handle 1H/2H
                            viewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded);
                            Hands.SetState(ViewModelState.Idle, viewModel, ViewModelHandednessState.TwoHanded);
                        }
                    }
                }
            }

        }

        //this is confusing and bloated because everything is pretty much designed around equipping/unequipping weapons being the same scenario
        //but they're actually quite different
        public void HandleWeaponChange(EquipSlot slot) //temporarily public because we're still running the message loop in PlayerController
        {
            //we should probably cache this at a higher level but it's probably not safe
            var player = GameState.Instance.PlayerRpgState;

            //reset ADS and accumulators
            IsADS = false;
            ResetAccumulators(); //probably exploitable

            if (slot == EquipSlot.RightWeapon)
            {
                //handle equip/unequip melee weapon
                if (player.Equipped.ContainsKey(EquipSlot.RightWeapon) && player.Equipped[EquipSlot.RightWeapon] != null)
                {
                    //fixed to equip *right* weapon
                    Debug.Log("Equipped right weapon!");

                    WeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.RightWeapon].ItemModel as WeaponItemModel;
                    if (wim != null && !string.IsNullOrEmpty(wim.ViewModel))
                    {
                        var prefab = CoreUtils.LoadResource<GameObject>("WeaponViewModels/" + wim.ViewModel);
                        if (prefab != null)
                        {
                            var go = Instantiate<GameObject>(prefab, RightViewModelPoint);
                            RightViewModel = go.GetComponent<WeaponViewModelScript>();
                            if (RightViewModel != null)
                                RightViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded); //TODO handle 1H/2H
                        }

                    }

                }
                else
                {
                    //fixed to unequip *right* model
                    Debug.Log("Unequipped right weapon!");
                    if (RightViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(RightViewModelPoint.transform.GetChild(0).gameObject);
                    }
                    RightViewModel = null;
                }
            }
            else if (slot == EquipSlot.LeftWeapon)
            {
                IsReloading = false;
                TimeToNext = 0;

                //handle equip/unequip ranged weapon
                if (player.Equipped.ContainsKey(EquipSlot.LeftWeapon) && player.Equipped[EquipSlot.LeftWeapon] != null)
                {
                    //fixed to equip *left* model
                    Debug.Log("Equipped left weapon!");

                    WeaponItemModel wim = GameState.Instance.PlayerRpgState.Equipped[EquipSlot.LeftWeapon].ItemModel as WeaponItemModel;
                    if (wim != null && !string.IsNullOrEmpty(wim.ViewModel))
                    {
                        var prefab = CoreUtils.LoadResource<GameObject>("WeaponViewModels/" + wim.ViewModel);
                        if (prefab != null)
                        {
                            var go = Instantiate<GameObject>(prefab, LeftViewModelPoint);
                            LeftViewModel = go.GetComponent<WeaponViewModelScript>();
                            if (LeftViewModel != null)
                                LeftViewModel.SetState(ViewModelState.Idle, ViewModelHandednessState.TwoHanded); //TODO handle 1H/2H
                        }

                    }
                }
                else
                {
                    //fixed to unequip *left* model
                    Debug.Log("Unequipped left weapon!");
                    if (LeftViewModelPoint.transform.childCount > 0)
                    {
                        Destroy(LeftViewModelPoint.transform.GetChild(0).gameObject);
                    }
                    LeftViewModel = null;
                }
            }

            Hands.SetState(ViewModelState.Idle, LeftViewModel, ViewModelHandednessState.TwoHanded); //TODO handle 1H/2H
        }

        private ITakeDamage GetMeleeHit(Transform origin, float range)
        {
            LayerMask lm = LayerMask.GetMask("Default", "ActorHitbox");
            var rc = Physics.RaycastAll(origin.position, origin.forward, range, lm, QueryTriggerInteraction.Collide);

            //TODO handle 2D/3D probe distance

            //totally fucked!
            ITakeDamage ac = null;
            foreach (var r in rc)
            {
                var go = r.collider.gameObject;
                var ahgo = go.GetComponent<ActorHitboxComponent>();
                if (ahgo != null)
                {
                    ac = ahgo.ParentController as ITakeDamage; //this works as long as we don't go MP or do Voodoo Dolls
                    break;
                }
                var acgo = go.GetComponent<ActorController>();
                if (acgo != null)
                {
                    ac = acgo;
                    break;
                }
            }

            return ac;
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

            if(TimeToNextKick <= 0)
            {
                if (MappedInput.GetButtonDown(DefaultControls.Offhand1))
                {
                    if (OffhandKickAnimator != null)
                    {
                        OffhandKickAnimator.gameObject.SetActive(true);
                        OffhandKickAnimator.Play("Kick");
                    }

                    OffhandKickSound.Ref()?.Play();

                    //deal damage
                    var itd = GetMeleeHit(OffhandKickPoint, OffhandKickRange);
                    if (itd != null)
                        itd.TakeDamage(new ActorHitInfo(OffhandKickDamage, 0, (int)DamageType.Impact, (int)ActorBodyPart.Unspecified, (int)DefaultHitMaterials.Unspecified, PlayerController));

                    //kick away kickable things
                    //if(Physics.Raycast(OffhandKickPoint.position, OffhandKickPoint.forward, out var hit, OffhandKickRange))
                    if(Physics.BoxCast(OffhandKickPoint.position, Vector3.one * 0.25f, OffhandKickPoint.forward, out var hit, Quaternion.identity, OffhandKickRange))
                    {
                        Rigidbody rb = hit.collider.attachedRigidbody;
                        if(rb != null)
                        {
                            rb.AddForce(OffhandKickPoint.forward * OffhandKickForce, ForceMode.Impulse);
                        }
                    }

                    TimeToNextKick = OffhandKickDelay;
                }
            }


        }

    }

    public enum ViewModelHandednessState
    {
        //two-handed is the "default"
        TwoHanded = 0, OneHanded, ADS
    }
}