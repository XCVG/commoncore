using UnityEngine;
using UnityEngine.UI;
using CommonCore.UI;
using CommonCore.State;
using CommonCore.RpgGame.Rpg;
using UnityEngine.Serialization;
using CommonCore.StringSub;

namespace CommonCore.RpgGame.UI
{

    /// <summary>
    /// Controller for the Status panel, displaying character portrait, some text, and handling level up button and prompt
    /// </summary>
    public class StatusPanelController : PanelController
    {
        private const string SubList = "RPG_AV";

        public bool CheckLevelUp = true;

        public RawImage CharacterImage;
        public Text HealthText;
        public Text ShieldsText;
        public Text EnergyText;
        public Text MagicText;
        public Text LevelText;
        public Text EquipText;
        public Button LevelUpButton;

        public override void SignalPaint()
        {
            CharacterModel pModel = GameState.Instance.PlayerRpgState;
            //PlayerControl pControl = PlayerControl.Instance;

            //realistically, I can't see anyone using most of these, especially since their 
            if(HealthText != null)
                HealthText.text = string.Format("{2}: {0}/{1}", (int) pModel.Health, (int) pModel.DerivedStats.MaxHealth, Sub.Replace("Health", SubList));

            if(LevelText != null)
                LevelText.text = string.Format("{3}: {0} ({1}/{2} XP)\n", pModel.Level, pModel.Experience, RpgValues.XPToNext(pModel.Level), Sub.Replace("Level", SubList));

            if (ShieldsText != null)
                ShieldsText.text = string.Format("{2}: {0}/{1}", (int)pModel.Shields, (int)pModel.DerivedStats.ShieldParams.MaxShields, Sub.Replace("Shields", SubList));

            if (EnergyText != null)
                EnergyText.text = string.Format("{2}: {0}/{1}", (int)pModel.Energy, (int)pModel.DerivedStats.MaxEnergy, Sub.Replace("Energy", SubList));

            if (MagicText != null)
                MagicText.text = string.Format("{2}: {0}/{1}", (int)pModel.Magic, (int)pModel.DerivedStats.MaxMagic, Sub.Replace("Magic", SubList));

            if(EquipText != null)
            {
                EquipText.text = string.Format("Armor: {0}\nLH Weapon: {1}\nRH Weapon: {2}",
                GetNameForSlot(EquipSlot.Body, pModel), GetNameForSlot(EquipSlot.LeftWeapon, pModel), GetNameForSlot(EquipSlot.RightWeapon, pModel));
            }

            if(LevelUpButton != null)
                LevelUpButton.interactable = !GameState.Instance.MenuGameStateLocked;

            //this is now somewhat broken because there are more choices in the struct
            if(CharacterImage != null)
            {
                string rid;
                switch (pModel.Gender)
                {
                    case Sex.Female:
                        rid = "portrait_f";
                        break;
                    case Sex.Male:
                        rid = "portrait_m";
                        break;
                    case Sex.Other:
                        rid = "portrait_o";
                        break;
                    default:
                        rid = "portrait";
                        break;
                }
                CharacterImage.texture = CoreUtils.LoadResource<Texture2D>("UI/Portraits/" + rid);
            }
            
        }

        //will generalize and move this
        private string GetNameForSlot(EquipSlot slot, CharacterModel pModel)
        {
            if (!pModel.Equipped.ContainsKey(slot))
                return "none";

            InventoryItemInstance itemInstance = pModel.Equipped[slot];
            if (itemInstance != null)
            {
                var def = InventoryModel.GetDef(itemInstance.ItemModel.Name);
                if (def != null)
                    return def.NiceName;
                else
                    return itemInstance.ItemModel.Name;
            }
            else return "none";
        }

        void OnEnable()
        {
            //why is this not on SignalPaint? hell if I know
            if(CheckLevelUp && !GameState.Instance.MenuGameStateLocked && GameState.Instance.PlayerRpgState.Experience >= RpgValues.XPToNext(GameState.Instance.PlayerRpgState.Level))
            {
                DefaultLevelUpModal.PushModal(OnLevelUpDone);
            }
        }

        private void OnLevelUpDone()
        {
            SignalPaint();
        }

        public void OnClickOpenLevelDialog()
        {
            DefaultLevelUpModal.PushModal(SignalPaint);
        }

    }
}