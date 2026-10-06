using FireSafety.Core;
using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class ResponseFailureUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private SceneFlowService sceneFlowService;
        [SerializeField] private CanvasGroup failurePanel;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button restartButton;

        [Header("Content")]
        [SerializeField] private string failureTitle = "Scenario Failed";
        [SerializeField, TextArea] private string failureMessage =
            "Your response allowed the situation to become too dangerous. " +
            "In a real fire, unsafe actions can quickly increase the risk " +
            "of serious injury.";

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioFailed += HandleScenarioFailed;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            PopulateContent();
            SetPanelVisible(IsResponseFailure());
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioFailed -= HandleScenarioFailed;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleScenarioFailed()
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

        private bool IsResponseFailure()
        {
            return scenarioManager != null
                && gameManager != null
                && scenarioManager.HasScenarioFailed
                && scenarioManager.CurrentPhase == ScenarioPhase.Finished
                && gameManager.CurrentState == GameState.Finished;
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
