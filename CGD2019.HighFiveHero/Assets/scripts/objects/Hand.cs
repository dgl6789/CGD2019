﻿using System.Collections;
using UnityEngine;

namespace App {
    public enum HandMovement { RANDOM, OSCILLATE, JUMP };

    public class Hand : MonoBehaviour {

        [SerializeField] HandMovement movementType;

        int orbitAngle;
        int moveInterval;
        float timePassed;
        bool intervalPassed;
        int prevSecond;
        float armRadius;
        int oscillateAngleStart;
        int oscillateAngleEnd;

        private float acceptableRange;
        private float perfectRange;

        private float targetStrength;
        public float TargetStrength { get { return targetStrength; } }

        /// <summary>
        /// Initialize the hand object.
        /// </summary>
        /// <param name="size">Size to scale the hand by.</param>
        /// <param name="acceptableRange">Range (+-) of strength of an input to accept as a success.</param>
        /// <param name="movementType">Movement type. Defaults to oscillating</param>
        public void Initialize(float size, float acceptableRange, float perfectRange, HandMovement movementType = HandMovement.RANDOM) {
            //Rendering Setup
            transform.localScale = new Vector2(size, size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);

            // Strength parameter setup
            this.acceptableRange = acceptableRange;
            this.perfectRange = perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);
            
            if (movementType == HandMovement.RANDOM)
            {
                if (Random.Range(0, 4) == 0)
                    this.movementType = HandMovement.JUMP;
                else
                    this.movementType = HandMovement.OSCILLATE;
            }
            else
                this.movementType = movementType;

            // initialize time variables
            prevSecond = 0;
            timePassed = 0.0f;

            //initialize movement variables
            orbitAngle = Random.Range(0, 360);

            switch (this.movementType)
            {
                case HandMovement.OSCILLATE:
                    moveInterval = Random.Range(1, 5);
                    oscillateAngleStart = Random.Range(0, 360);
                    int deltaAngle = 120;
                    if (Random.Range(0, 2) == 0)
                        oscillateAngleEnd = oscillateAngleStart - deltaAngle;
                    else
                        oscillateAngleEnd = oscillateAngleStart + deltaAngle;
                    break;
                case HandMovement.JUMP:
                    moveInterval = 1;
                    break;
            }

            //determine arm radius
            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);

            armRadius = (horzExtent / 2f) - (1f - Random.Range(0, 25) / 100f); //half screen width - padding
        }

        /// <summary>
        /// Move the hand along its individual path, determined by the movement type
        /// </summary>
        public void Move()
        {
            intervalPassed = false;

            //get time
            float interval = Time.deltaTime;
            timePassed += interval;

            //check if the movement interval has passed
            if (timePassed - prevSecond >= moveInterval)
                intervalPassed = true;

            switch (movementType)
            {
                case HandMovement.OSCILLATE:
                    Oscillate();
                    break;
                case HandMovement.JUMP:
                    Jump();
                    break;
                default:
                    Debug.Log(movementType + " isn't an accepted movement type");
                    break;
            }

            //Orbit();

            if (intervalPassed)
                prevSecond = (int)timePassed;
        }

        /// <summary>
        /// Jump the hand to a new spot on the arc
        /// </summary>
        private void Jump()
        {
            //jump after the designated time interval has passed
            if (intervalPassed)
            {
                //generate a new angle
                int newAngle = Random.Range(0, 360);

                //Move hand to new position
                MoveHand(newAngle);
            }
        }

        /// <summary>
        /// Move the hand smoothly along the arc
        /// </summary>
        private void Oscillate()
        {
            int targetAngle = Mathf.RoundToInt(Mathf.LerpAngle(
                oscillateAngleStart, 
                oscillateAngleEnd, 
                (timePassed - prevSecond) / moveInterval));

            //Move hand to new position
            MoveHand(targetAngle);

            //reverse direction after designated time interval has passed
            if (intervalPassed)
            {
                int temp = oscillateAngleEnd;
                oscillateAngleEnd = oscillateAngleStart;
                oscillateAngleStart = temp;
            }
        }

        /// <summary>
        /// Hands move in little circles
        /// </summary>
        private void Orbit()
        {
            Debug.Log("Orbitting");
        }

        /// <summary>
        /// Moves hand to a given position and adjusts elbow position
        /// </summary>
        /// <param name="newPos"> position to move the hand to</param>
        private void MoveHand(int armAngle)
        {
            //find hand position
            Vector3 handPos = new Vector3(HandManager.Instance.CosLookUp(armAngle), HandManager.Instance.SinLookUp(armAngle), 0.0f);

            handPos *= armRadius;

            //find elbow position
            Vector3 elbowPos = new Vector3(handPos.x, 0.6f * handPos.y, 0.0f);

            //move hand to new location
            GetComponentInParent<Arm>().AdjustJointPositions(elbowPos, handPos);
        }

        /// <summary>
        /// To do when a high five on this hand was in the acceptable strength range.
        /// </summary>
        public void OnSuccessfulFive(bool perfect) {
            int scoreReward = perfect ? HandManager.Instance.PerfectFiveScoreReward : HandManager.Instance.SuccessfulFiveScoreReward;
            int timeReward = perfect ? HandManager.Instance.PerfectFiveTimeReward : HandManager.Instance.SuccessfulFiveTimeReward;

            // Spawn a success indicator
            HandManager.Instance.SpawnScoreIndicator(transform, perfect, scoreReward);
            HandManager.Instance.SpawnTimeIndicator(transform, timeReward, true);

            // Add to the score and time.
            RunManager.Instance.AddScore(scoreReward);
            RunManager.Instance.AddTime(timeReward);

            // TODO: Spawn a visual effect.
        }

        /// <summary>
        /// To do when a high five on this hand was NOT in the acceptable strength range.
        /// </summary>
        public void OnFailedFive() {
            // Spawn a failure indicator
            HandManager.Instance.SpawnTimeIndicator(transform, HandManager.Instance.FailedFiveTimePenalty, false);

            // Subtract from the time.
            RunManager.Instance.AddTime(-HandManager.Instance.FailedFiveTimePenalty);

            // TODO: Spawn a visual effect.
        }

        /// <summary>
        /// Determine whether a strength value is acceptable to satisfy this hand.
        /// </summary>
        /// <param name="strength">Strength value to check.</param>
        /// <returns>True if the strength value falls within the acceptable range, false otherwise.</returns>
        public bool StrengthIsAcceptable(float strength) { return strength >= targetStrength - acceptableRange && strength <= targetStrength + acceptableRange; }

        /// <summary>
        /// Determine whether a strength value falls into the "perfect" range for this hand.
        /// </summary>
        /// <param name="strength">Strength value to check.</param>
        /// <returns>True if the strength value falls within the perfect range.</returns>
        public bool StrengthIsPerfect(float strength) { return strength >= targetStrength - perfectRange && strength <= targetStrength + perfectRange; }
    }
}
