using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Gameplay {
    public class GemBehavior : MonoBehaviour {

        //voxel mesh reference

        //inventory reference

        //gem behavior values
        int health = 0;

        public bool debugIsFree = true;

        // Use this for initialization
        void Initialize(MineralItem gemInfo) {
            health = 20;
        }

        // Update is called once per frame
        void Update() {
            //destroy the gem if it's taken too much damage
            if (health <= 0) {
                Destroy(gameObject);
            }
        }

        //triggers when clicked
        private void OnMouseDown() {
            //if the gem is sufficiently clear, mine it
            if (IsMineable()) {
                Mine();
            } else {
                health--;
            }
        }

        //method to mine gem
        void Mine() {
            //add points and gem to inventory

            //delete gameobject
            Destroy(gameObject);
        }

        //method to determine if the gem can be mined
        bool IsMineable() {
            return debugIsFree;
        }
    }
}