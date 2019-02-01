using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;
using System;

namespace App
{
    public enum InventoryType { PLAYER, SHOP }

    public class InventoryManager : MonoBehaviour
    {
        // Singleton instance (reference this class' members via InventoryManager.Instance from any context that is 'using App;')
        public static InventoryManager Instance;

        int playerCurrency;
        public int PlayerCurrency { get { return playerCurrency; } }

        public List<InventoryItem> playerItems;
        [HideInInspector] public List<ToolItem> equippedItems;

        public List<InventoryItem> shopItems;

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

            AdjustCurrency(2500);
        }

        /// <summary>
        /// Equip an item.
        /// </summary>
        /// <param name="item">Item to equip.</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetItemEquipped(InventoryItem item, bool value)
        {
            if (!playerItems.Contains(item) || !(item is ToolItem)) return false;

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
                AddItem(toRemove, InventoryType.PLAYER);
            }

            // Add the new item to the equipped items (the old one was either removed or never existed)
            if (value) {
                equippedItems.Add(i);
                playerItems.Remove(i);
            }

            UIManager.Instance.LoadInventoryToEquipmentBar();
            UIManager.Instance.LoadInventoryToInventoryModal(InventoryType.PLAYER);

            SaveInventory();

            return value;
        }

        public void AddItem(InventoryItem item, InventoryType inventory) {
            if (inventory.Equals(InventoryType.PLAYER)) playerItems.Add(item);
            else shopItems.Add(item);

            UIManager.Instance.LoadInventoryToInventoryModal(inventory);
        }

        public void RemoveItem(InventoryItem item, InventoryType inventory) {
            if (inventory.Equals(InventoryType.PLAYER)) playerItems.Remove(item);
            else shopItems.Remove(item);

            UIManager.Instance.LoadInventoryToInventoryModal(inventory);
        }

        public void AdjustCurrency(int adj) {
            playerCurrency += adj;

            // Update the currency text
            UIManager.Instance.SetCurrencyText();
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
