using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    [CreateAssetMenu(fileName = "Mineral", menuName = "Item/Mineral", order = 0)]
    public class MineralItem : InventoryItem
    {
        [SerializeField] string modelName;
        public string ModelName { get { return modelName; } }

        [SerializeField] Color color;
        public Color Color { get { return color; } }

        public void SetValues(string itemName, string itemText, int value, string spriteName, string modelName, Color color) {
            this.spriteName = spriteName;
            this.itemName = itemName;
            this.itemText = itemText;
            this.value = value;

            this.modelName = modelName;
            this.color = color;
        }
    }
}
