using Unity.Netcode;
using UnityEngine;

public class BusController : NetworkBehaviour
{
    [Header("Direction")]
    [SerializeField] private Transform forwardPivot;

    [Header("Bus Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 50f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 5f;
    
    [Header("Speed Limits")]
    [SerializeField] private float maxForwardSpeed = 20f;
    [SerializeField] private float maxReverseSpeed = 6f; // Capped for realism
    
    [Header("Mechanics")]
    [SerializeField] private float steerDelay = 1.5f;

    private Rigidbody rb;
    private float currentSpeed;
    private float currentSteer;
    private float currentThrottle;
    private bool isBraking;
    private float steerDelayTimer;
    private bool steerReady;
    private bool isInputEnabled;

    public float CurrentSpeed => currentSpeed;
    public bool IsStopped => Mathf.Abs(currentSpeed) < 0.5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, -0.5f, 0f);
    }

    public void StartSteerDelay()
    {
        steerReady = false;
        steerDelayTimer = steerDelay;
        isInputEnabled = true;
    }

    public void StopInput()
    {
        isInputEnabled = false;
        currentSteer = 0f;
        currentThrottle = 0f;
        isBraking = false;
    }

    private void FixedUpdate()
    {
        // Physics and movement are purely Server-Authoritative
        if (!IsServer) return;

        HandleSteerDelay();
        HandleMovement();
        HandleSteering();
        HandleBraking();
    }

    private void HandleSteerDelay()
    {
        if (steerReady) return;
        steerDelayTimer -= Time.fixedDeltaTime;
        if (steerDelayTimer <= 0f)
            steerReady = true;
    }

    private void HandleMovement()
    {
        if (!isInputEnabled)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }
        else if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * 2f * Time.fixedDeltaTime);
        }
        else
        {
            // Assign the correct speed limit based on whether we are moving forward or backward
            float activeMaxSpeed = currentThrottle < 0f ? maxReverseSpeed : maxForwardSpeed;
            float targetSpeed = currentThrottle * activeMaxSpeed;
            
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }

        Vector3 forward = forwardPivot != null ? forwardPivot.forward : transform.forward;

        rb.linearVelocity = new Vector3(
            forward.x * currentSpeed,
            rb.linearVelocity.y,
            forward.z * currentSpeed
        );
    }

    private void HandleSteering()
    {
        if (!steerReady || !isInputEnabled) return;
        
        // Prevent steering on the spot
        if (Mathf.Abs(currentSpeed) < 0.1f) return;

        // CRITICAL FIX: If we are going in reverse, we must invert the visual steering calculation
        // so the back of the bus swings in the direction of the steering wheel.
        float directionMultiplier = currentSpeed < -0.1f ? -1f : 1f;

        float turn = currentSteer * turnSpeed * directionMultiplier * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void HandleBraking()
    {
        if (isBraking || !isInputEnabled)
        {
            rb.linearVelocity = new Vector3(
                rb.linearVelocity.x * 0.95f,
                rb.linearVelocity.y,
                rb.linearVelocity.z * 0.95f
            );
        }
    }

    // ── Input Synchronization ───────────────────────────────────────

    // The client calls this method locally from BusInputHandler
    public void SetInputs(float steer, float throttle, bool brake)
    {
        if (IsServer)
        {
            // If the Host is driving, apply directly
            ApplyInputs(steer, throttle, brake);
        }
        else
        {
            // If a Client is driving, forward the inputs to the server
            SetInputsServerRpc(steer, throttle, brake);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetInputsServerRpc(float steer, float throttle, bool brake)
    {
        ApplyInputs(steer, throttle, brake);
    }

    private void ApplyInputs(float steer, float throttle, bool brake)
    {
        currentSteer = steer;
        currentThrottle = throttle;
        isBraking = brake;
    }

    public void StopBus()
    {
        StopInput();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}