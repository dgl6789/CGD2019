using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// put on any object to make it draggable
/// </summary>
public class Draggable : MonoBehaviour {
    RaycastHit2D touch;
    public static Draggable dragable;
    Vector3 lastPos;
    Vector3 lastMousePos;
    bool dragging = false;
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
        if (Input.GetButton("LeftMouse") && dragable == null)
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                if (GetComponent<BoxCollider2D>().bounds.Contains(Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0))))
                {
                    if (touch.transform.tag == "citizen")
                    {
                        
                            
                            //transform.position = Input.mousePosition;
                            transform.localScale = originalLscale * 1.4f;
                            dragable = this;
                        
                    }
                }
                else
                {
                    if (touch.transform.tag == "citizen")
                    {
                        touch.transform.GetComponent<App.CivilianMovement>().nextWaypoint = null;
                    }
                    dragable = null;
                    transform.localScale = originalLscale;
                }
                
                lastPos = transform.position;
            }
            else if (Application.platform == RuntimePlatform.Android)
            {

                if (GetComponent<BoxCollider2D>().bounds.Contains(Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y, 0))))
                {
                    if (touch.transform.tag == "citizen")
                    {

                            dragable = this;
                            transform.localScale = originalLscale * 1.4f;
                          
                    }
                }
                else
                {
                    if (touch.transform.tag == "citizen")
                    {
                        touch.transform.GetComponent<App.CivilianMovement>().nextWaypoint = null;
                    }
                    dragging = false;
                    transform.localScale = originalLscale;
                }

                lastPos = transform.position;
                
            }
            
        }
        if (dragable!=null)
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
            if (!Input.GetButton("LeftMouse"))
            {
                dragable = null;
                transform.localScale = originalLscale;
            }
        }
        

    }
    private void OnMouseOver()
    {
       
            /*if (renderer != null&&draggables.Count == 0||draggables.Contains(this))
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
            }*/
        
    }
    private void OnMouseExit()
    {
        
       
        
    }
}
