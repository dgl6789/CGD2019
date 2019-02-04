using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public enum ToolType { POINT, AREA, CHISEL }
    public enum ToolInputType { INSTANT, SUSTAINED }

    [CreateAssetMenu(fileName = "Tool", menuName = "Item/Tool", order = 1)]
    public class ToolItem : InventoryItem
    {
        [SerializeField] ToolType type;
        public ToolType Type { get { return type; } }

        [SerializeField] ToolInputType inputType;
        public ToolInputType InputType { get { return inputType; } }

        [SerializeField] int power;
        public int Power { get { return power; } }

        [SerializeField] float precision;
        public int Precision { get { return precision; } }

        [SerializeField] float sustainedBreakCooldown;
        public float SustainedBreakCooldown { get { return sustainedBreakCooldown; } }

        [SerializeField] float breakRadius;
        public float BreakRadius { get { return breakRadius; } }

        bool equipped;
        public bool Equipped {
            get { return equipped; }
        }

        public void Equip() {
            equipped = InventoryManager.Instance.SetItemEquipped(this, true);
        }

        public void SetValues (string itemName, string itemText, int value, string spriteName, ToolType type, ToolInputType inputType, int power, float sustainedBreakCooldown, float breakRadius) {
            this.spriteName = spriteName;
            this.itemName = itemName;
            this.itemText = itemText;
            this.value = value;

            this.type = type;
            this.inputType = inputType;
            this.power = power;
            this.sustainedBreakCooldown = sustainedBreakCooldown;
            this.breakRadius = breakRadius;
        }
    }
}