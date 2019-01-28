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

        [SerializeField] int value;
        public int Value {  get { return value; } }
    }
}
