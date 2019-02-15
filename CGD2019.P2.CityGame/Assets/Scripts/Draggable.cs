using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draggable : MonoBehaviour {
    Vector3 originalLScale;
    float mult;
	// Use this for initialization
	void Start () {
        originalLScale = transform.localScale;
        mult = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        
            transform.localScale = originalLScale*mult;
        
	}
    private void OnMouseOver()
    {
        transform.localScale=
    }
    private void OnMouseExit()
    {
        
    }
}
