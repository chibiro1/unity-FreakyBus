using UnityEngine;
using System.Collections.Generic;

public class PassengerSeatManager : MonoBehaviour
{
    public Transform[] seats;
    public bool dropPrepared = false;

    private List<Transform> availableSeats = new List<Transform>();

    // =========================
    // ONBOARD SYSTEM
    // =========================
    public List<PassengerAI> onboardPassengers = new List<PassengerAI>();

    // passengers selected at DropoffZone but not yet removed
    public List<PassengerAI> pendingDropPassengers = new List<PassengerAI>();

    void Awake()
    {
        foreach (Transform s in seats)
        {
            if (s != null)
                availableSeats.Add(s);
        }
    }

    // =========================
    // SEAT SYSTEM (UNCHANGED)
    // =========================
    public Transform GetAvailableSeat()
    {
        if (availableSeats.Count == 0) return null;

        int index = Random.Range(0, availableSeats.Count);
        Transform seat = availableSeats[index];

        availableSeats.RemoveAt(index);
        return seat;
    }

    public bool IsFull()
    {
        return availableSeats.Count == 0;
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
    // DROP SYSTEM (2 STEP)
    // =========================

    // STEP 1: SELECT passengers
    public void SelectRandomForDrop(int count)
    {
        if (dropPrepared) return; // 🔥 prevents repeated selection

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

        dropPrepared = true; // 🔥 lock selection
    }

    // STEP 2: EXECUTE drop
    public void ExecuteDrop()
    {
        if (!dropPrepared) return; // 🔥 prevents accidental full drop

        foreach (PassengerAI p in pendingDropPassengers)
        {
            if (p == null) continue;

            RemoveOnboard(p);
            p.InstantDropOff();
        }

        pendingDropPassengers.Clear();
        dropPrepared = false; // 🔥 reset for next stop
    }
}