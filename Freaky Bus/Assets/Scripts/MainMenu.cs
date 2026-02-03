using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string playSceneName; 

    [Header("UI Panels")]
    [SerializeField] private GameObject optionsPanel;

   
    public void Play()
    {
        SceneManager.LoadScene(playSceneName);
    }

    
    public void Options()
    {
        optionsPanel.SetActive(true);
    }

    
    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    
    public void ExitGame()
    {
        Debug.Log("Game Quit"); 
        Application.Quit();
    }
}
