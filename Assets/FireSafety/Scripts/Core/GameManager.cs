using System;
using UnityEngine;

namespace FireSafety.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        [SerializeField] private GameState initialState = GameState.Intro;

        public GameState CurrentState { get; private set; } = GameState.Intro;

        /// <summary>
        /// Raised after the state changes, with the previous and new states.
        /// </summary>
        public event Action<GameState, GameState> StateChanged;

        private void Start()
        {
            ChangeState(initialState);
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            GameState previousState = CurrentState;
            CurrentState = newState;
            StateChanged?.Invoke(previousState, newState);
        }
    }
}
