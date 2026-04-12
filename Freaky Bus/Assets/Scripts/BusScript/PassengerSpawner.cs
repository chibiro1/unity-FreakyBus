using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    public GameObject[] passengerPrefabs;
    public Transform spawnPoint;

    public float scatterRadius = 1.5f;
    public int spawnCount = 2;

    void Start()
    {
        BusStopPas busStop = GetComponent<BusStopPas>();
        if (busStop == null)
            busStop = GetComponentInParent<BusStopPas>();

        if (busStop == null)
        {
            Debug.LogError("BusStopPas missing!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
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
                continue;
            }

            // 🔥 CRITICAL FIX
            busStop.RegisterPassenger(ai);
        }
    }
}