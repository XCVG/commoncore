using UnityEngine;
using UnityEngine.UI;
using CommonCore.Rpg;
using CommonCore.RpgGame.UI;
using CommonCore.State;

namespace CommonCore.UI
{

    public class StatusPanelController : PanelController
    {
        public bool CheckLevelUp = true;

        public RawImage CharacterImage;
        public Text HealthText;
        public Text ArmorText;
        public Text AmmoText;

        public override void SignalPaint()
        {
            CharacterModel pModel = GameState.Instance.PlayerRpgState;
            //PlayerControl pControl = PlayerControl.Instance;

            //repaint 
            HealthText.text = string.Format("Health: {0}/{1}", (int) pModel.Health, (int) pModel.DerivedStats.MaxHealth);
            ArmorText.text = string.Format("Level: {0} ({1}/{2} XP)\n", pModel.Level, pModel.Experience, RpgValues.XPToNext(pModel.Level));

            string equipText = string.Format("Armor: {0}\nRanged: {1}\nMelee: {2}", 
                GetNameForSlot(EquipSlot.Body, pModel), GetNameForSlot(EquipSlot.RangedWeapon, pModel), GetNameForSlot(EquipSlot.MeleeWeapon, pModel));

            AmmoText.text = equipText;

            //this is now somewhat broken because there are more choices in the struct
            string rid = pModel.Gender == Sex.Female ? "portrait_f" : "portrait_m";
            CharacterImage.texture = CoreUtils.LoadResource<Texture2D>("UI/Portraits/" + rid);
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
            if(CheckLevelUp && GameState.Instance.PlayerRpgState.Experience >= RpgValues.XPToNext(GameState.Instance.PlayerRpgState.Level))
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