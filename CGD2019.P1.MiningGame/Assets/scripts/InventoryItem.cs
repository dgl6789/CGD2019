using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class InventoryItem : ScriptableObject
    {
        [SerializeField] protected string spriteName;
        public string SpriteName { get { return spriteName; } }

        [SerializeField] protected string itemName;
        public string ItemName { get { return itemName; } }

        [SerializeField] protected string itemText;
        public string ItemText { get { return itemText; } }

        [SerializeField] protected int value;
        public int Value { get { return value; } }

        public void SetValues(string spriteName, string itemName, string itemText, int value) {
            this.spriteName = spriteName;
            this.itemName = itemName;
            this.itemText = itemText;
            this.value = value;
        }
    }
}
