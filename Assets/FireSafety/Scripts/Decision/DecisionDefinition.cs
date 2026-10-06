using System;
using FireSafety.Scenario;
using UnityEngine;

namespace FireSafety.Decision
{
    [Serializable]
    public sealed class DecisionDefinition
    {
        [Header("Decision Content")]
        [SerializeField] private string decisionTitle = "Choose an action";
        [SerializeField, TextArea] private string decisionDescription =
            "What do you want to do?";
        [SerializeField] private string optionA = "Option A";
        [SerializeField] private string optionB = "Option B";
        [SerializeField] private string optionC = "Option C";

        [Header("Option Outcomes")]
        [SerializeField] private DecisionOutcomeData optionAOutcome =
            new DecisionOutcomeData();
        [SerializeField] private DecisionOutcomeData optionBOutcome =
            new DecisionOutcomeData();
        [SerializeField] private DecisionOutcomeData optionCOutcome =
            new DecisionOutcomeData();

        public DecisionData CreateDecisionData()
        {
            return new DecisionData(
                decisionTitle,
                decisionDescription,
                optionA,
                optionB,
                optionC,
                optionAOutcome,
                optionBOutcome,
                optionCOutcome);
        }
    }
}
