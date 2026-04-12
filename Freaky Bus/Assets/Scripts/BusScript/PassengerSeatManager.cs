using UnityEngine;
using System.Collections.Generic;

public class PassengerSeatManager : MonoBehaviour
{
    public Transform[] seats;

    private List<Transform> availableSeats = new List<Transform>();

    void Awake()
    {
        foreach (Transform s in seats)
        {
            if (s != null)
                availableSeats.Add(s);
        }
    }

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
}