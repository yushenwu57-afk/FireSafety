using System;
using FireSafety.Evaluation;
using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public enum VisualEffectCommand
    {
        NoChange,
        Start,
        Stop
    }

    [Serializable]
    public sealed class DecisionOutcomeData
    {
        [Header("Feedback")]
        [SerializeField] private string feedbackTitle = "Consequence";
        [SerializeField, TextArea] private string feedbackDescription =
            "Placeholder consequence.";

        [Header("Danger")]
        [SerializeField] private bool changeDangerLevel;
        [SerializeField] private DangerLevel targetDangerLevel = DangerLevel.Low;

        [Header("Visual Effects")]
        [SerializeField] private VisualEffectCommand fireCommand =
            VisualEffectCommand.NoChange;
        [SerializeField] private VisualEffectCommand smokeCommand =
            VisualEffectCommand.NoChange;

        [Header("Scenario Phase")]
        [SerializeField] private bool changeScenarioPhase;
        [SerializeField] private ScenarioPhase targetScenarioPhase =
            ScenarioPhase.Observation;

        [Header("Evaluation")]
        [SerializeField] private ResponseQuality responseQuality =
            ResponseQuality.Safe;
        [SerializeField] private bool isRecoveryAction;
        [SerializeField, TextArea] private string learningPoint = string.Empty;

        public string FeedbackTitle => feedbackTitle;
        public string FeedbackDescription => feedbackDescription;
        public bool ChangeDangerLevel => changeDangerLevel;
        public DangerLevel TargetDangerLevel => targetDangerLevel;
        public VisualEffectCommand FireCommand => fireCommand;
        public VisualEffectCommand SmokeCommand => smokeCommand;
        public bool ChangeScenarioPhase => changeScenarioPhase;
        public ScenarioPhase TargetScenarioPhase => targetScenarioPhase;
        public ResponseQuality ResponseQuality => responseQuality;
        public bool IsRecoveryAction => isRecoveryAction;
        public string LearningPoint => learningPoint;
    }
}
