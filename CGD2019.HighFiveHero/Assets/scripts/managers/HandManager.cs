using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace App {
    /// <summary>
    /// Manages hand input and resolution.
    /// </summary>
    public class HandManager : MonoBehaviour {

        /// Singleton instance.
        public static HandManager Instance;

        // The amount of time to measure for the largest delta after a touch on a hand registers.
        [SerializeField] float strengthDeltaInterval;

        // List of hands/fives that still need input checking.
        private List<Hand> ActiveHands;
        private List<HighFive> ActiveFives;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        /// <summary>
        /// Update the hand movement, resolution, and spawning. Called each frame in game state.
        /// </summary>
        public void UpdateHands() {

            #region HIGH FIVE RESOLUTION
            List<HighFive> inactiveFives = new List<HighFive>();

            foreach (HighFive five in ActiveFives) {
                if (five.HasDelta) {
                    // This five is ready to be evaluated.
                    if (five.Hand.StrengthIsAcceptable(five.MaxDelta))
                        five.Hand.OnSuccessfulFive();
                    else
                        five.Hand.OnFailedFive();

                    inactiveFives.Add(five);
                }
            }

            // Remove resolved fives from the active fives list.
            foreach (HighFive f in inactiveFives) ActiveFives.Remove(f);
            #endregion

        }

        /// <summary>
        /// Evaluate the strength of the inputs from InputManager, if they fall on active hands.
        /// </summary>
        /// <param name="touches">Array of touches for this frame.</param>
        public void GetHandInputs(Touch[] touches) {

            // Get the relative strength of each new touch.
            foreach(Touch t in touches) {
                bool newTouch = true;

                // Determine if this touch is actually a new one
                foreach(HighFive f in ActiveFives) {
                    if (f.Touch.fingerId == t.fingerId)
                        newTouch = false;
                }
                
                if (newTouch) {
                    // Check if this touch was on a hand.
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(t.position), t.position);

                    if (hit.collider.tag == "Hand") {
                        // Make a new high five and check its strength.
                        HighFive five = new HighFive(t, hit.collider.GetComponent<Hand>());
                        ActiveFives.Add(five);

                        StartCoroutine(GetMaxDelta(five));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the maximum accelerometer delta over the measurement interval and writes it to a HighFive.
        /// </summary>
        /// <param name="five">HighFive to write to.</param>
        /// <returns>Coroutine.</returns>
        private IEnumerator GetMaxDelta(HighFive five) {
            float timer = strengthDeltaInterval;

            float maxDelta = 0;
            Vector3 lastAcceleration = Input.acceleration;

            while(timer > 0) {
                timer -= Time.deltaTime;

                float thisDelta = (Input.acceleration - lastAcceleration).sqrMagnitude;

                // If a larger delta is read, write the new max delta.
                maxDelta = Mathf.Max(thisDelta, maxDelta);

                lastAcceleration = Input.acceleration;

                yield return null;
            }

            // write the max interval to the high five.
            five.MaxDelta = maxDelta;
            five.HasDelta = true;
        }

        /// <summary>
        /// Convert a hand size in unity units to a target strength for a high five.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public float HandSizetoTargetStrength(float size) {
            return size;
        }
    }

    /// <summary>
    /// HighFive aggregates a touch, accelerometer delta, and
    /// hand object into one struct for ease of reference.
    /// </summary>
    public struct HighFive {
        public Touch Touch;
        public Hand Hand;
        
        public bool HasDelta;
        public float MaxDelta;

        public HighFive(Touch touch, Hand hand) {
            HasDelta = false;
            MaxDelta = 0f;

            Touch = touch;
            Hand = hand;
        }
    }
}
