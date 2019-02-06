using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFade : MonoBehaviour {
    public int timer = 120;
	// Use this for initialization
	void Awake () {
        
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position += transform.up*.01f;
        timer--;
        if (timer < 1)
        {
            Destroy(this.gameObject);
        }
	}
}
