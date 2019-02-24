using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// put on any object to make it draggable
/// </summary>
public class Draggable : MonoBehaviour {
    Vector3 lastPos;
    Vector3 lastMousePos;
    public bool dragging = false;
    public SpriteRenderer renderer;
    Vector3 originalLscale;
    int phase = 0;
    int delta = 1;
	// Use this for initialization
	void Start () {
        lastPos = transform.position;
        lastMousePos = Input.mousePosition;
        originalLscale = transform.localScale;
	}
    private void FixedUpdate()
    {
        if (Input.GetButtonUp("LeftMouse"))
        {
            dragging = false;
        }
        if (dragging)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                lastPos = transform.position;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0));
                    lastPos = transform.position;
                }
            }
        }
        else
        {
            lastPos = transform.position;
        }
        if (phase > 99 || phase < 1)
        {
            delta *= -1;
        }
        phase += delta;
        lastMousePos = Input.mousePosition;
    }
    private void OnMouseOver()
    {
        if (renderer != null)
        {
            renderer.color = Color.blue;
        }
        transform.localScale = originalLscale * 1.1f;
        if (Input.GetButton("LeftMouse"))
        {
            dragging = true;
            //transform.position = Input.mousePosition;
            transform.localScale = originalLscale * 1.4f;
        }
        else
        {
            dragging = false;
        }
    }
    private void OnMouseExit()
    {
        
        if (renderer != null)
        {
            renderer.color = Color.white;
        }
        transform.localScale = originalLscale;
    }
}
