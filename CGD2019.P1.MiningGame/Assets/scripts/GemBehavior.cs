using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {

        [HideInInspector] bool debugIsFree = true;

        GameObject Camera;

        GameState startingState;

        MineralItem itemData;
        
        public void Initialize(MineralItem itemData) {
            Camera = GameObject.FindGameObjectWithTag("MainCamera");

            this.itemData = itemData;

            startingState = StateManager.Instance.State;

            // TODO: Set the gem's mesh to the mesh specified by itemData
        }

        public void Update()
        {
            if (StateManager.Instance.State != startingState)
            {
                Destroy(gameObject);
            }
        }

        //method to mine gem
        public void TryMine() {
            if (IsMineable()) {
                GameObject text = Instantiate<GameObject>(Resources.Load<GameObject>("GemText"),transform.position,Quaternion.Euler(Vector3.zero));
                text.GetComponent<TextMesh>().text = itemData.Value.ToString();
                text.transform.LookAt(Camera.transform,Camera.transform.up);
                text.transform.localScale = new Vector3(-1, 1, 1);
                //add currency to player
                InventoryManager.Instance.AdjustCurrency(itemData.Value);
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
