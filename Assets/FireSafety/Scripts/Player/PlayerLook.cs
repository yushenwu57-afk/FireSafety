using FireSafety.Core;
using UnityEngine;

namespace FireSafety.Player
{
    public sealed class PlayerLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraTransform;

        [Header("Look")]
        [SerializeField, Min(0f)] private float mouseSensitivity = 2f;
        [SerializeField] private float minimumPitch = -80f;
        [SerializeField] private float maximumPitch = 80f;

        private float pitch;
        private bool canLook;

        private void Awake()
        {
            float initialPitch = cameraTransform.localEulerAngles.x;
            pitch = initialPitch > 180f ? initialPitch - 360f : initialPitch;
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                SetLookEnabled(gameManager.CurrentState == GameState.Exploration);
            }
            else
            {
                SetLookEnabled(false);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            SetLookEnabled(false);
        }

        private void Update()
        {
            if (!canLook)
            {
                return;
            }

            float yawInput = Input.GetAxis("Mouse X") * mouseSensitivity;
            float pitchInput = Input.GetAxis("Mouse Y") * mouseSensitivity;

            playerBody.Rotate(Vector3.up * yawInput);

            pitch = Mathf.Clamp(pitch - pitchInput, minimumPitch, maximumPitch);
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            SetLookEnabled(newState == GameState.Exploration);
        }

        private void SetLookEnabled(bool isEnabled)
        {
            canLook = isEnabled;
            Cursor.lockState = canLook ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !canLook;
        }
    }
}
