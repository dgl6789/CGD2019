using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodTrail : MonoBehaviour {

    Transform carToFollow;

	public void SetEmitter(Transform toFollow) {
        carToFollow = toFollow;
    }

    void Update() {
        if (carToFollow != null) transform.position = carToFollow.transform.position;
        else Destroy(gameObject, 2f);
    }
}
