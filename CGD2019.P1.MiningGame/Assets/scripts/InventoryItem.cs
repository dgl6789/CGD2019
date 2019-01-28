using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public class InventoryItem : ScriptableObject
    {
        [SerializeField] Sprite itemSprite;
        public Sprite ItemSprite { get { return itemSprite; } }

        [SerializeField] string itemName;
        public string ItemName { get { return itemName; } }

        [SerializeField] string itemText;
        public string ItemText { get { return itemText; } }
    }
}
