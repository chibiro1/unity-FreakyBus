using UnityEngine;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        HandleMouse();
        HandleTouch();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                TryPay(hit);
            }
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    TryPay(hit);
                }
            }
        }
    }

    void TryPay(RaycastHit hit)
    {
        PassengerAI passenger = hit.collider.GetComponentInParent<PassengerAI>();

        if (passenger != null)
        {
            passenger.PayFare();
        }
    }
}