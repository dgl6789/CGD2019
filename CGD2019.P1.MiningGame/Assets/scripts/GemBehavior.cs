using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {

        [HideInInspector] bool debugIsFree = true;

        MineralItem itemData;
        
        public void Initialize(MineralItem itemData) {
            this.itemData = itemData;

            // TODO: Set the gem's mesh to the mesh specified by itemData
        }

        //method to mine gem
        public void TryMine() {
            if (IsMineable()) {
                //add gem to inventory
                InventoryManager.Instance.AddItem(itemData);

                //delete gameobject
                Destroy(gameObject);
            }
        }

        //method to determine if the gem can be mined
        bool IsMineable() {
            return debugIsFree;
        }
    }
}
