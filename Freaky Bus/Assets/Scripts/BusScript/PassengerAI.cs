using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PassengerAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;

    [Header("Fare UI")]
    public GameObject fareIndicator;

    private Animator animator;
    private Transform targetSeat;
    private Transform targetDoor;
    private float seatTime;

    private bool isBoarding;
    private bool isSeated;
    private bool hasPaid;

    private BusStopPas busStop;
    private PassengerSeatManager busManager;

    public bool IsSeated => isSeated;
    public bool IsBoarding => isBoarding;
    public bool CanPay => isSeated && !hasPaid && Time.time - seatTime > 0.2f;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (fareIndicator != null)
            fareIndicator.SetActive(false);
    }

    public void SetBusStop(BusStopPas stop)
    {
        busStop = stop;
        busStop?.RegisterPassenger(this);
    }

    void Update()
    {
        if (isBoarding && !isSeated && targetDoor != null)
        {
            MoveToDoor();
        }
    }

    void MoveToDoor()
    {
        Vector3 dir = targetDoor.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude > 0.1f)
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
    // BOARD
    // =========================
    public void BoardBus(Transform seat, Transform doorA, Transform doorB)
    {
        if (isBoarding || isSeated) return;

        targetSeat = seat;

        float distA = Vector3.Distance(seat.position, doorA.position);
        float distB = Vector3.Distance(seat.position, doorB.position);

        targetDoor = distA < distB ? doorA : doorB;

        isBoarding = true;
        isSeated = false;
        hasPaid = false;

        busStop?.UnregisterPassenger(this);
    }

    void Sit()
    {
        if (targetSeat == null) return;

        isSeated = true;
        isBoarding = false;

        transform.position = targetSeat.position;
        transform.rotation = targetSeat.rotation * Quaternion.Euler(0, 180f, 0);
        transform.SetParent(targetSeat);

        animator.SetBool("isWalking", false);
        animator.SetBool("isSeated", true);

        seatTime = Time.time;

        // 🔥 NEW: register onboard
        busManager = FindFirstObjectByType<PassengerSeatManager>();

        if (busManager != null)
        {
            busManager.RegisterOnboard(this);
        }

        UpdateUI();
    }

    // =========================
    // 🔥 DROP OFF (NEW)
    // =========================
    public void InstantDropOff()
    {
        if (busManager != null)
            busManager.RemoveOnboard(this);

        Destroy(gameObject);
    }

    public void PayFare()
    {
        if (!CanPay) return;

        hasPaid = true;

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(20);

        UpdateUI();
    }

    void UpdateUI()
    {
        if (fareIndicator != null)
            fareIndicator.SetActive(isSeated && !hasPaid);
    }

    void OnDestroy()
    {
        if (busStop != null)
            busStop.UnregisterPassenger(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isBoarding || isSeated) return;

        if (other.GetComponentInParent<BusDoorWayManager>() != null)
        {
            Sit();
        }
    }
}