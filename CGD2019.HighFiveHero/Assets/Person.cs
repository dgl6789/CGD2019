using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    public class Person : MonoBehaviour {

        [SerializeField] Transform shoulderLeft;
        [SerializeField] Transform shoulderRight;

        public int LeftArmCount;
        public int RightArmCount;

        /// <summary>
        /// Get the transform of the correct shoulder.
        /// </summary>
        /// <param name="startingPos">Starting position of the hand.</param>
        /// <returns>Transform of the shoulder.</returns>
        public Transform GetShoulderTransform(Vector2 startingPos, out bool left, bool random = false) {
            left = startingPos.x < transform.position.x;

            if(random) {
                if (LeftArmCount - RightArmCount != 0) {
                    left = Mathf.Min(LeftArmCount, RightArmCount) == LeftArmCount ? true : false;
                } else left = Random.Range(0, 2) == 0;
            }

            if (left) LeftArmCount++;
            else RightArmCount++;

            return left ? shoulderLeft : shoulderRight;
        }

        /// <summary>
        /// get the transform of a specific shoulder
        /// </summary>
        /// <param name="left">bool to specify shoulder</param>
        /// <returns></returns>
        public Transform GetShoulderTransform(bool left)
        {
            if (left)
                return shoulderLeft;
            else
                return shoulderRight;
        }

        public void RemoveArm(Transform shoulder) {
            if (shoulder.Equals(shoulderLeft)) LeftArmCount--;
            else RightArmCount--;
        }

        /// <summary>
        /// Change the luchador's outfit.
        /// </summary>
        /// <param name="mask">Mask of the outfit.</param>
        /// <param name="body">Body of the outfit.</param>
        public void ChangeOutfit(Sprite mask, Sprite body) {
            // TODO: Change the outfit.
        }
    }
}
