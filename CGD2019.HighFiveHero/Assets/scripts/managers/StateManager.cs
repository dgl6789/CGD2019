﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;

namespace App {

    /// <summary>
    /// Basic game states.
    /// </summary>
    public enum GameState { Menu, Game, GameOver, Customization, GooglePlay }

    /// <summary>
    /// Manages the basic app functionality and state machine.
    /// </summary>
    public class StateManager : MonoBehaviour {

        /// Singleton instance.
        public static StateManager Instance;

        // State to start the app in (swap to on startup).
        [SerializeField] GameState startingState;

        /// <summary>
        /// Singleton initialization.
        /// </summary>
        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            _state = startingState;
        }

        /// <summary>
        /// State swapping property.
        /// </summary>
        private GameState _state;
        public GameState State {
            get { return _state; }
            set { if(value != _state) SwapState(value); }
        }

        /// <summary>
        /// Per-state update method.
        /// </summary>
        private void Update() {
            InputManager.Instance.UpdateInputs();

            switch(State) {
                case GameState.Game:
                    if (RunManager.Instance.Running) {
                        RunManager.Instance.UpdateRun();
                        HandManager.Instance.UpdateHands();
                        DifficultyManager.Instance.UpdateDifficulty();
                    }
                    break;
            }
        }

        /// <summary>
        /// Swap the state machine state.
        /// </summary>
        /// <param name="state">State to swap to (int based, for UI).</param>
        public void SwapState(int state) { SwapState((GameState)state); }

        /// <summary>
        /// Swap the game state.
        /// </summary>
        /// <param name="state">State to swap to (enum).</param>
        private void SwapState(GameState state) {
            switch (state) {
                case GameState.Game:
                    StartCoroutine(RunManager.Instance.StartGame());
                    break;
                case GameState.GameOver:
                    HandManager.Instance.CleanupAllHands();
                    break;
                case GameState.Menu:
                    // TODO: Per-state setup.
                    break;
                case GameState.Customization:
                    UIManager.Instance.SetThemeCheckmarkStates();
                    break;
            }

            // Start the state swapping coroutine.
            StartCoroutine(UIManager.Instance.SwapState(_state, state));

            _state = state;
        }
    }
}
