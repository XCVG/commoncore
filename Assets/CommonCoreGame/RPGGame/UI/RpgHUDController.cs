using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Messaging;
using CommonCore.StringSub;
using CommonCore.State;
using CommonCore.RpgGame.Rpg;
using CommonCore.UI;

namespace CommonCore.RpgGame.UI
{
    public class RpgHUDController : BaseHUDController
    {
        //TODO move subtitle and messagebox handling into base

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

        public Image Crosshair;

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
                AppendHudMessage(Sub.Macro(((HUDPushMessage)message).Contents));
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
            }
            else if(message is SubtitleMessage)
            {
                SubtitleMessage subMessage = (SubtitleMessage)message;
                if(subMessage.Priority >= SubtitlePriority)
                {
                    SubtitlePriority = subMessage.Priority;
                    SubtitleTimer = subMessage.HoldTime;
                    SubtitleText.text = subMessage.UseSubstitution ? Sub.Macro(subMessage.Contents) : subMessage.Contents;
                }
            }
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
                    AmmoText.text = string.Format("{1}/{2} [{0}]", atype.ToString(), player.AmmoInMagazine, player.Inventory.CountItem(atype.ToString()));
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

        private void AppendHudMessage(string newMessage)
        {
            MessageText.text = MessageText.text + "\n" + newMessage;
            Canvas.ForceUpdateCanvases();
            MessageScrollRect.verticalNormalizedPosition = 0;
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