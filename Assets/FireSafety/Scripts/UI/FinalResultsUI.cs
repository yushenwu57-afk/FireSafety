using System;
using FireSafety.Core;
using FireSafety.Evaluation;
using FireSafety.Scenario;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSafety.UI
{
    public sealed class FinalResultsUI : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private PerformanceRankCalculator rankCalculator;
        [SerializeField] private PlayerPerformanceTracker performanceTracker;
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private HazardDiscoveryController discoveryController;
        [SerializeField] private SceneFlowService sceneFlowService;

        [Header("UI References")]
        [SerializeField] private CanvasGroup resultsPanel;
        [SerializeField] private TMP_Text rankText;
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private Button restartButton;

        private void OnEnable()
        {
            if (rankCalculator != null)
            {
                rankCalculator.FinalRankCalculated += HandleFinalRankCalculated;
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (rankCalculator != null && rankCalculator.HasFinalRank)
            {
                BuildAndShowResults(rankCalculator.FinalRank);
            }
            else
            {
                SetPanelVisible(false);
            }
        }

        private void OnDisable()
        {
            if (rankCalculator != null)
            {
                rankCalculator.FinalRankCalculated -= HandleFinalRankCalculated;
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            SetPanelVisible(false);
        }

        private void HandleFinalRankCalculated(PerformanceRank rank)
        {
            BuildAndShowResults(rank);
        }

        private void HandleRestartClicked()
        {
            if (sceneFlowService != null)
            {
                sceneFlowService.RestartCurrentScene();
            }
        }

        private void BuildAndShowResults(PerformanceRank rank)
        {
            bool discoveryFailed = discoveryController != null
                && discoveryController.HasDiscoveryFailed;
            bool responseFailed = !discoveryFailed
                && scenarioManager != null
                && scenarioManager.HasScenarioFailed;
            PerformanceRank displayedRank = discoveryFailed || responseFailed
                ? PerformanceRank.C
                : rank;

            SetText(rankText, $"Rank {displayedRank}");
            SetText(summaryText, GetSummary(displayedRank, discoveryFailed, responseFailed));
            SetText(
                feedbackText,
                BuildFeedback(displayedRank, discoveryFailed, responseFailed));

            SetPanelVisible(true);
        }

        private static string GetSummary(
            PerformanceRank rank,
            bool discoveryFailed,
            bool responseFailed)
        {
            if (discoveryFailed)
            {
                return "Hazard Not Identified";
            }

            if (responseFailed)
            {
                return "Scenario Failed";
            }

            switch (rank)
            {
                case PerformanceRank.S:
                    return "Excellent Response";
                case PerformanceRank.A:
                    return "Safe Recovery";
                case PerformanceRank.B:
                    return "Recovered After a Critical Mistake";
                default:
                    return "Scenario Failed";
            }
        }

        private string BuildFeedback(
            PerformanceRank rank,
            bool discoveryFailed,
            bool responseFailed)
        {
            if (discoveryFailed)
            {
                return "The hazard was not identified before the situation " +
                    "escalated. Recognise warning signs early and investigate " +
                    "potential hazards before they become more serious.";
            }

            if (responseFailed)
            {
                return AppendUniqueFeedback(
                    "Your response allowed the situation to become too dangerous.",
                    FindRelevantLearningPoint());
            }

            switch (rank)
            {
                case PerformanceRank.S:
                    return "You handled the emergency safely from beginning to end.\n" +
                        "Your decisions prevented the situation from escalating.";
                case PerformanceRank.A:
                    return AppendUniqueFeedback(
                        "You made an unsafe decision but corrected the situation " +
                        "before it became uncontrollable.",
                        FindFirstLearningPoint(ResponseQuality.Unsafe));
                case PerformanceRank.B:
                    string feedback = AppendUniqueFeedback(
                        "You made a critical safety mistake but successfully recovered.",
                        FindFirstLearningPoint(ResponseQuality.Critical));

                    if (performanceTracker != null && performanceTracker.HasRecovered)
                    {
                        feedback = AppendUniqueFeedback(
                            feedback,
                            "You recovered through safer follow-up decisions.");
                    }

                    return feedback;
                default:
                    return AppendUniqueFeedback(
                        "Your response allowed the situation to become too dangerous.",
                        FindRelevantLearningPoint());
            }
        }

        private string FindRelevantLearningPoint()
        {
            string learningPoint = FindFirstLearningPoint(ResponseQuality.Critical);
            return !string.IsNullOrEmpty(learningPoint)
                ? learningPoint
                : FindFirstLearningPoint(ResponseQuality.Unsafe);
        }

        private string FindFirstLearningPoint(ResponseQuality responseQuality)
        {
            if (performanceTracker == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < performanceTracker.RecordedActions.Count; i++)
            {
                PlayerActionRecord action = performanceTracker.RecordedActions[i];
                if (action != null
                    && action.ResponseQuality == responseQuality
                    && !string.IsNullOrWhiteSpace(action.LearningPoint))
                {
                    return action.LearningPoint.Trim();
                }
            }

            return string.Empty;
        }

        private static string AppendUniqueFeedback(string feedback, string addition)
        {
            if (string.IsNullOrWhiteSpace(addition)
                || feedback.IndexOf(addition, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return feedback;
            }

            return $"{feedback}\n\n{addition}";
        }

        private static void SetText(TMP_Text textField, string value)
        {
            if (textField != null)
            {
                textField.text = value;
            }
        }

        private void SetPanelVisible(bool isVisible)
        {
            if (resultsPanel == null)
            {
                return;
            }

            resultsPanel.alpha = isVisible ? 1f : 0f;
            resultsPanel.interactable = isVisible;
            resultsPanel.blocksRaycasts = isVisible;
        }
    }
}
