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

            // Measure the existing fonts; only resize and position the panel contents.
            RectTransform panelRect = introPanel.GetComponent<RectTransform>();
            const float padding = 40f;
            const float spacing = 24f;
            float width = Mathf.Max(panelRect.rect.width, 760f);
            float contentWidth = width - padding * 2f;
            float offset = padding;

            PlaceText(levelTitleText, contentWidth, ref offset, spacing);
            PlaceText(situationText, contentWidth, ref offset, spacing);
            PlaceText(objectivesText, contentWidth, ref offset, spacing);

            if (beginButton != null)
            {
                RectTransform buttonRect = beginButton.GetComponent<RectTransform>();
                float buttonHeight = buttonRect.rect.height;
                PlaceRect(buttonRect, buttonRect.rect.width, buttonHeight, offset);
                offset += buttonHeight + spacing;
            }

            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            panelRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical, offset - spacing + padding);
        }

        private static void PlaceText(TMP_Text text, float width, ref float offset, float spacing)
        {
            if (text == null)
            {
                return;
            }

            float height = Mathf.Ceil(text.GetPreferredValues(
                text.text, width, Mathf.Infinity).y);
            PlaceRect(text.rectTransform, width, height, offset);
            offset += height + spacing;
        }

        private static void PlaceRect(RectTransform rect, float width, float height, float offset)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            rect.anchoredPosition = new Vector2(0f, -offset - height * 0.5f);
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
