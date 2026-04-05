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
    [SerializeField] private float maxSpeed = 20f;
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
            float targetSpeed = currentThrottle * maxSpeed;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }

        // Use ForwardPivot direction for movement
        Vector3 forward = forwardPivot != null
            ? forwardPivot.forward
            : transform.forward;

        rb.linearVelocity = new Vector3(
            forward.x * currentSpeed,
            rb.linearVelocity.y,
            forward.z * currentSpeed
        );
    }

    private void HandleSteering()
    {
        if (!steerReady || !isInputEnabled) return;
        if (Mathf.Abs(currentSpeed) < 0.1f) return;

        float turn = currentSteer * turnSpeed * Time.fixedDeltaTime;
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

    public void SetInputs(float steer, float throttle, bool brake)
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