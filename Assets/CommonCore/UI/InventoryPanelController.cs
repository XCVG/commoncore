using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.Rpg;
using UnityEngine.UI;
using CommonCore.Messaging;
using CommonCore.State;

namespace CommonCore.UI
{

    public class InventoryPanelController : PanelController
    {
        public GameObject ItemTemplatePrefab;
        public RectTransform ScrollContent;

        public Text SelectedItemText;
        public RawImage SelectedItemImage;
        public Text SelectedItemDescription;
        public Text SelectedItemStats;
        public Button SelectedItemButton;
        public Button SelectedItemButton2;

        private int SelectedItem;
        private List<InventoryItemInstance> ItemLookupTable;

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

            List<InventoryItemInstance> itemList = GameState.Instance.PlayerRpgState.Inventory.GetItemsListActual();
            
            ItemLookupTable = new List<InventoryItemInstance>(itemList.Count);

            for (int i = 0; i < itemList.Count; i++)
            {
                var item = itemList[i];
                GameObject itemGO = Instantiate<GameObject>(ItemTemplatePrefab, ScrollContent);
                if(item.Quantity == InventoryItemInstance.UnstackableQuantity)
                    itemGO.GetComponentInChildren<Text>().text = item.ItemModel.Name; //for now
                else
                    itemGO.GetComponentInChildren<Text>().text = string.Format("{0} ({1})", item.ItemModel.Name, item.Quantity); //for now
                Button b = itemGO.GetComponent<Button>();
                int lexI = i;
                b.onClick.AddListener(delegate { OnItemSelected(lexI); }); //scoping is weird here
                ItemLookupTable.Add(item);
            }
        }

        public void OnItemSelected(int i)
        {
            //Debug.Log(i);
            SelectedItem = i;
            ClearDetailPane();
            PaintSelectedItem();
        }

        public void OnItemUsed()
        {
            //handle equipping an item
            if(SelectedItem >= 0)
            {
                InventoryItemInstance itemInstance = ItemLookupTable[SelectedItem];
                InventoryItemModel itemModel = itemInstance.ItemModel;

                if(itemModel is WeaponItemModel)
                {
                    if(!itemInstance.Equipped)
                    {
                        //TODO restore this functionality
                        //GameState.Instance.PlayerRpgState.EquipWeapon(itemInstance);
                        //QdmsMessageBus.Instance.PushBroadcast(new RpgChangeWeaponMessage());

                        SelectedItemText.text = SelectedItemText.text + " [!]";
                    }
                    //TODO unequip?

                }
                else if(itemModel is AidItemModel)
                {
                    var aim = (AidItemModel)itemModel;
                    aim.Apply();
                    GameState.Instance.PlayerRpgState.Inventory.RemoveItem(ItemLookupTable[SelectedItem]);

                    //TODO effect or at least a message
                }

                SignalPaint();
            }
        }

        public void OnItemDropped()
        {
            //TODO spawn worldobject, remove inventory, have quantity selection using modal dialog

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
                Texture2D tex = Resources.Load<Texture2D>("UI/Icons/" + itemDef.Image);
                if (tex != null)
                    SelectedItemImage.texture = tex;
            }

            SelectedItemStats.text = itemModel.GetStatsString();

            //handle equipped button and state
            if(itemModel is WeaponItemModel || itemModel is ArmorItemModel)
            {
                if (ItemLookupTable[SelectedItem].Equipped)
                {
                    SelectedItemText.text = SelectedItemText.text + " [!]";
                }

                SelectedItemButton.gameObject.SetActive(true);
                SelectedItemButton.transform.Find("Text").GetComponent<Text>().text = "Equip";
            }
            else if (itemModel is AidItemModel)
            {
                SelectedItemButton.gameObject.SetActive(true);
                SelectedItemButton.transform.Find("Text").GetComponent<Text>().text = "Use";
            }

            if(!itemModel.Essential)
                SelectedItemButton2.gameObject.SetActive(true);

            if (itemModel.Stackable)
            {
                int qty = ItemLookupTable[SelectedItem].Quantity;
                SelectedItemText.text = SelectedItemText.text + string.Format(" {0}", qty);
            }
            
        }

        private void ClearDetailPane()
        {
            SelectedItemText.text = string.Empty;
            SelectedItemDescription.text = string.Empty;
            SelectedItemStats.text = string.Empty;
            SelectedItemImage.texture = null;
            SelectedItemButton.gameObject.SetActive(false);
            SelectedItemButton2.gameObject.SetActive(false);
        }
    }
}