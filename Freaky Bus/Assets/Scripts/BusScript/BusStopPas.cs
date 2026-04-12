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

    public void TryBoardBus(PassengerSeatManager bus, BusDoorWayManager doors)
    {
        Debug.Log($"[{name}] Boarding passengers: {passengers.Count}");

        foreach (PassengerAI p in passengers)
        {
            if (p == null) continue;
            if (bus.IsFull()) break;

            // 🔥 prevent bugs
            if (p.IsSeated) continue;
            if (p.IsBoarding) continue;

            Transform seat = bus.GetAvailableSeat();
            if (seat != null)
            {
                p.BoardBus(seat, doors.doorA, doors.doorB);
            }
        }
    }
}