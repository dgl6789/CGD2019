using System.Collections;
using UnityEngine;

namespace App {
    public enum HandMovement { OSCILLATE, JUMP };

    public class Hand : MonoBehaviour {

        private HandMovement movementType;

        int orbitAngle;
        float moveInterval;
        float timePassed;
        int prevSecond;
        float armRadius;
        bool moveUp;
        int oscillateAngle;

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
        public void Initialize(float size, float acceptableRange, float perfectRange, HandMovement movementType = HandMovement.OSCILLATE) {
            //Rendering Setup
            transform.localScale = new Vector2(size, size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);

            // Strength parameter setup
            this.acceptableRange = acceptableRange;
            this.perfectRange = perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);

            this.movementType = movementType;

            // initialize movement variables
            orbitAngle = Random.Range(0, 359);
            //moveInterval = Random.Range(1.5f, 3f);
            moveInterval = 1.0f;
            prevSecond = 0;
            timePassed = 0.0f;
            moveUp = (Random.Range(0, 2) == 0);
            oscillateAngle = Random.Range(0, 359);

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
            //get time
            float interval = Time.deltaTime;
            timePassed += interval;

            Oscillate();

            //switch (movementType)
            //{
            //    case HandMovement.OSCILLATE:
            //        Oscillate();
            //        break;
            //    case HandMovement.JUMP:
            //        Jump();
            //        break;
            //}

            //Orbit();
        }

        /// <summary>
        /// Jump the hand to a new spot on the arc
        /// </summary>
        private void Jump()
        {
            //jump after the designated time interval has passed
            if (timePassed - prevSecond >= moveInterval)
            {
                prevSecond = (int)timePassed;

                //generate a new angle
                int newAngle = Random.Range(0, 360);

                //Move hand to new position
                MoveHand(new Vector3(HandManager.Instance.CosLookUp(newAngle), HandManager.Instance.SinLookUp(newAngle), 0.0f));
            }
        }

        /// <summary>
        /// Move the hand smoothly along the arc
        /// </summary>
        private void Oscillate()
        {
            if (moveUp)
                oscillateAngle++;
            else
                oscillateAngle--;

            Vector3 handPos = new Vector3(HandManager.Instance.CosLookUp(oscillateAngle), HandManager.Instance.SinLookUp(oscillateAngle), 0.0f);

            Debug.Log(oscillateAngle + " hand at " + handPos);

            //Move hand to new position
            MoveHand(handPos);

            //reverse direction after designated time interval has passed
            if (timePassed - prevSecond >= moveInterval)
            {
                prevSecond = (int)timePassed;

                moveUp = !moveUp;
            }
        }

        /// <summary>
        /// Hands move in little circles
        /// </summary>
        private void Orbit()
        {
            Debug.Log("Orbitting");
        }

        private void MoveHand(Vector3 newPos)
        {
            newPos *= armRadius;

            Vector3 elbowPos = new Vector3(newPos.x, 0.6f * newPos.y, 0.0f);

            //move hand to new location
            GetComponentInParent<Arm>().AdjustJointPositions(elbowPos, newPos);
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
