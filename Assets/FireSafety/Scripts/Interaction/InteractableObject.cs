using System;
using UnityEngine;

namespace FireSafety.Interaction
{
    public sealed class InteractableObject : MonoBehaviour
    {
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private bool interactionEnabled = true;

        public string InteractionPrompt => interactionPrompt;
        public bool InteractionEnabled => interactionEnabled;

        public event Action<InteractableObject> Interacted;

        public void Interact()
        {
            if (!interactionEnabled)
            {
                return;
            }

            Interacted?.Invoke(this);
        }
    }
}
