using CommonCore.Messaging;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.StringSub;
using CommonCore.UI;
using CommonCore.World;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.RpgGame.UI
{
    public class RpgHUDController : BaseHUDController
    {
        [Header("Top Bar")]
        public Text TargetText;
        public Slider TargetHealthLeft;
        public Slider TargetHealthRight;

        [Header("Left Bar")]
        public Slider HealthSlider;
        public Text HealthText;

        public Slider ShieldSlider;
        public Text ShieldText;

        public Slider EnergySlider;
        public Text EnergyText;

        [Header("Right Bar")]
        public Text RightWeaponText;
        public Text RightAmmoText;
        public Text RightAmmoReserveText;
        public Text RightAmmoTypeText;

        [Header("Misc")]
        public Image Crosshair;

        //local state is, as it turns out, unavoidable
        private bool WeaponReady = true;

        private string OverrideTarget = null;

        protected override void Start()
        {
            base.Start();

            UpdateStatusDisplays();
            UpdateWeaponDisplay();

            ClearTarget();
        }
        
        protected override void Update()
        {
            //this is all slow and dumb and temporary... which means it'll probably be untouched until Ferelden
            base.Update();

            UpdateStatusDisplays();            
        }

        protected override bool HandleMessage(QdmsMessage message)
        {
            if(base.HandleMessage(message))
            {
                return true;
            }
            else if(message is HUDPushMessage)
            {
                AppendHudMessage(Sub.Macro(((HUDPushMessage)message).Contents));
                return true;
            }
            else if(message is QdmsKeyValueMessage kvmessage)
            {
                switch (kvmessage.Flag)
                {
                    case "PlayerHasTarget":
                        SetTargetMessage(kvmessage.GetValue<string>("Target"));
                        break;
                    case "RpgBossHealthUpdate":
                        UpdateTargetOverrideHealth(kvmessage.GetValue<string>("Target"), kvmessage.GetValue<float>("Health"));
                        break;
                    case "RpgBossAwake":
                        SetTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                    case "RpgBossDead":
                        ClearTargetOverride(kvmessage.GetValue<string>("Target"));
                        break;
                }

                return true; //probably the wrong spot
            }
            else if(message is QdmsFlagMessage flagmessage)
            {
                switch (flagmessage.Flag)
                {                    
                    case "RpgChangeWeapon":
                        UpdateWeaponDisplay();
                        break;
                    case "WepReloading":
                    case "WepFired":
                        WeaponReady = false;
                        UpdateWeaponDisplay();
                        break;
                    case "WepReady":
                    case "WepReloaded":
                        WeaponReady = true;
                        UpdateWeaponDisplay();
                        break;
                    case "PlayerChangeView":
                        SetCrosshair(message);
                        break;
                    case "PlayerClearTarget":
                        ClearTarget();
                        break;
                    case "RpgQuestStarted":
                    case "RpgQuestEnded":
                        AddQuestMessage(message);
                        break;
                }

                return true;
            }

            return false;

        }

        private void SetCrosshair(QdmsMessage message)
        {
            //we actually don't care much if this fails
            //it'll throw an ugly exception but won't break anything

            var newView = ((QdmsKeyValueMessage)(message)).GetValue<PlayerViewType>("ViewType");
            if (newView == PlayerViewType.ForceFirst || newView == PlayerViewType.PreferFirst)
                Crosshair.gameObject.SetActive(true);
            else if(newView == PlayerViewType.ForceThird || newView == PlayerViewType.PreferThird)
                Crosshair.gameObject.SetActive(false);
            else
                Crosshair.gameObject.SetActive(false);
        }

        private void UpdateStatusDisplays()
        {
            var player = GameState.Instance.PlayerRpgState;
            HealthText.text = player.Health.ToString("f0");
            HealthSlider.value = player.HealthFraction;

            EnergyText.text = player.Energy.ToString("f0");
            EnergySlider.value = player.EnergyFraction;

            //null out the shields for now
            ShieldText.text = "";
            ShieldSlider.value = 0;
        }

        private void UpdateWeaponDisplay()
        {
            var player = GameState.Instance.PlayerRpgState;

            //ignore the left weapon even if it exists
            if (player.IsEquipped(EquipSlot.RightWeapon))
            {
                RightWeaponText.text = InventoryModel.GetNiceName(player.Equipped[EquipSlot.RightWeapon].ItemModel);
                if (player.Equipped[EquipSlot.RightWeapon].ItemModel is RangedWeaponItemModel rwim && !(rwim.AType == AmmoType.NoAmmo))
                {
                    RightAmmoText.text = player.AmmoInMagazine[EquipSlot.RightWeapon].ToString();
                    RightAmmoReserveText.text = player.Inventory.CountItem(rwim.AType.ToString()).ToString();
                    RightAmmoTypeText.text = InventoryModel.GetNiceName(InventoryModel.GetModel(rwim.AType.ToString()));
                }
                else
                {
                    RightAmmoText.text = "-";
                    RightAmmoReserveText.text = "-";
                    RightAmmoTypeText.text = "";
                }
            }
            else
            {
                RightWeaponText.text = "No Weapon";
                RightAmmoText.text = "-";
                RightAmmoReserveText.text = "-";
                RightAmmoTypeText.text = "";
            }

            /*
            //right weapon
            updateWeaponText(player, EquipSlot.RightWeapon, RightWeaponText, RightAmmoText);
            //left weapon
            updateWeaponText(player, EquipSlot.LeftWeapon, LeftWeaponText, LeftAmmoText);

            void updateWeaponText(CharacterModel playerModel, EquipSlot slot, Text weaponText, Text ammoText)
            {
                if(playerModel.IsEquipped(slot))
                {
                    weaponText.text = InventoryModel.GetNiceName(playerModel.Equipped[slot].ItemModel);
                    if (playerModel.Equipped[slot].ItemModel is RangedWeaponItemModel rwim && !(rwim.AType == AmmoType.NoAmmo))
                    {
                        ammoText.text = string.Format("{1}/{2} [{0}]", rwim.AType.ToString(), playerModel.AmmoInMagazine[slot], playerModel.Inventory.CountItem(rwim.AType.ToString()));
                    }
                    else
                    {
                        ammoText.text = "- / -";
                    }
                }
                else
                {
                    weaponText.text = "Not Equipped";
                    ammoText.text = "- / -";
                }
            }
            */

        }

        private void AddQuestMessage(QdmsMessage message)
        {
            var qMessage = message as QdmsKeyValueMessage;
            if (qMessage == null)
            {
                return;
            }                
            else if(qMessage.Flag == "RpgQuestStarted")
            {
                var qRawName = qMessage.GetValue<string>("Quest");
                var qDef = QuestModel.GetDef(qRawName);
                string questName = qDef == null ? qRawName : qDef.NiceName;
                AppendHudMessage(string.Format("Quest Started: {0}", questName));
            }
            else if(qMessage.Flag == "RpgQuestEnded")
            {
                var qRawName = qMessage.GetValue<string>("Quest");
                var qDef = QuestModel.GetDef(qRawName);
                string questName = qDef == null ? qRawName : qDef.NiceName;
                AppendHudMessage(string.Format("Quest Ended: {0}", questName));
            }
        }

        //handle target text, override target text, health bar

        private void ClearTarget()
        {
            TargetText.text = string.Empty;

            if (!string.IsNullOrEmpty(OverrideTarget))
                TargetText.text = OverrideTarget;
        }

        private void SetTargetMessage(string message)
        {
            if (!string.IsNullOrEmpty(OverrideTarget))
                return;

            TargetText.text = message;
        }

        private void SetTargetOverride(string overrideTarget)
        {
            OverrideTarget = overrideTarget;
            TargetText.text = overrideTarget;

            TargetHealthLeft.gameObject.SetActive(true);
            TargetHealthRight.gameObject.SetActive(true);
            TargetHealthLeft.value = 1;
            TargetHealthRight.value = 1;
        }

        private void UpdateTargetOverrideHealth(string overrideTarget, float health)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(RpgHUDController)}] Updated override target health for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
                SetTargetOverride(overrideTarget);                
            }

            TargetHealthLeft.value = health;
            TargetHealthRight.value = health;
        }

        private void ClearTargetOverride(string overrideTarget)
        {
            if (OverrideTarget == null || OverrideTarget != overrideTarget)
            {
                Debug.LogWarning($"[{nameof(RpgHUDController)}] Cleared override target for a different target than expected (old: \"{OverrideTarget}\", new: \"{overrideTarget}\")");
            }

            OverrideTarget = null;
            ClearTarget();

            TargetHealthLeft.gameObject.SetActive(false);
            TargetHealthRight.gameObject.SetActive(false);
        }
    }
}