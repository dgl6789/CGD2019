using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// put on any object to make it draggable
/// </summary>
public class Draggable : MonoBehaviour {
    public static List<Draggable> draggables;
    Vector3 lastPos;
    Vector3 lastMousePos;
    bool dragging = false;
    public SpriteRenderer renderer;
    Vector3 originalLscale;
    int phase = 0;
    int delta = 1;
	// Use this for initialization
	void Start () {
        if (draggables == null)
        {
            draggables = new List<Draggable>();
        }
        lastPos = transform.position;
        lastMousePos = Input.mousePosition;
        originalLscale = transform.localScale;
	}
    private void FixedUpdate()
    {
        
        if (Input.GetButtonUp("LeftMouse")&&dragging)
        {
            dragging = false;
            if (draggables.Contains(this))
            {
                draggables.Remove(this);
            }
        }
        if (dragging)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
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
            if (draggables.Contains(this))
            {
                //this.GetComponent<App.CivilianMovement>().nextWaypoint = null;
                draggables.Remove(this);
            }
            lastPos = transform.position;
            if (renderer != null)
            {
                renderer.color = Color.white;
            }
            transform.localScale = originalLscale;
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
       
            if (renderer != null&&draggables.Count == 0||draggables.Contains(this))
            {
                renderer.color = Color.blue;
            }
            transform.localScale = originalLscale * 1.1f;
            if (Input.GetButton("LeftMouse"))
            {
            if (draggables.Count == 0 || draggables.Contains(this))
            {
                dragging = true;
                //transform.position = Input.mousePosition;
                transform.localScale = originalLscale * 1.4f;
                if(!draggables.Contains(this))
                draggables.Add(this);
            }
            }
            else
            {
                dragging = false;
            }
        
    }
    private void OnMouseExit()
    {
        
       
        
    }
}
