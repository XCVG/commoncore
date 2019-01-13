using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CommonCore.State;
using CommonCore.RpgGame.Rpg;
using CommonCore.LockPause;
using CommonCore.UI;

namespace CommonCore.RpgGame.UI
{

    public class ContainerModal : BaseMenuController
    {
        public delegate void ContainerCallback();

        private enum SelectedState
        {
            None, Inventory, Container
        }

        [Header("Set On Creation")]
        public ContainerCallback Callback;
        public InventoryModel Inventory;
        public ContainerModel Container;
        public bool IsShop;

        [Header("UI Elements")]
        public RectTransform InventoryScrollContent;
        public Text InventoryTextMoney;
        public RectTransform ContainerScrollContent;
        public Text ContainerTextMoney;

        public GameObject QuantityPicker;
        public InputField QuantityInputField;

        public Text SelectedItemText;
        public RawImage SelectedItemImage;
        public Text SelectedItemDescription;
        public Text SelectedItemStats;

        public Text TransferText;
        public Button TransferButton;

        [Header("Other")]
        public GameObject ItemTemplatePrefab;

        //private state
        private Button SelectedButton;
        private SelectedState Selected;
        private InventoryItemInstance SelectedItem;

        public override void Start()
        {
            base.Start();

            LockPauseModule.LockControls(InputLockType.GameOnly, this);
            LockPauseModule.PauseGame(PauseLockType.All, this);

            ClearState();
            PaintAll();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            LockPauseModule.UnlockControls(this);
            LockPauseModule.UnpauseGame(this);
        }

        private void ClearState()
        {
            SelectedButton = null;
            Selected = SelectedState.None;
            SelectedItem = null;
        }

        private void PaintAll()
        {
            //reset detail column
            ClearDetails();

            //paint lists
            PaintLists();

            //paint money
            PaintMoney();
        }

        private void ClearDetails()
        {
            SelectedItemText.text = string.Empty;
            SelectedItemImage.texture = null;
            SelectedItemDescription.text = string.Empty;
            SelectedItemStats.text = string.Empty;

            QuantityInputField.text = "1";
            QuantityPicker.SetActive(false);

            TransferText.text = string.Empty;
            TransferButton.gameObject.SetActive(false);
        }

        private void PaintLists()
        {
            //clear existing lists
            foreach (Transform t in InventoryScrollContent)
            {
                Destroy(t.gameObject);
            }
            InventoryScrollContent.DetachChildren();

            foreach (Transform t in ContainerScrollContent)
            {
                Destroy(t.gameObject);
            }
            ContainerScrollContent.DetachChildren();

            string moneyTypeName = Enum.GetNames(typeof(MoneyType))[0];

            //player/inventory list
            var inventoryList = Inventory.GetItemsListActual();
            for (int i = 0; i < inventoryList.Count; i++)
            {
                var item = inventoryList[i];

                //hide equipped items
                if (item.Equipped)
                    continue;

                //hide player money if it's a shop
                if (IsShop && item.ItemModel.Name == moneyTypeName)
                    continue;

                GameObject itemGO = Instantiate<GameObject>(ItemTemplatePrefab, InventoryScrollContent);
                if (!item.ItemModel.Stackable)
                    itemGO.GetComponentInChildren<Text>().text = item.ItemModel.Name;
                else
                    itemGO.GetComponentInChildren<Text>().text = string.Format("{0} ({1})", item.ItemModel.Name, item.Quantity);

                Button b = itemGO.GetComponent<Button>();


                b.onClick.AddListener(delegate { OnItemSelected(SelectedState.Inventory, item, b); }); //should work (?)
            }

            var containerList = Container.GetItemsListActual();
            for(int i = 0; i < containerList.Count; i++)
            {
                var item = containerList[i];

                //hide money if it's a shop
                if (IsShop && item.ItemModel.Name == moneyTypeName)
                    continue;

                GameObject itemGO = Instantiate<GameObject>(ItemTemplatePrefab, ContainerScrollContent);
                if (!item.ItemModel.Stackable)
                    itemGO.GetComponentInChildren<Text>().text = item.ItemModel.Name;
                else
                    itemGO.GetComponentInChildren<Text>().text = string.Format("{0} ({1})", item.ItemModel.Name, item.Quantity);

                Button b = itemGO.GetComponent<Button>();


                b.onClick.AddListener(delegate { OnItemSelected(SelectedState.Container, item, b); }); //should work (?)
            }

        }

        private void PaintMoney()
        {
            string moneyTypeName = Enum.GetNames(typeof(MoneyType))[0];

            //paint player money always
            int playerMoney = Inventory.CountItem(moneyTypeName);
            InventoryTextMoney.text = string.Format("{0}: {1}", moneyTypeName, playerMoney);

            //paint container money if it's a shop
            if(IsShop)
            {
                int containerMoney = Container.CountItem(moneyTypeName);
                ContainerTextMoney.text = string.Format("{0}: {1}", moneyTypeName, containerMoney);
            }
            else
            {
                ContainerTextMoney.text = string.Empty;
            }
        }

        private void OnItemSelected(SelectedState selected, InventoryItemInstance item, Button button)
        {
            if (SelectedButton != null)
                SelectedButton.image.color = Color.white;

            SelectedButton = button;
            SelectedItem = item;
            Selected = selected;

            if(SelectedItem.ItemModel.Stackable)
            {
                QuantityPicker.gameObject.SetActive(true);
                QuantityInputField.text = "1";
            }

            //highlight button
            button.image.color = Color.blue; //dumb but will work for now

            PaintSelectedItem();
        }

        private void PaintSelectedItem()
        {
            var itemModel = SelectedItem.ItemModel;
            SelectedItemText.text = itemModel.Name;
            var itemDef = InventoryModel.GetDef(itemModel.Name);
            if (itemDef == null)
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


            TransferButton.gameObject.SetActive(true);
            var transferButtonText = TransferButton.GetComponentInChildren<Text>();
            if (Selected == SelectedState.Container && !IsShop)
                transferButtonText.text = "< Transfer";
            else if(Selected == SelectedState.Inventory && !IsShop)
                transferButtonText.text = "Transfer >";
            else if (Selected == SelectedState.Container && IsShop)
                transferButtonText.text = "< Buy";
            else if (Selected == SelectedState.Inventory && IsShop)
                transferButtonText.text = "Sell >";

            if(IsShop && Selected == SelectedState.Container)
            {
                int adjValue = RpgValues.AdjustedBuyPrice(GameState.Instance.PlayerRpgState, itemModel.Value);
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", adjValue, adjValue * 1, itemModel.Weight, itemModel.Weight * 1);
            }
            else if (IsShop && Selected == SelectedState.Inventory)
            {
                int adjValue = RpgValues.AdjustedSellPrice(GameState.Instance.PlayerRpgState, itemModel.Value);
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", adjValue, adjValue * 1, itemModel.Weight, itemModel.Weight * 1);
            }
            else
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", itemModel.Value, itemModel.Value * 1, itemModel.Weight, itemModel.Weight * 1);
        }

        public void OnClickExit()
        {            
            Destroy(this.gameObject);
            if(Callback != null)
                Callback.Invoke();
        }

        public void OnClickTransfer()
        {
            //immediate fail conditions
            if(Selected == SelectedState.None || SelectedItem == null)
            {
                return;
            }

            int quantity = CoreUtils.Clamp<int>(Convert.ToInt32(QuantityInputField.text), 1, Mathf.Abs(SelectedItem.Quantity));

            //if it's a shop and we don't have money, check and possibly fail
            //(we would also handle carry weight here if that was actually implemented ?)
            if(IsShop)
            {

                string moneyTypeName = Enum.GetNames(typeof(MoneyType))[0];

                if(Selected == SelectedState.Inventory)
                {
                    //we are SELLING, check CONTAINER money 
                    int neededMoney = Mathf.RoundToInt(quantity * RpgValues.AdjustedSellPrice(GameState.Instance.PlayerRpgState, SelectedItem.ItemModel.Value));
                    int haveMoney = Container.CountItem(moneyTypeName);

                    if(neededMoney > haveMoney)
                    {
                        Modal.PushMessageModal(string.Format("This shop doesn't have enough currency to pay for that (have {0}, need {1})", haveMoney, neededMoney), "Insufficient Currency", null, null);
                        return;
                    }
                    else
                    {
                        //var iMoneyItem = Inventory.FindItem(moneyTypeName)[0]; //it's weirdly asymmetrical and I don't know why
                        Inventory.AddItem(moneyTypeName, neededMoney);

                        var cMoneyItem = Container.FindItem(moneyTypeName)[0];
                        Container.TakeItem(cMoneyItem, neededMoney);
                    }
                }
                else if(Selected == SelectedState.Container)
                {
                    //we are BUYING, check PLAYER money
                    int neededMoney = Mathf.RoundToInt(quantity * RpgValues.AdjustedBuyPrice(GameState.Instance.PlayerRpgState, SelectedItem.ItemModel.Value));
                    int haveMoney = Inventory.CountItem(moneyTypeName);

                    if (neededMoney > haveMoney)
                    {
                        Modal.PushMessageModal(string.Format("You don't have enough currency to pay for that (have {0}, need {1})", haveMoney, neededMoney), "Insufficient Currency", null, null);
                        return;
                    }
                    else
                    {
                        var iMoneyItem = Inventory.FindItem(moneyTypeName)[0];
                        Inventory.RemoveItem(iMoneyItem, neededMoney);

                        var cMoneyItem = Container.FindItem(moneyTypeName)[0];
                        Container.PutItem(cMoneyItem, neededMoney);
                    }
                }

            }

            //simpler cases: transferring inventory to container or vice versa
            if (Selected == SelectedState.Inventory)
            {
                //transfer item to container
                if(!SelectedItem.ItemModel.Stackable)
                {
                    Inventory.RemoveItem(SelectedItem);
                    Container.PutItem(SelectedItem);
                }
                else
                {
                    Inventory.RemoveItem(SelectedItem, quantity);
                    Container.PutItem(SelectedItem, quantity);
                }
            }
            else if(Selected == SelectedState.Container)
            {
                //transfer item to inventory
                if (!SelectedItem.ItemModel.Stackable)
                {
                    Inventory.AddItem(SelectedItem);
                    Container.TakeItem(SelectedItem);
                }
                else
                {
                    Inventory.AddItem(SelectedItem.ItemModel.Name, quantity);
                    Container.TakeItem(SelectedItem, quantity);
                }
            }

            //don't forget to fix the stacks!
            Container.FixStacks();

            //clear and repaint
            ClearState();
            PaintAll();
        }

        public void OnQuantityValueChanged()
        {
            if (SelectedItem == null)
                return;

            var itemModel = SelectedItem.ItemModel;
            int itemQuantity = Convert.ToInt32(QuantityInputField.text);

            //clamp values
            if(itemQuantity <= 0 || itemQuantity > SelectedItem.Quantity)
            {
                itemQuantity = CoreUtils.Clamp<int>(itemQuantity, 1, SelectedItem.Quantity);
                QuantityInputField.text = itemQuantity.ToString();
            }

            if (IsShop && Selected == SelectedState.Container)
            {
                int adjValue = RpgValues.AdjustedBuyPrice(GameState.Instance.PlayerRpgState, itemModel.Value);
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", adjValue, adjValue * itemQuantity, itemModel.Weight, itemModel.Weight * itemQuantity);
            }
            else if (IsShop && Selected == SelectedState.Inventory)
            {
                int adjValue = RpgValues.AdjustedSellPrice(GameState.Instance.PlayerRpgState, itemModel.Value);
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", adjValue, adjValue * itemQuantity, itemModel.Weight, itemModel.Weight * itemQuantity);
            }
            else
                TransferText.text = string.Format("Value: {0}({1}) | Weight: {2}({3})", itemModel.Value, itemModel.Value * itemQuantity, itemModel.Weight, itemModel.Weight * itemQuantity);

        }

        public void OnClickChangeQuantity(int delta)
        {
            if (delta == 65535) //dear god this is horrible... but it won't be a problem, right?
                QuantityInputField.text = SelectedItem.Quantity.ToString();
            else
                QuantityInputField.text = (Convert.ToInt32(QuantityInputField.text) + delta).ToString();

        }

        private const string DefaultPrefab = "UI/IGUI_Container";

        public static void PushModal(InventoryModel inventory, ContainerModel container, bool isShop, ContainerCallback callback)
        {
            var go = Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>(DefaultPrefab), CoreUtils.GetWorldRoot());
            var modal = go.GetComponent<ContainerModal>();

            modal.Inventory = inventory;
            modal.Container = container;
            modal.IsShop = isShop;
            modal.Callback = callback;
        }

    }
}