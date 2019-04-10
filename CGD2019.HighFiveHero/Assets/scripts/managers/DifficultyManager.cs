using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {
    /// <summary>
    /// Handles the difficulty scaling functionality in the game mode.
    /// </summary>
    public class DifficultyManager : MonoBehaviour {

        /// Singleton instance.
        public static DifficultyManager Instance;
        public float handSpawnInterval;
        float handSpawnMult;
        public float maxHands;

        [Range(0f, 1f)]
        public float specialtyHandRate;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            ResetDifficulty();//initialize difficulty vars
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        public void OnHighFive()
        {
            maxHands = Mathf.Clamp(maxHands * 1.2f, 1, 3.9f / (((handSpawnInterval / 3f) + 2f) / 3f));
            handSpawnMult *=.95f;
            handSpawnInterval = Mathf.Clamp(3 * handSpawnMult, .24f, 3);
        }

        public void OnMiss()
        {
            maxHands = Mathf.Clamp(maxHands * .6f, 1, 3.9f);
        }

        /// <summary>
        /// Reset the difficulty parameters.
        /// </summary>
        public void ResetDifficulty() {
            maxHands = 1;
            handSpawnMult = 1;
            handSpawnInterval = 3;
            // TODO: Reset the difficulty parameters.
        }

        /// <summary>
        /// Gets a handmovement based on difficulty
        /// </summary>
        /// <returns>a hand movement behavior based on difficulty</returns>
        public HandMovement GetHandMovement ()
        {
            //hydra hands get more likely as difficulty increases, capped at specialty hand rate
            if (Random.Range(0f, 1f) > handSpawnMult && Random.Range(0f, 1f) <= specialtyHandRate)
                return HandMovement.HYDRA;

            //oscillation gets less common as things get more difficult, jumping gets more common
            if (Random.Range(0f, handSpawnMult) > 0.5f)
                return HandMovement.OSCILLATE;
            else
                return HandMovement.JUMP;
        }
    }
}