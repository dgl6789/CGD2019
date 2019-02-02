using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using App.UI;
using App.Gameplay;

namespace App {

    /// <summary>
    /// State of the input functionality. This is mostly used to inform
    /// the input manager as to which inputs to accept during each state.
    /// </summary>
    public enum GameState { MENU, PLAYSERVICES, MINING /* Add other game/UI states here as needed */ }

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
        }

        private void Start()
        {
            // Initial state setup.
            SwapState(defaultState);
        }

        /// <summary>
        /// Swap to a new game state. Use this in-editor when setting up Unity's UI buttons' click events.
        /// </summary>
        /// <param name="state">Integer-based game state to swap to.</param>
        public void SwapState(int state) { SwapState((GameState)state); }

        /// <summary>
        /// Swap to a new game state.
        /// </summary>
        /// <param name="state"></param>
        public void SwapState(GameState state) {

            // Do any cleanup necessary to switch away from the old state.
            switch (State) { 
                default:
                case GameState.MENU:
                    /// TODO: Cleanup for menu state.
                    break;
                case GameState.PLAYSERVICES:
                    Debug.Log("Closed Google Play services.");
                    break;
                case GameState.MINING:
                    /// TODO: Cleanup for mining state.
                    break;
            }

            // Swap out the UI
            UIManager.Instance.SwapState(state);

            State = state; // Set the new state

            // Do any setup necessary to switch to the new state.
            switch (State) { 
                default:
                case GameState.MENU:
                    /// TODO: Setup for menu state.
                    break;
                case GameState.PLAYSERVICES:
                    Debug.Log("Opened Google Play services.");
                    break;
                case GameState.MINING:
                    /// TODO: Setup for mining state.
                    UIManager.Instance.LoadInventoryToEquipmentBar();
                    UIManager.Instance.SetActiveToolBorder(InventoryManager.Instance.ActiveTool);

                    VoxelGrid.Instance.Generate();
                    break;
            }
        }
    }
}