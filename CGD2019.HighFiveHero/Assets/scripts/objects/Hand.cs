using System.Collections;
using UnityEngine;

namespace App {
    public enum HandMovement { RANDOM, GROW, SHRINK, OSCILLATE, JUMP };

    public class Hand : MonoBehaviour {

        //movement type
        private HandMovement targetMovement;
        private HandMovement movementType;
        public HandMovement MovementType { get { return movementType; } }
        public bool IsActive() { return (movementType == HandMovement.OSCILLATE || movementType == HandMovement.JUMP); }

        //in/out variables
        private float targetSize;
        private float minSize = 0.1f;

        //time tracking variables
        private float moveInterval;
        private float transitionInterval;
        private float timePassed;
        private bool intervalPassed;

        //movement variables
        private float armRadius;
        private int orbitAngle;
        private int oscillateAngleStart;
        private int oscillateAngleEnd;
        private int currentAngle;

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
            targetSize = size;

            // Strength parameter setup
            this.acceptableRange = acceptableRange;
            this.perfectRange = perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);
            
            if (movementType == HandMovement.RANDOM)
            {
                if (Random.Range(0, 4) == 0)
                    targetMovement = HandMovement.JUMP;
                else
                    targetMovement = HandMovement.OSCILLATE;
            }
            else
            {
                targetMovement = movementType;
            }

            this.movementType = HandMovement.GROW;

            // initialize time variables
            timePassed = 0.0f;
            transitionInterval = 0.5f;

            //initialize movement variables
            orbitAngle = Random.Range(0, 360);
            switch (targetMovement)
            {
                case HandMovement.OSCILLATE:
                    moveInterval = Random.Range(1.0f, 5.0f);
                    oscillateAngleStart = Random.Range(0, 360);

                    int deltaAngle = 120;

                    if (Random.Range(0, 2) == 0)
                        oscillateAngleEnd = oscillateAngleStart - deltaAngle;
                    else
                        oscillateAngleEnd = oscillateAngleStart + deltaAngle;

                    currentAngle = oscillateAngleStart;

                    break;
                case HandMovement.JUMP:
                    moveInterval = 1.0f;

                    currentAngle = Random.Range(0, 360);
                    break;
            }

            //determine arm radius
            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);

            armRadius = (horzExtent / 2f) - (1f - Random.Range(0, 25) / 100f); //half screen width - padding
        }

        /// <summary>
        /// Updates the hand according to its internal behavior
        /// </summary>
        public void DoUpdateStep()
        {
            intervalPassed = false;

            //get time
            timePassed += Time.deltaTime;

            //get correct interval
            float thisInterval = moveInterval;
            if (movementType == HandMovement.GROW || movementType == HandMovement.SHRINK)
                thisInterval = transitionInterval;

            //check if the movement interval has passed
            if (timePassed >= thisInterval)
                intervalPassed = true;

            switch (movementType)
            {
                case HandMovement.OSCILLATE:
                    Oscillate();
                    break;
                case HandMovement.JUMP:
                    Jump();
                    break;
                case HandMovement.GROW:
                    Grow();
                    break;
                case HandMovement.SHRINK:
                    Shrink();
                    break;
                default:
                    Debug.Log(movementType + " isn't an accepted movement type");
                    break;
            }

            //Orbit();

            //reset interval if it passed
            if (intervalPassed)
                timePassed = 0.0f;
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
                currentAngle = Random.Range(0, 360);

                //Move hand to new position
                MoveHand(currentAngle);
            }
        }

        /// <summary>
        /// Move the hand smoothly along the arc
        /// </summary>
        private void Oscillate()
        {
            int currentAngle = Mathf.RoundToInt(Mathf.LerpAngle(
                oscillateAngleStart, 
                oscillateAngleEnd, 
                timePassed / moveInterval));

            //Move hand to new position
            MoveHand(currentAngle);

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
            Debug.Log("Orbiting");
        }

        /// <summary>
        /// grow hand into existence
        /// </summary>
        private void Grow()
        {
            float size = 0.1f;
            float radius = armRadius;
            float t = timePassed / transitionInterval;

            //lerp scale and position
            size = Mathf.Lerp(minSize, targetSize, t);
            radius = Mathf.Lerp(minSize, armRadius, t);

            //start moving if growth is complete
            if (intervalPassed)
                ChangeMovementType(targetMovement);

            //adjust scale
            transform.localScale = new Vector2(size, size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);

            //move hand to new position
            MoveHand(currentAngle, radius);
        }

        /// <summary>
        /// shrink hand out of existence
        /// </summary>
        private void Shrink ()
        {
            float size = 0.1f;
            float radius = armRadius;
            float t = timePassed / transitionInterval;

            //lerp scale and position
            size = Mathf.Lerp(targetSize, minSize, t);
            radius = Mathf.Lerp(armRadius, minSize, t);

            //mark hand for removal if completed
            if (intervalPassed)
                HandManager.Instance.KillHand(this);

            //adjust scale
            transform.localScale = new Vector2(size, size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);

            //move hand to new position
            MoveHand(currentAngle, radius);
        }

        /// <summary>
        /// Moves hand to a given position and adjusts elbow position
        /// </summary>
        /// <param name="newPos"> position to move the hand to</param>
        private void MoveHand(int armAngle, float radius = -1.0f)
        {
            //find hand position
            Vector3 handPos = new Vector3(HandManager.Instance.CosLookUp(armAngle), HandManager.Instance.SinLookUp(armAngle), 0.0f);

            if (radius == -1.0f)
                handPos *= armRadius;
            else
                handPos *= radius;

            //find elbow position
            Vector3 elbowPos = new Vector3(handPos.x, 0.6f * handPos.y, 0.0f);

            //move hand to new location
            GetComponentInParent<Arm>().AdjustJointPositions(elbowPos, handPos);
        }

        /// <summary>
        /// changes movement type and resets timer
        /// </summary>
        /// <param name="newMovement"></param>
        private void ChangeMovementType (HandMovement newMovement)
        {
            timePassed = 0.0f;

            movementType = newMovement;
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

            //start shrinking
            ChangeMovementType(HandMovement.SHRINK);
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
