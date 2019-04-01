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
        public float handsPerWave;
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
            handsPerWave = Mathf.Clamp(handsPerWave * 1.3f, 1, 3.9f);
            handSpawnMult *=.95f;
            handSpawnInterval = Mathf.Clamp(3 * handSpawnMult, .24f, 3);
        }
        public void OnMiss()
        {
            handsPerWave = Mathf.Clamp(handsPerWave * .6f, 1, 3.9f);
        }
        /// <summary>
        /// Reset the difficulty parameters.
        /// </summary>
        public void ResetDifficulty() {
            handsPerWave = 1;
            handSpawnMult = 1;
            handSpawnInterval = 3;
            // TODO: Reset the difficulty parameters.
        }
    }
}