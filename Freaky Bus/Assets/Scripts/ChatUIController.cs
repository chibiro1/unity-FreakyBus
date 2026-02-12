using UnityEngine;

public class ChatUIController : MonoBehaviour
{
    public GameObject chatPanel;

    public void OpenChat()
    {
        chatPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void CloseChat()
    {
        chatPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
