using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace App
{

    public class CameraManager : MonoBehaviour
    {

        // Singleton instance (reference this class' members via InputManager.Instance from any context that is 'using App;')
        public static CameraManager Instance;

        [HideInInspector] public RuntimePlatform Platform;

        bool inputIsLocked;

        /// Reference to the graphics raycaster
        [SerializeField] GraphicRaycaster uiRaycaster;
        [SerializeField] float touchInputDelay;

        [SerializeField] SpriteRenderer world;

        [Header("Camera Controls")]
        Camera cam;
        Vector2 lastMousePosition, cameraBounds;
        float desiredZoom;
        
        [SerializeField] float panSpeed;
        [SerializeField] float panSmoothing;
        [SerializeField] float zoomDirectionThreshold;

        [SerializeField] Vector2 zoomSpeed;
        [SerializeField] Vector2 zoomBounds;
        [SerializeField] float touchSensitivity;
        [SerializeField] float zoomSmoothing;

        [SerializeField] Transform desiredTransform;

        Vector2 MouseDelta { get { return new Vector2(lastMousePosition.x - Input.mousePosition.x, lastMousePosition.y - Input.mousePosition.y); } }

        /// Singleton initialization.
        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        /// <summary>
        /// Initialization.
        /// </summary>
        private void Start()
        {
            cam = Camera.main;

            Platform = Application.platform;
            desiredZoom = cam.orthographicSize;
            desiredTransform.position = cam.transform.position;

            GenerateCameraBounds();
        }

        /// <summary>
        /// Checks for inputs each frame.
        /// </summary>
        public void Update()
        {
            // Wrap these in state machine checks wherever applicable.
            GameState state = StateManager.Instance.State;

            if (!inputIsLocked)
            { // Only perform input checks if input is unlocked.
                // Perform per-state input behavior
                switch (state)
                {
                    default:
                    case GameState.MENU:
                        /// TODO: Input behavior for menu state
                        break;
                    case GameState.INGAME:

                        // Do camera controls.
                        if (GetInputDelta() != Vector2.zero) { DoCameraPan(GetInputDelta()); }

                        DoCameraZoom();
                        break;
                }

                // Smooth the camera controls.
                SmoothCamera();

                lastMousePosition = Input.mousePosition;
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
        /// Determine whether a click/tap occured on the game window at the mouse position.
        /// </summary>
        /// <returns>Whether an unblocked click or tap occured at the mouse position.</returns>
        bool GameClick() { return Input.GetButtonDown("LeftMouse") && !UIBlocksRaycast(Input.mousePosition); }

        /// <summary>
        /// Determine whether a click/tap is occuring on the game window at the mouse position.
        /// </summary>
        /// <returns>Whether an unblocked click or tap is occuring at the mouse position.</returns>
        bool GameClickHold() { return Input.GetButton("LeftMouse") && !UIBlocksRaycast(Input.mousePosition); }

        /// <summary>
        /// Lock or unlock the input.
        /// </summary>
        /// <param name="locked">Whether input should be locked.</param>
        public void LockInput(bool locked = true) { inputIsLocked = locked; }

        #region Camera Controls
        /// <summary>
        /// Adjust the orthographic camera size.
        /// </summary>
        void DoCameraZoom()
        {
            if (Platform == RuntimePlatform.Android)
            {
                if (Input.touchCount != 2) return;

                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                Vector2 aPrev = a.position - a.deltaPosition;
                Vector2 bPrev = b.position - b.deltaPosition;

                float touchMag = (a.position - b.position).magnitude;
                float prevTouchMag = (aPrev - bPrev).magnitude;

                desiredZoom += (prevTouchMag - touchMag) * zoomSpeed.x * touchSensitivity;
            }
            else
            {
                desiredZoom -= Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed.y;
            }

            GenerateCameraBounds();

            desiredZoom = Mathf.Clamp(desiredZoom, zoomBounds.x, zoomBounds.y);
        }

        /// <summary>
        /// Adjust the rotation and position of the camera transformer to orbit the rock.
        /// </summary>
        /// <param name="delta"></param>
        void DoCameraPan(Vector2 delta)
        {
            desiredTransform.Translate(delta * panSpeed);

            desiredTransform.position = new Vector3(
                Mathf.Clamp(desiredTransform.position.x, -cameraBounds.x, cameraBounds.x), 
                Mathf.Clamp(desiredTransform.position.y, -cameraBounds.y, cameraBounds.y),
                desiredTransform.position.z);
        }

        /// <summary>
        /// Get the difference between the current and previous positions of the platform-specific inputs.
        /// </summary>
        /// <returns>Vector representing the change in input location.</returns>
        Vector2 GetInputDelta()
        {
            if (Platform == RuntimePlatform.Android)
            {
                if (Input.touchCount != 2) return Vector2.zero;

                Touch a = Input.GetTouch(0);
                Touch b = Input.GetTouch(1);

                if (a.phase == TouchPhase.Moved || b.phase == TouchPhase.Moved)
                {
                    float touchDelta = Vector2.Distance(a.deltaPosition, b.deltaPosition);

                    if (Mathf.Abs(touchDelta) > zoomDirectionThreshold)
                    {
                        // The gesture is a pan
                        // Return the greater deltaposition betweeen the two touches
                        return a.deltaPosition.magnitude > b.deltaPosition.magnitude ? a.deltaPosition * touchSensitivity : b.deltaPosition * touchSensitivity;
                    }
                }

                return Vector2.zero;
            }
            else
            {
                return Input.GetButton("RightMouse") ? MouseDelta : Vector2.zero;
            }
        }

        /// <summary>
        /// Smooth the camera effects by lerping between the camera position and camera transformer.
        /// </summary>
        void SmoothCamera() {
            if (Vector3.Distance(cam.transform.position, desiredTransform.position) > 0.01f)
                cam.transform.position = Vector3.Lerp(cam.transform.position, desiredTransform.position, Time.deltaTime * panSmoothing);

            if (Mathf.Abs(desiredZoom - cam.orthographicSize) > 0.01f)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, desiredZoom, Time.deltaTime * zoomSmoothing);
        }

        void GenerateCameraBounds()
        {
            float h = cam.orthographicSize;
            float w = cam.aspect * h;

            cameraBounds = new Vector2(
                world.sprite.bounds.extents.x * world.transform.localScale.x - w,
                world.sprite.bounds.extents.y * world.transform.localScale.y - h);
        }
        #endregion

    }
}
