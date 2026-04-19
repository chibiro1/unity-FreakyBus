using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode; 

public class BusStopPas : MonoBehaviour
{
    private List<PassengerAI> passengers = new List<PassengerAI>();

    public void RegisterPassenger(PassengerAI p)
    {
        if (!passengers.Contains(p)) passengers.Add(p);
    }

    public void UnregisterPassenger(PassengerAI p)
    {
        passengers.Remove(p);
    }

    public int CurrentPassengerCount() => passengers.Count;

    public void TryBoardBus(PassengerSeatManager bus, BusDoorWayManager doors)
    {
    if (!NetworkManager.Singleton.IsServer) return;

    foreach (PassengerAI p in passengers.ToArray())
    {
        if (p == null) continue;
        if (bus.IsFull()) break;
        if (p.IsSeated || p.IsBoarding) continue;

        // Get both index and seat transform
        int seatIndex = bus.GetAvailableSeatIndex();
        if (seatIndex < 0) break;

        Transform seat = bus.seats[seatIndex];
        p.BoardBus(seatIndex, seat, doors.doorA, doors.doorB);
    }
    }

    public void OnBusArrived(PassengerSeatManager bus)
    {
        // BULLETPROOF CHECK: Only the server kicks people off
        if (!NetworkManager.Singleton.IsServer) return;

        if (bus.onboardPassengers.Count == 0) return;

        int dropCount = Random.Range(1, bus.onboardPassengers.Count + 1);

        for (int i = 0; i < dropCount; i++)
        {
            if (bus.onboardPassengers.Count == 0) break;

            int index = Random.Range(0, bus.onboardPassengers.Count);
            PassengerAI p = bus.onboardPassengers[index];
            bus.onboardPassengers.RemoveAt(index);

            p.InstantDropOff();
        }
    }
}