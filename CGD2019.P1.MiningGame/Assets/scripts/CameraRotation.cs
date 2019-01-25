using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour {

    public static CameraRotation Instance;

    private Touch initTouch = new Touch();
    public Camera cam;

    private float rotX = 0f;
    private float rotY = 0f;
    private GameObject Rock;
    public Vector3 rockPos;
    public float rotSpeed = 1f;
    public float dir = 1;
    Vector2 lastMousePos;

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    // Use this for initialization
    void Start () {
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Rock = GameObject.Find("Rock");
        rockPos = Rock.transform.position;
        lastMousePos = Vector2.positiveInfinity;
        Vector3 RockSizeOffset = new Vector3(Rock.GetComponent<App.Gameplay.VoxelGrid>().X, Rock.GetComponent<App.Gameplay.VoxelGrid>().Y, Rock.GetComponent<App.Gameplay.VoxelGrid>().Z) / 2f;
        rockPos += RockSizeOffset;
    }
	
	// Update is called once per frame
	public void DoRotationUpdate()
    {
        
        if (lastMousePos.x != Vector2.positiveInfinity.x)
        {
            float deltaX = lastMousePos.x - Input.mousePosition.x;
            float deltaY = lastMousePos.y - Input.mousePosition.y;
            cam.transform.RotateAround(rockPos, transform.up, deltaX  * rotSpeed * dir *-1);
            cam.transform.RotateAround(rockPos, transform.right, deltaY  * rotSpeed * dir);
        }
        if (Input.GetMouseButton(1))
        {
            lastMousePos = Input.mousePosition;
        }
        else
        {
            lastMousePos = Vector2.positiveInfinity;
        }

        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                initTouch = touch;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                //swiping
                float deltaX = initTouch.position.x - touch.position.x;
                float deltaY = initTouch.position.y - touch.position.y;
                initTouch = touch;
                cam.transform.RotateAround(rockPos, transform.up, deltaX * Time.fixedDeltaTime * rotSpeed * dir*-1);
                cam.transform.RotateAround(rockPos, transform.right, deltaY * Time.fixedDeltaTime * rotSpeed * dir);
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                initTouch = new Touch();
            }
        }
	}
    
}
