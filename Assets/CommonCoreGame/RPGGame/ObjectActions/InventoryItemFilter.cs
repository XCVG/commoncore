using CommonCore.ObjectActions;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.RpgGame.ObjectActions
{

    /// <summary>
    /// Inventory item action special filter, only passes through if the player has an inventory item
    /// </summary>
    public class InventoryItemFilter : ActionSpecial
    {
        public ActionSpecialEvent Special;

        [SerializeField, Tooltip("If set, will pass through when activated by non-player")]
        private bool PassthroughNonPlayerAction = false;
        [SerializeField]
        private string InventoryItem = null;
        [SerializeField]
        private bool Consume = false;
        [SerializeField, Tooltip("If set, will disable non-repeatable action even if the condition failed.")]
        private bool LockEvenOnFail = false;

        private bool Locked;

        public override void Execute(ActionInvokerData data)
        {
            if (Locked || (!AllowInvokeWhenDisabled && !isActiveAndEnabled))
                return;

            if(data.Activator != null && WorldUtils.IsPlayer(data.Activator.gameObject))
            {
                InventoryModel inventory = GameState.Instance.PlayerRpgState.Inventory;
                if (inventory.CountItem(InventoryItem) > 0)
                {
                    Special.Invoke(data);
                    if (Consume)
                        inventory.RemoveItem(InventoryItem, 1);

                }

                if (!Repeatable)
                    Locked = true;
            }
            else if (PassthroughNonPlayerAction)
            {
                Special.Invoke(data);

                if (!Repeatable) //should we do this?
                    Locked = true;
            }

            if (!Repeatable && LockEvenOnFail)
                Locked = true;
        }
    }
}