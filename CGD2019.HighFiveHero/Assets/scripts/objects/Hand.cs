using System.Collections;
using UnityEngine;
using App.Util;

namespace App {
    public enum HandMovement { RANDOM, GROW, SHRINK, OSCILLATE, JUMP, HYDRA, FIST };

    public class Hand : MonoBehaviour {

        //movement type
        protected HandMovement targetMovement;
        private HandMovement movementType;
        public HandMovement MovementType { get { return movementType; } }
        public bool IsActive() {
            return (
                movementType != HandMovement.RANDOM && 
                movementType != HandMovement.GROW && 
                movementType != HandMovement.SHRINK);
        }

        //in/out variables
        protected float targetSize;
        protected float minSize = 0.1f;

        //time tracking variables
        protected float moveInterval;
        [SerializeField] protected float TransitionInterval;

        protected float timePassed;
        protected bool intervalPassed;

        // Effects
        [SerializeField] float shakeAmount;
        [SerializeField] float shakeDuration;

        //movement variables
        protected float armRadius;
        protected int angleStart;
        protected int angleEnd;
        protected int currentAngle;

        protected float acceptableRange;
        public float AcceptableRange
        {
            get { return acceptableRange; }
        }
        protected float perfectRange;

        protected float targetStrength;
        public float TargetStrength { get { return targetStrength; } }

        [SerializeField] float radius;

        //closed open state
        public bool isOpen = true;

        //initialized variables
        public bool left;
        protected float size;
        public int handObj;

        /// <summary>
        /// Initialize the hand object.
        /// </summary>
        /// <param name="size">Size to scale the hand by.</param>
        /// <param name="acceptableRange">Range (+-) of strength of an input to accept as a success.</param>
        /// <param name="movementType">Movement type. Defaults to oscillating</param>
        public void Initialize(float size, float acceptableRange, float perfectRange, bool left, int handObj, HandMovement movementType = HandMovement.RANDOM) {
            //Rendering Setup
            transform.localScale = new Vector2(size * (left ? -1 : 1), size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);
            targetSize = size;

            //set copying reference variables
            this.size = size;
            this.left = left;
            this.handObj = handObj;

            // Strength parameter setup
            this.acceptableRange = acceptableRange;
            this.perfectRange = perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);
            
            if (movementType == HandMovement.RANDOM)
            {
                //randomly determines movement type
                if (Random.Range(0, 2) == 0)
                {
                    targetMovement = HandMovement.OSCILLATE;
                }
                else
                {
                    targetMovement = HandMovement.JUMP;
                }
            }
            else
            {
                targetMovement = movementType;
            }

            this.movementType = HandMovement.GROW;

            // initialize time variables
            timePassed = 0.0f;

            //initialize movement variables
            switch (targetMovement)
            {
                case HandMovement.HYDRA:
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
                    moveInterval = Random.Range(0.25f, 1.0f) + 1f;
                    
                    currentAngle = angleEnd = angleStart = Random.Range(0, 360);
                    break;
                case HandMovement.FIST:
                    moveInterval = Random.Range(0.25f, 1.0f) + 2f;

                    currentAngle = angleEnd = angleStart = Random.Range(0, 360);
                    break;
            }

            //modify speed based on difficulty
            moveInterval *= DifficultyManager.Instance.currentSpeedMod;

            //determine arm radius
            float vertExtent = Camera.main.orthographicSize * 2f;
            float horzExtent = vertExtent * (Screen.width / (float)Screen.height);

            armRadius = (horzExtent / 2f) - (1f - Random.Range(0, 25) / 100f); //half screen width - padding
        }

        /// <summary>
        /// initilizes hand by cpoying data from a given hand
        /// </summary>
        /// <param name="parentHand">hand to copy from</param>
        /// <param name="oppositeDir">go in the same direction or the opposite one?</param>
        public void Initialize(Hand parentHand, bool oppositeDir)
        {
            //get size information from parent hand
            this.left = parentHand.left;
            this.size = parentHand.size;
            this.handObj = parentHand.handObj;

            // set hand to appropriate position
            this.transform.position = parentHand.transform.position;

            //Rendering Setup
            transform.localScale = new Vector2(size * (left ? -1 : 1), size);
            GetComponentInParent<Arm>().AdjustWidthForHand(size);
            targetSize = size;

            // Strength parameter setup
            this.acceptableRange = parentHand.acceptableRange;
            this.perfectRange = parentHand.perfectRange;

            targetStrength = HandManager.Instance.HandSizetoTargetStrength(size);

            // initialize time variables
            timePassed = moveInterval - parentHand.timePassed;

            //get movement information from parent hand
            this.movementType = parentHand.movementType;

            this.moveInterval = parentHand.moveInterval;
            this.TransitionInterval = parentHand.TransitionInterval;
            
            this.armRadius = parentHand.armRadius;

            if (oppositeDir)
            {
                //move opposite direction
                this.angleStart = parentHand.currentAngle;
                this.angleEnd = parentHand.currentAngle + 120;

                MoveHand(this.angleStart);

                parentHand.angleStart = parentHand.currentAngle;
                parentHand.angleEnd = parentHand.currentAngle - 120;

                timePassed = 0;
                parentHand.timePassed = 0;
            }
            else
            {
                //move in the same direction
                this.angleStart = parentHand.angleStart;
                this.angleEnd = parentHand.angleEnd;

                this.currentAngle = parentHand.currentAngle;

                ////initialize time variables
                timePassed = moveInterval - parentHand.timePassed;
            }
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
                case HandMovement.HYDRA:
                case HandMovement.OSCILLATE:
                    Oscillate();
                    break;
                case HandMovement.FIST:
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

            //reset interval if it passed
            if (intervalPassed)
                timePassed = 0.0f;
        }

        /// <summary>
        /// Jump the hand to a new spot on the arc
        /// </summary>
        protected virtual void Jump()
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
            {
                ChangeMovementType(targetMovement);
            }

            //adjust scale
            GetComponentInParent<Arm>().AdjustWidthForHand(size);
            transform.localScale = new Vector2(Mathf.Sign(transform.localScale.x) * size, size);

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
        protected void MoveHand(int armAngle, float radius = -1.0f)
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
        public void OnFailedFive(bool toWeak) {
            // Spawn a failure indicator
            HandManager.Instance.SpawnTimeIndicator(transform, HandManager.Instance.FailedFiveTimePenalty, isOpen, toWeak);

            // Subtract from the time.
            RunManager.Instance.AddTime(HandManager.Instance.FailedFiveTimePenalty);

            //Pull back the difficulty
            DifficultyManager.Instance.OnMiss();
            // TODO: Spawn a visual effect.

            //split if hydra
            if (movementType == HandMovement.HYDRA)
                HandManager.Instance.CopyHand(this, true);
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
