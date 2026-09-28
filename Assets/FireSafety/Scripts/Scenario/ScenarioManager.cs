using System;
using FireSafety.Core;
using FireSafety.Decision;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class ScenarioManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private DecisionManager decisionManager;

        public ConsequenceData CurrentConsequence { get; private set; }

        public event Action<ConsequenceData> ConsequenceOpened;
        public event Action<ConsequenceData> ConsequenceClosed;

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

        private void HandleDecisionSelected(
            DecisionData decision,
            int optionIndex,
            string selectedOptionText)
        {
            ConsequenceData consequence = new ConsequenceData(
                "Consequence",
                "You selected: " + selectedOptionText,
                optionIndex,
                selectedOptionText);

            CurrentConsequence = consequence;
            ConsequenceOpened?.Invoke(consequence);
        }
    }
}
