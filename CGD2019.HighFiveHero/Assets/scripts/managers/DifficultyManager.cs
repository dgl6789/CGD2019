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

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);
        }

        /// <summary>
        /// Reset the difficulty parameters.
        /// </summary>
        public void ResetDifficulty() {
            // TODO: Reset the difficulty parameters.
        }
    }
}