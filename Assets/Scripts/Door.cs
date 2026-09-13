using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 180f;
    [SerializeField] private bool disableColliderWhenOpen = true;

    [Header("Passage")]
    [SerializeField] private bool clearPassageForInteractor = true;
    [SerializeField] private Vector3 passagePadding = new Vector3(0.8f, 0.4f, 1.2f);

    private Quaternion closedRotation;
    private Quaternion targetRotation;
    private Collider[] doorColliders;
    private readonly List<Collider> ignoredPassageColliders = new List<Collider>();
    private Collider interactorCollider;
    private Transform interactorTransform;
    private Vector3 activePassageCenter;
    private Vector3 activePassageHalfExtents;
    private Quaternion activePassageRotation;
    private Bounds activePassageBounds;
    private bool hasActivePassage;
    private bool isOpen;

    private void Awake()
    {
        doorColliders = GetComponentsInChildren<Collider>();
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;
    }

    private void Update()
    {
        transform.localRotation = Quaternion.RotateTowards(
            transform.localRotation,
            targetRotation,
            openSpeed * Time.deltaTime);

        if (isOpen && interactorCollider != null)
        {
            if (IsInteractorNearActivePassage())
            {
                if (ignoredPassageColliders.Count == 0)
                {
                    ClearPassageCollisions(interactorTransform);
                }
            }
            else if (ignoredPassageColliders.Count > 0)
            {
                RestorePassageCollisions(false);
            }
        }
    }

    public void Toggle(Transform interactor)
    {
        isOpen = !isOpen;

        if (!isOpen)
        {
            targetRotation = closedRotation;
            SetCollidersEnabled(true);
            RestorePassageCollisions(true);

            return;
        }

        ClearPassageCollisions(interactor);

        if (disableColliderWhenOpen)
        {
            SetCollidersEnabled(false);
        }

        float direction = 1f;
        if (interactor != null)
        {
            Vector3 toInteractor = interactor.position - transform.position;
            direction = Vector3.Dot(transform.right, toInteractor) >= 0f ? -1f : 1f;
        }

        targetRotation = closedRotation * Quaternion.Euler(0f, openAngle * direction, 0f);
    }

    private void SetCollidersEnabled(bool enabled)
    {
        if (doorColliders == null)
        {
            return;
        }

        for (int i = 0; i < doorColliders.Length; i++)
        {
            doorColliders[i].enabled = enabled;
        }
    }

    private void ClearPassageCollisions(Transform interactor)
    {
        if (!clearPassageForInteractor || interactor == null)
        {
            return;
        }

        interactorCollider = interactor.GetComponent<Collider>();
        interactorTransform = interactor;
        if (interactorCollider == null)
        {
            interactorTransform = null;
            return;
        }

        Bounds passageBounds = hasActivePassage ? activePassageBounds : GetDoorBounds();
        if (!hasActivePassage)
        {
            activePassageBounds = passageBounds;
            activePassageCenter = passageBounds.center;
            activePassageHalfExtents = (passageBounds.size + passagePadding) * 0.5f;
            activePassageRotation = transform.rotation;
            hasActivePassage = true;
        }

        Collider[] nearbyColliders = Physics.OverlapBox(
            activePassageCenter,
            activePassageHalfExtents,
            activePassageRotation,
            ~0,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < nearbyColliders.Length; i++)
        {
            Collider nearbyCollider = nearbyColliders[i];
            if (ShouldIgnorePassageCollider(nearbyCollider, passageBounds))
            {
                Physics.IgnoreCollision(interactorCollider, nearbyCollider, true);
                if (!ignoredPassageColliders.Contains(nearbyCollider))
                {
                    ignoredPassageColliders.Add(nearbyCollider);
                }
            }
        }
    }

    private Bounds GetDoorBounds()
    {
        if (doorColliders == null || doorColliders.Length == 0)
        {
            return new Bounds(transform.position, Vector3.one);
        }

        Bounds bounds = doorColliders[0].bounds;
        for (int i = 1; i < doorColliders.Length; i++)
        {
            bounds.Encapsulate(doorColliders[i].bounds);
        }

        return bounds;
    }

    private bool ShouldIgnorePassageCollider(Collider nearbyCollider, Bounds passageBounds)
    {
        if (nearbyCollider == null || nearbyCollider == interactorCollider)
        {
            return false;
        }

        for (int i = 0; i < doorColliders.Length; i++)
        {
            if (nearbyCollider == doorColliders[i])
            {
                return false;
            }
        }

        Bounds nearbyBounds = nearbyCollider.bounds;
        bool isFloorOrCeiling = nearbyBounds.max.y < passageBounds.min.y + 0.25f ||
            nearbyBounds.min.y > passageBounds.max.y - 0.25f;

        return !isFloorOrCeiling;
    }

    private bool IsInteractorNearActivePassage()
    {
        Vector3 localOffset = Quaternion.Inverse(activePassageRotation) *
            (interactorCollider.bounds.center - activePassageCenter);
        Vector3 restorePadding = new Vector3(0.75f, 1f, 0.75f);
        Vector3 restoreExtents = activePassageHalfExtents + restorePadding;

        return Mathf.Abs(localOffset.x) <= restoreExtents.x &&
            Mathf.Abs(localOffset.y) <= restoreExtents.y &&
            Mathf.Abs(localOffset.z) <= restoreExtents.z;
    }

    private void RestorePassageCollisions(bool forgetInteractor)
    {
        if (interactorCollider == null)
        {
            ignoredPassageColliders.Clear();
            return;
        }

        for (int i = 0; i < ignoredPassageColliders.Count; i++)
        {
            Collider ignoredCollider = ignoredPassageColliders[i];
            if (ignoredCollider != null)
            {
                Physics.IgnoreCollision(interactorCollider, ignoredCollider, false);
            }
        }

        ignoredPassageColliders.Clear();
        if (forgetInteractor)
        {
            interactorCollider = null;
            interactorTransform = null;
            activePassageCenter = Vector3.zero;
            activePassageHalfExtents = Vector3.zero;
            activePassageRotation = Quaternion.identity;
            activePassageBounds = new Bounds();
            hasActivePassage = false;
        }
    }

    private void OnDisable()
    {
        RestorePassageCollisions(true);
    }
}
