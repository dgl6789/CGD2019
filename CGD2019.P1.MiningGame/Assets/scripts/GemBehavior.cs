using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {

        [HideInInspector] bool debugIsFree = true;

        MineralItem itemData;
        
        public void Initialize(MineralItem itemData) {
            this.itemData = itemData;
        }
        
        //method to mine gem
        public void TryMine() {
            if (IsMineable()) {
                //add gem to inventory
                InventoryManager.Instance.AddItem(itemData, InventoryType.PLAYER);

                //delete gameobject
                Gemeration.Instance.RemoveGem(this);
                Destroy(gameObject);
            }
        }

        //method to determine if the gem can be mined
        bool IsMineable() {
            return !Gemeration.Instance.InRock(transform.position);
        }
    }
}
