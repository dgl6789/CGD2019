using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using App.Util;

namespace App.UI {
    public class UIEquippedTool : MonoBehaviour {
        protected ToolItem item;
        public ToolItem Item {
            get { return item; }
        }

        /// <summary>
        /// Initializes an inventory UI item. 
        /// </summary>
        /// <param name="itemData">Item data to initialize the UI with.</param>
        public void InitializeWithData(ToolItem itemData) {
            item = itemData;

            // set the ui's sprite.
            GetComponent<Image>().sprite = AssetManager.Instance.GetSpriteFromManifest(itemData.SpriteName);
        }

        /// <summary>
        /// Executes when an item UI object is tapped.
        /// 
        /// Tools will set themselves as the currently equipped tool.
        /// </summary>
        public void OnTapItem() {
            InventoryManager.Instance.ActiveTool = item;
        }
    }
}
