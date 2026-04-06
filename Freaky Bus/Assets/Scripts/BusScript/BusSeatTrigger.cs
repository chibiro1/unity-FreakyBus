using Unity.Netcode;
using UnityEngine;

public class BusSeatTrigger : MonoBehaviour
{
    [SerializeField] private BusSeatManager busSeatManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null || !pc.IsOwner) return;

        if (busSeatManager.IsDriverSeatTaken) return;

       
        if (!busSeatManager.CanEnterSeat()) return;

        busSeatManager.SitInDriverSeat();
    }
}