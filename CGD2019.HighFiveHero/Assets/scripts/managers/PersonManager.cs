using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    public class PersonManager : MonoBehaviour {

        public static PersonManager Instance;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void CheckInput() {
            // TODO: Check input for the active person's hands.
        }
    }
}
