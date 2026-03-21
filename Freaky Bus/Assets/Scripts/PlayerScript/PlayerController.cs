using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float airControlMultiplier = 0.4f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float gravityMultiplier = 2.5f;
    [SerializeField] private float fallMultiplier = 3.5f;
    [SerializeField] private float jumpCooldown = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [SerializeField] private Transform groundCheckOrigin;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float slopeRayLength = 1.5f;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    [Header("Mobile UI")]
    [SerializeField] private Joystick moveJoystick;
    [SerializeField] private UnityEngine.UI.Button jumpButton;

    private Rigidbody rb;
    private CapsuleCollider col;

    private Vector2 moveInput;
    private Vector2 smoothedInput;
    private bool jumpRequested;
    private float jumpBufferTimer;
    private float jumpBufferWindow = 0.15f;
    private bool isGrounded;
    private bool wasGrounded;
    private float jumpCooldownTimer;
    private float jumpGroundGraceTimer;
    private float currentSpeed;

    private RaycastHit slopeHit;
    private bool isOnSlope;
    private bool mobileJumpPressed;

    private PlayerInputActions inputActions;

    #region Netcode

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;

        inputActions = new PlayerInputActions();

        if (!IsOwner)
        {
            // Disable camera and input for non-owners
            if (cameraController != null)
                cameraController.enabled = false;

            rb.isKinematic = true;
            enabled = false;
            return;
        }

        // Owner only — enable input
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;

        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnMobileJump);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;

        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(OnMobileJump);
    }

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null) return;
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;

        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnMobileJump);
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;

        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(OnMobileJump);
    }

    private void Update()
    {
        if (!IsOwner) return;

        CheckGround();
        SmoothInput();
        TickJumpCooldown();
        TickJumpBuffer();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        ApplyMovement();
        ApplyCustomGravity();
        HandleJump();
    }

    #endregion

    #region Input Callbacks

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpBufferTimer = jumpBufferWindow;
        jumpRequested = true;
    }

    private void OnMobileJump()
    {
        mobileJumpPressed = true;
    }

    #endregion

    #region Input Smoothing

    private void SmoothInput()
    {
        Vector2 rawInput = moveInput;
        if (moveJoystick != null)
        {
            Vector2 joyInput = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
            if (joyInput.magnitude > rawInput.magnitude)
                rawInput = joyInput;
        }

        smoothedInput = Vector2.Lerp(smoothedInput, rawInput, Time.deltaTime * 20f);
    }

    #endregion

    #region Ground Check

    private void CheckGround()
    {
        wasGrounded = isGrounded;

        Vector3 origin = groundCheckOrigin != null
            ? groundCheckOrigin.position
            : transform.position + Vector3.down * (col.height * 0.5f - col.radius);

        isGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        isOnSlope = CheckSlope();

        if (!wasGrounded && isGrounded)
            OnLanded();
    }

    private bool CheckSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, slopeRayLength, groundLayer))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0f;
        }
        return false;
    }

    private void OnLanded() { }

    #endregion

    #region Movement

    private void ApplyMovement()
    {
        Vector2 input = smoothedInput;

        float yaw = cameraController != null
            ? cameraController.YawAngle
            : transform.eulerAngles.y;

        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;

        Vector3 desiredDirection = (forward * input.y + right * input.x).normalized;

        float accel = input.magnitude > 0.01f ? acceleration : deceleration;
        if (!isGrounded) accel *= airControlMultiplier;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            input.magnitude > 0.01f ? moveSpeed : 0f,
            accel * Time.fixedDeltaTime
        );

        Vector3 moveVelocity = isOnSlope && isGrounded
            ? Vector3.ProjectOnPlane(desiredDirection, slopeHit.normal) * currentSpeed
            : desiredDirection * currentSpeed;

        Vector3 targetVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 30f);
    }

    #endregion

    #region Jump & Gravity

    private void TickJumpCooldown()
    {
        if (jumpCooldownTimer > 0f)
            jumpCooldownTimer -= Time.deltaTime;
    }

    private void TickJumpBuffer()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
        else
            jumpRequested = false;
    }

    private void HandleJump()
    {
        bool shouldJump = (jumpRequested && jumpBufferTimer > 0f) || mobileJumpPressed;

        if (shouldJump)
        {
            bool canJump = isGrounded && jumpCooldownTimer <= 0f;

            if (canJump)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                jumpCooldownTimer = jumpCooldown;
                jumpBufferTimer = 0f;
                jumpRequested = false;
            }

            if (!jumpRequested)
                mobileJumpPressed = false;
        }
    }

    private void ApplyCustomGravity()
    {
        if (isGrounded && rb.linearVelocity.y <= 0f) return;

        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * (fallMultiplier - 1f), ForceMode.Acceleration);
        else if (rb.linearVelocity.y > 0f)
            rb.AddForce(Vector3.down * Physics.gravity.magnitude * (gravityMultiplier - 1f), ForceMode.Acceleration);
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        CapsuleCollider c = GetComponent<CapsuleCollider>();
        Vector3 origin = groundCheckOrigin != null
            ? groundCheckOrigin.position
            : transform.position + Vector3.down * ((c != null ? c.height : 2f) * 0.5f - (c != null ? c.radius : 0.5f));

        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }

    #endregion

    #region Public API

    public bool IsGrounded => isGrounded;
    public float CurrentSpeed => currentSpeed;
    public bool CanJump => isGrounded && jumpCooldownTimer <= 0f;
    public float JumpCooldownRemaining => Mathf.Max(0f, jumpCooldownTimer);

    public void Teleport(Vector3 position)
    {
        rb.position = position;
        rb.linearVelocity = Vector3.zero;
    }

    #endregion
}