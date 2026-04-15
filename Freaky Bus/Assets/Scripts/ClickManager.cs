using UnityEngine;

public class ClickManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            PassengerAI bestTarget = null;
            float closest = Mathf.Infinity;

            foreach (RaycastHit hit in hits)
            {
                PassengerAI p = hit.collider.GetComponentInParent<PassengerAI>();

                if (p != null && p.IsSeated)
                {
                    if (hit.distance < closest)
                    {
                        closest = hit.distance;
                        bestTarget = p;
                    }
                }
            }

            if (bestTarget != null)
            {
                bestTarget.PayFare();
            }
        }
    }
}