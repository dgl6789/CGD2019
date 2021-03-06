﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.Util;

namespace App.UI
{
    public class UIInventoryItem : MonoBehaviour
    {
        protected InventoryItem item;
        public InventoryItem Item { get { return item; } }

        protected bool isTappedOnce;

        /// <summary>
        /// Initializes an inventory UI item. 
        /// </summary>
        /// <param name="itemData">Item data to initialize the UI with.</param>
        public virtual void InitializeWithData(InventoryItem itemData)
        {
            item = itemData;

            // set the ui's sprite.
            GetComponent<Image>().sprite = AssetManager.Instance.GetSpriteFromManifest(itemData.SpriteName);
        }

        public void UntapItem() {
            isTappedOnce = false;
        }

        /// <summary>
        /// Executes when an item UI object is tapped.
        /// 
        /// Minerals will load their information into the infobox.
        /// 
        /// Tools will load their information and swap equip states 
        /// with the currently equipped tool of the same type.
        /// 
        /// Consumables will be consumed and have their immediate effect.
        /// </summary>
        public virtual void OnTapItem() {
            // Set the inventory panel text
            UIManager.Instance.SetInventoryPanelText(item);
            UIManager.Instance.OnItemTapped(this);

            // Sell the item, if the shop is open and this is the second tap.
            if (UIManager.Instance.ModalIsOpen(ModalState.SHOP)) {
                if (isTappedOnce) {
                    // This should auto-update the inventories.
                    InventoryManager.Instance.RemoveItem(item, InventoryType.PLAYER);

                    if(item is ToolItem) InventoryManager.Instance.AddItem(item, InventoryType.SHOP);

                    InventoryManager.Instance.AdjustCurrency(item.Value);

                    UIManager.Instance.ResetInventoryPanelText();
                } else isTappedOnce = true;
            } else {
                // Equip an item if it is a tool and the shop is not open.
                if (item is ToolItem) {
                    ToolItem i = item as ToolItem;

                    InventoryManager.Instance.SetItemEquipped(i, true);
                }
                else if (item is ConsumableItem && StateManager.Instance.State == GameState.MINING) //use a consumable item if in game
                {
                    ConsumableItem i = item as ConsumableItem;

                    InventoryManager.Instance.UseConsumable(i);
                }
            }
        }
    }
}
