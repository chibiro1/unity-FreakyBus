using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
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
    [SerializeField] private float smoothSpeed = 25f;

    [Header("Cursor Settings")]
    [SerializeField] private bool lockCursorOnPlay = true;
    [SerializeField] private KeyCode toggleCursorKey = KeyCode.K;

    private float pitchAngle;
    private float yawAngle;
    private float targetPitch;
    private float targetYaw;

    private int activeTouchId = -1;
    private Vector2 lastTouchPosition;
    private Camera cam;

    #region Netcode

    public override void OnNetworkSpawn()
    {
        cam = GetComponent<Camera>();

        if (!IsOwner)
        {
            if (cam != null) cam.enabled = false;
            enabled = false;
            return;
        }

        if (playerBody != null)
            targetYaw = playerBody.eulerAngles.y;

        if (lockCursorOnPlay)
            LockCursor();
    }

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        if (!IsOwner) return;

        HandleDesktopInput();
        HandleMobileInput();
        HandleCursorToggle();
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;
        ApplyRotation();
    }

    #endregion

    #region Desktop Input

    private void HandleDesktopInput()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 mouseDelta = new Vector2(
            Input.GetAxis("Mouse X"),
            Input.GetAxis("Mouse Y")
        );

        targetYaw   += mouseDelta.x * mouseSensitivity;
        targetPitch -= mouseDelta.y * mouseSensitivity;
        targetPitch  = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleCursorToggle()
    {
        // Press K to toggle cursor lock on and off
        if (Input.GetKeyDown(toggleCursorKey))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
                UnlockCursor();
            else
                LockCursor();
        }
    }

    #endregion

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

                targetYaw   += delta.x * touchSensitivity;
                targetPitch -= delta.y * touchSensitivity;
                targetPitch  = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);
            }
            else if (touch.phase == UnityEngine.TouchPhase.Ended && touch.fingerId == activeTouchId)
            {
                activeTouchId = -1;
            }
        }
    }

    #endregion

    #region Apply Rotation

    private void ApplyRotation()
    {
        if (smoothLook)
        {
            pitchAngle = Mathf.LerpAngle(pitchAngle, targetPitch, smoothSpeed * Time.deltaTime);
            yawAngle   = Mathf.LerpAngle(yawAngle,   targetYaw,   smoothSpeed * Time.deltaTime);
        }
        else
        {
            pitchAngle = targetPitch;
            yawAngle   = targetYaw;
        }

        transform.localRotation = Quaternion.Euler(pitchAngle, 0f, 0f);

        if (playerBody != null)
            playerBody.rotation = Quaternion.Euler(0f, yawAngle, 0f);
    }

    #endregion

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

    #region Public API

    public float PitchAngle => pitchAngle;
    public float YawAngle => yawAngle;

    public void SetLookDirection(float yaw, float pitch)
    {
        targetYaw   = yaw;
        targetPitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
    }

    #endregion
}