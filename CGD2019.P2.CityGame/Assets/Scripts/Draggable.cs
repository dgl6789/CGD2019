using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour {
    public SpriteRenderer renderer;
    Vector3 originalLscale;
    int phase = 0;
    int delta = 1;
	// Use this for initialization
	void Start () {
        originalLscale = transform.localScale;
	}
    private void FixedUpdate()
    {
        if (phase > 99 || phase < 1)
        {
            delta *= -1;
        }
        phase += delta;
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
            transform.position = Input.mousePosition;
            transform.localScale = originalLscale * 1.4f;
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
