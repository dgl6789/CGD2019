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

        /// Reference to the graphics raycaster
        [SerializeField] GraphicRaycaster uiRaycaster;

        [Header("Camera Controls")]
        Camera camera;
        Vector2 inputRotation, lastMousePosition;

        [SerializeField] float rotationSpeed;
        [SerializeField] float rotationSmoothing;

        [SerializeField] float zoomSpeed;
        [SerializeField] float zoomSmoothing;

        Vector2 MouseDelta { get { return new Vector2(lastMousePosition.x - Input.mousePosition.x, lastMousePosition.y - Input.mousePosition.y); } }

        /// Singleton initialization.
        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        private void Start() {
            camera = Camera.main;
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
                    if (GameClick()) {
                        DeformRockPoint();
                        TryMineGem();
                    }
                    CameraRotation.Instance.DoRotationUpdate();
                    break;
            }

            lastMousePosition = Input.mousePosition;
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

        /// <summary>
        /// Returns whether the position overlaps any canvas elements.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <returns>Whether position overlaps UI canvas elements.</returns>
        bool UIBlocksRaycast(Vector2 position)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            pointerData.position = Input.mousePosition;
            uiRaycaster.Raycast(pointerData, results);

            return results.Count > 0;
        }

        /// <summary>
        /// Determine whether a click/tap occured on the game window.
        /// </summary>
        /// <returns>Whether an unblocked click or tap occured at the mouse position.</returns>
        bool GameClick() { return Input.GetButtonDown("Click") && !UIBlocksRaycast(Input.mousePosition); }

        void DoCameraZoom()
        {

        }

        void DoCameraPan()
        {

        }
    }
}
