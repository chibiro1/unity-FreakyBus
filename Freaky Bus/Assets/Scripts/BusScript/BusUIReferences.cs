using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BusUIReferences : MonoBehaviour
{
    public Button throttleButton;
    public Button brakeButton;
    public Button exitSeatButton;
    public Button reverseButton;
    public SteeringWheelUI steeringWheel;


    [Header("Fuel UI")]
    public Slider fuelIndicatorSId;
    public TMP_Text fuelIndicatorTxt;
    public GameObject refuelButtonUI;
    public Button refuelButtonClickable;
}