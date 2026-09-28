using FireSafety.Core;
using UnityEngine;

namespace FireSafety.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameManager gameManager;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.6f;
        [SerializeField, Min(0f)] private float acceleration = 10f;
        [SerializeField, Min(0f)] private float deceleration = 14f;
        [SerializeField] private float gravity = -22f;

        private CharacterController characterController;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private bool canMove;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                SetMovementEnabled(gameManager.CurrentState == GameState.Exploration);
            }
            else
            {
                SetMovementEnabled(false);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
            }

            SetMovementEnabled(false);
        }

        private void Update()
        {
            UpdateHorizontalVelocity();
            UpdateVerticalVelocity();

            Vector3 movement = horizontalVelocity + Vector3.up * verticalVelocity;
            characterController.Move(movement * Time.deltaTime);
        }

        private void UpdateHorizontalVelocity()
        {
            if (!canMove)
            {
                horizontalVelocity = Vector3.zero;
                return;
            }

            Vector2 input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 desiredVelocity =
                (transform.right * input.x + transform.forward * input.y) * walkSpeed;

            float changeRate = input.sqrMagnitude > 0f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(
                horizontalVelocity,
                desiredVelocity,
                changeRate * Time.deltaTime);
        }

        private void UpdateVerticalVelocity()
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
        }

        private void HandleStateChanged(GameState previousState, GameState newState)
        {
            SetMovementEnabled(newState == GameState.Exploration);
        }

        private void SetMovementEnabled(bool isEnabled)
        {
            canMove = isEnabled;

            if (!canMove)
            {
                horizontalVelocity = Vector3.zero;
            }
        }
    }
}
