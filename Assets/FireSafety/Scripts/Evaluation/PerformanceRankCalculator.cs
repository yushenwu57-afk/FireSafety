using System;
using FireSafety.Scenario;
using UnityEngine;

namespace FireSafety.Evaluation
{
    public sealed class PerformanceRankCalculator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerPerformanceTracker performanceTracker;
        [SerializeField] private ScenarioManager scenarioManager;
        [SerializeField] private HazardDiscoveryController discoveryController;

        [Header("Debugging")]
        [SerializeField] private bool logFinalRank;

        public PerformanceRank FinalRank { get; private set; }
        public bool HasFinalRank { get; private set; }

        public event Action<PerformanceRank> FinalRankCalculated;

        private void OnEnable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioCompleted += HandleScenarioCompleted;
                scenarioManager.ScenarioFailed += HandleScenarioFailed;
            }

            if (discoveryController != null)
            {
                discoveryController.DiscoveryFailed += HandleDiscoveryFailed;
            }
        }

        private void OnDisable()
        {
            if (scenarioManager != null)
            {
                scenarioManager.ScenarioCompleted -= HandleScenarioCompleted;
                scenarioManager.ScenarioFailed -= HandleScenarioFailed;
            }

            if (discoveryController != null)
            {
                discoveryController.DiscoveryFailed -= HandleDiscoveryFailed;
            }
        }

        private void HandleScenarioCompleted()
        {
            if (HasFinalRank || performanceTracker == null)
            {
                return;
            }

            PerformanceRank rank;

            if (performanceTracker.CriticalErrorCount > 0)
            {
                rank = PerformanceRank.B;
            }
            else if (performanceTracker.UnsafeActionCount > 0)
            {
                rank = PerformanceRank.A;
            }
            else
            {
                rank = PerformanceRank.S;
            }

            SetFinalRank(rank, null);
        }

        private void HandleScenarioFailed()
        {
            SetFinalRank(
                PerformanceRank.C,
                "Unsafe response path ended in failure");
        }

        private void HandleDiscoveryFailed()
        {
            SetFinalRank(
                PerformanceRank.C,
                "Hazard not identified in time");
        }

        private void SetFinalRank(PerformanceRank rank, string failureReason)
        {
            if (HasFinalRank)
            {
                return;
            }

            FinalRank = rank;
            HasFinalRank = true;

            if (logFinalRank)
            {
                LogFinalRank(failureReason);
            }

            FinalRankCalculated?.Invoke(FinalRank);
        }

        private void LogFinalRank(string failureReason)
        {
            if (!string.IsNullOrEmpty(failureReason))
            {
                Debug.Log(
                    $"[Evaluation] Final Rank: {FinalRank}\n" +
                    $"Reason: {failureReason}",
                    this);
                return;
            }

            Debug.Log(
                $"[Evaluation] Final Rank: {FinalRank}\n" +
                $"Safe Actions: {performanceTracker.SafeActionCount}\n" +
                $"Unsafe Actions: {performanceTracker.UnsafeActionCount}\n" +
                $"Critical Errors: {performanceTracker.CriticalErrorCount}\n" +
                $"Recovered: {performanceTracker.HasRecovered}",
                this);
        }
    }
}
