using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Attach to DriverSeatTrigger child GO inside the Bus.
/// Auto-sits the player when they enter the trigger near the driver seat.
/// Requires a Collider with Is Trigger checked.
/// </summary>
public class BusSeatTrigger : MonoBehaviour
{
    [SerializeField] private BusSeatManager busSeatManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null || !pc.IsOwner) return;

        if (busSeatManager.IsDriverSeatTaken) return;

        busSeatManager.SitInDriverSeat(NetworkManager.Singleton.LocalClientId);
    }
}