using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    /// <summary>
    /// FistHand is a special hand that alternates between hittable and not
    /// </summary>
    public class FistHand : Hand {
        [SerializeField] Sprite openSprite;
        [SerializeField] Sprite closedSprite;


        /// <summary>
        /// ovveride of Jump method to swap hands if necessary
        /// </summary>
        protected override void Jump()
        {
            //generate a new jump angle when the interval is over
            if (intervalPassed)
            {
                angleStart = angleEnd;
                angleEnd = Random.Range(0, 360);

                //swap between open and closed
                isOpen = !isOpen;

                SpriteRenderer handRenderer = GetComponentInChildren<SpriteRenderer>();

                if (isOpen)
                    handRenderer.sprite = openSprite;
                else
                    handRenderer.sprite = closedSprite;
            }

            if (timePassed / TransitionInterval < 1.0f)
                currentAngle = Mathf.RoundToInt(Mathf.LerpAngle(
                    angleStart,
                    angleEnd,
                    timePassed / TransitionInterval));

            //Move hand to new position
            MoveHand(currentAngle);
        }
    }
}
