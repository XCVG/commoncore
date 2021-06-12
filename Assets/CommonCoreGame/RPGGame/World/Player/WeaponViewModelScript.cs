using CommonCore.RpgGame.Rpg;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.World
{
    public enum ViewModelState
    {
        Idle, Raise, Lower, Block, Reload, Charge, Fire, Recock //(we may remove or defer Charge and Recock though we kinda need them for bows and bolt guns respectively)
    }

    public class ViewModelOptions
    {
        //these are the ones you'd generally look at
        public readonly bool UseShake;
        public readonly bool UseMovebob;
        public readonly bool UseCrosshair;
        public readonly bool AdsCrosshair;
        public readonly bool UseCharge;
        public readonly bool UseRecock;
        public readonly ViewModelSide Side;

        public readonly float LockTime;
        public readonly bool EffectWaitsForLockTime;

        //it's not recommended you refer to these but we make them available anyway
        public readonly InventoryItemInstance WeaponItemInstance;
        public readonly PlayerWeaponComponent WeaponComponent;
        public readonly WeaponViewShakeScript ShakeComponent;
        public readonly WeaponMovebobComponent MovebobComponent;

        public ViewModelOptions(InventoryItemInstance itemInstance, EquipSlot slot, PlayerWeaponComponent weaponComponent, WeaponViewShakeScript shakeComponent, WeaponMovebobComponent movebobComponent)
        {
            var wim = itemInstance.ItemModel as WeaponItemModel;
            UseShake = wim.CheckFlag(ItemFlag.WeaponShake);
            UseMovebob = !wim.CheckFlag(ItemFlag.WeaponNoMovebob);
            UseCrosshair = wim.CheckFlag(ItemFlag.WeaponUseCrosshair);
            AdsCrosshair = wim.CheckFlag(ItemFlag.WeaponCrosshairInADS);
            UseCharge = wim.CheckFlag(ItemFlag.WeaponHasCharge);
            UseRecock = wim.CheckFlag(ItemFlag.WeaponHasRecock);

            switch (slot)
            {
                case EquipSlot.LeftWeapon:
                    Side = ViewModelSide.Left;
                    break;
                case EquipSlot.RightWeapon:
                    Side = ViewModelSide.Right;
                    break;
                default:
                    Side = ViewModelSide.Undefined;
                    break;
            }

            LockTime = (wim as RangedWeaponItemModel)?.LockTime ?? 0f;
            EffectWaitsForLockTime = wim.CheckFlag(ItemFlag.WeaponEffectWaitsForLockTime);

            WeaponItemInstance = itemInstance;
            WeaponComponent = weaponComponent;
            ShakeComponent = shakeComponent;
            MovebobComponent = movebobComponent;
        }
    }

    public enum ViewModelSide
    {
        Undefined, Left, Center, Right
    }

    public abstract class WeaponViewModelScript : MonoBehaviour
    {
        public static readonly string HandsHidden = "Hidden";

        public virtual ViewModelOptions Options { get; set; }

        public virtual bool ViewHandlesCrosshair => false;

        protected abstract void Start();

        protected abstract void Update();

        public abstract void SetVisibility(bool visible);

        public abstract void SetState(ViewModelState newState, ViewModelHandednessState handedness, float timeScale);

        public abstract (string, float) GetHandAnimation(ViewModelState newState, ViewModelHandednessState handedness);

    }
}