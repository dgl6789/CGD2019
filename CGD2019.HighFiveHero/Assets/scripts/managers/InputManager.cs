using System.Linq;
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

        private Touch[] touches;
        public Touch[] Touches {
            get { return touches; }
        }

        [HideInInspector] public List<Ring> RingsToRemove;

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
            switch(StateManager.Instance.State) {
                case GameState.GameOver:
                case GameState.Menu:
                    // Do input behavior for menus here.
                    touches = new Touch[0];
                    break;

                case GameState.Game:
                    // Do input behavior for the ingame state here.
                    touches = Input.touches;

                    // Check input on the active ring objects.
                    if (RunManager.Instance.ActiveRings.Count > 0) {
                        RingsToRemove = new List<Ring>();

                        // On mobile
                        if (MobilePlatform) {
                            for (int i = 0; i < touches.Length; i++) {
                                
                                RaycastHit hit;
                                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(i).position), out hit)) {
                                    foreach (Ring r in RunManager.Instance.ActiveRings) {
                                        if (r && hit.collider == r.GetComponent<SphereCollider>()) {
                                            r.OnCollect();
                                            RingsToRemove.Add(r);
                                        }
                                    }
                                }
                            }
                        } 
                        // On desktop
                        else {
                            if (Input.GetButtonDown("Click")) {

                                RaycastHit hit;
                                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                                    foreach (Ring r in RunManager.Instance.ActiveRings) {
                                        if (r && hit.collider == r.GetComponent<SphereCollider>()) {
                                            r.OnCollect();
                                            RingsToRemove.Add(r);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (Ring r in RingsToRemove.ToList())
                            Destroy(RunManager.Instance.ActiveRings[RunManager.Instance.ActiveRings.IndexOf(r)].gameObject);
                    }
                    break;
            }
        }
    }
}
