﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App {

    public enum GameState { MENU, INGAME, GAMEOVER_GOOD, GAMEOVER_BAD }

    public class StateManager : MonoBehaviour {

        //instance
        public static StateManager Instance;

        public GameState State;

        public GameObject[] stateObjects;
        [SerializeField] GameObject[] stateUIObjects;

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(this);

            SwapState(State);
        }

        // Update is called once per frame
        void Update() {
            switch(State) {
                case GameState.INGAME:
                    CivilianManager.Instance.DoUpdateStep();
                    CarManager.Instance.DoUpdateStep();
                    ScoreManager.Instance.CheckGameover();
                    break;

                case GameState.MENU:

                    break;
            }
        }

        public void SwapState(int state) {
            SwapState((GameState)state);
        }

        public void SwapState(GameState state) {
            for(int i = 0; i < stateObjects.Length; i++) {
                stateObjects[i].SetActive(i == (int)state);
                stateUIObjects[i].SetActive(i == (int)state);
            }

            State = state;
        }
    }
}
