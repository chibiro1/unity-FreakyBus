using Unity.Netcode;
using UnityEngine;

public class BusController : NetworkBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider wheelFL;
    [SerializeField] private WheelCollider wheelFR;
    [SerializeField] private WheelCollider wheelRL;
    [SerializeField] private WheelCollider wheelRR;

    [Header("Bus Settings")]
    [SerializeField] private float motorTorque = 1500f;
    [SerializeField] private float brakeTorque = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxSpeed = 20f;

    private Rigidbody rb;
    private float currentSteer;
    private float currentThrottle;
    private bool isBraking;

    public float CurrentSpeed => rb != null ? rb.linearVelocity.magnitude : 0f;
    public bool IsStopped => CurrentSpeed < 0.1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        HandleMotor();
        HandleSteering();
    }

    private void HandleMotor()
    {
        if (isBraking)
        {
            ApplyBrakes(brakeTorque);
            return;
        }

        ApplyBrakes(0f);

        if (CurrentSpeed < maxSpeed)
        {
            wheelRL.motorTorque = currentThrottle * motorTorque;
            wheelRR.motorTorque = currentThrottle * motorTorque;
        }
        else
        {
            wheelRL.motorTorque = 0f;
            wheelRR.motorTorque = 0f;
        }
    }

    private void HandleSteering()
    {
        float steerAngle = currentSteer * maxSteerAngle;
        wheelFL.steerAngle = steerAngle;
        wheelFR.steerAngle = steerAngle;
    }

    private void ApplyBrakes(float torque)
    {
        wheelFL.brakeTorque = torque;
        wheelFR.brakeTorque = torque;
        wheelRL.brakeTorque = torque;
        wheelRR.brakeTorque = torque;
    }

    public void SetInputs(float steer, float throttle, bool brake)
    {
        currentSteer = steer;
        currentThrottle = throttle;
        isBraking = brake;
    }
}