using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton notification system for queued player message display.
/// Provides static ShowNotification() method for global message broadcasting with sequential processing.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    
    private Queue<string> notificationQueue = new Queue<string>();
    private bool isProcessingNotification;
    public float displayDuration = 2.5f;    
    public static event Action<string> OnShowNotification;

    private void Awake()
    {
        InitializeSingleton();
    }

    public static void ShowNotification(string message)
    {
        Instance?.QueueNotification(message);
    }
    
    public void NotificationFinished()
    {
        // Called by HUD when notification display completes
        isProcessingNotification = false;
        ProcessNextNotification();
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
    
    // Adds notification to queue and starts processing if idle
    private void QueueNotification(string message)
    {
        notificationQueue.Enqueue(message);
        
        if (!isProcessingNotification)
        {
            ProcessNextNotification();
        }
    }
    
    // Processes next notification in queue if available
    private void ProcessNextNotification()
    {
        if (notificationQueue.Count > 0 && !isProcessingNotification)
        {
            isProcessingNotification = true;
            string message = notificationQueue.Dequeue();
            OnShowNotification?.Invoke(message);
        }
    }
}