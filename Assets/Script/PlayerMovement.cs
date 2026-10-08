using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundStickForce = -2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
        UpdateAnimator();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                input.x -= 1f;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1f;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1f;

            if (Keyboard.current.wKey.isPressed)
                input.y += 1f;

            input = Vector2.ClampMagnitude(input, 1f);
        }

        bool sprinting =
            Keyboard.current != null &&
            Keyboard.current.leftShiftKey.isPressed &&
            input.y > 0.1f;

        if (animator != null)
        {
            animator.SetBool("IsSprinting", sprinting);
        }

        float currentSpeed = sprinting ? runSpeed : walkSpeed;

        Vector3 movement = Vector3.zero;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            movement = forward * input.y + right * input.x;
        }

        // Prevent tiny unwanted movement.
        if (movement.sqrMagnitude > 0.001f)
        {
            movement.Normalize();

            // Rotate character toward actual movement direction.
            Quaternion targetRotation =
                Quaternion.LookRotation(movement);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        controller.Move(
            movement * currentSpeed * Time.deltaTime
        );
    }

    private void HandleGravity()
    {
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = groundStickForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                moveX -= 1f;

            if (Keyboard.current.dKey.isPressed)
                moveX += 1f;

            if (Keyboard.current.sKey.isPressed)
                moveY -= 1f;

            if (Keyboard.current.wKey.isPressed)
                moveY += 1f;
        }

        animator.SetFloat(
            "MoveX",
            moveX,
            0.1f,
            Time.deltaTime
        );

        animator.SetFloat(
            "MoveY",
            moveY,
            0.1f,
            Time.deltaTime
        );
    }
}