using FireSafety.Scenario;

namespace FireSafety.Decision
{
    public sealed class DecisionData
    {
        public string Title { get; }
        public string Description { get; }
        public string OptionAText { get; }
        public string OptionBText { get; }
        public string OptionCText { get; }

        private DecisionOutcomeData OptionAOutcome { get; }
        private DecisionOutcomeData OptionBOutcome { get; }
        private DecisionOutcomeData OptionCOutcome { get; }

        public DecisionData(
            string title,
            string description,
            string optionAText,
            string optionBText,
            string optionCText,
            DecisionOutcomeData optionAOutcome,
            DecisionOutcomeData optionBOutcome,
            DecisionOutcomeData optionCOutcome)
        {
            Title = title;
            Description = description;
            OptionAText = optionAText;
            OptionBText = optionBText;
            OptionCText = optionCText;
            OptionAOutcome = optionAOutcome;
            OptionBOutcome = optionBOutcome;
            OptionCOutcome = optionCOutcome;
        }

        public DecisionOutcomeData GetOutcome(int optionIndex)
        {
            switch (optionIndex)
            {
                case 0:
                    return OptionAOutcome;
                case 1:
                    return OptionBOutcome;
                case 2:
                    return OptionCOutcome;
                default:
                    return null;
            }
        }
    }
}
