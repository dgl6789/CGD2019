﻿using System.Collections.Generic;
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
        [SerializeField] GameObject[] HandObjects;
        [SerializeField] GameObject[] indicatorObjects;

        // Min and max size of a hand.
        [SerializeField] Vector2 handSizeRange;
        [SerializeField] float[] possibleHandSizes;

        // Object that holds all the hands.
        public Person handParent;

        [SerializeField] float acceptableStrengthRange;
        [SerializeField] float perfectStrengthRange;

        // The amount of time to measure for the largest delta after a touch on a hand registers.
        [SerializeField] float strengthDeltaInterval;

        // The range that high five input strengths are bound to.
        [SerializeField] Vector2 parsedStrengthRange;
        [Range(0f, 10f)]
        [SerializeField] float maximumRawStrength;

        [Range(0f, 1f)]
        [SerializeField] float ringHandSpawnRate;

        // List of hands/fives that still need input checking.
        private List<Hand> ActiveHands;
        private List<HighFive> ActiveFives;
        private List<Hand> DeadHands;
        public int HandCount
        {
            get { return ActiveHands.Count; }
        }

        // Previous timestamp to regulate spawning
        private float previousGameTime = 0.0f;
        public float PreviousGameTime
        {
            get { return previousGameTime; }
            set { previousGameTime = value; }
        }

        // Trig look up tables
        private float[] sinLookUp = new float[360];
        public float SinLookUp(int i)
        {
            i %= 360;

            if (i < 0)
                i = 360 + i;

            return sinLookUp[i];
        }
        private float[] cosLookUp = new float[360];
        public float CosLookUp(int i)
        {
            i %= 360;

            if (i < 0)
                i = 360 + i;
            
            return cosLookUp[i];
        }
        private Vector2[] ovalPositions = new Vector2[360];
        public Vector2 OvalPositions (int i)
        {
            i %= 360;

            if (i < 0)
                i = 360 + i;

            return ovalPositions[i];
        }

        [Header("Scoring")]

        public int SuccessfulFiveScoreReward;
        public int SuccessfulFiveTimeReward;
        public int PerfectFiveScoreReward;
        public int PerfectFiveTimeReward;
        public int ClearBonusTimeReward;

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
                DeadHands = new List<Hand>();

                FillTrigLookupTables();
            }
        }

        /// <summary>
        /// Update the hand movement, resolution, and spawning. Called each frame in game state.
        /// </summary>
        public void UpdateHands() {
            if (StateManager.Instance.State.Equals(GameState.Game)) {

                #region HAND SPAWNING

                //spawns a hand every second
                //if (RunManager.Instance.TimePassed(previousGameTime, DifficultyManager.Instance.handSpawnInterval)) {
                //    for (int i = 0; i < DifficultyManager.Instance.maxHands; i++) {
                //        if (ActiveHands.Count <= DifficultyManager.Instance.maxHands)
                //        {
                //            //spawns handmovement based on difficulty
                //            SpawnHand(DifficultyManager.Instance.GetHandMovement());
                //        }
                //    }

                //    previousGameTime = RunManager.Instance.CurrentGameTimer;
                //} else if (RunManager.Instance.CurrentGameTimer >= 9.7f && previousGameTime == 10f) {
                //    if (ActiveHands.Count <= DifficultyManager.Instance.maxHands) SpawnHand(HandMovement.RANDOM);

                //    previousGameTime = RunManager.Instance.CurrentGameTimer;
                //}

                if (DifficultyManager.Instance.DoSpawn)
                {
                    SpawnHand(DifficultyManager.Instance.GetHandMovement());
                }
                #endregion

                #region HAND MOVEMENT

                foreach (Hand h in ActiveHands)
                {
                    h.DoUpdateStep();
                }

                #endregion

                #region HIGH FIVE RESOLUTION

                GetHandInputs(InputManager.Instance.Touches);

                List<HighFive> resolvedFives = new List<HighFive>();

                foreach (HighFive five in ActiveFives) {
                    if (five.Hand != null && five.Hand.IsActive()) {
                        if (five.MaxDelta == Mathf.Infinity) {
                            // Infinite delta means an automatic normal success (testing on desktop only).
                            five.Hand.StrengthIsAcceptable(five.MaxDelta);
                            five.Hand.OnSuccessfulFive(false);
                        } else if (five.MaxDelta > 0) {
                            // This five is ready to be evaluated.
                            if (five.Hand.StrengthIsAcceptable(five.MaxDelta) && five.Hand.isOpen)
                                five.Hand.OnSuccessfulFive(five.Hand.StrengthIsPerfect(five.MaxDelta));
                            else
                                five.Hand.OnFailedFive(five.MaxDelta <= five.Hand.TargetStrength - five.Hand.AcceptableRange);
                        }

                        resolvedFives.Add(five);
                    }
                }

                // Remove resolved fives from the active fives list.
                foreach (HighFive f in resolvedFives)
                {
                    ActiveFives.Remove(f);
                }

                //remove dead hands from the game
                foreach (Hand h in DeadHands)
                {
                    RemoveHand(h);
                }

                #endregion
            }
        }

        /// <summary>
        /// Spawn a hand object.
        /// </summary>
        /// <param name="movementType">Movement type for hand. Defaults to random selection</param>
        private void SpawnHand(HandMovement movementType = HandMovement.RANDOM) {
            // TODO: Have this method attach the spawned hand to the guy, put it at a reasonable starting position, etc.
            int handObj;

            switch (movementType)
            {
                case HandMovement.HYDRA:
                    handObj = 2;
                    break;
                case HandMovement.FIST:
                    handObj = 3;
                    break;
                default:
                    handObj = Random.Range(0f, 1f) < ringHandSpawnRate ? 1 : 0;
                    break;
            }

            Hand h = Instantiate(HandObjects[handObj], handParent.transform).GetComponentInChildren<Hand>();
            Arm a = h.GetComponentInParent<Arm>();

            bool left;
            a.Shoulder = handParent.GetShoulderTransform(h.transform.position, out left, true);

            float size = Mathf.Clamp(possibleHandSizes[Random.Range(0, possibleHandSizes.Length)], handSizeRange.x, handSizeRange.y);

            h.Initialize(size, acceptableStrengthRange, perfectStrengthRange, left, handObj, movementType);

            ActiveHands.Add(h);
        }

        /// <summary>
        /// creates a hand copying the given hand
        /// </summary>
        /// <param name
        /// ="hand">hand to copy</param>
        public void CopyHand(Hand hand, bool oppositeDir = false)
        {
            // TODO: Have this method attach the spawned hand to the guy, put it at a reasonable starting position, etc.
            Hand h = Instantiate(HandObjects[hand.handObj], handParent.transform).GetComponentInChildren<Hand>();
            Arm a = h.GetComponentInParent<Arm>();

            a.Shoulder = handParent.GetShoulderTransform(hand.left);

            h.Initialize(hand, oppositeDir);

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
            if(hand) Destroy(hand.GetComponentInParent<Arm>().gameObject);
        }

        /// <summary>
        /// Adds a hand to the list of hands to be removed
        /// </summary>
        /// <param name="hand">Hand to be removed</param>
        public void KillHand(Hand hand)
        {
            DeadHands.Add(hand);
        }

        /// <summary>
        /// Cleanup all the active hands (used on game over).
        /// </summary>
        public void CleanupAllHands()
        {
            while (ActiveHands.Count > 0)
                RemoveHand(ActiveHands[0]);

            ActiveFives.Clear();
            ActiveHands.Clear();
            DeadHands.Clear();
        }

        /// <summary>
        /// Evaluate the strength of the inputs from InputManager, if they fall on active hands.
        /// </summary>
        /// <param name="touches">Array of touches for this frame.</param>
        public void GetHandInputs(Touch[] touches) {
            if (InputManager.Instance.MobilePlatform) {
                // Get the relative strength of each new touch.
                foreach (Touch t in touches) {

                    if (t.phase == TouchPhase.Began) {
                        // Check if this touch was on a hand.
                        foreach(Hand h in ActiveHands) {
                            if(h.CheckCollision(Camera.main.ScreenToWorldPoint(t.position))) {
                                StartCoroutine(GetMaxDelta(new HighFive(t, h)));
                                break;
                            }
                        }
                    }
                }
            } else {
                if (Input.GetMouseButtonDown(0)) {
                    foreach (Hand h in ActiveHands) {
                        if (h.CheckCollision(Camera.main.ScreenToWorldPoint(Input.mousePosition))) {
                            StartCoroutine(GetMaxDelta(new HighFive(h)));
                            break;
                        }
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
            // UIManager.Instance.WriteDebugText(five.Hand.TargetStrength.ToString("n3") + " " + five.MaxDelta.ToString("n3"));
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

            Indicator i = Instantiate(indicatorObjects[0], origin.position, Quaternion.identity, handParent.transform).GetComponent<Indicator>();
            i.Initialize(count,Color.white, description, perfect);
        }

        /// <summary>
        /// Spawn a time indicator object.
        /// </summary>
        /// <param name="origin">Transform of the origin hand.</param>
        /// <param name="count">Number of seconds to reflect.</param>
        /// <param name="success">Whether the time is being added or subtracted.</param>
        public void SpawnTimeIndicator(Transform origin, int count, bool isOpen, bool tooWeak, bool success = false) {
            string description = success ? "" : scoreIndicatorFailureDescriptions[Random.Range(0, scoreIndicatorFailureDescriptions.Length)];
            GameObject j = Instantiate(indicatorObjects[1], origin.position, Quaternion.identity, handParent.transform);
            Indicator i = j.GetComponent<Indicator>();
            Color c = Color.white;
            if (!success)
            {
                c = Color.red;
            }
            if (!isOpen)
            {
                description  = "CLOSED!";
            }
            else if (tooWeak)
            {
                description = "Too Weak!";
            }
            else
            {
                description = "Too Strong!";
            }
            i.Initialize(count,c, description, false, success ? 0.15f : 0f);
        }

        /// <summary>
        /// Spawns a clear bonus indicator
        /// </summary>
        /// <param name="origin">transform of the origin</param>
        /// <param name="count">number of seconds to reflect</param>
        public void SpawnClearBonusIndicator(Transform origin, int count)
        {
            GameObject j = Instantiate(indicatorObjects[1], origin.position, Quaternion.identity, handParent.transform);
            Indicator i = j.GetComponent<Indicator>();

            Color c = Color.yellow;
            string description = "CLEAR";

            i.Initialize(count, c, description, false, 0.15f);
        }

        /// <summary>
        /// Spawn a ring indicator object.
        /// </summary>
        /// <param name="origin">Transform of the origin hand.</param>
        public void SpawnRingIndicator(Transform origin) {
            Indicator i = Instantiate(indicatorObjects[2], origin.position, Quaternion.identity, handParent.transform).GetComponent<Indicator>();
            i.Initialize(1,Color.white);
        }

        /// <summary>
        /// Fills trig lookup tables and maps out oval path
        /// </summary>
        private void FillTrigLookupTables ()
        {
            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);
            float horzPadding = 1f;
            float vertPadding = 1.5f;
            float a = horzExtent / 2f - horzPadding;
            float b = vertExtent / 2f - vertPadding;

            for (int i = 0; i < 360; i++)
            {
                float r = i * Mathf.PI / 180;

                cosLookUp[i] = Mathf.Cos(r);
                sinLookUp[i] = Mathf.Sin(r);

                float x = a * cosLookUp[i];
                float y = b * sinLookUp[i];

                ovalPositions[i] = new Vector2(x, y);
            }
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
