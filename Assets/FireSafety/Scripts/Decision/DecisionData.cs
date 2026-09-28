namespace FireSafety.Decision
{
    public sealed class DecisionData
    {
        public string Title { get; }
        public string Description { get; }
        public string OptionAText { get; }
        public string OptionBText { get; }
        public string OptionCText { get; }

        public DecisionData(
            string title,
            string description,
            string optionAText,
            string optionBText,
            string optionCText)
        {
            Title = title;
            Description = description;
            OptionAText = optionAText;
            OptionBText = optionBText;
            OptionCText = optionCText;
        }
    }
}
