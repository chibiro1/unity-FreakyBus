using UnityEngine;
using UnityEngine.UI;

public class ButtonStripe : MonoBehaviour
{
    public GameObject stripe;

    public void ShowStripe()
    {
        stripe.SetActive(true);
    }

    public void HideStripe()
    {
        stripe.SetActive(false);
    }
}
