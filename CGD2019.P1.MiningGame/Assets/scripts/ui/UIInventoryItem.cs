using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace App.UI
{
    public class UIInventoryItem : MonoBehaviour
    {
        [SerializeField] InventoryItem item;
        
        [SerializeField] Image itemImage;

        /// <summary>
        /// Initializes an inventory UI item. 
        /// </summary>
        /// <param name="itemData">Item data to initialize the UI with.</param>
        public void InitializeWithData(InventoryItem itemData)
        {
            item = itemData;

            // set the ui's sprite.
            itemImage.sprite = itemData.ItemSprite;
        }

        /// <summary>
        /// Executes when an item UI object is tapped.
        /// 
        /// Minerals will load their information into the infobox.
        /// 
        /// Tools will load their information and swap equip states 
        /// with the currently equipped tool of the same type.
        /// </summary>
        public void OnTapItem() {
            UIManager.Instance.SetInventoryPanelText(item);

            if (item is ToolItem) {
                ToolItem i = item as ToolItem;

                InventoryManager.Instance.SetItemEquipped(i, true);
            }
        }
    }
}
