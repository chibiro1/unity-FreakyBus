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

    public int CurrentPassengerCount()
    {
        return passengers.Count;
    }

    public void TryBoardBus(PassengerSeatManager bus, BusDoorWayManager doors)
    {
        foreach (PassengerAI p in passengers.ToArray())
        {
            if (p == null) continue;
            if (bus.IsFull()) break;

            if (p.IsSeated || p.IsBoarding) continue;

            Transform seat = bus.GetAvailableSeat();
            if (seat != null)
            {
                p.BoardBus(seat, doors.doorA, doors.doorB);
            }
        }
    }

    // =========================
    // 🔥 DROP OFF SYSTEM (NEW)
    // =========================
    public void OnBusArrived(PassengerSeatManager bus)
    {
        if (bus.onboardPassengers.Count == 0)
            return;

        int dropCount = Random.Range(1, bus.onboardPassengers.Count + 1);

        for (int i = 0; i < dropCount; i++)
        {
            if (bus.onboardPassengers.Count == 0)
                break;

            int index = Random.Range(0, bus.onboardPassengers.Count);

            PassengerAI p = bus.onboardPassengers[index];

            bus.onboardPassengers.RemoveAt(index);

            p.InstantDropOff();
        }
    }
}