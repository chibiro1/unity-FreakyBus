using UnityEngine;
using System.Collections.Generic;

public class BusStopPas : MonoBehaviour
{
    private List<PassengerAI> passengers = new List<PassengerAI>();

    public void RegisterPassenger(PassengerAI passenger)
    {
        passengers.Add(passenger);
        Debug.Log("Passenger registered: " + passengers.Count + " total.");
    }

    public void OnBusEnter(Collider other)
    {
        Debug.Log("OnBusEnter called by: " + other.gameObject.name);

        PassengerSeatManager bus = other.GetComponentInParent<PassengerSeatManager>();
        if (bus == null)
        {
            Debug.Log("No SeatManager found on: " + other.gameObject.name);
            return;
        }

        Debug.Log("Bus found! Passengers to board: " + passengers.Count);

        foreach (PassengerAI passenger in passengers)
        {
            if (passenger == null) continue;
            if (bus.IsFull()) break;

            Transform seat = bus.GetAvailableSeat();
            if (seat != null)
                passenger.BoardBus(seat);
        }
    }
}