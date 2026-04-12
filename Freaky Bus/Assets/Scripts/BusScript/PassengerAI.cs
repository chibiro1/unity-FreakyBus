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

    private bool isBoarding;
    private bool isSeated;
    private bool hasPaid;

    public bool IsSeated => isSeated;
    public bool IsBoarding => isBoarding;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (fareIndicator != null)
            fareIndicator.SetActive(false);
    }

    void Start()
    {
        // 🔥 IMPORTANT: register to correct bus stop
        GetComponentInParent<BusStopPas>()?.RegisterPassenger(this);
    }

    void Update()
    {
        if (isBoarding && !isSeated && targetDoor != null)
        {
            MoveToDoor();
        }
    }

    // =========================
    // MOVE
    // =========================
    void MoveToDoor()
    {
        Vector3 dir = targetDoor.position - transform.position;
        dir.y = 0f;

        float distance = dir.magnitude;

        if (distance > 0.1f)
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
    }

    // =========================
    // SIT
    // =========================
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

        UpdateUI();
    }

    // =========================
    // PAYMENT
    // =========================
    public void PayFare()
    {
        if (!isSeated || hasPaid) return;

        hasPaid = true;

        Debug.Log($"{name} Fare Paid");

        UpdateUI();
    }

    // =========================
    // UI
    // =========================
    void UpdateUI()
    {
        if (fareIndicator == null) return;

        fareIndicator.SetActive(isSeated && !hasPaid);
    }

    // =========================
    // CLICK
    // =========================
    void OnMouseDown()
    {
        PayFare();
    }

    // =========================
    // DOOR TRIGGER (backup)
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (!isBoarding || isSeated) return;

        if (other.GetComponentInParent<BusDoorWayManager>() != null)
        {
            Sit();
        }
    }
}