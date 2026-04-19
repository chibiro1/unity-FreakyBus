using UnityEngine;
using Unity.Netcode;
using System.Collections; 
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

    [Header("Boarding Settings")]
    public float boardingDelay = 0.75f; // Time they stand at the door before "sitting"

    // --- NETWORK VARIABLES ---
    private NetworkVariable<bool> isAnomaly = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isBoarding = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isSeated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> hasPaid = new NetworkVariable<bool>(false);

    private int assignedSeatIndex = -1;

    public bool IsSeated => isSeated.Value;
    public bool IsBoarding => isBoarding.Value;
    public bool IsAnomaly => isAnomaly.Value;
    public bool CanPay => isSeated.Value && !hasPaid.Value;

    private Animator animator;
    private Transform targetSeat;
    private Transform targetDoor;
    private PassengerSeatManager busManager;
    private BusStopPas busStop;
    private NetworkTransform netTransform;
    private bool isTransitioning = false; // Prevents multiple Sit calls

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
        
        // Listen for seated status to disable components on all clients
        isSeated.OnValueChanged += HandleSeatedStateChanged;
        hasPaid.OnValueChanged += (prev, current) => UpdateUI();
        
        UpdateUI();
    }

    public override void OnNetworkDespawn()
    {
        isSeated.OnValueChanged -= HandleSeatedStateChanged;
        hasPaid.OnValueChanged -= (prev, current) => UpdateUI();
    }

    // This runs on EVERY client when isSeated changes
    private void HandleSeatedStateChanged(bool wasSeated, bool nowSeated)
    {
        if (nowSeated)
        {
            // KILL the NetworkTransform so it stops fighting the parenting
            if (netTransform != null) netTransform.enabled = false;
            
            // Disable collider so we don't push the bus or block players
            if (TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        }
        else
        {
            // Re-enable if they ever exit the bus
            if (netTransform != null) netTransform.enabled = true;
            if (TryGetComponent<Collider>(out Collider col)) col.enabled = true;
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

    [ServerRpc(RequireOwnership = false)]
    public void KickOutServerRpc() => InstantDropOff();

    public void InstantDropOff()
    {
        if (busManager != null) busManager.RemoveOnboard(this);
        if (IsServer) GetComponent<NetworkObject>().Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAnomaly.Value && other.CompareTag("Player"))
        {
            var sanity = other.GetComponent<SanityManager>();
            if (sanity != null && other.GetComponent<NetworkBehaviour>().IsOwner)
                sanity.RegisterAnomaly(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isAnomaly.Value && other.CompareTag("Player"))
        {
            var sanity = other.GetComponent<SanityManager>();
            if (sanity != null && other.GetComponent<NetworkBehaviour>().IsOwner)
                sanity.UnregisterAnomaly(this);
        }
    }

    public void SetBusStop(BusStopPas stop)
    {
        busStop = stop;
        busStop?.RegisterPassenger(this);
    }

    void Update()
    {
        if (!IsServer || isSeated.Value || isTransitioning) return; 

        if (isBoarding.Value && targetDoor != null)
            MoveToDoor();
    }

    void MoveToDoor()
    {
        Vector3 dir = targetDoor.position - transform.position;
        dir.y = 0f;

        // Threshold to stop before colliding with the bus exterior
        float arrivalThreshold = 1.0f; 

        if (dir.magnitude > arrivalThreshold)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            transform.position += dir.normalized * walkSpeed * Time.deltaTime;
            animator.SetBool("isWalking", true);
        }
        else 
        {
            // NPC reached the door, start the boarding sequence
            StartCoroutine(BoardingSequence());
        }
    }

    IEnumerator BoardingSequence()
    {
        isTransitioning = true;
        
        // Snap to door and stop walking
        animator.SetBool("isWalking", false);
        transform.position = targetDoor.position;

        // Delay to let the network stabilize before parenting
        yield return new WaitForSeconds(boardingDelay);

        Sit();
        isTransitioning = false;
    }

    public void BoardBus(int seatIndex, Transform seat, Transform doorA, Transform doorB)
    {
        if (!IsServer || isBoarding.Value || isSeated.Value) return;

        assignedSeatIndex = seatIndex;
      
        targetSeat = seat;
        float distA = Vector3.Distance(transform.position, doorA.position);
        float distB = Vector3.Distance(transform.position, doorB.position);
        targetDoor = distA < distB ? doorA : doorB;
        
        isBoarding.Value = true;
        hasPaid.Value = false;
        
        busStop?.UnregisterPassenger(this);
    }

    void Sit()
    {
        if (!IsServer) return;

        isSeated.Value = true;
        isBoarding.Value = false;
        
        GetComponent<NetworkObject>().TrySetParent(targetSeat, false);

        ForceSeatSnapClientRpc(assignedSeatIndex);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 180f, 0);

        animator.SetBool("isSeated", true);
        
        busManager = Object.FindFirstObjectByType<PassengerSeatManager>();
        if (busManager != null) busManager.RegisterOnboard(this);
        
        UpdateUI();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ForceSeatSnapClientRpc(int seatIndex)
    {

    var manager = Object.FindFirstObjectByType<PassengerSeatManager>();
    if (manager != null && seatIndex >= 0 && seatIndex < manager.seats.Length)
    {
        Transform actualSeat = manager.seats[seatIndex];
        
        // Manual local snap
        transform.SetParent(actualSeat, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0, 180f, 0);
    }
    }

    public void PayFare() { if (CanPay) PayFareServerRpc(); }

    [ServerRpc(RequireOwnership = false)]
    private void PayFareServerRpc(ServerRpcParams rpcParams = default)
    {
        if (hasPaid.Value) return; 
        hasPaid.Value = true; 
        ConfirmPaymentClientRpc(rpcParams.Receive.SenderClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ConfirmPaymentClientRpc(ulong clickerId)
    {
        if (NetworkManager.Singleton.LocalClientId == clickerId)
        {
            if (MoneyManager.Instance != null) MoneyManager.Instance.AddMoney(20);
        }
    }

    void UpdateUI()
    {
        if (fareIndicator != null) 
            fareIndicator.SetActive(isSeated.Value && !hasPaid.Value);
    }

    void OnDestroy() { if (busStop != null) busStop.UnregisterPassenger(this); }
}