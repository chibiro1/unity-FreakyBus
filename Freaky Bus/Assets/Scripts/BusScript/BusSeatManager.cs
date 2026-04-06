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

    
    private float lastExitTime;
    [SerializeField] private float exitCooldown = 1f;

    public bool CanEnterSeat()
    {
        return Time.time - lastExitTime > exitCooldown;
    }

    private void LateUpdate()
    {
        if (!IsServer) return;
        if (!isPlayerSeated || driverClientId == ulong.MaxValue) return;
        if (seatedPlayerObject == null) return;

        seatedPlayerObject.transform.position = driverSeat.position;
        seatedPlayerObject.transform.rotation = driverSeat.rotation;
    }

    // ── Sit ─────────────────────────────────────────

    public void SitInDriverSeat()
    {
        if (IsDriverSeatTaken) return;
        if (!CanEnterSeat()) return;

        SitInDriverSeatServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SitInDriverSeatServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

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

        if (busDriverPanel != null) busDriverPanel.SetActive(true);
        if (playerPanel != null) playerPanel.SetActive(false);

        busInputHandler.EnableInput();
    }

    // ── Exit ─────────────────────────────────────────

    public void ExitDriverSeat()
    {
        if (isExiting) return;
        isExiting = true;

        ExitDriverSeatServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ExitDriverSeatServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (driverClientId != clientId) return;

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

        Vector3 exitPos = exitSeatPoint.position;
        ExitSeatClientRpc(exitPos, clientId);
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

        
        lastExitTime = Time.time;

        busInputHandler.DisableInput();
        if (busDriverPanel != null) busDriverPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(true);
    }
}