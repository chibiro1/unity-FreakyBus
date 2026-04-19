using UnityEngine;
using UnityEngine.UI;

public class PlayerUIReferences : MonoBehaviour
{
    [Header("Walking Controls")]
    public FixedJoystick moveJoystick;
    public Button jumpButton;
    public GameObject playerPanel; 

    [Header("Driving Controls")]
    public GameObject busDriverPanel; 

    [Header("Sanity System")]
    public Image[] sanityBarImages;
    public Button kickOutButton;

    [Header("Feedback")]
    public CanvasGroup damageOverlay;
}
