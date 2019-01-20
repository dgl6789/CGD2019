using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.Gameplay;

namespace App {

    public class InputManager : MonoBehaviour {

        public static InputManager Instance;

        [SerializeField] VoxelGrid voxelGrid;

        // Use this for initialization
        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void Update() {
            // Wrap these in state machine checks when applicable.
            GameState state = StateManager.Instance.State;


            switch(state) {
                default:
                case GameState.MINING:
                    if(Input.GetButtonDown("Click")) {
                        DoRockTap();
                    }
                    break;
            }
        }

        public void DoRockTap() {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                Vector3 pos = hit.point - hit.normal * 0.5f;

                voxelGrid.SetVoxelTypeAtPosition(new Vector3Int(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z)), VoxelType.AIR);
            }
        }
    }
}
