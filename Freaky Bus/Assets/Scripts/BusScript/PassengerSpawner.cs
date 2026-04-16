using UnityEngine;
using System.Collections;

public class PassengerSpawner : MonoBehaviour
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
            // 🔥 Only wait for empty AFTER first wave
            if (!firstWave)
            {
                yield return new WaitUntil(() => busStop.CurrentPassengerCount() == 0);

                float cooldown = Random.Range(minCooldown, maxCooldown);
                yield return new WaitForSeconds(cooldown);
            }

            int spawnCount = Random.Range(minSpawn, maxSpawn + 1);

            for (int i = 0; i < spawnCount; i++)
            {
                SpawnPassenger();
            }

            firstWave = false;
        }
    }

    void SpawnPassenger()
    {
        int index = Random.Range(0, passengerPrefabs.Length);

        Vector3 basePos = spawnPoint ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint ? spawnPoint.rotation : transform.rotation;

        Vector2 rand = Random.insideUnitCircle * scatterRadius;
        Vector3 pos = basePos + new Vector3(rand.x, 0, rand.y);

        GameObject obj = Instantiate(passengerPrefabs[index], pos, rot);

        PassengerAI ai = obj.GetComponent<PassengerAI>();

        if (ai == null)
        {
            Debug.LogWarning("Missing PassengerAI!");
            return;
        }

        // 🔥 proper registration
        ai.SetBusStop(busStop);
    }
}