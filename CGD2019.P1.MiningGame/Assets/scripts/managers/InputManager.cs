using UnityEngine;
using App.Gameplay;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace App {

    public class InputManager : MonoBehaviour {

        // Singleton instance (reference this class' members via InputManager.Instance from any context that is 'using App;')
        public static InputManager Instance;

        [HideInInspector] public RuntimePlatform Platform;

        /// Reference to the rock's voxel grid.
        [SerializeField] VoxelGrid voxelGrid;

        /// Reference to the graphics raycaster
        [SerializeField] GraphicRaycaster uiRaycaster;

        [Header("Camera Controls")]
        Camera camera;
        Vector2 inputRotation, lastMousePosition;
        Touch initTouch;
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
            camera = Camera.main;

            Platform = Application.platform;
            initTouch = new Touch();
            desiredZoom = camera.orthographicSize;
            desiredTransform.position = camera.transform.position;
        }

        /// <summary>
        /// Checks for inputs each frame.
        /// </summary>
        public void Update() {
            // Wrap these in state machine checks wherever applicable.
            GameState state = StateManager.Instance.State;

            Debug.DrawLine(camera.transform.position, voxelGrid.Center);
            Debug.DrawLine(camera.transform.position, Vector3.zero);

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

                    if (GetInputDelta() != Vector2.zero) { DoCameraPan(GetInputDelta()); }

                    DoCameraZoom();
                    break;
            }

            SmoothCamera();

            lastMousePosition = Input.mousePosition;
        }

        /// <summary>
        /// Deforms the voxel rock by removing the voxel under the mouse position.
        /// </summary>
        public void DeformRockPoint() {
            if (Platform == RuntimePlatform.Android && Input.touchCount > 1) return;

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
            System.Collections.Generic.List<RaycastResult> results = new System.Collections.Generic.List<RaycastResult>();

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
            desiredTransform.RotateAround(voxelGrid.Center, camera.transform.up, delta.x * (Platform == RuntimePlatform.Android ? rotationSpeed.x : -rotationSpeed.y));
            desiredTransform.RotateAround(voxelGrid.Center, camera.transform.right, delta.y * (Platform == RuntimePlatform.Android ? -rotationSpeed.x : rotationSpeed.y));

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
            if (Vector3.Distance(camera.transform.position, desiredTransform.position) > 0.01f) {
                camera.transform.position = Vector3.Lerp(camera.transform.position, desiredTransform.position, Time.deltaTime * rotationSmoothing);

                camera.transform.LookAt(voxelGrid.Center, camera.transform.up);
            }

            if (Mathf.Abs(desiredZoom - camera.orthographicSize) > 0.01f) {
                camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, desiredZoom, Time.deltaTime * zoomSmoothing);
            }
        }
        #endregion

    }
}
