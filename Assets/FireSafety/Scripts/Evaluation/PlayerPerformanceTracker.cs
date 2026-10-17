using System.Collections.Generic;
using FireSafety.Decision;
using FireSafety.Scenario;
using UnityEngine;

namespace FireSafety.Evaluation
{
    public sealed class PlayerActionRecord
    {
        public string SelectedOptionText { get; }
        public ResponseQuality ResponseQuality { get; }
        public bool IsRecoveryAction { get; }
        public string LearningPoint { get; }
        public ScenarioPhase ScenarioPhaseAfterOutcome { get; }

        public PlayerActionRecord(
            string selectedOptionText,
            ResponseQuality responseQuality,
            bool isRecoveryAction,
            string learningPoint,
            ScenarioPhase scenarioPhaseAfterOutcome)
        {
            SelectedOptionText = selectedOptionText;
            ResponseQuality = responseQuality;
            IsRecoveryAction = isRecoveryAction;
            LearningPoint = learningPoint;
            ScenarioPhaseAfterOutcome = scenarioPhaseAfterOutcome;
        }
    }

    public sealed class PlayerPerformanceTracker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DecisionManager decisionManager;
        [SerializeField] private ScenarioManager scenarioManager;

        [Header("Debugging")]
        [SerializeField] private bool logSelections;

        private readonly List<PlayerActionRecord> recordedActions =
            new List<PlayerActionRecord>();

        public IReadOnlyList<PlayerActionRecord> RecordedActions => recordedActions;
        public int SafeActionCount { get; private set; }
        public int UnsafeActionCount { get; private set; }
        public int CriticalErrorCount { get; private set; }
        public int RecoveryActionCount { get; private set; }
        public bool HasMadeUnsafeChoice =>
            UnsafeActionCount > 0 || CriticalErrorCount > 0;
        public bool HasMadeCriticalError => CriticalErrorCount > 0;
        public bool HasRecovered { get; private set; }

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

        private void HandleDecisionSelected(
            DecisionData decision,
            int optionIndex,
            string selectedOptionText)
        {
            DecisionOutcomeData outcome = decision?.GetOutcome(optionIndex);
            if (outcome == null)
            {
                return;
            }

            bool hadPreviouslyMadeUnsafeChoice = HasMadeUnsafeChoice;
            ScenarioPhase phaseAfterOutcome = GetPhaseAfterOutcome(outcome);

            recordedActions.Add(new PlayerActionRecord(
                selectedOptionText,
                outcome.ResponseQuality,
                outcome.IsRecoveryAction,
                outcome.LearningPoint,
                phaseAfterOutcome));

            UpdateCounts(outcome.ResponseQuality);

            if (outcome.IsRecoveryAction)
            {
                RecoveryActionCount++;

                if (hadPreviouslyMadeUnsafeChoice)
                {
                    HasRecovered = true;
                }
            }

            if (logSelections)
            {
                Debug.Log(
                    $"[Evaluation] \"{selectedOptionText}\"\n" +
                    $"Quality: {outcome.ResponseQuality}\n" +
                    $"Recovery: {outcome.IsRecoveryAction}\n" +
                    $"Critical Errors: {CriticalErrorCount}",
                    this);
            }
        }

        private ScenarioPhase GetPhaseAfterOutcome(DecisionOutcomeData outcome)
        {
            if (outcome.ChangeScenarioPhase)
            {
                return outcome.TargetScenarioPhase;
            }

            return scenarioManager != null
                ? scenarioManager.CurrentPhase
                : ScenarioPhase.Observation;
        }

        private void UpdateCounts(ResponseQuality responseQuality)
        {
            switch (responseQuality)
            {
                case ResponseQuality.Safe:
                    SafeActionCount++;
                    break;
                case ResponseQuality.Unsafe:
                    UnsafeActionCount++;
                    break;
                case ResponseQuality.Critical:
                    CriticalErrorCount++;
                    break;
            }
        }
    }
}
