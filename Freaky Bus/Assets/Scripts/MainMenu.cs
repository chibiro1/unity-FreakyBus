using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string playSceneName = "Gameplay";
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private SceneFader sceneFader;

    // ---------- PLAY ----------
    public void Play()
    {
        if (sceneFader != null)
        {
            sceneFader.FadeToScene(playSceneName);
        }
        else
        {
            Debug.LogWarning("SceneFader not assigned!");
        }
    }

    // ---------- OPTIONS ----------
    public void Options()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    // ---------- EXIT ----------
    public void ExitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
