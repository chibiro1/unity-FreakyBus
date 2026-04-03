using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnPoint : NetworkBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;

    [Header("Settings")]
    [SerializeField] private float spawnDelay = 1.5f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Handle host immediately
        StartCoroutine(WaitForPlayerObjectThenReposition(NetworkManager.Singleton.LocalClientId));
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(WaitForPlayerObjectThenReposition(clientId));
    }

    private IEnumerator WaitForPlayerObjectThenReposition(ulong clientId)
    {
        NetworkObject playerObject = null;
        float timeout = 5f;
        float elapsed = 0f;

        while (playerObject == null && elapsed < timeout)
        {
            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
                playerObject = client.PlayerObject;

            if (playerObject == null)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        if (playerObject == null)
        {
            Debug.LogWarning($"PlayerObject for client {clientId} never became available.");
            yield break;
        }

        Rigidbody rb = playerObject.GetComponent<Rigidbody>();

        // Freeze rigidbody so player cant fall during delay
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.linearVelocity = Vector3.zero;
        }

        // Move to spawn point immediately while frozen
        int playerIndex = GetPlayerIndex(clientId);
        Transform spawnPoint = playerIndex == 0 ? spawnPoint1 : spawnPoint2;

        if (spawnPoint != null)
            playerObject.transform.position = spawnPoint.position;

        yield return new WaitForSeconds(spawnDelay);

        // Unfreeze
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        Debug.Log($"Player {clientId} repositioned to {spawnPoint?.position}");
    }

    private int GetPlayerIndex(ulong clientId)
    {
        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId == clientId) return index;
            index++;
        }
        return 0;
    }
}