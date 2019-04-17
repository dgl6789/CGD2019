using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App
{
    /// <summary>
    /// Handles the difficulty scaling functionality in the game mode.
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {

        /// Singleton instance.
        public static DifficultyManager Instance;
        //public float handSpawnInterval;
        //float handSpawnMult;
        //public float maxHands;
        //[Range(0f, 1f)]
        //public float specialtyHandRate;

        private float totalTimePassed;
        private float intervalTimePassed;
        private float spawnTimePassed;

        [Header("Overall Difficulty")]
        [SerializeField] int difficultyLevel;
        [Range(0f, 5f)]
        public float diffInterval; //seconds between difficulty increase
        public Vector2 diffIntervalBounds; //bounds of time between difficulty increases

        [Header("Hand Speed")]
        [Range(0f, 2f)]
        public float speedMod; //percentage adjustment to hand speed
        private float speedModReset; //speed mod value to reset to
        public Vector2 speedModBounds; //minimum and maximum of speed modifier
        public float speedModDelta; //percentage by which to adjust speed modifier

        [Header("Hand Spawning")]
        [Range(0f, 3f)]
        public float spawnInterval; //seconds between spawns
        private float spawnIntervalReset; //spawn interval value to reset to
        public Vector2 spawnIntervalBounds; //bounds of time between hand spawns
        public float spawnIntervalDelta; //percentage by which to adjust spawn interval

        private bool doSpawn; //bool to tell the handmanager to spawn a hand
        public bool DoSpawn
        {
            get { return doSpawn; }
        }

        [Header("Specialty Hands")]
        [Range(0f, 1f)]
        public float specialtySpawnRate; //chance of a specialty hand being spawned
        private float specialtyRateReset; //specialty spawn rate value to reset to
        public Vector2 specialtyRateBounds; //bounds of chance of a specialty hand spawning
        public float specialtyRateDelta; //percentage by which to adjust specialty hand spawn chance

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            //initialize difficulty vars
            ResetDifficulty();

            //grab reset values
            GetResetValues();
        }

        /// <summary>
        /// Updates difficulty based on time passing
        /// </summary>
        public void UpdateDifficulty ()
        {
            //reset spawning bool
            doSpawn = false;

            float t = Time.deltaTime;

            totalTimePassed += t;
            intervalTimePassed += t;
            spawnTimePassed += t;

            //interval has passed, ramp up difficulty
            if (intervalTimePassed >= diffInterval)
            {
                intervalTimePassed = 0f;

                difficultyLevel++;

                speedMod = Mathf.Clamp(speedMod * speedModDelta, speedModBounds.x, speedModBounds.y);
                spawnInterval = Mathf.Clamp(spawnInterval * spawnIntervalDelta, spawnIntervalBounds.x, spawnIntervalBounds.y);
                specialtySpawnRate = Mathf.Clamp(specialtySpawnRate * specialtyRateDelta, specialtyRateBounds.x, specialtyRateBounds.y);

                Debug.Log("Increasing difficulty to " + difficultyLevel);
                Debug.Log("  SpeedMod is now " + speedMod);
                Debug.Log("  Spawn interval is now " + spawnInterval + " seconds");
                Debug.Log("  Specialty Spawn Rate is now " + specialtySpawnRate + "%");
            }

            //interval for spawning has passed
            if (spawnTimePassed >= spawnInterval)
            {
                spawnTimePassed = 0f;

                doSpawn = true;
            }

            //spawn hands initially
            if (totalTimePassed - t == 0)
                doSpawn = true;
        }

        public void OnHighFive()
        {
            //maxHands = Mathf.Clamp(maxHands * 1.2f, 1, 3.9f / (((handSpawnInterval / 3f) + 2f) / 3f));
            //handSpawnMult *=.95f;
            //handSpawnInterval = Mathf.Clamp(3 * handSpawnMult, .24f, 3);
        }

        public void OnMiss()
        {
            //maxHands = Mathf.Clamp(maxHands * .6f, 1, 3.9f);
        }

        /// <summary>
        /// Reset the difficulty parameters.
        /// </summary>
        public void ResetDifficulty()
        {
            //maxHands = 1;
            //handSpawnMult = 1;
            //handSpawnInterval = 3;

            totalTimePassed = 0f;
            intervalTimePassed = 0f;
            spawnTimePassed = 0f;

            difficultyLevel = 0;

            speedMod = speedModReset;
            spawnInterval = spawnIntervalReset;
            specialtySpawnRate = specialtyRateReset;
            //diffInterfal = 2f;

            //speedMod = 1.5f;

            //spawnRate = 2f;
            //specialtySpawnRate = 0f;
        }

        /// <summary>
        /// sets initial reset values
        /// </summary>
        private void GetResetValues()
        {
            speedModReset = speedMod;
            spawnIntervalReset = spawnInterval;
            specialtyRateReset = specialtySpawnRate;
        }

        /// <summary>
        /// Gets a handmovement based on difficulty
        /// </summary>
        /// <returns>a hand movement behavior based on difficulty</returns>
        public HandMovement GetHandMovement()
        {
            ////hydra hands get more likely as difficulty increases, capped at specialty hand rate
            //if (Random.Range(0f, 1f) > handSpawnMult && Random.Range(0f, 1f) <= specialtyHandRate)
            //{
            //    if (Random.Range(0, 2) == 0)
            //        return HandMovement.HYDRA;
            //    else
            //        return HandMovement.FIST;
            //}

            ////oscillation gets less common as things get more difficult, jumping gets more common
            //if (Random.Range(0f, Mathf.Clamp(handSpawnMult, 0.65f, 1f)) > 0.5f)
            //    return HandMovement.OSCILLATE;
            //else
            //    return HandMovement.JUMP;

            if (Random.Range(0f, 1f) <= specialtySpawnRate)
            {
                if (Random.Range(0, 2) == 0)
                    return HandMovement.HYDRA;
                else
                    return HandMovement.FIST;
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                    return HandMovement.JUMP;
                else
                    return HandMovement.OSCILLATE;
            }

            //return HandMovement.RANDOM;
        }
    }
}