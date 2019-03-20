using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using App.Util;
using App.UI;

namespace App {
    /// <summary>
    /// Manages hand input and resolution.
    /// </summary>
    public class HandManager : MonoBehaviour {

        /// Singleton instance.
        public static HandManager Instance;

        // Hand prefab object.
        [SerializeField] GameObject handObject;
        [SerializeField] GameObject[] indicatorObjects;

        // Min and max size of a hand.
        [SerializeField] Vector2 handSizeRange;

        // Object that holds all the hands.
        public Transform handParent;

        [SerializeField] float acceptableStrengthRange;
        [SerializeField] float perfectStrengthRange;

        // The amount of time to measure for the largest delta after a touch on a hand registers.
        [SerializeField] float strengthDeltaInterval;

        // The range that high five input strengths are bound to.
        [SerializeField] Vector2 parsedStrengthRange;
        [SerializeField] float maximumRawStrength;

        // List of hands/fives that still need input checking.
        private List<Hand> ActiveHands;
        private List<HighFive> ActiveFives;

        // Previous timestamp to regulate spawning
        private float previousGameTime = 0.0f;
        public float PreviousGameTime
        {
            get { return previousGameTime; }
            set { previousGameTime = value; }
        }

        [Header("Scoring")]

        public int SuccessfulFiveScoreReward;
        public int SuccessfulFiveTimeReward;
        public int PerfectFiveScoreReward;
        public int PerfectFiveTimeReward;

        public int FailedFiveTimePenalty;

        [Space(10)]

        [SerializeField] string[] scoreIndicatorSuccessDescriptions;
        [SerializeField] string[] scoreIndicatorFailureDescriptions;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if(this) {
                ActiveHands = new List<Hand>();
                ActiveFives = new List<HighFive>();
            }
        }

        /// <summary>
        /// Update the hand movement, resolution, and spawning. Called each frame in game state.
        /// </summary>
        public void UpdateHands() {
            if (StateManager.Instance.State.Equals(GameState.Game)) {

                #region HAND SPAWNING

                // TODO: Actual hand spawning code goes here (Interval only -- positioning, etc. should be in SpawnHand()).
                //if (ActiveHands.Count == 0) { SpawnHand(); }

                //spawns a hand every second
                if (RunManager.Instance.TimePassed(previousGameTime))
                {
                    previousGameTime = RunManager.Instance.CurrentGameTimer;

                    Debug.Log(previousGameTime);

                    SpawnHand();
                }

                #endregion

                #region HIGH FIVE RESOLUTION

                GetHandInputs(InputManager.Instance.Touches);

                List<HighFive> resolvedFives = new List<HighFive>();

                foreach (HighFive five in ActiveFives) {
                    if (five.Hand != null) {
                        if (five.MaxDelta == Mathf.Infinity) {
                            // Infinite delta means an automatic normal success (testing on desktop only).
                            five.Hand.OnSuccessfulFive(false);
                        } else if (five.MaxDelta > 0) {
                            // This five is ready to be evaluated.
                            if (five.Hand.StrengthIsAcceptable(five.MaxDelta))
                                five.Hand.OnSuccessfulFive(five.Hand.StrengthIsPerfect(five.MaxDelta));
                            else
                                five.Hand.OnFailedFive();
                        }

                        resolvedFives.Add(five);
                    }
                }

                // Remove resolved fives from the active fives list.
                foreach (HighFive f in resolvedFives) {
                    RemoveHand(f.Hand);
                    ActiveFives.Remove(f);
                }

                #endregion
            }
        }

        /// <summary>
        /// Spawn a hand object.
        /// </summary>
        private void SpawnHand() {
            // TODO: Have this method attach the spawned hand to the guy, put it at a reasonable starting position, etc.
            Hand h = Instantiate(handObject, handParent).GetComponent<Hand>();

            h.Initialize(Random.Range(handSizeRange.x, handSizeRange.y), acceptableStrengthRange, perfectStrengthRange);

            ActiveHands.Add(h);
        }

        /// <summary>
        /// Remove a hand.
        /// </summary>
        /// <param name="hand">Hand to remove.</param>
        private void RemoveHand(Hand hand) {
            if (ActiveHands.Contains(hand)) {
                ActiveHands.Remove(hand);
            }

            // TODO: Replace this with behavior to clean up a hand nicely.
            if(hand) Destroy(hand.gameObject);
        }

        /// <summary>
        /// Cleanup all the active hands (used on game over).
        /// </summary>
        public void CleanupAllHands() {
            while(ActiveHands.Count > 0)
                RemoveHand(ActiveHands[0]);
        }

        /// <summary>
        /// Evaluate the strength of the inputs from InputManager, if they fall on active hands.
        /// </summary>
        /// <param name="touches">Array of touches for this frame.</param>
        public void GetHandInputs(Touch[] touches) {
            if (InputManager.Instance.MobilePlatform) {
                // Get the relative strength of each new touch.
                foreach (Touch t in touches) {
                    bool newTouch = true;

                    // Determine if this touch is actually a new one
                    foreach (HighFive f in ActiveFives) {
                        if (f.Touch.fingerId == t.fingerId)
                            newTouch = false;
                    }

                    if (newTouch) {
                        // Check if this touch was on a hand.
                        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(t.position), t.position);

                        if (hit.collider && hit.collider.tag == "Hand") {
                            // Make a new high five and check its strength.
                            HighFive five = new HighFive(t, hit.collider.GetComponent<Hand>());
                            StartCoroutine(GetMaxDelta(five));
                        }
                    }
                }
            } else {
                if (Input.GetMouseButtonDown(0)) {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                    if (hit.collider && hit.collider.tag == "Hand") {
                        // Make a new high five and check its strength.
                        HighFive five = new HighFive(hit.collider.GetComponent<Hand>());
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

                float thisDelta = Mathf.Abs((Input.acceleration - lastAcceleration).sqrMagnitude);

                // If a larger delta is read, write the new max delta.
                maxDelta = Mathf.Max(thisDelta, maxDelta);

                lastAcceleration = Input.acceleration;

                yield return null;
            }

            // write the max delta to the high five object.
            five.MaxDelta = InputManager.Instance.MobilePlatform ? MathUtility.Map(Mathf.Clamp(maxDelta, 0, maximumRawStrength), 0, maximumRawStrength, parsedStrengthRange.x, parsedStrengthRange.y) : Mathf.Infinity;
            ActiveFives.Add(five);
        }

        /// <summary>
        /// Convert a hand scale in unity units to a target strength for a high five.
        /// </summary>
        /// <param name="size">Scale factor of the hand object.</param>
        /// <returns>Scale factor of the hand scaled into the parsed strength range.</returns>
        public float HandSizetoTargetStrength(float size) {
            return MathUtility.Map(size, handSizeRange.x, handSizeRange.y, parsedStrengthRange.x, parsedStrengthRange.y);
        }

        /// <summary>
        /// Spawn a score indicator object.
        /// </summary>
        /// <param name="origin">Transform of the origin hand.</param>
        /// <param name="count">Number of score to reflect.</param>
        public void SpawnScoreIndicator(Transform origin, bool perfect = false, int count = 1) {
            string description = scoreIndicatorSuccessDescriptions[Random.Range(0, scoreIndicatorSuccessDescriptions.Length)];

            Indicator i = Instantiate(indicatorObjects[0], origin.position, Quaternion.identity, handParent).GetComponent<Indicator>();
            i.Initialize(count, description, perfect);
        }

        /// <summary>
        /// Spawn a time indicator object.
        /// </summary>
        /// <param name="origin">Transform of the origin hand.</param>
        /// <param name="count">Number of seconds to reflect.</param>
        /// <param name="success">Whether the time is being added or subtracted.</param>
        public void SpawnTimeIndicator(Transform origin, int count, bool success = false) {
            string description = success ? "" : scoreIndicatorFailureDescriptions[Random.Range(0, scoreIndicatorFailureDescriptions.Length)];

            Indicator i = Instantiate(indicatorObjects[1], origin.position, Quaternion.identity, handParent).GetComponent<Indicator>();
            i.Initialize(count, description, false, success ? 0.15f : 0f);
        }

        /// <summary>
        /// Spawn a ring indicator object.
        /// </summary>
        /// <param name="origin">Transform of the origin hand.</param>
        public void SpawnRingIndicator(Transform origin) {
            Indicator i = Instantiate(indicatorObjects[2], origin.position, Quaternion.identity, handParent).GetComponent<Indicator>();
            i.Initialize(1);
        }
    }

    /// <summary>
    /// HighFive aggregates a touch, accelerometer delta, and
    /// hand object into one struct for ease of reference.
    /// </summary>
    public struct HighFive {
        public Touch Touch;
        public Hand Hand;
        
        public float MaxDelta;

        public HighFive(Touch touch, Hand hand) {
            MaxDelta = -1f;

            Touch = touch;
            Hand = hand;
        }

        // Overload for auto-success (desktop testing only).
        public HighFive(Hand hand) {
            MaxDelta = Mathf.Infinity;

            Touch = new Touch();
            Hand = hand;
        }
    }
}
