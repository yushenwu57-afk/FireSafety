using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class player : MonoBehaviour
{
    private const float DefaultCameraHeight = 1.35f;
    private const float MinimumStandingCameraHeight = 1.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpHeight = 1.4f;
    [SerializeField] private float gravity = -20f;

    [Header("Look")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float cameraHeight = DefaultCameraHeight;
    [SerializeField] private float cameraFieldOfView = 65f;
    [SerializeField] private float cameraNearClipPlane = 0.05f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Body")]
    [SerializeField] private bool hidePlayerModel = true;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactLayers = ~0;
    [SerializeField] private float selectionDistance = 3f;

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float cameraPitch;
    private ObservableContext hoveredContext;
    private ActionContext hoveredActionContext;
    private HighlightableObject hoveredHighlight;

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            playerCamera = childCamera != null ? childCamera.transform : Camera.main != null ? Camera.main.transform : null;
        }

        if (playerCamera != null && playerCamera.parent != transform)
        {
            playerCamera.SetParent(transform);
        }

        if (playerCamera != null)
        {
            cameraHeight = Mathf.Max(cameraHeight, MinimumStandingCameraHeight);
            ApplyCameraPosition();
            playerCamera.localRotation = Quaternion.identity;

            Camera cameraComponent = playerCamera.GetComponent<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.fieldOfView = cameraFieldOfView;
                cameraComponent.nearClipPlane = cameraNearClipPlane;
            }
        }

        if (hidePlayerModel)
        {
            MeshRenderer playerRenderer = GetComponent<MeshRenderer>();
            if (playerRenderer != null)
            {
                playerRenderer.enabled = false;
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        ApplyCameraPosition();
    }

    private void Update()
    {
        if (GameHud.IsMenuOpen || GameHud.IsActionPanelOpen)
        {
            return;
        }

        LookAround();
        Move();
        UpdateSelection();
        Interact();

        if (Input.GetMouseButtonDown(0))
        {
            if (InspectHoveredObject())
            {
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void LookAround()
    {
        if (playerCamera == null)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move()
    {
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        controller.Move(moveDirection * speed * Time.deltaTime);

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void Interact()
    {
        if (playerCamera == null || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        Door door = hit.collider.GetComponentInParent<Door>();
        if (door == null && hit.collider.gameObject.name.Contains("Door"))
        {
            door = hit.collider.gameObject.AddComponent<Door>();
        }

        if (door != null)
        {
            door.Toggle(transform);
        }
    }

    private void UpdateSelection()
    {
        ObservableContext context = null;
        ActionContext actionContext = null;
        HighlightableObject highlight = null;
        GetLookedAtContexts(out context, out actionContext, out highlight);

        if (context == hoveredContext && actionContext == hoveredActionContext && highlight == hoveredHighlight)
        {
            return;
        }

        if (hoveredHighlight != null)
        {
            hoveredHighlight.SetHighlighted(false);
        }

        hoveredContext = context;
        hoveredActionContext = actionContext;
        hoveredHighlight = highlight;

        if (hoveredHighlight != null)
        {
            hoveredHighlight.SetHighlighted(true);
        }

        GameHud.ActiveHud?.HideContextPrompt();
        GameHud.ActiveHud?.HideActionPanel();
    }

    private bool InspectHoveredObject()
    {
        if (hoveredContext == null && hoveredActionContext == null)
        {
            return false;
        }

        if (hoveredContext != null)
        {
            GameHud.ActiveHud?.ShowContextPrompt(hoveredContext);
        }

        if (hoveredActionContext != null)
        {
            GameHud.ActiveHud?.ShowActionPanel(hoveredActionContext);
        }

        return true;
    }

    private void GetLookedAtContexts(
        out ObservableContext context,
        out ActionContext actionContext,
        out HighlightableObject highlight)
    {
        context = null;
        actionContext = null;
        highlight = null;

        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, selectionDistance, interactLayers, QueryTriggerInteraction.Ignore))
        {
            return;
        }

        context = hit.collider.GetComponentInParent<ObservableContext>();
        actionContext = hit.collider.GetComponentInParent<ActionContext>();
        highlight = hit.collider.GetComponentInParent<HighlightableObject>();
    }

    private void ApplyCameraPosition()
    {
        if (playerCamera == null)
        {
            return;
        }

        float standingCameraHeight = Mathf.Max(cameraHeight, MinimumStandingCameraHeight);
        playerCamera.localPosition = new Vector3(0f, standingCameraHeight, 0f);
    }
}
