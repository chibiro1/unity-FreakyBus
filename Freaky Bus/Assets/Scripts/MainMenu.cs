using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private string playSceneName = "Gameplay";
    [SerializeField] private float playClickDelay = 1.2f;
    [SerializeField] private float optionsClickDelay = 0.4f;
    [SerializeField] private float exitClickDelay = 0.6f;

    [SerializeField] private GameObject optionsPanel;

    private bool isBusy = false;

    private void OnEnable()
    {
        
        isBusy = false;
    }

    

    // ---------------- PLAY ----------------

    public void Play()
    {
        if (isBusy) return;

        isBusy = true;

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
        optionsPanel.SetActive(false);
        isBusy = false;
    }

    // ---------------- EXIT ----------------

    public void ExitGame()
    {
        if (isBusy) return;

        isBusy = true;

        StartCoroutine(ExitWithDelay());
    }

    private IEnumerator ExitWithDelay()
    {
        yield return new WaitForSecondsRealtime(exitClickDelay);
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
