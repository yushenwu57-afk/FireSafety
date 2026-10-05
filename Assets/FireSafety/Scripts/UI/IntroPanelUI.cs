using FireSafety.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class IntroPanelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CanvasGroup introPanel;
        [SerializeField] private TMP_Text levelTitleText;
        [SerializeField] private TMP_Text situationText;
        [SerializeField] private TMP_Text objectivesText;
        [SerializeField] private Button beginButton;

        [Header("Content")]
        [SerializeField] private string levelTitle =
            "Level 1 — Kitchen Emergency";
        [SerializeField, TextArea] private string situation =
            "You are alone in a residential apartment. Something in the " +
            "environment may require your attention.";
        [SerializeField, TextArea] private string objectives =
            "Objectives\n\n" +
            "• Identify the hazard\n" +
            "• Respond appropriately\n" +
            "• Prevent the situation from escalating";

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
            }

            if (beginButton != null)
            {
                beginButton.onClick.AddListener(HandleBeginClicked);
            }

            PopulateContent();

            if (gameManager != null)
            {
                UpdateVisibility(gameManager.CurrentState);
            }
            else
            {
                SetPanelVisible(false);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            if (beginButton != null)
            {
                beginButton.onClick.RemoveListener(HandleBeginClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            UpdateVisibility(newState);
        }

        private void HandleBeginClicked()
        {
            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Exploration);
            }
        }

        private void PopulateContent()
        {
            if (levelTitleText != null)
            {
                levelTitleText.text = levelTitle;
            }

            if (situationText != null)
            {
                situationText.text = situation;
            }

            if (objectivesText != null)
            {
                objectivesText.text = objectives;
            }
        }

        private void UpdateVisibility(GameState state)
        {
            SetPanelVisible(state == GameState.Intro);
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (introPanel == null)
            {
                return;
            }

            introPanel.alpha = isVisible ? 1f : 0f;
            introPanel.interactable = isVisible;
            introPanel.blocksRaycasts = isVisible;
        }
    }
}
