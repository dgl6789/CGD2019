using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    [CreateAssetMenu(fileName = "Gem", menuName = "Item/Gem", order = 1)]
    public class MineralItem : InventoryItem
    {
        //model
        [SerializeField] string gemModelName;

        //value
        [SerializeField] int gemValue;

        //durability
        [SerializeField] int gemDurability;
    }
}
