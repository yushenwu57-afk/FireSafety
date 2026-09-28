using FireSafety.Interaction;
using TMPro;
using UnityEngine;

namespace FireSafety.UI
{
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private CanvasGroup promptCanvasGroup;
        [SerializeField] private TMP_Text promptText;

        private void OnEnable()
        {
            if (playerInteraction != null)
            {
                playerInteraction.CurrentTargetChanged += HandleCurrentTargetChanged;
                UpdatePrompt(playerInteraction.CurrentTarget);
            }
            else
            {
                HidePrompt();
            }
        }

        private void OnDisable()
        {
            if (playerInteraction != null)
            {
                playerInteraction.CurrentTargetChanged -= HandleCurrentTargetChanged;
            }

            HidePrompt();
        }

        private void HandleCurrentTargetChanged(
            InteractableObject previousTarget,
            InteractableObject newTarget)
        {
            UpdatePrompt(newTarget);
        }

        private void UpdatePrompt(InteractableObject target)
        {
            if (target == null || !target.InteractionEnabled)
            {
                HidePrompt();
                return;
            }

            promptText.text = target.InteractionPrompt;
            SetPromptVisible(true);
        }

        private void HidePrompt()
        {
            SetPromptVisible(false);
        }

        private void SetPromptVisible(bool isVisible)
        {
            promptCanvasGroup.alpha = isVisible ? 1f : 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }
}
