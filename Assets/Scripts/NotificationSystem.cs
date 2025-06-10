using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton notification system for displaying player messages.
/// Use NotificationSystem.ShowNotification() from anywhere to display messages.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    
    private Queue<string> notificationQueue = new Queue<string>();
    public static event Action<string> OnShowNotification;
    private bool isProcessing = false;

    private void Awake()
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

    public static void ShowNotification(string message)
    {
        Instance?.QueueNotification(message);
    }
    
    private void QueueNotification(string message)
    {
        Debug.Log("enqueue element - " + message + " - can begin processing? " + !isProcessing);
        notificationQueue.Enqueue(message);
        
        // Start processing if not already
        if (!isProcessing)
        {
            ProcessQueue();
        }
    }
    
    private void ProcessQueue()
    {
        if (notificationQueue.Count > 0 && !isProcessing)
        {
            isProcessing = true;
            string message = notificationQueue.Dequeue();
            Debug.Log("process queue element - " + message);
            Debug.Log($"is event null? {OnShowNotification == null}");
            OnShowNotification?.Invoke(message);
        }
    }
    
    public void NotificationFinished()
    {
        // Called by HUD when done displaying
        isProcessing = false;
        ProcessQueue(); // Show next one if any
    }
}