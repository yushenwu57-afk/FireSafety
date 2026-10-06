using System;
using FireSafety.Core;
using FireSafety.Hazard;
using FireSafety.Interaction;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class HazardDiscoveryController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private DangerManager dangerManager;
        [SerializeField] private InteractableObject hazardInteractable;

        public bool IsHazardDiscovered { get; private set; }
        public bool IsHintActive { get; private set; }
        public bool HasShownHint { get; private set; }
        public bool HasDiscoveryFailed { get; private set; }

        public event Action HazardDiscovered;
        public event Action HintOpened;
        public event Action HintClosed;
        public event Action DiscoveryFailed;

        private void OnEnable()
        {
            if (hazardInteractable != null)
            {
                hazardInteractable.Interacted += HandleHazardInteracted;
            }

            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged += HandleDangerLevelChanged;
            }
        }

        private void OnDisable()
        {
            if (hazardInteractable != null)
            {
                hazardInteractable.Interacted -= HandleHazardInteracted;
            }

            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged -= HandleDangerLevelChanged;
            }
        }

        public void CloseHint()
        {
            if (!IsHintActive)
            {
                return;
            }

            IsHintActive = false;
            HintClosed?.Invoke();

            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Exploration);
            }
        }

        private void HandleHazardInteracted(InteractableObject interactedObject)
        {
            if (IsHazardDiscovered || HasDiscoveryFailed)
            {
                return;
            }

            IsHazardDiscovered = true;
            HazardDiscovered?.Invoke();

            if (scenarioManager == null || IsTerminalPhase(scenarioManager.CurrentPhase))
            {
                return;
            }

            scenarioManager.ChangePhase(ScenarioPhase.Response);
        }

        private void HandleDangerLevelChanged(
            DangerLevel previousLevel,
            DangerLevel newLevel)
        {
            if (IsHazardDiscovered
                || HasDiscoveryFailed
                || scenarioManager == null
                || IsTerminalPhase(scenarioManager.CurrentPhase))
            {
                return;
            }

            if ((int)newLevel >= (int)DangerLevel.High)
            {
                TriggerDiscoveryFailure();
                return;
            }

            if ((int)newLevel < (int)DangerLevel.Medium
                || HasShownHint
                || gameManager == null
                || gameManager.CurrentState != GameState.Exploration)
            {
                return;
            }

            HasShownHint = true;
            IsHintActive = true;

            if (scenarioManager.CurrentPhase == ScenarioPhase.Observation)
            {
                scenarioManager.ChangePhase(ScenarioPhase.Developing);
            }

            gameManager.ChangeState(GameState.Paused);
            HintOpened?.Invoke();
        }

        private void TriggerDiscoveryFailure()
        {
            HasDiscoveryFailed = true;

            bool wasHintActive = IsHintActive;
            IsHintActive = false;

            if (wasHintActive)
            {
                HintClosed?.Invoke();
            }

            scenarioManager.ChangePhase(ScenarioPhase.Finished);

            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Finished);
            }

            DiscoveryFailed?.Invoke();
        }

        private static bool IsTerminalPhase(ScenarioPhase phase)
        {
            return phase == ScenarioPhase.Resolved
                || phase == ScenarioPhase.Finished;
        }
    }
}
