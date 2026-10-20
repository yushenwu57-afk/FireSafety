using System;
using System.Collections.Generic;
using FireSafety.Evaluation;

namespace FireSafety.Scenario
{
    public enum EvacuationStep { LeaveNow, LeaveAfterDelay, SafeRoute, ReturnFromKitchen, Outside, Finished }
    public enum EvacuationStatus { InProgress, Completed, Failed }
    public enum EvacuationFailure { None, EnteredKitchen, ReenteredForPhone }

    public sealed class EvacuationActionRecord
    {
        public EvacuationStep Step { get; }
        public string SelectedOptionText { get; }
        public ResponseQuality ResponseQuality { get; }
        public bool IsRecoveryAction { get; }
        public string LearningPoint { get; }
        public EvacuationFailure FailureEvent { get; }

        public EvacuationActionRecord(EvacuationStep step, string text, ResponseQuality quality,
            bool recovery, string learningPoint, EvacuationFailure failure)
        {
            Step = step;
            SelectedOptionText = text;
            ResponseQuality = quality;
            IsRecoveryAction = recovery;
            LearningPoint = learningPoint;
            FailureEvent = failure;
        }
    }

    // Level-local behavior record. Reuses the existing evaluation vocabulary without
    // changing the first level's tracker or claiming an official CBAS standard.
    public sealed class EvacuationSession
    {
        private readonly List<EvacuationActionRecord> actions = new List<EvacuationActionRecord>();
        public IReadOnlyList<EvacuationActionRecord> RecordedActions { get; }
        public EvacuationStep Step { get; private set; }
        public EvacuationStatus Status { get; private set; }
        public EvacuationFailure FailureEvent { get; private set; }
        public int OrdinaryMistakeCount { get; private set; }
        public int SeriousMistakeCount { get; private set; }
        public int Revision { get; private set; }
        public bool HasFinalRank => Status != EvacuationStatus.InProgress;
        public bool SuccessfullyEvacuated => Status == EvacuationStatus.Completed;
        public PerformanceRank? FinalRank => CalculateRank(Status, FailureEvent,
            OrdinaryMistakeCount, SeriousMistakeCount);

        public EvacuationSession()
        {
            RecordedActions = actions.AsReadOnly();
            Reset();
        }

        public void Reset()
        {
            actions.Clear();
            OrdinaryMistakeCount = SeriousMistakeCount = 0;
            Status = EvacuationStatus.InProgress;
            FailureEvent = EvacuationFailure.None;
            Step = EvacuationStep.LeaveNow;
            // Never reuse a revision, including across replay, so stale UI callbacks fail.
            Revision++;
        }

        public static PerformanceRank? CalculateRank(EvacuationStatus status,
            EvacuationFailure failure, int ordinary, int serious)
        {
            if (ordinary < 0 || serious < 0) throw new ArgumentOutOfRangeException();
            if (status == EvacuationStatus.InProgress) return null;
            if (status == EvacuationStatus.Failed || failure != EvacuationFailure.None) return PerformanceRank.C;
            if (serious >= 1 || ordinary >= 2) return PerformanceRank.B;
            if (ordinary == 1) return PerformanceRank.A;
            return PerformanceRank.S;
        }

        public string Title => Step == EvacuationStep.Outside
            ? "You have reached a safe place outside" : "Level 2: Fire Evacuation";
        public string Description
        {
            get
            {
                switch (Step)
                {
                    case EvacuationStep.LeaveNow:
                        return "The kitchen fire is spreading and the smoke alarm is sounding. The exit to the outside is still safe and clear. Decide what to do next.\n\nDecision 1: What will you do now?";
                    case EvacuationStep.LeaveAfterDelay: return "You have delayed your evacuation. Leave immediately.";
                    case EvacuationStep.SafeRoute: return "Decision 2: During your evacuation, what will you do?";
                    case EvacuationStep.ReturnFromKitchen: return "The smoke ahead is getting thicker. Moving closer will increase the danger.";
                    case EvacuationStep.Outside: return "Decision 3: You are safely outside, but your phone is still inside. What will you do?";
                    default: return "This attempt has ended.";
                }
            }
        }
        public int OptionCount => HasFinalRank ? 0 : Step == EvacuationStep.LeaveAfterDelay ? 1 : 2;

        public string GetOptionText(int index)
        {
            if (index < 0 || index >= OptionCount) throw new ArgumentOutOfRangeException(nameof(index));
            switch (Step)
            {
                case EvacuationStep.LeaveNow: return index == 0 ? "Evacuate immediately through the safe exit" : "Collect valuables first";
                case EvacuationStep.LeaveAfterDelay: return "Evacuate immediately";
                case EvacuationStep.SafeRoute: return index == 0 ? "Continue along the safe exit route" : "Return to the kitchen to check the fire";
                case EvacuationStep.ReturnFromKitchen: return index == 0 ? "Return to the safe route and leave immediately" : "Continue into the kitchen";
                case EvacuationStep.Outside: return index == 0 ? "Stay in the safe place and ask someone to report the fire" : "Re-enter the building to find your phone";
                default: throw new InvalidOperationException();
            }
        }

        public bool TrySelect(int expectedRevision, int index)
        {
            if (expectedRevision != Revision || HasFinalRank || index < 0 || index >= OptionCount) return false;
            string label = GetOptionText(index);
            EvacuationStep previousStep = Step;
            ResponseQuality quality = ResponseQuality.Safe;
            bool recovery = false;
            string feedback;
            switch (Step)
            {
                case EvacuationStep.LeaveNow:
                    Step = index == 0 ? EvacuationStep.SafeRoute : EvacuationStep.LeaveAfterDelay;
                    if (index == 1) { OrdinaryMistakeCount++; quality = ResponseQuality.Unsafe; }
                    feedback = index == 0 ? "You promptly chose to evacuate through the safe exit." : "Collecting valuables delayed your evacuation. Leave immediately.";
                    break;
                case EvacuationStep.LeaveAfterDelay:
                    recovery = true;
                    Step = EvacuationStep.SafeRoute;
                    feedback = "You corrected the delay and began evacuating. The mistake still counts.";
                    break;
                case EvacuationStep.SafeRoute:
                    Step = index == 0 ? EvacuationStep.Outside : EvacuationStep.ReturnFromKitchen;
                    if (index == 1) { SeriousMistakeCount++; quality = ResponseQuality.Critical; }
                    feedback = index == 0 ? "You followed the safe route outside." : "Returning to the kitchen increased smoke exposure. Keep moving towards the exit.";
                    break;
                case EvacuationStep.ReturnFromKitchen:
                    recovery = index == 0;
                    if (index == 0) Step = EvacuationStep.Outside;
                    else { Fail(EvacuationFailure.EnteredKitchen); quality = ResponseQuality.Critical; }
                    feedback = index == 0 ? "You returned to the safe route and left. The serious mistake remains recorded." : "Entering the kitchen caused evacuation failure. Return to the safe route instead.";
                    break;
                case EvacuationStep.Outside:
                    if (index == 0) { Status = EvacuationStatus.Completed; Step = EvacuationStep.Finished; }
                    else { Fail(EvacuationFailure.ReenteredForPhone); quality = ResponseQuality.Critical; }
                    feedback = index == 0 ? "You stayed safe and asked for help reporting the fire. Call the local fire emergency number." : "Re-entering for your phone caused evacuation failure. Stay outside once safe.";
                    break;
                default: return false;
            }
            actions.Add(new EvacuationActionRecord(previousStep, label, quality, recovery, feedback, FailureEvent));
            Revision++;
            return true;
        }

        private void Fail(EvacuationFailure failure)
        {
            FailureEvent = failure;
            Status = EvacuationStatus.Failed;
            Step = EvacuationStep.Finished;
        }

        public string BuildFeedback()
        {
            var lines = new List<string>();
            foreach (EvacuationActionRecord action in actions) lines.Add(action.LearningPoint);
            return string.Join("\n", lines);
        }
    }
}
