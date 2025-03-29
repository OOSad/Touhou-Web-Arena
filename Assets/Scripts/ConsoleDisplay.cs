using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class ConsoleDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI consoleText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxMessages = 15;
    [SerializeField] private bool showTimestamp = true;
    
    private Queue<string> messages = new Queue<string>();
    
    void Awake()
    {
        if (consoleText == null)
        {
            Debug.LogError("Console Text component not assigned!");
            enabled = false;
            return;
        }
        
        // Register to receive log messages
        Application.logMessageReceived += HandleLog;
    }
    
    void OnDestroy()
    {
        // Unregister when destroyed to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }
    
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string color = "white";
        
        // Color code different log types
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                color = "red";
                break;
            case LogType.Warning:
                color = "yellow";
                break;
            case LogType.Log:
                color = "white";
                break;
        }
        
        string timestamp = showTimestamp ? $"[{DateTime.Now.ToString("HH:mm:ss")}] " : "";
        string message = $"{timestamp}<color={color}>{logString}</color>";
        
        // Add the message to the queue
        messages.Enqueue(message);
        
        // Remove old messages if we exceed the maximum
        while (messages.Count > maxMessages)
        {
            messages.Dequeue();
        }
        
        // Update the text display
        UpdateConsoleText();
    }
    
    private void UpdateConsoleText()
    {
        // Combine all messages with new lines
        consoleText.text = string.Join("\n", messages);
        
        // Scroll to bottom to show most recent messages
        // We do this in the next frame to ensure layout has been updated
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
} 