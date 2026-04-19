using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class PassengerSeatManager : NetworkBehaviour
{
    public Transform[] seats;
    public bool dropPrepared = false;

    private List<int> availableSeatIndices = new List<int>();

    public List<PassengerAI> onboardPassengers = new List<PassengerAI>();
    public List<PassengerAI> pendingDropPassengers = new List<PassengerAI>();

    void Awake()
    {
        InitializeSeats();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        InitializeSeats();
    }

    void InitializeSeats()
    {
        availableSeatIndices.Clear();

        for (int i = 0; i < seats.Length; i++)
        {
            if (seats[i] != null)
                availableSeatIndices.Add(i);
        }

        Debug.Log("[SeatManager] Seats initialized: " + availableSeatIndices.Count);
    }

    // ✅ FIXED: INDEX-BASED SYSTEM
    public int GetAvailableSeatIndex()
    {
        if (availableSeatIndices.Count == 0)
        {
            Debug.LogWarning("[SeatManager] NO AVAILABLE SEATS!");
            return -1;
        }

        int rand = Random.Range(0, availableSeatIndices.Count);
        int seatIndex = availableSeatIndices[rand];

        availableSeatIndices.RemoveAt(rand);

        return seatIndex;
    }

    public bool IsFull()
    {
        return availableSeatIndices.Count == 0;
    }

    // =========================
    // ONBOARD TRACKING
    // =========================
    public void RegisterOnboard(PassengerAI p)
    {
        if (!onboardPassengers.Contains(p))
            onboardPassengers.Add(p);
    }

    public void RemoveOnboard(PassengerAI p)
    {
        onboardPassengers.Remove(p);
    }

    // =========================
    // DROP SYSTEM
    // =========================
    public void SelectRandomForDrop(int count)
    {
        if (dropPrepared) return;

        pendingDropPassengers.Clear();

        if (onboardPassengers.Count == 0)
            return;

        List<PassengerAI> temp = new List<PassengerAI>(onboardPassengers);

        count = Mathf.Min(count, temp.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, temp.Count);

            pendingDropPassengers.Add(temp[index]);
            temp.RemoveAt(index);
        }

        dropPrepared = true;
    }

    public void ExecuteDrop()
    {
        if (!IsServer) return;
        if (!dropPrepared) return;

        foreach (PassengerAI p in pendingDropPassengers)
        {
            if (p == null) continue;

            RemoveOnboard(p);
            p.InstantDropOff();
        }

        pendingDropPassengers.Clear();
        dropPrepared = false;
    }
}