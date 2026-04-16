using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DropoffTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        PassengerSeatManager bus = other.GetComponentInParent<PassengerSeatManager>();

        if (bus == null) return;

        if (bus.onboardPassengers.Count == 0) return;

        int dropCount = Random.Range(1, bus.onboardPassengers.Count + 1);

        bus.SelectRandomForDrop(dropCount);
    Debug.Log("DropoffZone selected " + dropCount + " passengers");
    }
}