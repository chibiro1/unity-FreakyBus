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

    public bool CanEnterSeat() => Time.time - lastExitTime > exitCooldown;

    private void LateUpdate()
    {
        if (!IsServer || !isPlayerSeated || driverClientId == ulong.MaxValue || seatedPlayerObject == null) return;

        seatedPlayerObject.transform.position = driverSeat.position;
        seatedPlayerObject.transform.rotation = driverSeat.rotation;
    }

    public void SitInDriverSeat()
    {
        if (IsDriverSeatTaken || !CanEnterSeat()) return;
        SitInDriverSeatServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SitInDriverSeatServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (IsDriverSeatTaken) return;

        driverClientId = clientId;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        seatedPlayerObject = playerObject;
        isPlayerSeated = true;
        isExiting = false;

        busController.StartSteerDelay();

        // Disable Player Physics/Logic
        if (playerObject.TryGetComponent<CapsuleCollider>(out var col)) col.enabled = false;
        if (playerObject.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
        if (playerObject.TryGetComponent<PlayerController>(out var pc)) pc.enabled = false;

        TeleportToSeatClientRpc(driverSeat.position, driverSeat.rotation, clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TeleportToSeatClientRpc(Vector3 position, Quaternion rotation, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer == null) return;

        // 1. ACTIVATE UI IMMEDIATELY so InputHandler can find references
        if (busDriverPanel != null) busDriverPanel.SetActive(true);
        if (playerPanel != null) playerPanel.SetActive(false);

        // 2. Handle Camera/Transform
        cachedCameraController = localPlayer.GetComponentInChildren<CameraController>();
        if (localPlayer.TryGetComponent<ClientNetworkTransform>(out var cnt)) cnt.enabled = false;

        localPlayer.transform.position = position;
        localPlayer.transform.rotation = rotation;

        if (cachedCameraController != null)
        {
            cachedCameraController.SetLookDirection(driverSeat.eulerAngles.y, 0f);
            cachedCameraController.UnlockCursor();
        }

        // 3. ENABLE INPUT (Now that UI is active)
        busInputHandler.EnableInput();
    }

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

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        if (playerObject.TryGetComponent<CapsuleCollider>(out var col)) col.enabled = true;
        if (playerObject.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;
        if (playerObject.TryGetComponent<PlayerController>(out var pc)) pc.enabled = true;

        busController.StopBus();
        ExitSeatClientRpc(exitSeatPoint.position, clientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void ExitSeatClientRpc(Vector3 exitPosition, ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayer == null) return;

        if (localPlayer.TryGetComponent<ClientNetworkTransform>(out var cnt)) cnt.enabled = true;
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