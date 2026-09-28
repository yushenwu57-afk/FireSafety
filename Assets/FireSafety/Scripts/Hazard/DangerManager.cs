using System;
using UnityEngine;

namespace FireSafety.Hazard
{
    public sealed class DangerManager : MonoBehaviour
    {
        [SerializeField] private DangerLevel initialLevel = DangerLevel.Low;

        public DangerLevel CurrentLevel { get; private set; }

        public event Action<DangerLevel, DangerLevel> DangerLevelChanged;

        private void Awake()
        {
            CurrentLevel = initialLevel;
        }

        public void SetDangerLevel(DangerLevel newLevel)
        {
            if (newLevel == CurrentLevel)
            {
                return;
            }

            DangerLevel previousLevel = CurrentLevel;
            CurrentLevel = newLevel;
            DangerLevelChanged?.Invoke(previousLevel, newLevel);
        }
    }
}
