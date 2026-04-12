using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PassengerAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;

    [Header("Fare UI (MUST BE CHILD OF PREFAB)")]
    public GameObject fareIndicator;

    private Animator animator;

    private Transform targetSeat;
    private Transform targetDoor;

    private bool isBoarding;
    private bool isSeated;
    private bool hasPaid;

    public bool IsSeated => isSeated;
    public bool HasPaid => hasPaid;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (fareIndicator != null)
            fareIndicator.SetActive(false);
    }

    void Update()
    {
        if (isBoarding && !isSeated && targetDoor != null)
        {
            MoveToDoor();
        }

        // 🔥 IMPORTANT: keeps UI always correct (fixes stuck indicator)
        RefreshFareUI();
    }

    // =========================
    // MOVE
    // =========================
    void MoveToDoor()
    {
        Vector3 dir = targetDoor.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude > 0.05f)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            transform.position += dir.normalized * walkSpeed * Time.deltaTime;

            animator.SetBool("isWalking", true);
            animator.SetBool("isSeated", false);
        }
    }

    // =========================
    // BOARD BUS
    // =========================
    public void BoardBus(Transform seat, Transform doorA, Transform doorB)
    {
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

        transform.SetParent(targetSeat.root);

        animator.SetBool("isWalking", false);
        animator.SetBool("isSeated", true);
    }

    // =========================
    // PAYMENT (called externally)
    // =========================
    public void PayFare()
    {
        if (!isSeated || hasPaid) return;

        hasPaid = true;

        Debug.Log($"{name} Fare Paid");
    }

    // =========================
    // UI CONTROL (AUTO SYNC)
    // =========================
    void RefreshFareUI()
    {
        if (fareIndicator == null) return;

        bool shouldShow = isSeated && !hasPaid;

        if (fareIndicator.activeSelf != shouldShow)
            fareIndicator.SetActive(shouldShow);
    }

    // =========================
    // DOOR TRIGGER
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (!isBoarding || isSeated) return;

        if (other.transform == targetDoor)
        {
            Sit();
        }
    }
}