using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string playSceneName = "Gameplay";
    [SerializeField] private GameObject optionsPanel;

    // ---------- PLAY ----------
    public void Play()
    {
        SceneManager.LoadScene(playSceneName);
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
