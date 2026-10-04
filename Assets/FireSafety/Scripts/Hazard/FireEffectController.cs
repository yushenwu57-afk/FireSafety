using UnityEngine;

namespace FireSafety.Hazard
{
    public sealed class FireEffectController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] fireParticleSystems;
        [SerializeField] private bool startActive = false;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (startActive)
            {
                StartFire();
            }
            else
            {
                StopFire();
            }
        }

        public void StartFire()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;

            if (fireParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < fireParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = fireParticleSystems[i];
                if (particleSystem != null)
                {
                    particleSystem.Play(true);
                }
            }
        }

        public void StopFire()
        {
            IsActive = false;

            if (fireParticleSystems == null)
            {
                return;
            }

            for (int i = 0; i < fireParticleSystems.Length; i++)
            {
                ParticleSystem particleSystem = fireParticleSystems[i];
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
