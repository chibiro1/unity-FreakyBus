using UnityEngine;
using System.Collections.Generic;

public class BusStopPas : MonoBehaviour
{
    private List<PassengerAI> passengers = new List<PassengerAI>();

    public void RegisterPassenger(PassengerAI p)
    {
        if (!passengers.Contains(p))
            passengers.Add(p);
    }

    public void UnregisterPassenger(PassengerAI p)
    {
        passengers.Remove(p);
    }

    private void OnTriggerEnter(Collider other)
    {
        PassengerSeatManager bus = other.GetComponentInParent<PassengerSeatManager>();
        BusDoorWayManager doors = other.GetComponentInParent<BusDoorWayManager>();

        if (bus == null || doors == null) return;

        foreach (PassengerAI p in passengers)
        {
            if (p == null) continue;
            if (bus.IsFull()) break;

            Transform seat = bus.GetAvailableSeat();
            if (seat != null)
            {
                p.BoardBus(seat, doors.doorA, doors.doorB);
            }
        }
    }
}