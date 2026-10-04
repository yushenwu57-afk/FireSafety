using FireSafety.Core;
using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class ScenarioProgression : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private DangerManager dangerManager;

        [Header("Progression")]
        [SerializeField, Min(0f)] private float mediumDangerDelay = 10f;
        [SerializeField, Min(0f)] private float highDangerDelay = 20f;

        private float elapsedScenarioTime;
        private bool isProgressing;
        private bool mediumDangerTriggered;
        private bool highDangerTriggered;

        public float ElapsedScenarioTime => elapsedScenarioTime;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
            }

            if (scenarioManager != null)
            {
                scenarioManager.ScenarioPhaseChanged += HandleScenarioPhaseChanged;
            }

            UpdateProgressionState();
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            if (scenarioManager != null)
            {
                scenarioManager.ScenarioPhaseChanged -= HandleScenarioPhaseChanged;
            }

            isProgressing = false;
        }

        private void Update()
        {
            if (!isProgressing)
            {
                return;
            }

            elapsedScenarioTime += Time.deltaTime;
            UpdateDangerThresholds();
        }

        public void ResetProgression()
        {
            elapsedScenarioTime = 0f;
            mediumDangerTriggered = false;
            highDangerTriggered = false;

            if (dangerManager != null)
            {
                dangerManager.SetDangerLevel(DangerLevel.Low);
            }
        }

        private void UpdateDangerThresholds()
        {
            if (dangerManager == null)
            {
                return;
            }

            if (!mediumDangerTriggered && elapsedScenarioTime >= mediumDangerDelay)
            {
                mediumDangerTriggered = true;
                dangerManager.RaiseDangerLevel(DangerLevel.Medium);
            }

            if (mediumDangerTriggered
                && !highDangerTriggered
                && elapsedScenarioTime >= highDangerDelay)
            {
                highDangerTriggered = true;
                dangerManager.RaiseDangerLevel(DangerLevel.High);
            }
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            UpdateProgressionState();
        }

        private void HandleScenarioPhaseChanged(
            ScenarioPhase previousPhase,
            ScenarioPhase newPhase)
        {
            UpdateProgressionState();
        }

        private void UpdateProgressionState()
        {
            if (gameManager == null || scenarioManager == null)
            {
                isProgressing = false;
                return;
            }

            ScenarioPhase phase = scenarioManager.CurrentPhase;
            isProgressing = gameManager.CurrentState == GameState.Exploration
                && phase != ScenarioPhase.Resolved
                && phase != ScenarioPhase.Finished;
        }
    }
}
