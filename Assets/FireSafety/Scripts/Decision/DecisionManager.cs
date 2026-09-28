using System;
using FireSafety.Core;
using UnityEngine;

namespace FireSafety.Decision
{
    public sealed class DecisionManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        public DecisionData CurrentDecision { get; private set; }

        public event Action<DecisionData> DecisionOpened;
        public event Action<DecisionData, int, string> DecisionSelected;
        public event Action<DecisionData> DecisionClosed;

        public void OpenDecision(DecisionData decision)
        {
            if (decision == null || gameManager == null || CurrentDecision != null)
            {
                return;
            }

            CurrentDecision = decision;
            gameManager.ChangeState(GameState.Decision);
            DecisionOpened?.Invoke(decision);
        }

        public void SelectOption(int optionIndex)
        {
            if (CurrentDecision == null || optionIndex < 0 || optionIndex > 2)
            {
                return;
            }

            DecisionData selectedDecision = CurrentDecision;
            string selectedOptionText = GetOptionText(selectedDecision, optionIndex);

            DecisionSelected?.Invoke(
                selectedDecision,
                optionIndex,
                selectedOptionText);

            CurrentDecision = null;
            DecisionClosed?.Invoke(selectedDecision);
            gameManager.ChangeState(GameState.Consequence);
        }

        private static string GetOptionText(DecisionData decision, int optionIndex)
        {
            switch (optionIndex)
            {
                case 0:
                    return decision.OptionAText;
                case 1:
                    return decision.OptionBText;
                case 2:
                    return decision.OptionCText;
                default:
                    return string.Empty;
            }
        }
    }
}
