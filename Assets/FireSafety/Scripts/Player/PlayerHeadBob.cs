using FireSafety.Core;
using UnityEngine;

namespace FireSafety.Player
{
    public sealed class PlayerHeadBob : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;

        [Header("Head Bob")]
        [SerializeField, Min(0f)] private float verticalAmplitude = 0.03f;
        [SerializeField, Min(0f)] private float horizontalAmplitude = 0.015f;
        [SerializeField, Min(0f)] private float frequency = 1.8f;
        [SerializeField, Min(0f)] private float returnSpeed = 8f;
        [SerializeField, Min(0f)] private float movementThreshold = 0.1f;

        private Vector3 restingLocalPosition;
        private float bobPhase;
        private bool canBob;

        private void Awake()
        {
            restingLocalPosition = cameraTransform.localPosition;
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                canBob = gameManager.CurrentState == GameState.Exploration;
            }
            else
            {
                canBob = false;
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }
        }

        private void Update()
        {
            Vector3 horizontalVelocity = characterController.velocity;
            horizontalVelocity.y = 0f;

            bool isWalking = canBob
                && characterController.isGrounded
                && horizontalVelocity.magnitude > movementThreshold;

            if (isWalking)
            {
                bobPhase += Time.deltaTime * frequency * Mathf.PI * 2f;

                Vector3 bobOffset = new Vector3(
                    Mathf.Sin(bobPhase * 0.5f) * horizontalAmplitude,
                    Mathf.Sin(bobPhase) * verticalAmplitude,
                    0f);

                cameraTransform.localPosition = restingLocalPosition + bobOffset;
                return;
            }

            bobPhase = 0f;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                restingLocalPosition,
                returnSpeed * Time.deltaTime);
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            canBob = newState == GameState.Exploration;
        }
    }
}
