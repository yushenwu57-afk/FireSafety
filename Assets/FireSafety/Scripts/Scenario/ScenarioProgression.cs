using FireSafety.Core;
using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class ScenarioProgression : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
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
                isProgressing = gameManager.CurrentState == GameState.Exploration;
            }
            else
            {
                isProgressing = false;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
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
                dangerManager.SetDangerLevel(DangerLevel.Medium);
            }

            if (mediumDangerTriggered
                && !highDangerTriggered
                && elapsedScenarioTime >= highDangerDelay)
            {
                highDangerTriggered = true;
                dangerManager.SetDangerLevel(DangerLevel.High);
            }
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            isProgressing = newState == GameState.Exploration;
        }
    }
}
