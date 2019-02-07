using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Util;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {
        GameObject Camera;

        MineralItem itemData;

        MaterialPropertyBlock materialProps;
        Renderer _renderer;
        
        public void Initialize(MineralItem itemData) {
            _renderer = GetComponent<Renderer>();
            Camera = GameObject.FindGameObjectWithTag("MainCamera");

            this.itemData = itemData;

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
                FXManager.Instance.SpawnFloatingText(transform.position, itemData);

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
