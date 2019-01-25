using UnityEngine;
using App.Gameplay;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace App {

    public class InputManager : MonoBehaviour {

        // Singleton instance (reference this class' members via InputManager.Instance from any context that is 'using App;')
        public static InputManager Instance;

        /// Reference to the rock's voxel grid.
        [SerializeField] VoxelGrid voxelGrid;

        /// Singleton initialization.
        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        /// <summary>
        /// Checks for inputs each frame.
        /// </summary>
        public void Update() {
            // Wrap these in state machine checks wherever applicable.
            GameState state = StateManager.Instance.State;

            switch (state) {
                default:
                case GameState.MENU:
                    /// TODO: Input behavior for menu state
                    break;
                case GameState.MINING:
                    if (Input.GetButtonDown("Click")) {
                        DeformRockPoint();
                        TryMineGem();
                    }
                    break;
            }
        }

        /// <summary>
        /// Deforms the voxel rock by removing the voxel under the mouse position.
        /// </summary>
        public void DeformRockPoint() {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {

                Vector3 voxelPosition = hit.point - hit.normal * 0.5f;

                voxelGrid.SetVoxelTypeAtIndex(
                    Mathf.FloorToInt(voxelPosition.x), 
                    Mathf.FloorToInt(voxelPosition.y), 
                    Mathf.FloorToInt(voxelPosition.z), 
                    VoxelType.AIR);
            }
        }

        public void TryMineGem() {
            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                if (hit.collider.tag.Equals("Mineral")) hit.collider.GetComponent<GemBehavior>().TryMine();
            }
        }
    }
}
