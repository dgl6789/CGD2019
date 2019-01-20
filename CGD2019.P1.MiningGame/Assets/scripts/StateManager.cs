﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {

    /// <summary>
    /// State of the input functionality. This is mostly used to inform
    /// the input manager as to which inputs to accept during each state.
    /// </summary>
    public enum GameState { MINING /* Add other game/UI states here as needed */ }

    public class StateManager : MonoBehaviour {

        // Singleton instance (reference this class' members via StateManager.Instance from any context that is 'using App;')
        public static StateManager Instance;

        // Current game functionality state (FSM)
        public GameState State { get; set; }

        // State to swap to on startup (for testing)
        [SerializeField] GameState defaultState;

        void Awake() {
            // Singleton intitialization.
            if (Instance == null) Instance = this;
            else Destroy(this);

            // Initial state setup.
            SwapState(defaultState);
        }

        /// <summary>
        /// Swap to a new game state. Use this in-editor when setting up Unity UI buttons' click events.
        /// </summary>
        /// <param name="state"></param>
        public void SwapState(GameState state) {
            // Do any cleanup necessary to switch away from the old state.
            switch (State) { 
                default:
                case GameState.MINING:
                    /// TODO: Setup for mining state.
                    break;
            }

            State = state; // Set the new state

            // Do any setup necessary to switch to the new state.
            switch (State) { 
                default:
                case GameState.MINING:
                    /// TODO: Cleanup for mining state.
                    break;
            }
        }
    }
}