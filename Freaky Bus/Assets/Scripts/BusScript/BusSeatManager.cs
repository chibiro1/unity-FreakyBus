using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Handles seating, exiting and UI toggling for the bus.
/// Attach to the Bus root GameObject.
/// </summary>
public class BusSeatManager : NetworkBehaviour
{
    [Header("Seats")]
    [SerializeField] private Transform driverSeat;
    [SerializeField] private Transform conductorStandPoint;

    [Header("Exit Points")]
    [SerializeField] private Transform exitPointFront;
    [SerializeField] private Transform exitPointRear;

    [Header("References")]
    [SerializeField] private BusController busController;
    [SerializeField] private BusInputHandler busInputHandler;
    [SerializeField] private RoleManager roleManager;
    [SerializeField] private GameObject busDriverPanel;
    [SerializeField] private GameObject playerPanel;

    private ulong driverClientId = ulong.MaxValue;
    private ulong conductorClientId = ulong.MaxValue;

    public bool IsDriverSeatTaken => driverClientId != ulong.MaxValue;
    public bool IsConductorSeatTaken => conductorClientId != ulong.MaxValue;

    // ── Sit As Driver ──────────────────────────────────────────────

    public void SitAsDriver(ulong clientId)
    {
        driverClientId = clientId;
        PositionPlayerServerRpc(clientId, true);
    }

    // ── Sit As Conductor ───────────────────────────────────────────

    public void SitAsConductor(ulong clientId)
    {
        conductorClientId = clientId;
        PositionPlayerServerRpc(clientId, false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PositionPlayerServerRpc(ulong clientId, bool isDriver)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        // Disable player controller
        PlayerController pc = playerObject.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // Teleport to seat
        Transform seat = isDriver ? driverSeat : conductorStandPoint;
        playerObject.transform.position = seat.position;
        playerObject.transform.rotation = seat.rotation;

        // Enable bus controls only for driver
        if (isDriver)
            EnableDriverControlsClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            });
    }

    [ClientRpc]
    private void EnableDriverControlsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        busInputHandler.EnableInput();
        if (busDriverPanel != null) busDriverPanel.SetActive(true);
        if (playerPanel != null) playerPanel.SetActive(false);
    }

    // ── Exit Bus ───────────────────────────────────────────────────

    public void RequestExit()
    {
        ulong localId = NetworkManager.Singleton.LocalClientId;
        ExitBusServerRpc(localId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExitBusServerRpc(ulong clientId)
    {
        // Bus must be stopped to exit
        if (!busController.IsStopped) return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        // Teleport to nearest exit point
        Transform exitPoint = GetNearestExitPoint(playerObject.transform.position);
        playerObject.transform.position = exitPoint.position;

        // Re-enable player controller
        PlayerController pc = playerObject.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = true;

        // Clear seat so others can pick it
        if (driverClientId == clientId)
            driverClientId = ulong.MaxValue;
        else if (conductorClientId == clientId)
            conductorClientId = ulong.MaxValue;

        // Disable bus controls for this client
        DisableDriverControlsClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        });
    }

    [ClientRpc]
    private void DisableDriverControlsClientRpc(ClientRpcParams clientRpcParams = default)
    {
        busInputHandler.DisableInput();
        if (busDriverPanel != null) busDriverPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(true);
        if (roleManager != null) roleManager.ClearRoleText();
    }

    // ── Helpers ────────────────────────────────────────────────────

    private Transform GetNearestExitPoint(Vector3 position)
    {
        float distFront = Vector3.Distance(position, exitPointFront.position);
        float distRear = Vector3.Distance(position, exitPointRear.position);
        return distFront < distRear ? exitPointFront : exitPointRear;
    }
}