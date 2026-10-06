using FireSafety.Core;
using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class DiscoveryFailureUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HazardDiscoveryController discoveryController;
        [SerializeField] private SceneFlowService sceneFlowService;
        [SerializeField] private CanvasGroup failurePanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button restartButton;

        [Header("Content")]
        [SerializeField] private string failureTitle = "Scenario Failed";
        [SerializeField, TextArea] private string failureMessage =
            "You did not identify the hazard in time. The situation has " +
            "escalated beyond safe control.";

        private void OnEnable()
        {
            if (discoveryController != null)
            {
                discoveryController.DiscoveryFailed += HandleDiscoveryFailed;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            PopulateContent();
            SetPanelVisible(
                discoveryController != null
                && discoveryController.HasDiscoveryFailed);
        }

        private void OnDisable()
        {
            if (discoveryController != null)
            {
                discoveryController.DiscoveryFailed -= HandleDiscoveryFailed;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleDiscoveryFailed()
        {
            PopulateContent();
            SetPanelVisible(true);
        }

        private void HandleRestartClicked()
        {
            if (sceneFlowService != null)
            {
                sceneFlowService.RestartCurrentScene();
            }
        }

        private void PopulateContent()
        {
            if (titleText != null)
            {
                titleText.text = failureTitle;
            }

            if (messageText != null)
            {
                messageText.text = failureMessage;
            }
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (failurePanel == null)
            {
                return;
            }

            failurePanel.alpha = isVisible ? 1f : 0f;
            failurePanel.interactable = isVisible;
            failurePanel.blocksRaycasts = isVisible;
        }
    }
}
