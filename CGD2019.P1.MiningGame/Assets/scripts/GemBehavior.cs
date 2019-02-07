using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Util;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {
        GameObject Camera;

        GameState startingState;
        MineralItem itemData;

        MaterialPropertyBlock materialProps;
        Renderer _renderer;
        
        public void Initialize(MineralItem itemData) {
            _renderer = GetComponent<Renderer>();
            Camera = GameObject.FindGameObjectWithTag("MainCamera");

            this.itemData = itemData;

            startingState = StateManager.Instance.State;

            // Assign the gem's mesh and give it a random rotation.
            GetComponent<MeshFilter>().mesh = AssetManager.Instance.GetModelFromManifest(itemData.ModelName);

            transform.localRotation = Quaternion.Euler(Random.insideUnitSphere * 360f);

            // Set the gem's material tint.
            materialProps = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(materialProps);
            materialProps.SetColor("_Color", itemData.Color);
            _renderer.SetPropertyBlock(materialProps);
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
            return !Gemeration.Instance.InRock(transform.position);
        }
    }
}
