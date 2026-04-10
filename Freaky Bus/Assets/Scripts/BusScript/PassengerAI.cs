using UnityEngine;

public class PassengerAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;

    private Animator animator;
    private Transform targetSeat;
    private bool isBoarding = false;
    private bool isSeated = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isBoarding || isSeated) return;

        // Walk toward the seat
        Vector3 direction = targetSeat.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > 0.2f)
        {
            // Face the seat and walk
            transform.rotation = Quaternion.LookRotation(direction);
            transform.position += direction.normalized * walkSpeed * Time.deltaTime;
            animator.SetBool("isWalking", true);
            animator.SetBool("isSeated", false);
        }
        else
        {
            // Reached the seat
            transform.position = targetSeat.position;
            transform.rotation = targetSeat.rotation * Quaternion.Euler(0, 180, 0);
            animator.SetBool("isWalking", false);
            animator.SetBool("isSeated", true);
            isSeated = true;
        }
    }

    public void BoardBus(Transform seat)
    {
        targetSeat = seat;
        isBoarding = true;
        isSeated = false;
    }
}