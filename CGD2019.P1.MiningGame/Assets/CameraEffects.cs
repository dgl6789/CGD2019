using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour {

    public static CameraEffects Instance;

    Camera cam;
    
	void Awake () {
        if (Instance == null) Instance = this;
        else Destroy(this);

        cam = Camera.main;
	}

    public void Shake(float amount, float duration) {
        StartCoroutine(DoShake(amount, duration));
    }
	
    IEnumerator DoShake(float amount, float duration) {
        float initialDuration = duration;

        while(duration > 0) {
            duration -= Time.deltaTime;

            float durationFactor = duration / initialDuration;

            cam.transform.position += (Random.insideUnitSphere * durationFactor);

            yield return null;
        }
    }
}
