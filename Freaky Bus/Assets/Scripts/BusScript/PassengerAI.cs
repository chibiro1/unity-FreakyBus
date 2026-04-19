using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PassengerAI : NetworkBehaviour
{
    [Header("Anomaly Settings")]
    [Range(0, 100)] public float anomalyChance = 40f;
    public GameObject anomalyEffects;
    public AudioSource anomalySFX;

    [Header("Movement")]
    public float walkSpeed = 2f;

    [Header("Fare UI")]
    public GameObject fareIndicator;

    [Header("Seat Rotation Fix")]
    [SerializeField] private Vector3 seatRotationOffset = new Vector3(0, 180f, 0);

    // =========================
    // NETWORK VARIABLES
    // =========================
    private NetworkVariable<bool> isAnomaly = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBoarding = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isSeated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> hasPaid = new NetworkVariable<bool>(false);

    private int assignedSeatIndex = -1;

    public bool IsSeated => isSeated.Value;
    public bool IsBoarding => isBoarding.Value;
    public bool CanPay => isSeated.Value && !hasPaid.Value;

    private bool followSeat = false;

    private Animator animator;
    private Transform targetSeat;
    private Transform targetDoor;

    private PassengerSeatManager busManager;
    private BusStopPas busStop;

    private NetworkTransform netTransform;
    private Collider busCollider; // reference to bus collider
    

    // =========================
    // INIT
    // =========================
    void Awake()
    {
        animator = GetComponent<Animator>();
        netTransform = GetComponent<NetworkTransform>();

        if (fareIndicator != null) fareIndicator.SetActive(false);
        if (anomalyEffects != null) anomalyEffects.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            isAnomaly.Value = Random.Range(0f, 100f) < anomalyChance;
        }

        ApplyAnomalyState(isAnomaly.Value);

        isSeated.OnValueChanged += HandleSeatedStateChanged;
        hasPaid.OnValueChanged += (prev, curr) => UpdateUI();

        UpdateUI();
    }

    public override void OnNetworkDespawn()
    {
        isSeated.OnValueChanged -= HandleSeatedStateChanged;
    }

    // =========================
    // STATE
    // =========================
    private void HandleSeatedStateChanged(bool oldVal, bool newVal)
    {
        if (netTransform != null)
            netTransform.enabled = !newVal;

        if (TryGetComponent<Collider>(out Collider col))
        {
            if (newVal && busCollider != null)
                Physics.IgnoreCollision(col, busCollider, true);  // seated: ignore bus physics
            else if (!newVal && busCollider != null)
                Physics.IgnoreCollision(col, busCollider, false); // standing: restore collision

            col.enabled = true; // always keep on for click raycasts
        }

        UpdateUI();
    }

    private void ApplyAnomalyState(bool state)
    {
        if (anomalyEffects != null) anomalyEffects.SetActive(state);

        if (anomalySFX != null)
        {
            if (state) anomalySFX.Play();
            else anomalySFX.Stop();
        }
    }

    // =========================
    // MOVEMENT
    // =========================
    void Update()
    {
        if (!IsServer || isSeated.Value) return;

        if (isBoarding.Value && targetDoor != null)
            MoveToDoor();
    }

    void LateUpdate()
    {
        if (!IsServer) return;
        if (!followSeat || targetSeat == null) return;

        transform.position = targetSeat.position;
        transform.rotation = targetSeat.rotation * Quaternion.Euler(seatRotationOffset);
    }

    void MoveToDoor()
    {
        Vector3 dir = targetDoor.position - transform.position;
        dir.y = 0f;

        float stopDistance = 1.5f;

        if (dir.magnitude > stopDistance)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            transform.position += dir.normalized * walkSpeed * Time.deltaTime;

            animator.SetBool("isWalking", true);
        }
        else
        {
            Sit();
        }
    }

    // =========================
    // BOARD BUS
    // =========================
    public void BoardBus(int seatIndex, Transform seat, Transform doorA, Transform doorB, PassengerSeatManager manager)
    {
        if (!IsServer || isBoarding.Value || isSeated.Value) return;

        assignedSeatIndex = seatIndex;
        targetSeat = seat;
        busManager = manager;
        busCollider = manager.GetComponentInChildren<Collider>();

        float distA = Vector3.Distance(transform.position, doorA.position);
        float distB = Vector3.Distance(transform.position, doorB.position);
        targetDoor = distA < distB ? doorA : doorB;

        isBoarding.Value = true;
        hasPaid.Value = false;

        busStop?.UnregisterPassenger(this);
    }

    // =========================
    // SIT
    // =========================
    void Sit()
    {
        if (!IsServer || targetSeat == null) return;

        isSeated.Value = true;
        isBoarding.Value = false;

        followSeat = true;

        animator.SetBool("isSeated", true);

        if (busManager != null)
            busManager.RegisterOnboard(this);

        UpdateUI();
    }

    // =========================
    // CLIENT SYNC
    // =========================
    [Rpc(SendTo.ClientsAndHost)]
    private void ForceSeatSnapClientRpc(int seatIndex)
    {
        if (busManager == null) return;

        if (seatIndex >= 0 && seatIndex < busManager.seats.Length)
        {
            Transform seat = busManager.seats[seatIndex];
            transform.position = seat.position;
            transform.rotation = seat.rotation * Quaternion.Euler(seatRotationOffset);
        }
    }

    // =========================
    // FARE
    // =========================
    public void PayFare()
    {
        if (CanPay)
            PayFareServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PayFareServerRpc(ServerRpcParams rpcParams = default)
    {
        if (hasPaid.Value) return;

        hasPaid.Value = true;

        ConfirmPaymentClientRpc(rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ConfirmPaymentClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (MoneyManager.Instance != null)
                MoneyManager.Instance.AddMoney(20);
        }
    }

    void UpdateUI()
    {
        if (fareIndicator != null)
            fareIndicator.SetActive(isSeated.Value && !hasPaid.Value);
    }

    // =========================
    // DROP SYSTEM
    // =========================
    [ServerRpc(RequireOwnership = false)]
    public void KickOutServerRpc()
    {
        InstantDropOff();
    }

    public void InstantDropOff()
    {
        if (busManager != null)
            busManager.RemoveOnboard(this);

        if (IsServer)
            GetComponent<NetworkObject>().Despawn();
    }

    // =========================
    // BUS STOP
    // =========================
    public void SetBusStop(BusStopPas stop)
    {
        busStop = stop;
        busStop?.RegisterPassenger(this);
    }

    void OnDestroy()
    {
        if (busStop != null)
            busStop.UnregisterPassenger(this);
    }
}