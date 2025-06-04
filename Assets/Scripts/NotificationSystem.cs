using System;
using UnityEngine;

/// <summary>
/// Singleton notification system for displaying player messages.
/// Use NotificationSystem.ShowNotification() from anywhere to display messages.
/// </summary>
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance { get; private set; }
    public static event Action<string> OnNotificationRequested;
    
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
        OnNotificationRequested?.Invoke(message);
    }
}