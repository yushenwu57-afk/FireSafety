using FireSafety.Core;
using FireSafety.Scenario;
using TMPro;
using UnityEngine;

namespace FireSafety.UI
{
    public sealed class ScenarioObjectiveUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private CanvasGroup objectiveGroup;

        [Header("Objective Text")]
        [SerializeField] private string observationObjective =
            "Identify the hazard";
        [SerializeField] private string developingObjective =
            "Identify the hazard";
        [SerializeField] private string responseObjective =
            "Respond to the kitchen fire";
        [SerializeField] private string stabilisingObjective =
            "Complete the safe response";
        [SerializeField] private string escalatingObjective =
            "Get to safety";
        [SerializeField] private string recoveringObjective =
            "Contact emergency services";

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioPhaseChanged += HandleScenarioPhaseChanged;
            }

            if (gameManager != null)
            {
                gameManager.StateChanged += HandleGameStateChanged;
            }

            RefreshObjective();
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioPhaseChanged -= HandleScenarioPhaseChanged;
            }

            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleGameStateChanged;
            }

            SetObjectiveVisible(false);
        }

        private void HandleScenarioPhaseChanged(
            ScenarioPhase previousPhase,
            ScenarioPhase newPhase)
        {
            RefreshObjective();
        }

        private void HandleGameStateChanged(
            GameState previousState,
            GameState newState)
        {
            RefreshObjective();
        }

        private void RefreshObjective()
        {
            if (scenarioManager == null || gameManager == null)
            {
                SetObjectiveText(string.Empty);
                SetObjectiveVisible(false);
                return;
            }

            string currentObjective = GetObjectiveText(scenarioManager.CurrentPhase);
            bool hasObjective = !string.IsNullOrWhiteSpace(currentObjective);
            bool shouldBeVisible = hasObjective
                && IsObjectiveVisibleInState(gameManager.CurrentState);

            SetObjectiveText(currentObjective);
            SetObjectiveVisible(shouldBeVisible);
        }

        private string GetObjectiveText(ScenarioPhase phase)
        {
            switch (phase)
            {
                case ScenarioPhase.Observation:
                    return observationObjective;
                case ScenarioPhase.Developing:
                    return developingObjective;
                case ScenarioPhase.Response:
                    return responseObjective;
                case ScenarioPhase.Stabilising:
                    return stabilisingObjective;
                case ScenarioPhase.Escalating:
                    return escalatingObjective;
                case ScenarioPhase.Recovering:
                    return recoveringObjective;
                default:
                    return string.Empty;
            }
        }

        private static bool IsObjectiveVisibleInState(GameState state)
        {
            return state == GameState.Exploration
                || state == GameState.Decision
                || state == GameState.Consequence;
        }

        private void SetObjectiveText(string value)
        {
            if (objectiveText != null)
            {
                objectiveText.text = value;
            }
        }

        private void SetObjectiveVisible(bool isVisible)
        {
            if (objectiveGroup == null)
            {
                return;
            }

            objectiveGroup.alpha = isVisible ? 1f : 0f;
            objectiveGroup.blocksRaycasts = false;
            objectiveGroup.interactable = false;
        }
    }
}
