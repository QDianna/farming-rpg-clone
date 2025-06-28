using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Notification data container with message and custom duration.
/// </summary>
[System.Serializable]
public class NotificationData
{
    public string message;
    public float duration;
    
    public NotificationData(string message, float duration)
    {
        this.message = message;
        this.duration = duration;
    }
}

/// <summary>
/// Singleton notification system with separate queues for dialogue and help messages.
/// Provides static methods for both dialogue (custom duration) and help (2s default).
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    
    private Queue<NotificationData> dialogueQueue = new Queue<NotificationData>();
    private Queue<string> helpQueue = new Queue<string>();
    
    private bool isProcessingDialogue;
    private bool isProcessingHelp;
    
    private const float HELP_DURATION = 2f;
    
    public static event Action<string, float> OnShowDialogue;
    public static event Action<string, float> OnShowHelp;

    private void Awake()
    {
        InitializeSingleton();
    }

    // Show dialogue with custom duration
    public static void ShowDialogue(string message, float duration)
    {
        Instance?.QueueDialogue(message, duration);
    }
    
    // Show help message with default 2s duration
    public static void ShowHelp(string message)
    {
        Instance?.QueueHelp(message);
    }
    
    public void DialogueFinished()
    {
        // Called by HUD when dialogue display completes
        isProcessingDialogue = false;
        ProcessNextDialogue();
    }
    
    public void HelpFinished()
    {
        // Called by HUD when help display completes
        isProcessingHelp = false;
        ProcessNextHelp();
    }
    
    // Sets up singleton instance with persistence
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Adds dialogue to queue and starts processing if idle
    private void QueueDialogue(string message, float duration)
    {
        if (dialogueQueue.Count > 0)
        {
            NotificationData last = null;
            foreach (var item in dialogueQueue)
                last = item;

            if (last != null && last.message == message)
                return; // prevent duplicate
        }

        dialogueQueue.Enqueue(new NotificationData(message, duration));

        if (!isProcessingDialogue)
        {
            ProcessNextDialogue();
        }
    }
    
    // Adds help message to queue and starts processing if idle
    private void QueueHelp(string message)
    {
        if (helpQueue.Count > 0)
        {
            string last = null;
            foreach (var item in helpQueue)
                last = item;

            if (last != null && last == message)
                return;
        }

        helpQueue.Enqueue(message);

        if (!isProcessingHelp)
        {
            ProcessNextHelp();
        }
    }
    
    // Processes next dialogue in queue if available
    private void ProcessNextDialogue()
    {
        if (dialogueQueue.Count > 0 && !isProcessingDialogue)
        {
            isProcessingDialogue = true;
            NotificationData data = dialogueQueue.Dequeue();
            OnShowDialogue?.Invoke(data.message, data.duration);
        }
    }
    
    // Processes next help message in queue if available
    private void ProcessNextHelp()
    {
        if (helpQueue.Count > 0 && !isProcessingHelp)
        {
            isProcessingHelp = true;
            string message = helpQueue.Dequeue();
            OnShowHelp?.Invoke(message, HELP_DURATION);
        }
    }
}