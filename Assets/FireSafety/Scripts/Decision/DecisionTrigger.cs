using FireSafety.Interaction;
using UnityEngine;

namespace FireSafety.Decision
{
    public sealed class DecisionTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InteractableObject interactableObject;
        [SerializeField] private DecisionManager decisionManager;

        [Header("Decision Content")]
        [SerializeField] private string decisionTitle = "Choose an action";
        [SerializeField, TextArea] private string decisionDescription =
            "What do you want to do?";
        [SerializeField] private string optionA = "Option A";
        [SerializeField] private string optionB = "Option B";
        [SerializeField] private string optionC = "Option C";

        private void OnEnable()
        {
            if (interactableObject != null)
            {
                interactableObject.Interacted += HandleInteracted;
            }
        }

        private void OnDisable()
        {
            if (interactableObject != null)
            {
                interactableObject.Interacted -= HandleInteracted;
            }
        }

        private void HandleInteracted(InteractableObject interactedObject)
        {
            if (decisionManager == null)
            {
                return;
            }

            DecisionData decision = new DecisionData(
                decisionTitle,
                decisionDescription,
                optionA,
                optionB,
                optionC);

            decisionManager.OpenDecision(decision);
        }
    }
}
