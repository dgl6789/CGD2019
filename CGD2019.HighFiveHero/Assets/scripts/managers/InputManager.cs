using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    /// <summary>
    /// Handles all the non-UI input for the game by state. All non-UI input should
    /// be handled through this class.
    /// </summary>
    public class InputManager : MonoBehaviour {

        /// Singleton instance.
        public static InputManager Instance;

        [HideInInspector] public bool MobilePlatform;

        public float AccelerometerForceFactor;

        private Touch[] touches;
        public Touch[] Touches {
            get { return touches; }
        }

        public GameState InputState { get; set; }

        /// <summary>
        /// Singleton initialization and platform check.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            MobilePlatform = Application.platform == RuntimePlatform.Android;
        }

        /// <summary>
        /// Update per-state input behavior.
        /// </summary>
        public void UpdateInputs() {
            switch(InputState) {
                case GameState.GameOver:
                case GameState.Menu:
                    // Do input behavior for menus here.
                    touches = new Touch[0];
                    break;

                case GameState.Game:
                    // Do input behavior for the ingame state here.
                    touches = Input.touches;

                    HandManager.Instance.GetHandInputs(touches);
                    break;
            }
        }
    }
}
