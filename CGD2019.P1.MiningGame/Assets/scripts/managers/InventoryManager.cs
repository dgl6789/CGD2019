using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;
using System;

namespace App
{
    public class InventoryManager : MonoBehaviour
    {
        // Singleton instance (reference this class' members via InventoryManager.Instance from any context that is 'using App;')
        public static InventoryManager Instance;

        public List<InventoryItem> items;
        [HideInInspector] public List<ToolItem> equippedItems;

        int inventoryValue;
        public int InventoryValue { get { return inventoryValue; } }

        // The tool that is currently being used.
        ToolItem activeTool;
        public ToolItem ActiveTool {
            get { return activeTool; }
            set {
                if (equippedItems.Contains(value)) activeTool = value;

                UIManager.Instance.SetActiveToolBorder(value);
            }
        }

        // Use this for initialization
        void Awake()
        {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start() {
            // items = new List<InventoryItem>();
            equippedItems = new List<ToolItem>();

            LoadInventory();

            // TODO: Add some default items to the inventory
        }

        /// <summary>
        /// Equip an item.
        /// </summary>
        /// <param name="item">Item to equip.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetItemEquipped(InventoryItem item, bool value)
        {
            if (!items.Contains(item) || !(item is ToolItem)) return false;

            ToolItem i = item as ToolItem;
            ToolItem toRemove = null;
            
            foreach (ToolItem t in equippedItems)
            {
                // Set the item that will be swapped out
                if (t.Type.Equals(i.Type)) { toRemove = t; break; }
            }

            // Remove the existing item of the swap type and put it back in the inventory
            if (toRemove != null) {
                equippedItems.Remove(toRemove);
                AddItem(toRemove);
            }

            // Add the new item to the equipped items (the old one was either removed or never existed)
            if (value) {
                equippedItems.Add(i);
                items.Remove(i);
            }

            UIManager.Instance.LoadInventoryToEquipmentBar();
            UIManager.Instance.LoadInventoryToInventoryModal();

            SaveInventory();

            return value;
        }

        public void AddItem(InventoryItem item) {
            if(item is MineralItem) {
                MineralItem m = item as MineralItem;

                inventoryValue += m.Value;

                items.Add(m);
            } else {
                ToolItem t = item as ToolItem;

                items.Add(t);
            }

            UIManager.Instance.LoadInventoryToInventoryModal();
        }

    #region Save / Load

        public void LoadInventory()
        {
            // Load the inventory from disk.
        }

        public void SaveInventory()
        {
            // Save the inventory to disk.
        }

    #endregion
    }
}
