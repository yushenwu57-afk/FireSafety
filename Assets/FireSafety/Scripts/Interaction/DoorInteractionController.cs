using UnityEngine;

namespace FireSafety.Interaction
{
    public sealed class DoorInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InteractableObject interactableObject;
        [SerializeField] private Transform doorHinge;

        [Header("Door Rotation")]
        [SerializeField] private float closedLocalY;
        [SerializeField] private float openLocalY;
        [SerializeField, Min(0f)] private float rotationSpeed = 120f;
        [SerializeField] private bool startsOpen;

        private bool isOpen;
        private float targetLocalY;

        private void Awake()
        {
            isOpen = startsOpen;
            targetLocalY = isOpen ? openLocalY : closedLocalY;
            SetDoorLocalY(targetLocalY);
        }

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

        private void Update()
        {
            if (doorHinge == null)
            {
                return;
            }

            Vector3 localEulerAngles = doorHinge.localEulerAngles;
            float nextLocalY = Mathf.MoveTowardsAngle(
                localEulerAngles.y,
                targetLocalY,
                rotationSpeed * Time.deltaTime);

            if (Mathf.Approximately(localEulerAngles.y, nextLocalY))
            {
                return;
            }

            localEulerAngles.y = nextLocalY;
            doorHinge.localEulerAngles = localEulerAngles;
        }

        private void HandleInteracted(InteractableObject interactedObject)
        {
            isOpen = !isOpen;
            targetLocalY = isOpen ? openLocalY : closedLocalY;
        }

        private void SetDoorLocalY(float localY)
        {
            if (doorHinge == null)
            {
                return;
            }

            Vector3 localEulerAngles = doorHinge.localEulerAngles;
            localEulerAngles.y = localY;
            doorHinge.localEulerAngles = localEulerAngles;
        }
    }
}
