using UnityEngine;

namespace FireSafety.Hazard
{
    public sealed class SmokeEffectController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject smokeRoot;
        [SerializeField] private ParticleSystem[] smokeParticleSystems;

        [Header("Startup")]
        [SerializeField] private bool startActive = false;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (startActive)
            {
                StartSmoke();
            }
            else
            {
                StopSmoke();
            }
        }

        public void StartSmoke()
        {
            if (smokeRoot != null && !smokeRoot.activeSelf)
            {
                smokeRoot.SetActive(true);
            }

            if (IsActive)
            {
                return;
            }

            IsActive = true;

            if (smokeParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < smokeParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = smokeParticleSystems[i];
                if (particleSystem != null)
                {
                    particleSystem.Play(true);
                }
            }
        }

        public void StopSmoke()
        {
            IsActive = false;

            if (smokeParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < smokeParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = smokeParticleSystems[i];
                if (particleSystem != null)
                {
                    particleSystem.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }
}
