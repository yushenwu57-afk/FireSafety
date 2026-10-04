using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class TimedSmokeEvent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioProgression scenarioProgression;
        [SerializeField] private SmokeEffectController smokeEffectController;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float smokeStartTime = 16f;

        private bool hasTriggered;

        private void Update()
        {
            if (hasTriggered
                || scenarioProgression == null
                || smokeEffectController == null)
            {
                return;
            }

            if (scenarioProgression.ElapsedScenarioTime < smokeStartTime)
            {
                return;
            }

            hasTriggered = true;
            smokeEffectController.StartSmoke();
        }

        public void ResetEvent()
        {
            hasTriggered = false;

            if (smokeEffectController != null)
            {
                smokeEffectController.StopSmoke();
            }
        }
    }
}
