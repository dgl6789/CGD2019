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
        public float currentSpeedMod; //current speed modifier
        public Vector2 speedModBounds; //minimum and maximum of speed modifier
        public float speedModDelta; //percentage by which to adjust speed modifier

        [Header("Hand Spawning")]
        [Range(0f, 3f)]
        public float spawnInterval; //seconds between spawns
        public float currentSpawnInterval; //current spawn interval
        public Vector2 spawnIntervalBounds; //bounds of time between hand spawns
        public float spawnIntervalDelta; //percentage by which to adjust spawn interval
        public int maxHands; //maximum number of hands that can spawn

        private bool doSpawn; //bool to tell the handmanager to spawn a hand
        public bool DoSpawn
        {
            get { return (HandManager.Instance.HandCount < maxHands && doSpawn); }
        }

        [Header("Specialty Hands")]
        [Range(0f, 1f)]
        public float specialtySpawnRate; //chance of a specialty hand being spawned
        public float currentSpecialtySpawnRate; //current specialty spawn rate
        public Vector2 specialtyRateBounds; //bounds of chance of a specialty hand spawning
        public float specialtyRateDelta; //percentage by which to adjust specialty hand spawn chance

        [Header("Clear Bonus")]
        public float scoreReward;
        private float currentScoreReward;
        public float scoreRewardDelta;
        public float timeReward;
        private float currentTimeReward;
        public float timeRewardDelta;
        public Transform clearBonusTransform;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(this);

            //initialize difficulty vars
            ResetDifficulty();
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

                //increase hand values
                currentSpeedMod = Mathf.Clamp(currentSpeedMod * speedModDelta, speedModBounds.x, speedModBounds.y);
                currentSpawnInterval = Mathf.Clamp(currentSpawnInterval * spawnIntervalDelta, spawnIntervalBounds.x, spawnIntervalBounds.y);
                currentSpecialtySpawnRate = Mathf.Clamp(currentSpecialtySpawnRate * specialtyRateDelta, specialtyRateBounds.x, specialtyRateBounds.y);

                //increase clear bonus rewards
                currentScoreReward += scoreRewardDelta;
                currentTimeReward += timeRewardDelta;
            }

            //interval for spawning has passed
            if (spawnTimePassed >= currentSpawnInterval)
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
            //check for clear bonus
            if (HandManager.Instance.HandCount == 1) //cleaner way to check? hand still exists when called
            {                
                // Spawn a success indicator
                HandManager.Instance.SpawnClearBonusIndicator(transform, Mathf.FloorToInt(scoreReward));
                // HandManager.Instance.SpawnTimeIndicator(transform, timeReward, true);

                // Add to the score and time.
                RunManager.Instance.AddScore(Mathf.FloorToInt(scoreReward));
                RunManager.Instance.AddTime(Mathf.FloorToInt(timeReward));
            }
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
            //reset time values
            totalTimePassed = 0f;
            intervalTimePassed = 0f;
            spawnTimePassed = 0f;

            //reset difficulty
            difficultyLevel = 0;

            //reset hand values
            currentSpeedMod = speedMod;
            currentSpawnInterval = spawnInterval;
            currentSpecialtySpawnRate = specialtySpawnRate;

            //reset clear bonus rewards
            currentScoreReward = scoreReward;
            currentTimeReward = timeReward;
        }

        /// <summary>
        /// Gets a handmovement based on difficulty
        /// </summary>
        /// <returns>a hand movement behavior based on difficulty</returns>
        public HandMovement GetHandMovement()
        {
            if (Random.Range(0f, 1f) <= currentSpecialtySpawnRate)
            {
                //if (Random.Range(0, 2) == 0)
                    return HandMovement.HYDRA;
                //else
                //    return HandMovement.FIST;
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