using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    private BusStopPas busStop;

    void Start()
    {
        busStop = GetComponentInParent<BusStopPas>();

        if (busStop == null)
            Debug.LogWarning("TriggerForwarder could not find BusStopPas in parent!");
        else
            Debug.Log("TriggerForwarder connected to BusStopPas successfully.");
    }

    void OnTriggerEnter(Collider other)
    {
        PassengerSeatManager bus = other.GetComponentInParent<PassengerSeatManager>();

        if (bus == null) return;

        PassengerAI[] nearbyPassengers = FindObjectsOfType<PassengerAI>();

        foreach (var p in nearbyPassengers)
        {
            if (bus.IsFull()) break;

            Transform seat = bus.GetAvailableSeat();
            if (seat != null)
                p.BoardBus(seat, bus.GetComponent<BusDoorWayManager>().doorA,
                                  bus.GetComponent<BusDoorWayManager>().doorB);
        }
    }
}