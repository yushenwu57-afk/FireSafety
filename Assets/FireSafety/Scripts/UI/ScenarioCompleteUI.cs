using FireSafety.Core;
using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class ScenarioCompleteUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SceneFlowService sceneFlowService;
        [SerializeField] private CanvasGroup completePanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button restartButton;

        [Header("Content")]
        [SerializeField] private string completeTitle = "Scenario Complete";
        [SerializeField, TextArea] private string completeMessage =
            "You successfully identified and responded to the kitchen hazard.";

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioCompleted += HandleScenarioCompleted;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            PopulateContent();
            SetPanelVisible(IsScenarioComplete());
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioCompleted -= HandleScenarioCompleted;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleScenarioCompleted()
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

        private bool IsScenarioComplete()
        {
            return scenarioManager != null
                && gameManager != null
                && scenarioManager.CurrentPhase == ScenarioPhase.Resolved
                && gameManager.CurrentState == GameState.Finished;
        }

        private void PopulateContent()
        {
            if (titleText != null)
            {
                titleText.text = completeTitle;
            }

            if (messageText != null)
            {
                messageText.text = completeMessage;
            }
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (completePanel == null)
            {
                return;
            }

            completePanel.alpha = isVisible ? 1f : 0f;
            completePanel.interactable = isVisible;
            completePanel.blocksRaycasts = isVisible;
        }
    }
}
