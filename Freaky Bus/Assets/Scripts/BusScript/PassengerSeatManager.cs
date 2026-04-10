using UnityEngine;
using System.Collections.Generic;

public class PassengerSeatManager : MonoBehaviour
{
    [Header("Seats")]
    public Transform[] seats;

    private List<Transform> availableSeats = new List<Transform>();

    void Awake()
    {
        // Add all seats to the available list
        foreach (Transform seat in seats)
            availableSeats.Add(seat);
    }

    public Transform GetAvailableSeat()
    {
        if (availableSeats.Count == 0) return null;

        // Pick a completely random seat from whatever is left
        int randomIndex = Random.Range(0, availableSeats.Count);
        Transform chosenSeat = availableSeats[randomIndex];

        // Remove it so no one else takes it
        availableSeats.RemoveAt(randomIndex);

        return chosenSeat;
    }

    public bool IsFull()
    {
        return availableSeats.Count == 0;
    }
}