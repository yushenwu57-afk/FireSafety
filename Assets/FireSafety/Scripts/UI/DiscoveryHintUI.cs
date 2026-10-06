using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class DiscoveryHintUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HazardDiscoveryController discoveryController;
        [SerializeField] private CanvasGroup hintPanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button continueButton;

        [Header("Content")]
        [SerializeField] private string hintTitle = "Something is wrong";
        [SerializeField, TextArea] private string hintMessage =
            "You notice signs of a problem coming from the kitchen. " +
            "Investigate the area immediately.";

        private void OnEnable()
        {
            if (discoveryController != null)
            {
                discoveryController.HintOpened += HandleHintOpened;
                discoveryController.HintClosed += HandleHintClosed;
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinueClicked);
            }

            PopulateContent();
            SetPanelVisible(
                discoveryController != null && discoveryController.IsHintActive);
        }

        private void OnDisable()
        {
            if (discoveryController != null)
            {
                discoveryController.HintOpened -= HandleHintOpened;
                discoveryController.HintClosed -= HandleHintClosed;
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(HandleContinueClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleHintOpened()
        {
            PopulateContent();
            SetPanelVisible(true);
        }

        private void HandleHintClosed()
        {
            SetPanelVisible(false);
        }

        private void HandleContinueClicked()
        {
            if (discoveryController != null)
            {
                discoveryController.CloseHint();
            }
        }

        private void PopulateContent()
        {
            if (titleText != null)
            {
                titleText.text = hintTitle;
            }

            if (messageText != null)
            {
                messageText.text = hintMessage;
            }
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (hintPanel == null)
            {
                return;
            }

            hintPanel.alpha = isVisible ? 1f : 0f;
            hintPanel.interactable = isVisible;
            hintPanel.blocksRaycasts = isVisible;
        }
    }
}
