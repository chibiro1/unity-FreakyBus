using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    public AudioSource audioSource;

    public void PlayClickSound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
