using FireSafety.Hazard;
using TMPro;
using UnityEngine;

namespace FireSafety.UI
{
    public sealed class DangerLevelUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DangerManager dangerManager;
        [SerializeField] private TMP_Text dangerText;

        private void OnEnable()
        {
            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged += HandleDangerLevelChanged;
                UpdateDangerText(dangerManager.CurrentLevel);
            }
        }

        private void OnDisable()
        {
            if (dangerManager != null)
            {
                dangerManager.DangerLevelChanged -= HandleDangerLevelChanged;
            }
        }

        private void HandleDangerLevelChanged(
            DangerLevel previousLevel,
            DangerLevel newLevel)
        {
            UpdateDangerText(newLevel);
        }

        private void UpdateDangerText(DangerLevel level)
        {
            dangerText.text = "Danger: " + level;
        }
    }
}
