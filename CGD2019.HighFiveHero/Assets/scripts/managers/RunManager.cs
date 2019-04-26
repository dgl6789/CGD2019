using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;

namespace App {
    /// <summary>
    /// Manages the game's scoring and currency, as well as ingame flow behaviors.
    /// </summary>
    public class RunManager : MonoBehaviour {

        /// Singleton instance.
        public static RunManager Instance;

        [SerializeField] int startCountdownLength;
        [SerializeField] float initialTimerValue;

        bool gameStarted;
        public bool Running {
            get { return gameStarted; }
        }

        // List of active ringg objects
        [HideInInspector] public List<Ring> ActiveRings;

        // High score list (saved and loaded from file).
        [HideInInspector] public int HighScore;

        private int currentScore;
        public int CurrentScore {
            get { return currentScore; }
            private set {
                currentScore = value;

                UIManager.Instance.UpdateGameScoreText(currentScore);
            }
        }

        private float currentGameTimer;
        public float CurrentGameTimer
        {
            get { return currentGameTimer; }
        }

        // Currency value.
        private int currency;
        public int Currency {
            get { return currency; }
            set {
                currency = value;

                SaveManager.Instance.SaveCurrency(currency);
                UIManager.Instance.SetCurrencyValue(currency);
                
            }
        }

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            if (this) ActiveRings = new List<Ring>();
        }

        /// <summary>
        /// Increment the currency by 1.
        /// </summary>
        public void AddCurrency() {
            Currency++;
        }

        /// <summary>
        /// Remove a number of currency.
        /// </summary>
        /// <param name="amt">Amount to remove.</param>
        public void RemoveCurrency(int amt) {
            Currency -= Mathf.Max(0, amt);
        }

        /// <summary>
        /// Add to the game score.
        /// </summary>
        /// <param name="amt">Amount to add.</param>
        public void AddScore(int amt) {
            CurrentScore += Mathf.Max(0, amt);
        }

        /// <summary>
        /// Add to the game's timer.
        /// </summary>
        /// <param name="amount">Amount to add.</param>
        public void AddTime(float amount) {
            currentGameTimer += amount;
            HandManager.Instance.PreviousGameTime += amount;
        }

        /// <summary>
        /// Update the state of the run, difficulty, timer, etc.
        /// </summary>
        public void UpdateRun() {
            if (gameStarted) {
                if (!HandManager.Instance.smothered)
                {
                    currentGameTimer -= Time.deltaTime;

                    UIManager.Instance.UpdateGameTimerText(currentGameTimer);

                    if (currentGameTimer <= 0)
                    {
                        currentGameTimer = 0;

                        HandManager.Instance.StartSmothering();
                    }
                }
                else
                {
                    EndGame();
                }
            }
        }

        /// <summary>
        /// Starts the game with a countdown timer.
        /// </summary>
        public IEnumerator StartGame() {
            // Reset the score/timer
            CurrentScore = 0;
            currentGameTimer = initialTimerValue;
            UIManager.Instance.UpdateGameTimerText(currentGameTimer);
            HandManager.Instance.PreviousGameTime = currentGameTimer;
            DifficultyManager.Instance.ResetDifficulty();

            // Turn on the countdown, and count it down.
            UIManager.Instance.SetStartGameCountdownTextActive(true);

            float timer = startCountdownLength;

            while (timer > 0) {
                timer -= Time.deltaTime;

                UIManager.Instance.SetStartGameCountdownText(Mathf.RoundToInt(timer));

                yield return null;
            }
            
            UIManager.Instance.SetStartGameCountdownTextActive(false);

            gameStarted = true;
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        private void EndGame() {
            gameStarted = false;

            StateManager.Instance.State = GameState.GameOver;
            HandManager.Instance.CleanupAllHands();
            UIManager.Instance.SetGameOverTexts(CurrentScore, CurrentScore > HighScore, HighScore);

            TrySetHighScore(CurrentScore);
        }

        /// <summary>
        /// Add a score to the high scores list (if it is good enough).
        /// </summary>
        /// <param name="value">Value to test and add if necessary.</param>
        public void TrySetHighScore(int value) {
            if (HighScore < value) {
                HighScore = value;

                SaveManager.Instance.SetHighScore();
                UIManager.Instance.SetBestScoreValue(HighScore);
            }
        }

        /// <summary>
        /// Checks if a certain amount of time has passed
        /// </summary>
        /// <param name="time">Time to compare to CurrentTime</param>
        /// <param name="numSeconds">Number of seconds to check for. Defaults to 1 second</param>
        /// <returns></returns>
        public bool TimePassed(float time, int numSeconds = 1)
        {
            return (time - currentGameTimer >= numSeconds);
        }
        public bool TimePassed(float time, float numSeconds = 1)
        {
            return (time - currentGameTimer >= numSeconds);
        }
    }
}
