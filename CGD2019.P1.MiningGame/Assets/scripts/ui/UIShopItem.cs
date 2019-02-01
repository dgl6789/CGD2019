using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.UI {
    public class UIShopItem : UIInventoryItem {
        [SerializeField] TextMeshProUGUI priceTag;

        /// <summary>
        /// Initializes an inventory UI item. 
        /// </summary>
        /// <param name="itemData">Item data to initialize the UI with.</param>
        public override void InitializeWithData(InventoryItem itemData) {
            priceTag.text = string.Format("${0:n0}", itemData.Value); 

            base.InitializeWithData(itemData);
        }

        /// <summary>
        /// Executes when an item UI object is tapped.
        /// </summary>
        public override void OnTapItem() {
            UIManager.Instance.SetInventoryPanelText(item);
            UIManager.Instance.OnItemTapped(this);

            if (isTappedOnce) {
                // Buy the item, if possible.
                if (InventoryManager.Instance.PlayerCurrency >= item.Value) {
                    // This should auto-update the inventories.
                    InventoryManager.Instance.AddItem(item, InventoryType.PLAYER);
                    InventoryManager.Instance.RemoveItem(item, InventoryType.SHOP);

                    InventoryManager.Instance.AdjustCurrency(-item.Value);
                    UIManager.Instance.ResetInventoryPanelText();
                }
            } else isTappedOnce = true;
        }
    }
}
