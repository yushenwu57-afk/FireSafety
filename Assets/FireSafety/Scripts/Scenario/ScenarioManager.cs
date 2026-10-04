using System;
using FireSafety.Core;
using FireSafety.Decision;
using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class ScenarioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private DecisionManager decisionManager;
        [SerializeField] private DangerManager dangerManager;
        [SerializeField] private FireEffectController fireEffectController;
        [SerializeField] private SmokeEffectController smokeEffectController;

        [Header("Scenario")]
        [SerializeField] private ScenarioPhase initialPhase =
            ScenarioPhase.Observation;

        public ConsequenceData CurrentConsequence { get; private set; }
        public ScenarioPhase CurrentPhase { get; private set; }

        public event Action<ConsequenceData> ConsequenceOpened;
        public event Action<ConsequenceData> ConsequenceClosed;
        public event Action<ScenarioPhase, ScenarioPhase> ScenarioPhaseChanged;

        private void Awake()
        {
            CurrentPhase = initialPhase;
        }

        private void OnEnable()
        {
            if (decisionManager != null)
            {
                decisionManager.DecisionSelected += HandleDecisionSelected;
            }
        }

        private void OnDisable()
        {
            if (decisionManager != null)
            {
                decisionManager.DecisionSelected -= HandleDecisionSelected;
            }
        }

        public void ContinueAfterConsequence()
        {
            if (CurrentConsequence == null)
            {
                return;
            }

            ConsequenceData closedConsequence = CurrentConsequence;
            CurrentConsequence = null;
            ConsequenceClosed?.Invoke(closedConsequence);

            if (gameManager != null)
            {
                gameManager.ChangeState(GameState.Exploration);
            }
        }

        public void ChangePhase(ScenarioPhase newPhase)
        {
            if (newPhase == CurrentPhase)
            {
                return;
            }

            ScenarioPhase previousPhase = CurrentPhase;
            CurrentPhase = newPhase;
            ScenarioPhaseChanged?.Invoke(previousPhase, newPhase);
        }

        private void HandleDecisionSelected(
            DecisionData decision,
            int optionIndex,
            string selectedOptionText)
        {
            DecisionOutcomeData outcome = decision?.GetOutcome(optionIndex);

            if (outcome != null)
            {
                ApplyOutcome(outcome);
            }

            string feedbackTitle = outcome != null
                ? outcome.FeedbackTitle
                : "Consequence";
            string feedbackDescription = outcome != null
                ? outcome.FeedbackDescription
                : "You selected: " + selectedOptionText;

            ConsequenceData consequence = new ConsequenceData(
                feedbackTitle,
                feedbackDescription,
                optionIndex,
                selectedOptionText);

            CurrentConsequence = consequence;
            ConsequenceOpened?.Invoke(consequence);
        }

        private void ApplyOutcome(DecisionOutcomeData outcome)
        {
            if (outcome.ChangeDangerLevel && dangerManager != null)
            {
                dangerManager.SetDangerLevel(outcome.TargetDangerLevel);
            }

            ApplyFireCommand(outcome.FireCommand);
            ApplySmokeCommand(outcome.SmokeCommand);

            if (outcome.ChangeScenarioPhase)
            {
                ChangePhase(outcome.TargetScenarioPhase);
            }
        }

        private void ApplyFireCommand(VisualEffectCommand command)
        {
            if (fireEffectController == null)
            {
                return;
            }

            switch (command)
            {
                case VisualEffectCommand.Start:
                    fireEffectController.StartFire();
                    break;
                case VisualEffectCommand.Stop:
                    fireEffectController.StopFire();
                    break;
            }
        }

        private void ApplySmokeCommand(VisualEffectCommand command)
        {
            if (smokeEffectController == null)
            {
                return;
            }

            switch (command)
            {
                case VisualEffectCommand.Start:
                    smokeEffectController.StartSmoke();
                    break;
                case VisualEffectCommand.Stop:
                    smokeEffectController.StopSmoke();
                    break;
            }
        }
    }
}
