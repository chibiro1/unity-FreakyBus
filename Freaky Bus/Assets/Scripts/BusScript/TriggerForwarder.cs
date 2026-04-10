using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    private BusStopPas busStop;

    void Start()
    {
        busStop = GetComponentInParent<BusStopPas>();

        if (busStop == null)
            Debug.LogWarning("TriggerForwarder could not find BusStopPas in parent!");
        else
            Debug.Log("TriggerForwarder connected to BusStopPas successfully.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("TriggerForwarder hit by: " + other.gameObject.name);
        if (busStop != null)
            busStop.OnBusEnter(other);
    }
}