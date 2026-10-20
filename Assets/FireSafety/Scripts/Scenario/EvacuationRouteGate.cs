namespace FireSafety.Scenario
{
    // One arrival per round; spatial checks are independent of Unity for validation.
    public sealed class EvacuationRouteGate
    {
        private enum Progress { NotStarted, Walking, Arrived }
        private Progress progress;
        private readonly EvacuationStep requiredStep;

        public EvacuationRouteGate(EvacuationStep requiredStep = EvacuationStep.SafeRoute)
        {
            this.requiredStep = requiredStep;
        }
        public bool IsWaiting => progress == Progress.Walking;

        public bool Begin(EvacuationStep step)
        {
            if (progress != Progress.NotStarted || step != requiredStep) return false;
            progress = Progress.Walking;
            return true;
        }

        public bool TryArrive(float horizontalDistanceSquared, float verticalDistance,
            float radius, float heightTolerance)
        {
            if (!IsWaiting || horizontalDistanceSquared < 0 || radius <= 0 || heightTolerance < 0
                || horizontalDistanceSquared > radius * radius
                || System.Math.Abs(verticalDistance) > heightTolerance) return false;
            progress = Progress.Arrived;
            return true;
        }

        public void Reset() { progress = Progress.NotStarted; }
    }
}
