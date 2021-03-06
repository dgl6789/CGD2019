﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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

        ConsumableItem activeConsumable;
        public ConsumableItem ActiveConsumable
        {
            get { return activeConsumable; }
            set
            {
                if (playerItems.Contains(value)) activeConsumable = value;
                consumableDuration = value.Duration;
            }
        }
        [SerializeField] int consumableDuration;

        [Header("Save/Load")]
        [SerializeField] bool loadSaveDataOnStart;

        [SerializeField] string folderName;
        [SerializeField] string fileExtension;

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

            if(loadSaveDataOnStart) LoadInventories();
        }

        private void Update()
        {
            //only decrement consumable duration if mining
            if (StateManager.Instance.State == GameState.MINING)
            {
                //decrement consumable duration, and any effects when they run out
                if (consumableDuration > 0)
                {
                    consumableDuration--;
                }
                else if (activeConsumable != null)
                {
                    EndConsumable();
                }
            }
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

            return value;
        }

        /// <summary>
        /// Ends the effects of the current consumable item
        /// </summary>
        public void EndConsumable()
        {
            consumableDuration = 0;

            switch(activeConsumable.Type)
            {
                case ConsumableType.RADAR: //makes rock opaque when radar is over
                    Gameplay.VoxelGrid.Instance.AdjustTransparency();
                    break;
            }

            activeConsumable = null;
        }
        
        /// <summary>
        /// Uses a consumable
        /// </summary>
        /// <param name="item">consumable to use</param>
        public void UseConsumable(InventoryItem item)
        {
            if (!playerItems.Contains(item) || !(item is ConsumableItem)) return;

            ConsumableItem i = item as ConsumableItem;

            switch (i.Type)
            {
                case ConsumableType.RADAR:
                    if (consumableDuration > 0) return; //can only have one consumable with a duration active at once

                    Gameplay.VoxelGrid.Instance.AdjustTransparency(i.Strength);
                    consumableDuration = i.Duration;
                    activeConsumable = i;
                    break;
                case ConsumableType.REPAIR:
                    RockManager.Instance.AdjustRockIntegrityPercentage(i.Strength);
                    break;
            }

            RemoveItem(item, InventoryType.PLAYER);
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

        public void LoadInventories()
        {
            // Load the inventory from disk.
            string filePath = Path.Combine(Path.Combine(Application.persistentDataPath, folderName), "data" + fileExtension);

            if (!File.Exists(filePath)) return;
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SaveData data;

            using (FileStream fileStream = File.Open(filePath, FileMode.Open)) {
                data = (SaveData)binaryFormatter.Deserialize(fileStream);
            }

            playerItems = new List<InventoryItem>();
            equippedItems = new List<ToolItem>();
            shopItems = new List<InventoryItem>();

            playerCurrency = 0;

            foreach(SaveItem i in data.PlayerInventory) {
                if (i.InventoryItemType.Equals(ItemType.TOOL)) playerItems.Add(i.GetToolData());
                else if (i.InventoryItemType.Equals(ItemType.MINERAL)) playerItems.Add(i.GetMineralData());
                else if (i.InventoryItemType.Equals(ItemType.CONSUMABLE)) playerItems.Add(i.GetConsumableData());
                else playerItems.Add(i.GetInventoryItem());
            }

            foreach (SaveItem i in data.EquippedItems) equippedItems.Add(i.GetToolData());

            foreach (SaveItem i in data.ShopInventory) {
                if (i.InventoryItemType.Equals(ItemType.TOOL)) shopItems.Add(i.GetToolData());
                else if (i.InventoryItemType.Equals(ItemType.MINERAL)) shopItems.Add(i.GetMineralData());
                else if (i.InventoryItemType.Equals(ItemType.CONSUMABLE)) shopItems.Add(i.GetConsumableData());
                else shopItems.Add(i.GetInventoryItem());
            }

            AdjustCurrency(data.PlayerCurrency);
        }

        public void SaveInventories()
        {
            // Save the inventory to disk.
            string folderPath = Path.Combine(Application.persistentDataPath, folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            string dataPath = Path.Combine(folderPath, "data" + fileExtension);

            // Generate the save data
            List<SaveItem> p = new List<SaveItem>();
            List<SaveItem> e = new List<SaveItem>();
            List<SaveItem> s = new List<SaveItem>();

            foreach(InventoryItem i in playerItems) p.Add(new SaveItem(i));
            foreach(InventoryItem i in equippedItems) e.Add(new SaveItem(i));
            foreach(InventoryItem i in shopItems) s.Add(new SaveItem(i));

            SaveData data = new SaveData(p, e, s, playerCurrency);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            using (FileStream fileStream = File.Open(dataPath, FileMode.OpenOrCreate)) {
                binaryFormatter.Serialize(fileStream, data);
            }
        }

        // This is guaranteed to run when application goes to the background on mobile.
        private void OnApplicationPause(bool pause) {
            if (pause) SaveInventories();
        }

        private void OnApplicationQuit() {
            SaveInventories();
        }

        #endregion
    }

    /// <summary>
    /// Serializable wrapper that contains the data required to save and load progress.
    /// </summary>
    [Serializable]
    public class SaveData {
        public int PlayerCurrency;

        public List<SaveItem> PlayerInventory;
        public List<SaveItem> EquippedItems;
        public List<SaveItem> ShopInventory;

        public SaveData() {
            PlayerInventory = new List<SaveItem>();
            EquippedItems = new List<SaveItem>();
            ShopInventory = new List<SaveItem>();
        }

        public SaveData(List<SaveItem> playerInventory, List<SaveItem> equippedItems, List<SaveItem> shopInventory, int currency) {
            PlayerCurrency = currency;

            PlayerInventory = playerInventory;
            EquippedItems = equippedItems;
            ShopInventory = shopInventory;
        }
    }

    public enum ItemType { TOOL, MINERAL, CONSUMABLE, OTHER }

    /// <summary>
    /// Item saved into the SaveData.
    /// </summary>
    [Serializable]
    public class SaveItem {
        ItemType type;
        public ItemType InventoryItemType { get { return type; } }

        string itemName;
        string itemText;
        int value;
        string spriteName;

        string modelName;
        float colorR, colorG, colorB;

        ToolType toolType;
        ToolInputType inputType;
        int power;
        float precision;
        float sustainedBreakCooldown;
        float breakRadius;

        ConsumableType consumableType;
        float strength;
        int duration;

        public SaveItem(InventoryItem item) {
            if (item is ToolItem) type = ItemType.TOOL;
            else if (item is MineralItem) type = ItemType.MINERAL;
            else if (item is ConsumableItem) type = ItemType.CONSUMABLE;
            else type = ItemType.OTHER;

            itemName = item.ItemName;
            itemText = item.ItemText;
            value = item.Value;
            spriteName = item.SpriteName;

            switch (type) {
                case ItemType.TOOL:
                    ToolItem t = item as ToolItem;

                    toolType = t.Type;
                    inputType = t.InputType;
                    power = t.Power;
                    precision = t.Precision;
                    sustainedBreakCooldown = t.SustainedBreakCooldown;
                    breakRadius = t.BreakRadius;
                    break;

                case ItemType.MINERAL:
                    MineralItem m = item as MineralItem;

                    modelName = m.ModelName;
                    colorR = m.Color.r;
                    colorG = m.Color.g;
                    colorB = m.Color.b;
                    break;

                case ItemType.CONSUMABLE:
                    ConsumableItem c = item as ConsumableItem;

                    consumableType = c.Type;
                    strength = c.Strength;
                    duration = c.Duration;
                    break;
            }
        }

        public InventoryItem GetInventoryItem() {
            InventoryItem i = ScriptableObject.CreateInstance<InventoryItem>();
            i.SetValues(spriteName, itemName, itemText, value);

            return i;
        }

        public ToolItem GetToolData() {
            ToolItem i = ScriptableObject.CreateInstance<ToolItem>();
            i.SetValues(itemName, itemText, value, spriteName, toolType, inputType, power, precision, sustainedBreakCooldown, breakRadius);

            return i;
        }

        public MineralItem GetMineralData()
        {
            MineralItem i = ScriptableObject.CreateInstance<MineralItem>();
            i.SetValues(itemName, itemText, value, spriteName, modelName, new Color(colorR, colorG, colorB, 1.0f));

            return i;
        }

        public ConsumableItem GetConsumableData()
        {
            ConsumableItem i = ScriptableObject.CreateInstance<ConsumableItem>();
            i.SetValues(itemName, itemText, value, spriteName, consumableType, strength, duration);

            return i;
        }
    }
}
