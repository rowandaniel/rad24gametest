using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform playerCamera;
    [Tooltip("Sensitivity for mouse or gamepad look. Adjust as needed.")]
    public float lookSensitivity = 0.5f;
    
    [Header("Input Action References")]
    [Tooltip("Vector2 Action for WASD/Gamepad stick")]
    public InputActionReference moveAction;
    [Tooltip("Vector2 Action for Mouse Delta/Gamepad stick")]
    public InputActionReference lookAction;
    [Tooltip("Button Action for jumping")]
    public InputActionReference jumpAction;
    [Tooltip("Button Action for sprinting")]
    public InputActionReference sprintAction;
    [Tooltip("Button Action to toggle cursor lock (e.g., Escape)")]
    public InputActionReference toggleCursorAction;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float cameraPitch = 0f;
    private bool isCursorLocked = true;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        UpdateCursorState();
    }

    private void Update()
    {
        HandleCursorToggle();
        ApplyGravity();
        HandleMovement();
        HandleLook();
        HandleJump();
    }

    private void HandleCursorToggle()
    {
        if (toggleCursorAction != null && toggleCursorAction.action.WasPressedThisFrame())
        {
            isCursorLocked = !isCursorLocked;
            UpdateCursorState();
        }
    }

    private void UpdateCursorState()
    {
        if (isCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void HandleMovement()
    {
        if (moveAction == null) return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
        
        bool isSprinting = sprintAction != null && sprintAction.action.IsPressed();
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        // Prevent looking around if the cursor is unlocked
        if (lookAction == null || !isCursorLocked) return;

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        
        float lookX = lookInput.x * lookSensitivity;
        float lookY = lookInput.y * lookSensitivity;

        cameraPitch -= lookY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }

        transform.Rotate(Vector3.up * lookX);
    }

    private void ApplyGravity()
    {
        isGrounded = controller.isGrounded;
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (isGrounded && jumpAction != null && jumpAction.action.WasPressedThisFrame())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (lookAction != null) lookAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (sprintAction != null) sprintAction.action.Enable();
        if (toggleCursorAction != null) toggleCursorAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (lookAction != null) lookAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (sprintAction != null) sprintAction.action.Disable();
        if (toggleCursorAction != null) toggleCursorAction.action.Disable();
    }
}