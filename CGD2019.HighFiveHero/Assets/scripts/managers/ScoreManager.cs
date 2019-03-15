using System.Collections.Generic;
using UnityEngine;
using App.UI;

namespace App {
    /// <summary>
    /// Manages the game's scoring and currency, as well as ingame flow behaviors.
    /// </summary>
    public class ScoreManager : MonoBehaviour {

        /// Singleton instance.
        public static ScoreManager Instance;

        [SerializeField] float initialTimerValue;
        bool gameStarted;

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

        private float CurrentGameTimer;

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
        /// Update the state of the run, difficulty, timer, etc.
        /// </summary>
        public void UpdateRun() {
            if (gameStarted) {
                CurrentGameTimer -= Time.deltaTime;

                UIManager.Instance.UpdateGameTimerText(CurrentGameTimer);

                if (CurrentGameTimer <= 0) {
                    CurrentGameTimer = 0;

                    EndGame();
                }
            }
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame() {
            gameStarted = true;
            CurrentGameTimer = initialTimerValue;

            CurrentScore = 0;
        }

        /// <summary>
        /// Ends the game.
        /// </summary>
        private void EndGame() {
            gameStarted = false;
            
            TrySetHighScore(CurrentScore);

            StateManager.Instance.SwapState(GameState.GameOver);
            UIManager.Instance.SetGameOverTexts(CurrentScore, CurrentScore > HighScore, HighScore);
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
    }
}
