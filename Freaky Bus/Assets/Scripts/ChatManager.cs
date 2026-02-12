using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("Chat Panel")]
    public GameObject chatPanel;

    [Header("UI References")]
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent;
    public GameObject chatMessagePrefab;

    [Header("Settings")]
    public string playerName = "Player";
    public int maxMessages = 50;
    public bool pauseGameplayWhenChatOpen = true;

    private List<GameObject> messageObjects = new List<GameObject>();

    void Start()
    {
        // Ensure chat is hidden at start
        chatPanel.SetActive(false);

        sendButton.onClick.AddListener(SendMessage);
        chatInputField.onSubmit.AddListener(delegate { SendMessage(); });
    }

    void Update()
    {
        // Open chat (optional key, pwede mo alisin)
        if (Input.GetKeyDown(KeyCode.C) && !chatPanel.activeSelf)
        {
            OpenChat();
        }

        // Close chat with ESC
        if (chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }

        // Send message with Enter
        if (chatPanel.activeSelf &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (chatInputField.isFocused && !string.IsNullOrEmpty(chatInputField.text))
            {
                SendMessage();
            }
        }
    }

    // ===============================
    // CHAT OPEN / CLOSE
    // ===============================

    public void OpenChat()
    {
        chatPanel.SetActive(true);

        if (pauseGameplayWhenChatOpen)
            Time.timeScale = 0f;

        chatInputField.ActivateInputField();
    }

    public void CloseChat()
    {
        chatPanel.SetActive(false);

        if (pauseGameplayWhenChatOpen)
            Time.timeScale = 1f;
    }

    // ===============================
    // CHAT MESSAGE LOGIC
    // ===============================

    public void SendMessage()
    {
        string message = chatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
            return;

        AddMessage(playerName, message);

        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    public void AddMessage(string sender, string message)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text messageText = newMessage.GetComponentInChildren<TMP_Text>();

        messageText.text = $"<b>{sender}</b>: {message}";

        messageObjects.Add(newMessage);

        if (messageObjects.Count > maxMessages)
        {
            Destroy(messageObjects[0]);
            messageObjects.RemoveAt(0);
        }

        Canvas.ForceUpdateCanvases();
        chatContent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    public void AddSystemMessage(string message)
    {
        AddMessage("System", $"<i>{message}</i>");
    }
}
