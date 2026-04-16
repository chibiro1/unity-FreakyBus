using UnityEngine;

public class TriggerForwarding : MonoBehaviour
{
    private BusStopPas busStop;

    void Awake()
    {
        busStop = GetComponentInParent<BusStopPas>();
    }

    void OnTriggerEnter(Collider other)
    {
        PassengerSeatManager bus = other.GetComponentInParent<PassengerSeatManager>();
        BusDoorWayManager doors = other.GetComponentInParent<BusDoorWayManager>();

        if (bus == null || doors == null) return;

        // =========================
        // BOARDING (your existing system)
        // =========================
        if (busStop != null)
        {
            Debug.Log("BUS ARRIVED AT STOP: " + busStop.name);
            busStop.TryBoardBus(bus, doors);
        }

        // =========================
        // DROP EXECUTION (NEW)
        // =========================
        bus.ExecuteDrop();
    }
}