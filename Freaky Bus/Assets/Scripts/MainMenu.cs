using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName = "Gameplay";

    [Header("Click Delays (seconds)")]
    [SerializeField] private float playClickDelay = 1.2f;
    [SerializeField] private float optionsClickDelay = 0.4f;
    [SerializeField] private float exitClickDelay = 0.6f;

    [Header("UI Panels")]
    [SerializeField] private GameObject optionsPanel;

    [Header("Button Feedback Stripes")]
    [SerializeField] private GameObject playStripe;
    [SerializeField] private GameObject optionsStripe;
    [SerializeField] private GameObject exitStripe;

    private bool isBusy = false;

    private void OnEnable()
    {
        ResetStripes();
        isBusy = false;
    }

    private void ResetStripes()
    {
        if (playStripe != null) playStripe.SetActive(false);
        if (optionsStripe != null) optionsStripe.SetActive(false);
        if (exitStripe != null) exitStripe.SetActive(false);
    }

    // ---------------- PLAY ----------------

    public void Play()
    {
        if (isBusy) return;

        isBusy = true;
        ResetStripes();

        if (playStripe != null)
            playStripe.SetActive(true);

        StartCoroutine(PlayWithDelay());
    }

    private IEnumerator PlayWithDelay()
    {
        yield return new WaitForSecondsRealtime(playClickDelay);
        SceneManager.LoadScene(playSceneName);
    }

    // ---------------- OPTIONS ----------------

    public void Options()
    {
        if (isBusy) return;

        isBusy = true;
        ResetStripes();

        if (optionsStripe != null)
            optionsStripe.SetActive(true);

        StartCoroutine(OpenOptionsWithDelay());
    }

    private IEnumerator OpenOptionsWithDelay()
    {
        yield return new WaitForSecondsRealtime(optionsClickDelay);
        optionsPanel.SetActive(true);
        isBusy = false;
    }

    public void CloseOptions()
    {
        ResetStripes();
        optionsPanel.SetActive(false);
        isBusy = false;
    }

    // ---------------- EXIT ----------------

    public void ExitGame()
    {
        if (isBusy) return;

        isBusy = true;
        ResetStripes();

        if (exitStripe != null)
            exitStripe.SetActive(true);

        StartCoroutine(ExitWithDelay());
    }

    private IEnumerator ExitWithDelay()
    {
        yield return new WaitForSecondsRealtime(exitClickDelay);
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
