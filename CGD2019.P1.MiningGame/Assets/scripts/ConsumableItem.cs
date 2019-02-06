using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public enum ConsumableType { RADAR, REPAIR }

    [CreateAssetMenu(fileName = "Consumable", menuName = "Item/Consumable", order = 2)]
    public class ConsumableItem : InventoryItem
    {
        [SerializeField] ConsumableType type;
        public ConsumableType Type { get { return type; } }

        [SerializeField] float strength;
        public float Strength { get { return strength; } }

        [SerializeField] int duration;
        public int Duration { get { return duration; } }

        public void SetValues(string itemName, string itemText, int value, string spriteName, ConsumableType type, float strength, int duration)
        {
            this.spriteName = spriteName;
            this.itemName = itemName;
            this.itemText = itemText;
            this.value = value;

            this.type = type;
            this.strength = strength;
            this.duration = duration;
        }
    }
}
