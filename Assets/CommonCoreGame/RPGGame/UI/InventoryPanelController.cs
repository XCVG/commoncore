using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.Messaging;
using CommonCore.State;
using CommonCore.World;
using CommonCore.UI;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.World;
using System.Linq;

namespace CommonCore.RpgGame.UI
{

    public class InventoryPanelController : PanelController
    {
        public bool ApplyTheme = true;

        public GameObject ItemTemplatePrefab;
        public RectTransform ScrollContent;

        public Text SelectedItemText;
        public RawImage SelectedItemImage;
        public Text SelectedItemDescription;
        public Text SelectedItemStats;
        public Button SelectedItemUse;
        public Button SelectedItemUse2;
        public Button SelectedItemDrop;

        private int SelectedItem;
        private InventoryItemInstance[] ItemLookupTable;

        public override void SignalPaint()
        {
            SelectedItem = -1;
            PaintInventoryList();
            ClearDetailPane();
        }

        private void PaintInventoryList()
        {
            foreach(Transform t in ScrollContent)
            {
                Destroy(t.gameObject);
            }
            ScrollContent.DetachChildren();

            List<InventoryItemInstance> itemList = GameState.Instance.PlayerRpgState.Inventory.EnumerateItems().ToList();

            ItemLookupTable = new InventoryItemInstance[itemList.Count];

            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];

                if (item.ItemModel.Hidden)
                    continue;

                GameObject itemGO = Instantiate<GameObject>(ItemTemplatePrefab, ScrollContent);
                if(!item.ItemModel.Stackable)
                    itemGO.GetComponentInChildren<Text>().text = InventoryModel.GetNiceName(item.ItemModel) + (item.Equipped ? " [Equipped]" : string.Empty); //for now
                else
                    itemGO.GetComponentInChildren<Text>().text = string.Format("{0} ({1})", InventoryModel.GetNiceName(item.ItemModel), item.Quantity); //for now
                Button b = itemGO.GetComponent<Button>();
                int lexI = i;
                b.onClick.AddListener(delegate { OnItemSelected(lexI); }); //scoping is weird here
                ItemLookupTable[i] = item;
                if(ApplyTheme)
                    ApplyThemeToElements(b.transform);
            }
        }

        public void OnItemSelected(int i)
        {
            //Debug.Log(i);
            SelectedItem = i;
            ClearDetailPane();
            PaintSelectedItem();
        }

        public void OnItemUsed(int button)
        {
            //handle equipping an item
            if(SelectedItem >= 0)
            {
                InventoryItemInstance itemInstance = ItemLookupTable[SelectedItem];
                InventoryItemModel itemModel = itemInstance.ItemModel;

                if(itemModel is WeaponItemModel)
                {
                    if(itemInstance.Equipped)
                    {
                        GameState.Instance.PlayerRpgState.UnequipItem(itemInstance);                        
                    }
                    else
                    {
                        GameState.Instance.PlayerRpgState.EquipItem(itemInstance);

                        SelectedItemText.text = SelectedItemText.text + " [E]"; //needed?
                    }
                }
                else if (itemModel is ArmorItemModel)
                {
                    if (itemInstance.Equipped)
                    {
                        GameState.Instance.PlayerRpgState.UnequipItem(itemInstance);
                    }
                    else
                    {
                        GameState.Instance.PlayerRpgState.EquipItem(itemInstance);

                        SelectedItemText.text = SelectedItemText.text + " [E]";
                    }
                }
                else if(itemModel is AidItemModel)
                {
                    var aim = (AidItemModel)itemModel;
                    aim.Apply();
                    GameState.Instance.PlayerRpgState.Inventory.RemoveItem(ItemLookupTable[SelectedItem]);

                    string message = string.Format("{0} {1} {2}", aim.RType.ToString(), aim.Amount, aim.AType.ToString()); //temporary, will fix this up with lookups later
                    Modal.PushMessageModal(message, "Aid Applied", null, null, true);
                }

                SignalPaint();
            }
        }

        public void OnItemDropped()
        {
            if (SelectedItem >= 0)
            {
                InventoryItemInstance itemInstance = ItemLookupTable[SelectedItem];
                InventoryItemModel itemModel = itemInstance.ItemModel;

                if (itemModel.Essential)
                {
                    Debug.LogWarning("Tried to drop an essential item!");
                }

                if (itemInstance.Equipped)
                {
                    GameState.Instance.PlayerRpgState.UnequipItem(itemInstance);
                }

                if (itemModel.Stackable)
                {
                    //do quantity selection with modal dialogue if inventory is stackable

                    Modal.PushQuantityModal("Quantity To Drop", 0, itemInstance.Quantity, itemInstance.Quantity, true, string.Empty,
                        delegate (ModalStatusCode status, string tag, int quantity) { CompleteItemDrop(itemInstance, itemModel, quantity, status); }, true);
                    //like I don't see why that won't work but holy shit is it ugly
                
                }
                else
                {
                    CompleteItemDrop(itemInstance, itemModel, 1, ModalStatusCode.Complete);
                }

                
            }

        }

        private void CompleteItemDrop(InventoryItemInstance itemInstance, InventoryItemModel itemModel, int quantity, ModalStatusCode status)
        {
            if (quantity == 0 || status != ModalStatusCode.Complete)
                return;

            GameState.Instance.PlayerRpgState.Inventory.RemoveItem(itemInstance, quantity);
            Transform playerT = WorldUtils.GetPlayerObject().transform;
            Vector3 dropPos = (playerT.position + (playerT.forward.normalized * 1.0f));
            RpgWorldUtils.DropItem(itemModel, quantity, dropPos);

            SignalPaint();
        }

        private void PaintSelectedItem()
        {
            var itemModel = ItemLookupTable[SelectedItem].ItemModel;
            SelectedItemText.text = itemModel.Name;
            var itemDef = InventoryModel.GetDef(itemModel.Name);
            if(itemDef == null)
            {
                SelectedItemDescription.text = "{missing def}";
            }
            else
            {
                SelectedItemText.text = itemDef.NiceName;
                SelectedItemDescription.text = itemDef.Description;
                Texture2D tex = CoreUtils.LoadResource<Texture2D>("UI/Icons/" + itemDef.Image);
                if (tex != null)
                    SelectedItemImage.texture = tex;
            }

            SelectedItemStats.text = itemModel.GetStatsString();

            //handle equipped button and state
            if(itemModel is WeaponItemModel || itemModel is ArmorItemModel)
            {
                if (ItemLookupTable[SelectedItem].Equipped)
                {
                    SelectedItemText.text = SelectedItemText.text + " [E]";
                    SelectedItemUse.gameObject.SetActive(true);
                    SelectedItemUse.transform.Find("Text").GetComponent<Text>().text = "Unequip";
                }
                else
                {
                    SelectedItemUse.gameObject.SetActive(true);
                    SelectedItemUse.transform.Find("Text").GetComponent<Text>().text = "Equip";
                }                
            }
            else if (itemModel is AidItemModel)
            {
                SelectedItemUse.gameObject.SetActive(true);
                SelectedItemUse.transform.Find("Text").GetComponent<Text>().text = "Use";
            }

            if(!itemModel.Essential && (GameParams.AllowItemDropOutsideWorldScene || WorldUtils.IsWorldScene()) && !GameState.Instance.PlayerFlags.Contains(PlayerFlags.NoDropItems))
                SelectedItemDrop.gameObject.SetActive(true);
            else
                SelectedItemDrop.gameObject.SetActive(false);

            if (itemModel.Stackable)
            {
                int qty = ItemLookupTable[SelectedItem].Quantity;
                SelectedItemText.text = SelectedItemText.text + string.Format(" ({0})", qty);
            }

            bool allowInteraction = AllowGameStateInteraction;
            SelectedItemUse.interactable = allowInteraction;
            SelectedItemUse2.interactable = allowInteraction;
            SelectedItemDrop.interactable = allowInteraction;
        }

        private void ClearDetailPane()
        {
            SelectedItemText.text = string.Empty;
            SelectedItemDescription.text = string.Empty;
            SelectedItemStats.text = string.Empty;
            SelectedItemImage.texture = null;
            SelectedItemUse.gameObject.SetActive(false);
            SelectedItemUse2.gameObject.SetActive(false);
            SelectedItemDrop.gameObject.SetActive(false);
        }
    }
}