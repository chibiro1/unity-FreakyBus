using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerBody;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float touchSensitivity = 0.15f;

    [Header("Vertical Clamp")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Smoothing")]
    [SerializeField] private bool smoothLook = true;
    [SerializeField] private float smoothSpeed = 25f;       // Increased for less lag

    [Header("Desktop Settings")]
    [SerializeField] private bool lockCursorOnPlay = true;

    // ── Private State ──────────────────────────────────────────────
    private float pitchAngle;
    private float yawAngle;
    private float targetPitch;
    private float targetYaw;

    private int activeTouchId = -1;
    private Vector2 lastTouchPosition;

    // ══════════════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        if (playerBody != null)
            targetYaw = playerBody.eulerAngles.y;

        if (lockCursorOnPlay)
            LockCursor();
    }

    private void Update()
    {
        // Only read input in Update — do NOT apply rotation here
        HandleDesktopInput();
        HandleMobileInput();
        HandleCursorLock();
    }

    // KEY FIX: Apply rotation in LateUpdate, not Update
    // This ensures camera moves AFTER the Rigidbody has been moved by FixedUpdate
    // which is the main cause of stutter in first person controllers
    private void LateUpdate()
    {
        ApplyRotation();
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    #region Desktop Input

    private void HandleDesktopInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        targetYaw += mouseDelta.x * mouseSensitivity;
        targetPitch -= mouseDelta.y * mouseSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleCursorLock()
    {
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            LockCursor();

        if (Input.GetKeyDown(KeyCode.Escape))
            UnlockCursor();
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    #region Mobile Input

    private void HandleMobileInput()
    {
        if (Input.touchCount == 0)
        {
            activeTouchId = -1;
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            // Right half of screen = camera control
            // Left half of screen = joystick
            bool isRightSide = touch.position.x > Screen.width * 0.5f;
            if (!isRightSide) continue;

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                activeTouchId = touch.fingerId;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == UnityEngine.TouchPhase.Moved && touch.fingerId == activeTouchId)
            {
                Vector2 delta = touch.position - lastTouchPosition;
                lastTouchPosition = touch.position;

                targetYaw += delta.x * touchSensitivity;
                targetPitch -= delta.y * touchSensitivity;
                targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
            }
            else if (touch.phase == UnityEngine.TouchPhase.Ended && touch.fingerId == activeTouchId)
            {
                activeTouchId = -1;
            }
        }
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    #region Apply Rotation

    private void ApplyRotation()
    {
        if (smoothLook)
        {
            // Use Time.deltaTime here since we're in LateUpdate
            pitchAngle = Mathf.LerpAngle(pitchAngle, targetPitch, smoothSpeed * Time.deltaTime);
            yawAngle = Mathf.LerpAngle(yawAngle, targetYaw, smoothSpeed * Time.deltaTime);
        }
        else
        {
            pitchAngle = targetPitch;
            yawAngle = targetYaw;
        }

        // Camera rotates vertically (up/down) — pitch only
        transform.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);

        // Player body rotates horizontally (left/right) — yaw only
        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, yawAngle, 0f);
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    #region Cursor Helpers

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    #endregion

    // ══════════════════════════════════════════════════════════════
    #region Public API

    public float PitchAngle => pitchAngle;
    public float YawAngle => yawAngle;

    public void SetLookDirection(float yaw, float pitch)
    {
        targetYaw = yaw;
        targetPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    #endregion
}