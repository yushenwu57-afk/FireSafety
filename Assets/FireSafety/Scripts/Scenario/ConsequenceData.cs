namespace FireSafety.Scenario
{
    public sealed class ConsequenceData
    {
        public string Title { get; }
        public string Description { get; }
        public int SelectedOptionIndex { get; }
        public string SelectedOptionText { get; }

        public ConsequenceData(
            string title,
            string description,
            int selectedOptionIndex,
            string selectedOptionText)
        {
            Title = title;
            Description = description;
            SelectedOptionIndex = selectedOptionIndex;
            SelectedOptionText = selectedOptionText;
        }
    }
}
