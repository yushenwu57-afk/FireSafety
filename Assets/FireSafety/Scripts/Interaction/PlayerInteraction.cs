using System;
using FireSafety.Core;
using UnityEngine;

namespace FireSafety.Interaction
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Transform cameraTransform;

        [Header("Interaction")]
        [SerializeField, Min(0f)] private float interactionDistance = 2.5f;
        [SerializeField] private LayerMask interactionLayerMask;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;

        private bool canInteract;

        public InteractableObject CurrentTarget { get; private set; }

        public event Action<InteractableObject, InteractableObject> CurrentTargetChanged;

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                SetInteractionEnabled(gameManager.CurrentState == GameState.Exploration);
            }
            else
            {
                SetInteractionEnabled(false);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            SetInteractionEnabled(false);
        }

        private void Update()
        {
            if (!canInteract)
            {
                return;
            }

            UpdateCurrentTarget();

            if (CurrentTarget != null
                && CurrentTarget.InteractionEnabled
                && Input.GetKeyDown(interactionKey))
            {
                CurrentTarget.Interact();
            }
        }

        private void UpdateCurrentTarget()
        {
            InteractableObject detectedTarget = null;

            if (Physics.Raycast(
                cameraTransform.position,
                cameraTransform.forward,
                out RaycastHit hit,
                interactionDistance,
                interactionLayerMask))
            {
                InteractableObject interactable =
                    hit.collider.GetComponentInParent<InteractableObject>();

                if (interactable != null && interactable.InteractionEnabled)
                {
                    detectedTarget = interactable;
                }
            }

            SetCurrentTarget(detectedTarget);
        }

        private void SetCurrentTarget(InteractableObject newTarget)
        {
            if (CurrentTarget == newTarget)
            {
                return;
            }

            InteractableObject previousTarget = CurrentTarget;
            CurrentTarget = newTarget;
            CurrentTargetChanged?.Invoke(previousTarget, newTarget);
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            SetInteractionEnabled(newState == GameState.Exploration);
        }

        private void SetInteractionEnabled(bool isEnabled)
        {
            canInteract = isEnabled;

            if (!canInteract)
            {
                SetCurrentTarget(null);
            }
        }
    }
}
