using UnityEngine;

public class PlayerUIReferences : MonoBehaviour
{
    [Header("Walking Controls")]
    public FixedJoystick moveJoystick;
    public UnityEngine.UI.Button jumpButton;
    public GameObject playerPanel; 

    [Header("Driving Controls")]
    public GameObject busDriverPanel; 
}
