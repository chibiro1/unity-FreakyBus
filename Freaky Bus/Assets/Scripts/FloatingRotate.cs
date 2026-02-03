using UnityEngine;

public class FloatingRotate : MonoBehaviour
{
    [Header("Float Settings")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.5f;

    [Header("Rotation Settings")]
    public float rotationSpeedY = 50f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
       
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);

        
        transform.Rotate(0f, rotationSpeedY * Time.deltaTime, 0f);
    }
}
