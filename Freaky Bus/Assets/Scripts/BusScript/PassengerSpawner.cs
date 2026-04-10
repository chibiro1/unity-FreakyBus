using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [Header("Passenger Prefabs")]
    public GameObject[] passengerPrefabs;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    [Header("Scatter Settings")]
    public float scatterRadius = 1.5f;
    public int spawnCount = 2;

    void Start()
    {
        BusStopPas busStop = GetComponent<BusStopPas>();
        if (busStop == null)
            busStop = GetComponentInParent<BusStopPas>();

        if (busStop == null)
        {
            Debug.LogError("BusStopPas is missing on: " + gameObject.name);
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            int index = Random.Range(0, passengerPrefabs.Length);
            SpawnPassenger(passengerPrefabs[index], busStop);
        }
    }

    void SpawnPassenger(GameObject prefabToSpawn, BusStopPas busStop)
    {
        if (passengerPrefabs == null || passengerPrefabs.Length == 0)
        {
            Debug.LogWarning("No passenger prefabs assigned!");
            return;
        }

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        Vector2 randomCircle = Random.insideUnitCircle * scatterRadius;
        Vector3 scatteredPos = basePos + new Vector3(randomCircle.x, 0f, randomCircle.y);

        GameObject spawnedPassenger = Instantiate(prefabToSpawn, scatteredPos, rot);

        PassengerAI ai = spawnedPassenger.GetComponent<PassengerAI>();

        if (ai == null)
            Debug.LogWarning("PassengerAI missing on prefab: " + prefabToSpawn.name);
        else
            busStop.RegisterPassenger(ai);
    }
}