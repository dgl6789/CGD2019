using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    public enum ToolType { LARGE, SMALL }

    public class ToolItem : InventoryItem
    {
        [SerializeField] ToolType type;
        public ToolType Type { get { return Type; } }

        bool equipped;
        public bool Equipped {
            get { return equipped; }
        }

        public void Equip() {
            equipped = InventoryManager.Instance.SetItemEquipped(this, true);
        }
    }
}