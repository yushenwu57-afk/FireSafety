using FireSafety.Core;
using UnityEngine;

namespace FireSafety.UI
{
    public sealed class CrosshairUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject crosshairRoot;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                UpdateVisibility(gameManager.CurrentState);
            }
            else
            {
                SetCrosshairVisible(false);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            SetCrosshairVisible(false);
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            UpdateVisibility(newState);
        }

        private void UpdateVisibility(GameState state)
        {
            SetCrosshairVisible(state == GameState.Exploration);
        }

        private void SetCrosshairVisible(bool isVisible)
        {
            if (crosshairRoot != null)
            {
                crosshairRoot.SetActive(isVisible);
            }
        }
    }
}
