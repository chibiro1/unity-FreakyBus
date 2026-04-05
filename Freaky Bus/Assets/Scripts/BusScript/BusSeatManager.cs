using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BusSeatManager : NetworkBehaviour
{
    [Header("Driver Seat")]
    [SerializeField] private Transform driverSeat;
    [SerializeField] private Transform exitSeatPoint;

    [Header("References")]
    [SerializeField] private BusController busController;
    [SerializeField] private BusInputHandler busInputHandler;

    [Header("Bus UI")]
    [SerializeField] private GameObject busDriverPanel;
    [SerializeField] private GameObject playerPanel;

    private ulong driverClientId = ulong.MaxValue;
    public bool IsDriverSeatTaken => driverClientId != ulong.MaxValue;

    private CameraController cachedCameraController;
    private NetworkObject seatedPlayerObject;
    private bool isPlayerSeated;
    private bool isExiting;

    // ── Update — server only, glues player to seat ─────────────────

    private void Update()
    {
        if (!IsServer) return;
        if (!isPlayerSeated) return;
        if (seatedPlayerObject == null) return;

        seatedPlayerObject.transform.position = driverSeat.position;
        seatedPlayerObject.transform.rotation = driverSeat.rotation;
    }

    // ── Sit in Driver Seat ─────────────────────────────────────────

    public void SitInDriverSeat(ulong clientId)
    {
        if (IsDriverSeatTaken) return;
        SitInDriverSeatServerRpc(clientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SitInDriverSeatServerRpc(ulong clientId)
    {
        if (IsDriverSeatTaken) return;

        driverClientId = clientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        seatedPlayerObject = playerObject;
        isPlayerSeated = true;
        isExiting = false;

        busController.StartSteerDelay();

        CapsuleCollider col = playerObject.GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = false;

        Rigidbody playerRb = playerObject.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.isKinematic = true;

        PlayerController pc = playerObject.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        TeleportToSeatClientRpc(driverSeat.position, driverSeat.rotation, clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TeleportToSeatClientRpc(Vector3 position, Quaternion rotation, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer == null) return;

        cachedCameraController = localPlayer.GetComponentInChildren<CameraController>();

        ClientNetworkTransform cnt = localPlayer.GetComponent<ClientNetworkTransform>();
        if (cnt != null) cnt.enabled = false;

        localPlayer.transform.position = position;
        localPlayer.transform.rotation = rotation;

        if (cachedCameraController != null)
        {
            cachedCameraController.SetLookDirection(driverSeat.eulerAngles.y, 0f);
            cachedCameraController.UnlockCursor();
        }

        busInputHandler.EnableInput();
        if (busDriverPanel != null) busDriverPanel.SetActive(true);
        if (playerPanel != null) playerPanel.SetActive(false);
    }

    // ── Exit Driver Seat ───────────────────────────────────────────

    public void ExitDriverSeat()
    {
        if (isExiting) return;
        isExiting = true;
        ExitDriverSeatServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void ExitDriverSeatServerRpc(ulong clientId)
    {
        if (driverClientId != clientId) return;

        // Clear seated state on server immediately
        isPlayerSeated = false;
        seatedPlayerObject = null;
        driverClientId = ulong.MaxValue;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        CapsuleCollider col = playerObject.GetComponent<CapsuleCollider>();
        if (col != null) col.enabled = true;

        Rigidbody playerRb = playerObject.GetComponent<Rigidbody>();
        if (playerRb != null) playerRb.isKinematic = false;

        PlayerController pc = playerObject.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = true;

        busController.StopBus();

        ExitSeatClientRpc(exitSeatPoint.position, clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExitSeatClientRpc(Vector3 exitPosition, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer == null) return;

        ClientNetworkTransform cnt = localPlayer.GetComponent<ClientNetworkTransform>();
        if (cnt != null) cnt.enabled = true;

        localPlayer.transform.position = exitPosition;

        if (cachedCameraController != null) cachedCameraController.LockCursor();
        cachedCameraController = null;

        isExiting = false;

        busInputHandler.DisableInput();
        if (busDriverPanel != null) busDriverPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(true);
    }
}