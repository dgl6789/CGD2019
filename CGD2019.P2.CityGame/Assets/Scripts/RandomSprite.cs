using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSprite : MonoBehaviour {

    public Sprite[] sprites;

	// Use this for initialization
	void Start () {
        GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
	}
}
