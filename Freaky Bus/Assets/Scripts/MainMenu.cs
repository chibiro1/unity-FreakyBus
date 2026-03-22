using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Gameplay";
    [SerializeField] private SceneFader sceneFader;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject playPanel;
    [SerializeField] private GameObject joinCodePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Join Code")]
    [SerializeField] private TMP_InputField joinCodeInput;

    [Header("Loading")]
    [SerializeField] private TMP_Text loadingText;

    // ---------- MAIN PANEL ----------

    public void Play()
    {
        ShowPanel(playPanel);
    }

    public void Options()
    {
        ShowPanel(optionsPanel);
    }

    public void ExitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }

    // ---------- PLAY PANEL ----------

    public async void Host()
    {
        ShowLoading("Creating session...");

        // Netcode handles scene loading internally — no SceneFader here
        string code = await NetworkManagerSetup.Instance.HostSession(maxPlayers: 2, sceneName: gameSceneName);

        if (code == null)
        {
            Debug.LogError("Failed to create session.");
            ShowPanel(playPanel);
        }
    }

    public void Join()
    {
        ShowPanel(joinCodePanel);
    }

    // ---------- JOIN CODE PANEL ----------

    public async void ConfirmJoin()
    {
        string code = joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Please enter a session code.");
            return;
        }

        ShowLoading("Joining session...");

        bool success = await NetworkManagerSetup.Instance.JoinSession(code);

        if (success)
        {
            // Client uses SceneFader since Netcode scene load is triggered by host
            if (sceneFader != null)
                sceneFader.FadeToScene(gameSceneName);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Failed to join. Check the code and try again.");
            ShowPanel(joinCodePanel);
        }
    }

    // ---------- BACK BUTTONS ----------

    public void BackToMain()
    {
        ShowPanel(mainPanel);
    }

    public void BackToPlay()
    {
        ShowPanel(playPanel);
    }

    public void CloseOptions()
    {
        ShowPanel(mainPanel);
    }

    // ---------- HELPERS ----------

    private void ShowPanel(GameObject target)
    {
        mainPanel.SetActive(false);
        playPanel.SetActive(false);
        joinCodePanel.SetActive(false);
        optionsPanel.SetActive(false);
        loadingPanel.SetActive(false);

        if (target != null)
            target.SetActive(true);
    }

    private void ShowLoading(string message)
    {
        ShowPanel(loadingPanel);
        if (loadingText != null)
            loadingText.text = message;
    }
}