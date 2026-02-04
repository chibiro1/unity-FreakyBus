using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public GameObject optionsPanel;    // assign in Inspector
    public GameObject mainMenuPanel;   // assign in Inspector

    public void ReturnToMainMenu()
    {
        // Play the sound
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // Start coroutine to switch panels after sound
        StartCoroutine(SwitchMenuAfterSound());
    }

    private IEnumerator SwitchMenuAfterSound()
    {
        if (clickSound != null)
            yield return new WaitForSeconds(clickSound.length);

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
}
