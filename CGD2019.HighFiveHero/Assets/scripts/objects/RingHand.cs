using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    /// <summary>
    /// RingHand is a special hand that spawns a ring object that can be collected 
    /// if it is high-fived very hard.
    /// </summary>
    public class RingHand : Hand {
        [SerializeField] GameObject RingObject;

        [SerializeField] float minStrengthToSpawnRing;

        [SerializeField] float upwardBias;

        /// <summary>
        /// Override method checks if the five was hard enough to spawn a ring from the hand.
        /// </summary>
        /// <param name="strength">Strength of the tap.</param>
        /// <returns>True if a ring was spawned or the strength was in the acceptable range, false otherwise.</returns>
        public override bool StrengthIsAcceptable(float strength) {
            if (strength >= minStrengthToSpawnRing) {
                Ring r = Instantiate(RingObject, transform.position, Quaternion.identity, HandManager.Instance.handParent.transform).GetComponent<Ring>();

                r.Initialize(new Vector2(
                    Random.Range(-0.5f, 0.5f), 
                    Random.Range(0f, upwardBias)),
                    size);

                return true;
            }

            return base.StrengthIsAcceptable(strength);
        }
    }
}
