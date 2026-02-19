using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("Chat Panel")]
    public GameObject chatPanel;              // Active chat panel in scene
    public GameObject chatPanelPrefab;       // Prefab to spawn if missing

    [Header("UI References")]
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent;
    public GameObject chatMessagePrefab;

    [Header("Open Chat Button")]
    public GameObject openChatButton;        // Button that opens chat

    [Header("Settings")]
    public string playerName = "Player";
    public int maxMessages = 50;
    public bool pauseGameplayWhenChatOpen = true;

    private List<GameObject> messageObjects = new List<GameObject>();

    void Start()
    {
        // If no chat panel in scene, spawn from prefab
        if (chatPanel == null)
        {
            if (chatPanelPrefab != null)
            {
                chatPanel = Instantiate(chatPanelPrefab);
            }
            else
            {
                Debug.LogError("Chat Panel Prefab is not assigned!");
                return;
            }
        }

        chatPanel.SetActive(false);

        sendButton.onClick.AddListener(SendMessage);
        chatInputField.onSubmit.AddListener(delegate { SendMessage(); });

        // Make sure open chat button is visible at start
        if (openChatButton != null)
            openChatButton.SetActive(true);
    }

    void Update()
    {
        // Optional key open
        if (Input.GetKeyDown(KeyCode.C) && !chatPanel.activeSelf)
        {
            OpenChat();
        }

        // Close with ESC
        if (chatPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChat();
        }

        // Send with Enter
        if (chatPanel.activeSelf &&
            (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (chatInputField.isFocused && !string.IsNullOrEmpty(chatInputField.text))
            {
                SendMessage();
            }
        }
    }

    public void OpenChat()
    {
        chatPanel.SetActive(true);

        // Hide open chat button
        if (openChatButton != null)
            openChatButton.SetActive(false);

        if (pauseGameplayWhenChatOpen)
            Time.timeScale = 0f;

        chatInputField.ActivateInputField();
    }

    public void CloseChat()
    {
        chatPanel.SetActive(false);

        // Show open chat button again
        if (openChatButton != null)
            openChatButton.SetActive(true);

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
