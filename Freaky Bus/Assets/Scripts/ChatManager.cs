using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField chatInputField;
    public Button sendButton;
    public Transform chatContent;
    public GameObject chatMessagePrefab;

    [Header("Settings")]
    public string playerName = "Player";
    public int maxMessages = 50;

    private List<GameObject> messageObjects = new List<GameObject>();

    void Start()
    {
        // Add listeners
        sendButton.onClick.AddListener(SendMessage);
        chatInputField.onSubmit.AddListener(delegate { SendMessage(); });
    }

    void Update()
    {
        // Send message with Enter key
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (chatInputField.isFocused && !string.IsNullOrEmpty(chatInputField.text))
            {
                SendMessage();
            }
        }
    }

    public void SendMessage()
    {
        string message = chatInputField.text.Trim();

        if (string.IsNullOrEmpty(message))
            return;

        // Create the chat message
        AddMessage(playerName, message);

        // Clear input field
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    public void AddMessage(string sender, string message)
    {
        // Create new message object
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text messageText = newMessage.GetComponentInChildren<TMP_Text>();

        messageText.text = $"<b>{sender}</b>: {message}";

        messageObjects.Add(newMessage);

        // Limit message history
        if (messageObjects.Count > maxMessages)
        {
            Destroy(messageObjects[0]);
            messageObjects.RemoveAt(0);
        }

        // Scroll to bottom
        Canvas.ForceUpdateCanvases();
        chatContent.GetComponentInParent<ScrollRect>().verticalNormalizedPosition = 0f;
    }

    public void AddSystemMessage(string message)
    {
        AddMessage("System", $"<i>{message}</i>");
    }
}
