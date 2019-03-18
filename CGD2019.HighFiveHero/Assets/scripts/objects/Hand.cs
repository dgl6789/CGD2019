using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    enum HandMovement { OSCILATE, JUMP };

    public class Hand : MonoBehaviour {

        private float acceptableRange;
        private float targetStrength;

        /// <summary>
        /// Initialize the hand object.
        /// </summary>
        /// <param name="size">Size to scale the hand by.</param>
        /// <param name="acceptableRange">Range (+-) of strength of an input to accept as a success.</param>
        public void Initialize(float size, float acceptableRange) {
            transform.localScale = new Vector2(size, size);

            this.acceptableRange = acceptableRange;

            // Parse the target strength from the given size.
            targetStrength = size * HandManager.Instance.HandSizetoTargetStrength(size);
        }

        /// <summary>
        /// STUB: To do when a high five on this hand was in the acceptable strength range.
        /// </summary>
        public void OnSuccessfulFive() {
            Debug.Log("Success!");
        }

        /// <summary>
        /// STUB: To do when a high five on this hand was NOT in the acceptable strength range.
        /// </summary>
        public void OnFailedFive() {
            Debug.Log("Failure!");
        }

        /// <summary>
        /// Determine whether a strength value is acceptable to satisfy this hand.
        /// </summary>
        /// <param name="strength">Strength value to check.</param>
        /// <returns>True if the strength value falls within the acceptable range, false otherwise.</returns>
        public bool StrengthIsAcceptable(float strength) { return strength >= targetStrength - acceptableRange && strength <= targetStrength + acceptableRange; }
    }
}
