using UnityEngine;
using UnityEngine.EventSystems;

public class SteeringWheelUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Settings")]
    [SerializeField] private float maxRotationAngle = 120f;
    [SerializeField] private float returnSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 centerPoint;
    private float currentAngle;
    private float targetAngle;
    private bool isHeld;

    public float SteerValue { get; private set; }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!isHeld)
        {
            targetAngle = 0f;
        }

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, returnSpeed * Time.deltaTime);
        rectTransform.localEulerAngles = new Vector3(0f, 0f, -currentAngle);
        SteerValue = Mathf.Clamp(currentAngle / maxRotationAngle, -1f, 1f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
        centerPoint = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, rectTransform.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 direction = eventData.position - centerPoint;
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        targetAngle = Mathf.Clamp(angle, -maxRotationAngle, maxRotationAngle);
    }
}
