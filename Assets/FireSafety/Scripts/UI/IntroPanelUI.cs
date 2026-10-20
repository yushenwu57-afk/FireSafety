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
            "Level — Kitchen Emergency";
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
            ConfigureLayout();

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

        [ContextMenu("Refresh Intro Layout")]
        private void ConfigureLayout()
        {
            if (introPanel == null)
            {
                return;
            }

            // Match Level 2's fixed modal layout; content and event wiring stay intact.
            RectTransform panelRect = introPanel.GetComponent<RectTransform>();
            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1100f);
            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 760f);
            if (levelTitleText != null) PlaceRect(levelTitleText.rectTransform, 1020f, 65f, 30f);
            if (situationText != null) PlaceRect(situationText.rectTransform, 1000f, 120f, 112f);
            if (objectivesText != null) PlaceRect(objectivesText.rectTransform, 1000f, 206f, 256f);
            if (beginButton != null) PlaceRect(beginButton.GetComponent<RectTransform>(), 1000f, 78f, 510f);
        }

        private static void PlaceRect(RectTransform rect, float width, float height, float offset)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 1f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rect.anchoredPosition = new Vector2(0f, -offset);
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
                levelTitleText.text = levelTitle == "Level 1 — Kitchen Emergency"
                    ? "Level — Kitchen Emergency"
                    : levelTitle;
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
