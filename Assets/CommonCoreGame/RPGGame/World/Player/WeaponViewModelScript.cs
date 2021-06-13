using CommonCore.RpgGame.Rpg;
using CommonCore.World;
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

    public static class ViewModelUtils
    {
        public static void EjectShell(Transform shellEjectPoint, string shellPrefab, PlayerWeaponComponent weaponComponent)
        {
            if (shellEjectPoint == null || shellPrefab == null || shellEjectPoint.childCount == 0)
            {
                //can't eject shell
                return;
            }

            Transform shellDirTransform = shellEjectPoint.GetChild(0);
            ShellEjectionComponent shellEjectionComponent = shellEjectPoint.GetComponent<ShellEjectionComponent>();

            //var shell = Instantiate(ShellPrefab, ShellEjectPoint.position, ShellEjectPoint.rotation, CoreUtils.GetWorldRoot());
            var shell = WorldUtils.SpawnEffect(shellPrefab, shellEjectPoint.position, shellEjectPoint.rotation.eulerAngles, CoreUtils.GetWorldRoot(), false);

            if (shell == null)
                return;

            //shell parameters (use ShellEjectionComponent if available)
            float shellScale;
            float shellVelocity;
            float shellTorque;
            float shellRandomVelocity;
            float shellRandomTorque;

            if (shellEjectionComponent)
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
            if (shellRB != null)
            {
                Vector3 velocityDirection = shellDirTransform.forward;

                Vector3 playerVelocity = weaponComponent.Ref()?.PlayerController.Ref()?.MovementComponent.Ref()?.Velocity ?? Vector3.zero;
                Vector3 randomVelocity = new Vector3(UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity, UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity, UnityEngine.Random.Range(-1f, 1f) * shellRandomVelocity);

                Vector3 velocity = velocityDirection * shellVelocity;
                shellRB.AddForce(velocity + playerVelocity + randomVelocity, ForceMode.VelocityChange);

                Vector3 randomTorque = new Vector3(UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque, UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque, UnityEngine.Random.Range(-1f, 1f) * shellRandomTorque);

                shellRB.AddTorque(velocity * shellTorque, ForceMode.VelocityChange);
            }
        }

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