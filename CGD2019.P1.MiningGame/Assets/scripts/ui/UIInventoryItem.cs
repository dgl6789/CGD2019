using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.UI
{
    public class UIInventoryItem : MonoBehaviour
    {
        protected InventoryItem item;

        protected bool isTappedOnce;

        /// <summary>
        /// Initializes an inventory UI item. 
        /// </summary>
        /// <param name="itemData">Item data to initialize the UI with.</param>
        public virtual void InitializeWithData(InventoryItem itemData)
        {
            item = itemData;

            // set the ui's sprite.
            GetComponent<Image>().sprite = itemData.ItemSprite;
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
        /// </summary>
        public virtual void OnTapItem() {
            // Sell the item, if the shop is open and this is the second tap.
            if (UIManager.Instance.ModalIsOpen(ModalState.SHOP)) {
                if (isTappedOnce) {
                    // This should auto-update the inventories.
                    InventoryManager.Instance.RemoveItem(item, InventoryType.PLAYER);

                    if(item is ToolItem) InventoryManager.Instance.AddItem(item, InventoryType.SHOP);

                    InventoryManager.Instance.AdjustCurrency(item.Value);
                } else isTappedOnce = true;

                UIManager.Instance.OnItemTapped(this);
            } else {
                // Equip an item if it is a tool and the shop is not open.
                if (item is ToolItem) {
                    ToolItem i = item as ToolItem;

                    InventoryManager.Instance.SetItemEquipped(i, true);
                }
            }

            UIManager.Instance.SetInventoryPanelText(item);
        }
    }
}
