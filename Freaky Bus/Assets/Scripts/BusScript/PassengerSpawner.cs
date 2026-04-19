using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PassengerSpawner : NetworkBehaviour
{
    public GameObject[] passengerPrefabs;
    public Transform spawnPoint;

    public float scatterRadius = 1.5f;

    public int minSpawn = 1;
    public int maxSpawn = 5;

    public float minCooldown = 180f;
    public float maxCooldown = 680f;

    private BusStopPas busStop;

    void Start()
    {
        busStop = GetComponent<BusStopPas>() ?? GetComponentInParent<BusStopPas>();

        if (busStop == null)
        {
            Debug.LogError("BusStopPas missing!");
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        bool firstWave = true;
        while (true)
        {
            if (!firstWave)
            {
                yield return new WaitUntil(() => busStop.CurrentPassengerCount() == 0);
                yield return new WaitForSeconds(Random.Range(minCooldown, maxCooldown));
            }

            int spawnCount = Random.Range(minSpawn, maxSpawn + 1);
            for (int i = 0; i < spawnCount; i++)
            {
                SpawnPassenger();
            }
            firstWave = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        
        if (!IsServer) return;

        busStop = GetComponent<BusStopPas>() ?? GetComponentInParent<BusStopPas>();
        if (busStop != null)
        {
            StartCoroutine(SpawnLoop());
        }
    }

    void SpawnPassenger()
    {
        int index = Random.Range(0, passengerPrefabs.Length);
        Vector3 basePos = spawnPoint ? spawnPoint.position : transform.position;
        Vector2 rand = Random.insideUnitCircle * scatterRadius;
        Vector3 pos = basePos + new Vector3(rand.x, 0, rand.y);
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        // 1. Standard Instantiate (Server only)
        GameObject obj = Instantiate(passengerPrefabs[index], pos, rot);

        // 2. NETWORK SPAWN (Crucial: This sends it to all clients)
        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn(); 
        }

        // 3. Setup AI
        PassengerAI ai = obj.GetComponent<PassengerAI>();
        if (ai != null)
        {
            ai.SetBusStop(busStop);
        }
    }
}