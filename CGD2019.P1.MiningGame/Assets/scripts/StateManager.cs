using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {

    /// <summary>
    /// State of the input functionality. This is mostly used to inform
    /// the input manager as to which inputs to accept during each state.
    /// </summary>
    public enum GameState { MINING /* Add other game/UI states here as needed */ }

    public class StateManager : MonoBehaviour {

        public static StateManager Instance; // Singleton instance (reference this class' members via StateManager.Instance from any context that is 'using App;')

        public GameState State { get; set; } // Current game functionality state (FSM)

        [SerializeField] GameState defaultState; // State to swap to on startup (for testing)

        void Awake() {
            // Singleton intitialization
            if (Instance == null) Instance = this;
            else Destroy(this);

            SwapState(defaultState);
        }

        /// <summary>
        /// Swap to a new game state. Use this in-editor when setting up Unity UI buttons' click events.
        /// </summary>
        /// <param name="state"></param>
        public void SwapState(GameState state) {
            switch(State) { // Do any cleanup necessary to switch away from this case's state.
                default:
                case GameState.MINING:
                    
                    break;
            }

            State = state; // Set the new state

            switch (State) { // Do any setup necessary to switch to this case's state.
                default:
                case GameState.MINING:
                    
                    break;
            }

        }
    }
}