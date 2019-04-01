using System.Collections;
using UnityEngine;
using App.Util;

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
        [SerializeField] private float TransitionInterval;

        private float timePassed;
        private bool intervalPassed;

        // Effects
        [SerializeField] float shakeAmount;
        [SerializeField] float shakeDuration;

        //movement variables
        private float armRadius;
        private int orbitAngle;
        private int angleStart;
        private int angleEnd;
        private int currentAngle;

        private float acceptableRange;
        private float perfectRange;

        private float targetStrength;
        public float TargetStrength { get { return targetStrength; } }

        [SerializeField] float radius;

        protected float size;

        /// <summary>
        /// Initialize the hand object.
        /// </summary>
        /// <param name="size">Size to scale the hand by.</param>
        /// <param name="acceptableRange">Range (+-) of strength of an input to accept as a success.</param>
        /// <param name="movementType">Movement type. Defaults to oscillating</param>
        public void Initialize(float size, float acceptableRange, float perfectRange, bool left, HandMovement movementType = HandMovement.RANDOM) {
            //Rendering Setup
            transform.localScale = new Vector2(size * (left ? -1 : 1), size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);
            targetSize = size;
            this.size = size;

            // Strength parameter setup
            this.acceptableRange = acceptableRange;
            this.perfectRange = perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);
            
            if (movementType == HandMovement.RANDOM)
            {
                //fifty percent chance to become oscillating or jumping
                if (Random.Range(0, 2) == 0)
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

            //initialize movement variables
            orbitAngle = Random.Range(0, 360);
            switch (targetMovement)
            {
                case HandMovement.OSCILLATE:
                    moveInterval = Random.Range(1.0f, 5.0f);
                    angleStart = Random.Range(0, 360);

                    int deltaAngle = 120;

                    if (Random.Range(0, 2) == 0)
                        angleEnd = angleStart - deltaAngle;
                    else
                        angleEnd = angleStart + deltaAngle;

                    currentAngle = angleStart;
                    break;
                case HandMovement.JUMP:
                    moveInterval = Random.Range(0.0f, 1.0f) + 0.5f;
                    
                    currentAngle = angleEnd = angleStart = Random.Range(0, 360);
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
                thisInterval = TransitionInterval;

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
                    // Debug.Log(movementType + " isn't an accepted movement type");
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
            //generate a new jump angle when the interval is over
            if (intervalPassed)
            {
                angleStart = angleEnd;
                angleEnd = Random.Range(0, 360);
            }

            if (timePassed / TransitionInterval < 1.0f)
                currentAngle = Mathf.RoundToInt(Mathf.LerpAngle(
                    angleStart,
                    angleEnd,
                    timePassed / TransitionInterval));

            //Move hand to new position
            MoveHand(currentAngle);
        }

        /// <summary>
        /// Move the hand smoothly along the arc
        /// </summary>
        private void Oscillate()
        {
            currentAngle = Mathf.RoundToInt(Mathf.LerpAngle(
                angleStart, 
                angleEnd, 
                timePassed / moveInterval));

            //Move hand to new position
            MoveHand(currentAngle);

            //reverse direction after designated time interval has passed
            if (intervalPassed)
            {
                int temp = angleEnd;
                angleEnd = angleStart;
                angleStart = temp;
            }
        }

        /// <summary>
        /// Hands move in little circles
        /// </summary>
        private void Orbit()
        {
            // Debug.Log("Orbiting");
        }

        /// <summary>
        /// grow hand into existence
        /// </summary>
        private void Grow()
        {
            float size = 0.1f;
            float radius = armRadius;
            float t = timePassed / TransitionInterval;

            //lerp scale and position
            size = Mathf.Lerp(minSize, targetSize, t);
            radius = Mathf.Lerp(minSize, armRadius, t);

            //start moving if growth is complete
            if (intervalPassed)
                ChangeMovementType(targetMovement);

            //adjust scale
            transform.localScale = new Vector2(Mathf.Sign(transform.localScale.x) * size, size);
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
            float t = timePassed / TransitionInterval;

            //lerp scale and position
            size = Mathf.Lerp(targetSize, minSize, t);
            radius = Mathf.Lerp(armRadius, minSize, t);

            //mark hand for removal if completed
            if (intervalPassed)
                HandManager.Instance.KillHand(this);

            //adjust scale
            transform.localScale = new Vector2(Mathf.Sign(transform.localScale.x) * size, size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);

            //move hand to new position
            MoveHand(currentAngle, radius);
        }

        /// <summary>
        /// Moves hand to a given position and adjusts elbow position
        /// </summary>
        /// <param name="armAngle">angle to move the hand to</param>
        /// <param name="radius">distance from the shoulder to position the hand</param>
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
            // HandManager.Instance.SpawnTimeIndicator(transform, timeReward, true);

            // Add to the score and time.
            RunManager.Instance.AddScore(scoreReward);
            RunManager.Instance.AddTime(timeReward);

            // Play a sound and shake the screen
            SoundManager.Instance.PlayHighFiveSound(HandManager.Instance.HandSizetoTargetStrength(size));
            CameraEffects.Instance.ShakeFromHand(HandManager.Instance.HandSizetoTargetStrength(size));

            // Ramp up the difficulty
            DifficultyManager.Instance.OnHighFive();

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
            RunManager.Instance.AddTime(HandManager.Instance.FailedFiveTimePenalty);

            //Pull back the difficulty
            DifficultyManager.Instance.OnMiss();
            // TODO: Spawn a visual effect.
        }

        /// <summary>
        /// Check collision on this hand with a touch or mouse position.
        /// </summary>
        /// <param name="position">Touch/mouse position to check against this hand.</param>
        /// <returns>Whether the touch or mouse position collides with this hand.</returns>
        public bool CheckCollision(Vector2 position) {
            return Vector2.Distance(position, transform.position) <= radius * size;
        }

        /// <summary>
        /// Determine whether a strength value is acceptable to satisfy this hand.
        /// </summary>
        /// <param name="strength">Strength value to check.</param>
        /// <returns>True if the strength value falls within the acceptable range, false otherwise.</returns>
        public virtual bool StrengthIsAcceptable(float strength) { return strength >= targetStrength - acceptableRange && strength <= targetStrength + acceptableRange; }

        /// <summary>
        /// Determine whether a strength value falls into the "perfect" range for this hand.
        /// </summary>
        /// <param name="strength">Strength value to check.</param>
        /// <returns>True if the strength value falls within the perfect range.</returns>
        public bool StrengthIsPerfect(float strength) { return strength >= targetStrength - perfectRange && strength <= targetStrength + perfectRange; }
    }
}
