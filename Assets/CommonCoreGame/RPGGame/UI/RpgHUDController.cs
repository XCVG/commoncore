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
        public Text LeftWeaponText;
        public Text LeftAmmoText;
        public Image ReadyBarImage;

        [Header("Misc")]
        public Image Crosshair;

        //local state is, as it turns out, unavoidable
        private bool WeaponReady = true;

        protected override void Start()
        {
            base.Start();

            UpdateStatusDisplays();
            UpdateWeaponDisplay();            
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
            else if(message is QdmsFlagMessage)
            {
                string flag = ((QdmsFlagMessage)message).Flag;
                switch (flag)
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
        }

        private void UpdateWeaponDisplay()
        {
            var player = GameState.Instance.PlayerRpgState;

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

        //TODO move to messaging

        public void ClearTarget()
        {
            TargetText.text = string.Empty;
        }

        public void SetTargetMessage(string message)
        {
            TargetText.text = message;
        }
    }
}