using System;
using FireSafety.Evaluation;
using FireSafety.Scenario;

// Standalone executable using the same production domain model; no Unity dependency.
public static class EvacuationSessionChecks
{
    private static int checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception("FAILED: " + name);
        checks++;
    }
    private static void Choose(EvacuationSession session, int choice)
    {
        int revision = session.Revision;
        Check(session.TrySelect(revision, choice), "accept choice");
        int count = session.RecordedActions.Count;
        Check(!session.TrySelect(revision, choice), "reject duplicate/stale callback");
        Check(session.RecordedActions.Count == count, "duplicate does not record");
    }
    public static void Main()
    {
        var s = new EvacuationSession();
        Check(!s.HasFinalRank && !s.FinalRank.HasValue, "unfinished/exit is not C");
        Check(!s.TrySelect(s.Revision, -1) && !s.TrySelect(s.Revision, 2), "invalid options");
        Choose(s, 0); Choose(s, 0); Choose(s, 0);
        Check(s.FinalRank == PerformanceRank.S && s.SuccessfullyEvacuated, "correct S");
        Check(s.RecordedActions.Count == 3 && s.OrdinaryMistakeCount == 0 && s.SeriousMistakeCount == 0, "S record");
        Check(!s.TrySelect(s.Revision, 0), "terminal choice ignored");
        s.Reset(); Choose(s, 1); Choose(s, 0); Choose(s, 0); Choose(s, 0);
        Check(s.FinalRank == PerformanceRank.A && s.OrdinaryMistakeCount == 1 && s.SeriousMistakeCount == 0, "ordinary A retained after recovery");
        Check(s.RecordedActions[1].IsRecoveryAction && s.BuildFeedback().Contains("Collecting valuables"), "A feedback from actual choices");
        s.Reset(); Choose(s, 0); Choose(s, 1); Choose(s, 0); Choose(s, 0);
        Check(s.FinalRank == PerformanceRank.B && s.SeriousMistakeCount == 1 && s.SuccessfullyEvacuated, "serious B after recovery");
        Check(s.BuildFeedback().Contains("serious mistake remains recorded"), "B feedback");
        s.Reset(); Choose(s, 0); Choose(s, 1); Choose(s, 1);
        Check(s.FinalRank == PerformanceRank.C && s.FailureEvent == EvacuationFailure.EnteredKitchen && !s.SuccessfullyEvacuated, "kitchen C");
        s.Reset(); Choose(s, 0); Choose(s, 0); Choose(s, 1);
        Check(s.FinalRank == PerformanceRank.C && s.FailureEvent == EvacuationFailure.ReenteredForPhone, "phone C");
        Check(!s.BuildFeedback().Contains("asked for help reporting the fire"), "failure does not praise unchosen reporting");
        int stale = s.Revision;
        s.Reset();
        Check(s.RecordedActions.Count == 0 && s.OrdinaryMistakeCount == 0 && s.SeriousMistakeCount == 0 && s.FailureEvent == EvacuationFailure.None && !s.HasFinalRank, "replay clears all data");
        Check(!s.TrySelect(stale, 0), "old round callback rejected");
        var independent = new EvacuationSession();
        Choose(s, 1);
        Check(independent.RecordedActions.Count == 0 && independent.OrdinaryMistakeCount == 0, "round isolation");
        Check(EvacuationSession.CalculateRank(EvacuationStatus.Completed, EvacuationFailure.None, 2, 0) == PerformanceRank.B, "two ordinary B");
        Check(EvacuationSession.CalculateRank(EvacuationStatus.Completed, EvacuationFailure.None, 7, 0) == PerformanceRank.B, "many ordinary B");
        Check(EvacuationSession.CalculateRank(EvacuationStatus.Completed, EvacuationFailure.None, 1, 1) == PerformanceRank.B, "B before A");
        Check(EvacuationSession.CalculateRank(EvacuationStatus.Failed, EvacuationFailure.None, 0, 0) == PerformanceRank.C, "unsuccessful end C");
        Check(EvacuationSession.CalculateRank(EvacuationStatus.Completed, EvacuationFailure.EnteredKitchen, 2, 1) == PerformanceRank.C, "C priority");
        var gate = new EvacuationRouteGate();
        Check(!gate.TryArrive(0, 0, .9f, .75f), "arrival before decision one ignored");
        Check(!gate.Begin(EvacuationStep.LeaveNow), "cannot walk before first decision");
        Check(!gate.Begin(EvacuationStep.LeaveAfterDelay), "ordinary error requires correction before walking");
        Check(gate.Begin(EvacuationStep.SafeRoute) && gate.IsWaiting, "safe first decision starts walk");
        Check(!gate.Begin(EvacuationStep.SafeRoute), "walk cannot start twice");
        Check(!gate.TryArrive(4, 0, .9f, .75f) && gate.IsWaiting, "far from target does not open decision two");
        Check(!gate.TryArrive(0, 2.5f, .9f, .75f) && gate.IsWaiting, "wrong floor cannot trigger");
        Check(gate.TryArrive(.25f, .22f, .9f, .75f) && !gate.IsWaiting, "near target opens decision two");
        Check(!gate.TryArrive(0, 0, .9f, .75f), "arrival triggers once");
        Check(!gate.Begin(EvacuationStep.SafeRoute), "arrived gate cannot restart in same round");
        gate.Reset();
        var delayed = new EvacuationSession();
        Choose(delayed, 1);
        Check(!gate.Begin(delayed.Step), "take valuables still waits for correction");
        Choose(delayed, 0);
        Check(gate.Begin(delayed.Step), "correction then starts walking");
        Check(!delayed.HasFinalRank && delayed.OrdinaryMistakeCount == 1, "walking keeps error without settlement");
        Check(gate.TryArrive(.1f, -.2f, .9f, .75f), "replay gate accepts arrival again");
        Choose(delayed, 0); Choose(delayed, 0);
        Check(delayed.FinalRank == PerformanceRank.A, "walking leaves grading unchanged");
        var kitchenGate = new EvacuationRouteGate(EvacuationStep.ReturnFromKitchen);
        Check(!kitchenGate.TryArrive(0, 0, 1.5f, .75f), "kitchen arrival before unsafe choice ignored");
        Check(!kitchenGate.Begin(EvacuationStep.SafeRoute), "second main decision does not arm kitchen followup");
        Check(!kitchenGate.Begin(EvacuationStep.Outside), "safe exit skips kitchen followup");
        var kitchenRound = new EvacuationSession();
        Choose(kitchenRound, 0); Choose(kitchenRound, 1);
        Check(kitchenGate.Begin(kitchenRound.Step), "return choice starts kitchen traversal");
        Check(kitchenRound.SeriousMistakeCount == 1 && !kitchenRound.HasFinalRank, "serious error recorded before arrival without failure");
        int recordsBeforeArrival = kitchenRound.RecordedActions.Count;
        Check(!kitchenGate.TryArrive(16, 0, 1.5f, .75f), "far from furniture no followup");
        Check(!kitchenGate.TryArrive(0, 2.5f, 1.5f, .75f), "upstairs cannot trigger downstairs furniture");
        Check(kitchenGate.TryArrive(1, .2f, 1.5f, .75f), "near furniture opens kitchen followup");
        Check(kitchenRound.RecordedActions.Count == recordsBeforeArrival && kitchenRound.SeriousMistakeCount == 1, "walking and arrival do not duplicate error");
        Check(!kitchenGate.TryArrive(0, 0, 1.5f, .75f), "kitchen followup triggers once");
        Choose(kitchenRound, 0); Choose(kitchenRound, 0);
        Check(kitchenRound.FinalRank == PerformanceRank.B, "arrival and recovery retain B");
        kitchenGate.Reset(); kitchenRound.Reset();
        Check(!kitchenGate.IsWaiting, "replay clears kitchen gate");
        Choose(kitchenRound, 0); Choose(kitchenRound, 1);
        Check(kitchenGate.Begin(kitchenRound.Step) && kitchenGate.TryArrive(.5f, 0, 1.5f, .75f), "new round kitchen gate works again");
        Choose(kitchenRound, 1);
        Check(kitchenRound.FinalRank == PerformanceRank.C && kitchenRound.FailureEvent == EvacuationFailure.EnteredKitchen, "continued entry after arrival still C");
        var thirdGate = new EvacuationRouteGate(EvacuationStep.Outside);
        Check(!thirdGate.TryArrive(0, 0, .45f, .75f), "early carpet arrival cannot open third decision");
        Check(!thirdGate.Begin(EvacuationStep.SafeRoute), "decision two does not start third gate");
        Check(!thirdGate.Begin(EvacuationStep.ReturnFromKitchen), "kitchen followup does not start third gate");
        var thirdRound = new EvacuationSession();
        Choose(thirdRound, 0); Choose(thirdRound, 0);
        Check(thirdGate.Begin(thirdRound.Step), "safe exit starts carpet traversal");
        int recordsBeforeThird = thirdRound.RecordedActions.Count;
        Check(!thirdGate.TryArrive(.8f, 0, .45f, .75f), "kitchen furniture center is outside third radius");
        Check(!thirdGate.TryArrive(0, 2.5f, .45f, .75f), "wrong floor cannot open third decision");
        Check(thirdGate.TryArrive(.1f, .1f, .45f, .75f), "carpet arrival opens third decision");
        Check(!thirdGate.TryArrive(0, 0, .45f, .75f), "third decision triggers once");
        Check(thirdRound.RecordedActions.Count == recordsBeforeThird && !thirdRound.HasFinalRank, "third arrival adds no choice or settlement");
        Choose(thirdRound, 0);
        Check(thirdRound.FinalRank == PerformanceRank.S, "safe completion after carpet still S");
        thirdGate.Reset(); thirdRound.Reset();
        Choose(thirdRound, 0); Choose(thirdRound, 1); Choose(thirdRound, 0);
        Check(thirdGate.Begin(thirdRound.Step), "kitchen recovery also starts carpet traversal");
        Check(thirdGate.TryArrive(0, .2f, .45f, .75f), "replay third arrival works");
        Choose(thirdRound, 0);
        Check(thirdRound.FinalRank == PerformanceRank.B, "third arrival preserves serious error B");
        thirdGate.Reset(); thirdRound.Reset();
        Choose(thirdRound, 0); Choose(thirdRound, 0);
        Check(thirdGate.Begin(thirdRound.Step) && thirdGate.TryArrive(0, 0, .45f, .75f), "phone failure path reaches carpet");
        Choose(thirdRound, 1);
        Check(thirdRound.FinalRank == PerformanceRank.C, "phone reentry remains C after carpet");
        // Traverse every reachable path to check terminal rank, choice count and isolation.
        Walk(new int[0]);
        Console.WriteLine("PASS: " + checks + " checks; all 10 terminal paths traversed.");
    }
    private static void Walk(int[] path)
    {
        var s = new EvacuationSession();
        foreach (int option in path) Check(s.TrySelect(s.Revision, option), "reachable path");
        if (s.HasFinalRank)
        {
            Check(s.RecordedActions.Count == path.Length, "path records");
            Check(s.FinalRank == (s.FailureEvent != EvacuationFailure.None ? PerformanceRank.C : s.SeriousMistakeCount > 0 ? PerformanceRank.B : s.OrdinaryMistakeCount > 0 ? PerformanceRank.A : PerformanceRank.S), "path rank");
            return;
        }
        Check(!s.FinalRank.HasValue, "no premature settlement");
        for (int i = 0; i < s.OptionCount; i++)
        {
            var next = new int[path.Length + 1];
            Array.Copy(path, next, path.Length);
            next[path.Length] = i;
            Walk(next);
        }
    }
}
