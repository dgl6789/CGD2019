using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianManager : MonoBehaviour {

    public static CivilianManager Instance;

    //civilians
    List<GameObject> civilianList = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
