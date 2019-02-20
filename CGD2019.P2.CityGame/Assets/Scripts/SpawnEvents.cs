﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEvents : MonoBehaviour {
    float lastGenTime;
    public float bedTime;
    public float wakeTime;
    public List<float> peakHours;
    float time = 0;
    [Range(0, 100)]
    public float maxCivillians;
    [Range(0, 100)]
    public float minCivillians;
    [Range(0,100)]
    public float maxBadEvents;
    [Range(0,100)]
    public float maxGoodEvents;
   	// Use this for initialization
	void Start () {
        lastGenTime = 0;
        if (peakHours == null)
        {
            peakHours = new List<float>();
            peakHours.Add(120);
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        time += (Time.fixedDeltaTime / 2.5f)%240;//temporary time implementation 10 minute days
        if (time > wakeTime&&time<bedTime)
        {
            float desiredPeoplenum = 0;
            foreach (var peaktime in peakHours)
            {
                float tempNum = maxCivillians - Mathf.Abs(peaktime - time);
                if (tempNum > desiredPeoplenum)
                {
                    desiredPeoplenum = tempNum; 
                }
            }
            if (time - lastGenTime > 1 && App.CivilianManager.Instance.CivilianList.Count < desiredPeoplenum)
            {
                App.CivilianManager.Instance.SpawnCivilian();
                lastGenTime = time;
            }
            
        }
       
	}
}
