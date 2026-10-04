using FireSafety.Hazard;
using UnityEngine;

namespace FireSafety.Scenario
{
    public sealed class TimedFireEvent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ScenarioProgression scenarioProgression;
        [SerializeField] private FireEffectController fireEffectController;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float fireStartTime = 12f;

        private bool hasTriggered;

        private void Update()
        {
            if (hasTriggered
                || scenarioProgression == null
                || fireEffectController == null)
            {
                return;
            }

            if (scenarioProgression.ElapsedScenarioTime < fireStartTime)
            {
                return;
            }

            hasTriggered = true;
            fireEffectController.StartFire();
        }

        public void ResetEvent()
        {
            hasTriggered = false;

            if (fireEffectController != null)
            {
                fireEffectController.StopFire();
            }
        }
    }
}
