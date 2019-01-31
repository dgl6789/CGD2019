using UnityEngine;
using App.Gameplay;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace App {

    public class InputManager : MonoBehaviour {

        // Singleton instance (reference this class' members via InputManager.Instance from any context that is 'using App;')
        public static InputManager Instance;

        [HideInInspector] public RuntimePlatform Platform;

        /// Reference to the rock's voxel grid.
        [SerializeField] VoxelGrid voxelGrid;
        
        /// Reference to the graphics raycaster
        [SerializeField] GraphicRaycaster uiRaycaster;
        [SerializeField] float touchInputDelay;

        [Header("Camera Controls")]
        Camera cam;
        Vector2 inputRotation, lastMousePosition;
        float desiredZoom;

        [SerializeField] Vector2 rotationSpeed;
        [SerializeField] float rotationSmoothing;
        [SerializeField] float zoomDirectionThreshold;
        
        [SerializeField] Vector2 zoomSpeed;
        [SerializeField] Vector2 zoomBounds;
        [SerializeField] float touchSensitivity;
        [SerializeField] float zoomSmoothing;
        [SerializeField] float distanceFromVoxelGrid;

        [SerializeField] Transform desiredTransform;
        
        Vector2 MouseDelta { get { return new Vector2(lastMousePosition.x - Input.mousePosition.x, lastMousePosition.y - Input.mousePosition.y); } }
        
        /// Singleton initialization.
        void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }
        
        private void Start() {
            cam = Camera.main;

            Platform = Application.platform;
            desiredZoom = cam.orthographicSize;
            desiredTransform.position = cam.transform.position;
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
                    ToolItem i = InventoryManager.Instance.ActiveTool;

                    if (i != null) {
                        switch (i.InputType) {
                            case ToolInputType.INSTANT:
                                if (Input.GetButtonDown("LeftClick")) StartCoroutine(DeformRock(i));
                                break;
                            case ToolInputType.SUSTAINED:
                                if (Input.GetButton("LeftClick")) StartCoroutine(DeformRock(i));
                                break;
                        }

                        if (i.Type == ToolType.CHISEL && Input.GetButtonDown("LeftClick")) TryMineGem();
                    }

                    if (GetInputDelta() != Vector2.zero) { DoCameraPan(GetInputDelta()); }

                    DoCameraZoom();
                    break;
            }

            SmoothCamera();

            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// Deforms the voxel rock by removing the voxel(s) under the mouse position.
        /// </summary>
        public IEnumerator DeformRock(ToolItem item) {
            if (item == null) yield break;

            if (Platform == RuntimePlatform.Android) {
                yield return new WaitForSeconds(touchInputDelay);

                if(Input.touchCount > 1) yield break;
            }

            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) yield break;

            Vector3 voxelPosition = hit.point - hit.normal * 0.5f;

            switch (item.Type) {
                case ToolType.POINT:
                    voxelGrid.TryVoxelDestruction(
                        Mathf.FloorToInt(voxelPosition.x),
                        Mathf.FloorToInt(voxelPosition.y),
                        Mathf.FloorToInt(voxelPosition.z),
                        item.Power);
                    break;
                case ToolType.AREA:
                    List<Vector3Int> indices = new List<Vector3Int>();

                    for(int x = 0; x < voxelGrid.X; x++) {
                        for (int y = 0; y < voxelGrid.Y; y++) {
                            for (int z = 0; z < voxelGrid.Z; z++) {
                                if (Mathf.Pow(x - voxelPosition.x, 2) + Mathf.Pow(y - voxelPosition.y, 2) + Mathf.Pow(z - voxelPosition.z, 2) <= item.BreakRadius) indices.Add(new Vector3Int(x, y, z));
                            }
                        }
                    }

                    voxelGrid.TryMultipleVoxelDestruction(indices.ToArray(), item.Power);
                    break;
            }
        }

        public void TryMineGem() {
            if (Platform == RuntimePlatform.Android && Input.touchCount > 1) return;

            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                if (hit.collider.tag.Equals("Mineral")) hit.collider.GetComponent<GemBehavior>().TryMine();
            }
        }

        /// <summary>
        /// Returns whether the position overlaps any canvas elements.
        /// </summary>
        /// <param name="position">Position (screen) to check.</param>
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
        bool GameClick() { return Input.GetButtonDown("LeftClick") && !UIBlocksRaycast(Input.mousePosition); }

    #region Camera Controls
        /// <summary>
        /// Adjust the orthographic camera size.
        /// </summary>
        void DoCameraZoom() {
            if (Platform == RuntimePlatform.Android) {
                if (Input.touchCount != 2) return;

                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                Vector2 aPrev = a.position - a.deltaPosition;
                Vector2 bPrev = b.position - b.deltaPosition;
                
                float touchMag = (a.position - b.position).magnitude;
                float prevTouchMag = (aPrev - bPrev).magnitude;

                desiredZoom += (prevTouchMag - touchMag) * zoomSpeed.x * touchSensitivity;
            } else {
                desiredZoom -= Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed.y;
            }

            desiredZoom = Mathf.Clamp(desiredZoom, zoomBounds.x, zoomBounds.y);
        }

        /// <summary>
        /// Adjust the rotation and position of the camera transformer to orbit the rock.
        /// </summary>
        /// <param name="delta"></param>
        void DoCameraPan(Vector2 delta) {
            desiredTransform.RotateAround(voxelGrid.Center, cam.transform.up, delta.x * (Platform == RuntimePlatform.Android ? rotationSpeed.x : -rotationSpeed.y));
            desiredTransform.RotateAround(voxelGrid.Center, cam.transform.right, delta.y * (Platform == RuntimePlatform.Android ? -rotationSpeed.x : rotationSpeed.y));

            desiredTransform.position = (desiredTransform.position - voxelGrid.Center).normalized * distanceFromVoxelGrid + voxelGrid.Center;
        }

        /// <summary>
        /// Get the difference between the current and previous positions of the platform-specific inputs.
        /// </summary>
        /// <returns>Vector representing the change in input location.</returns>
        Vector2 GetInputDelta() {
            if(Platform == RuntimePlatform.Android) {
                if (Input.touchCount != 2) return Vector2.zero;

                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                if(a.phase == TouchPhase.Moved || b.phase == TouchPhase.Moved) {
                    float touchDelta = Vector2.Distance(a.deltaPosition, b.deltaPosition);

                    if(Mathf.Abs(touchDelta) > zoomDirectionThreshold) {
                        // The gesture is a pan
                        // Return the greater deltaposition betweeen the two touches
                        return a.deltaPosition.magnitude > b.deltaPosition.magnitude ? a.deltaPosition * touchSensitivity : b.deltaPosition * touchSensitivity;
                    }
                }

                return Vector2.zero;
            } else {
                return Input.GetButton("RightClick") ? MouseDelta : Vector2.zero;
            }
        }

        /// <summary>
        /// Smooth the camera effects by lerping between the camera position and camera transformer.
        /// </summary>
        void SmoothCamera() {
            if (Vector3.Distance(cam.transform.position, desiredTransform.position) > 0.01f) {
                cam.transform.position = Vector3.Lerp(cam.transform.position, desiredTransform.position, Time.deltaTime * rotationSmoothing);

                cam.transform.LookAt(voxelGrid.Center, cam.transform.up);
            }

            if (Mathf.Abs(desiredZoom - cam.orthographicSize) > 0.01f) {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredZoom, Time.deltaTime * zoomSmoothing);
            }
        }
        #endregion
        
    }
}
