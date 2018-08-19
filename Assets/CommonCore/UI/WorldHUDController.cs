using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Messaging;
using CommonCore.StringSub;
using CommonCore.State;
using CommonCore.Rpg;

namespace CommonCore.UI
{
    public class WorldHUDController : MonoBehaviour
    {
        public static WorldHUDController Current { get; private set; }

        public Text TargetText;
        public Text MessageText;
        public ScrollRect MessageScrollRect;

        public Slider HealthSlider;
        public Text HealthText;

        public Slider ShieldSlider;
        public Text ShieldText;

        public Slider EnergySlider;
        public Text EnergyText;

        public Text RangedWeaponText;
        public Text AmmoText;
        public Text MeleeWeaponText;
        public Image ReadyBarImage;

        public Text SubtitleText;
        private float SubtitleTimer;
        private int SubtitlePriority = int.MinValue;
        
        private QdmsMessageInterface MessageInterface;

        //local state is, as it turns out, unavoidable
        private bool WeaponReady = true;

        void Awake()
        {
            MessageInterface = new QdmsMessageInterface();
            Current = this;
        }

        void Start()
        {
            MessageText.text = string.Empty;

            UpdateStatusDisplays();
            UpdateWeaponDisplay();
            UpdateSubtitles();
        }
        
        void Update()
        {
            //this is all slow and dumb and temporary... which means it'll probably be untouched until Ferelden

            while(MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }

            UpdateStatusDisplays();
            UpdateSubtitles();
        }

        private void HandleMessage(QdmsMessage message)
        {
            if(message is HUDPushMessage)
            {
                MessageText.text = MessageText.text + "\n" + Sub.Macro(((HUDPushMessage)message).Contents);
                Canvas.ForceUpdateCanvases();
                MessageScrollRect.verticalNormalizedPosition = 0;
            }
            else if(message is QdmsFlagMessage)
            {
                string flag = ((QdmsFlagMessage)message).Flag;
                switch (flag)
                {
                    case "RpgChangeWeapon":
                        UpdateWeaponDisplay();
                        break;
                    case "WepFired":
                        WeaponReady = false;
                        UpdateWeaponDisplay();
                        break;
                    case "WepReady":
                        WeaponReady = true;
                        UpdateWeaponDisplay();
                        break;
                }
            }
            else if(message is SubtitleMessage)
            {
                SubtitleMessage subMessage = (SubtitleMessage)message;
                if(subMessage.Priority > SubtitlePriority)
                {
                    SubtitlePriority = subMessage.Priority;
                    SubtitleTimer = subMessage.HoldTime;
                    SubtitleText.text = subMessage.UseSubstitution ? Sub.Macro(subMessage.Contents) : subMessage.Contents;
                }
            }
        }

        private void UpdateSubtitles()
        {

            if(SubtitleTimer <= 0)
            {
                SubtitleText.text = string.Empty;
                SubtitlePriority = int.MinValue;
            }
            else
            {
                SubtitleTimer -= Time.deltaTime;
            }
        }

        private void UpdateStatusDisplays()
        {
            var player = GameState.Instance.PlayerRpgState;
            HealthText.text = player.Health.ToString("f0");
            HealthSlider.value = player.HealthFraction;

            EnergyText.text = player.Energy.ToString("f0");
            EnergySlider.value = player.EnergyFraction;
        }

        //this needs to die in a fire, the degree of interdependency is insane
        private void UpdateWeaponDisplay()
        {
            var player = GameState.Instance.PlayerRpgState;

            if(player.IsEquipped(EquipSlot.RangedWeapon))
            {
                RangedWeaponText.text = InventoryModel.GetName(player.Equipped[EquipSlot.RangedWeapon].ItemModel);
                AmmoType atype = ((RangedWeaponItemModel)player.Equipped[EquipSlot.RangedWeapon].ItemModel).AType;
                if(atype != AmmoType.NoAmmo)
                {
                    AmmoText.text = string.Format("{1}/{2} [{0}]", atype.ToString(), "-", player.Inventory.CountItem(atype.ToString()));
                    //TODO magazine
                }
                else
                {
                    AmmoText.text = "- / -";

                }
            }
            else
            {
                RangedWeaponText.text = "Not Equipped";
                AmmoText.text = "- / -";
            }

            if (player.IsEquipped(EquipSlot.MeleeWeapon))
            {
                MeleeWeaponText.text = InventoryModel.GetName(player.Equipped[EquipSlot.MeleeWeapon].ItemModel);
            }
            else
            {
                MeleeWeaponText.text = "Not Equipped";
            }

            //TODO tri-state (disabled)
            if(WeaponReady)
            {
                ReadyBarImage.color = Color.green;
            }
            else
            {
                ReadyBarImage.color = Color.red;
            }
        }

        //TODO move to messaging

        internal void ClearTarget()
        {
            TargetText.text = string.Empty;
        }

        internal void SetTargetMessage(string message)
        {
            TargetText.text = message;
        }
    }
}