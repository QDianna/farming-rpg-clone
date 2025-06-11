using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Time display UI showing current season, day, and time with real-time updates.
/// Updates automatically when time system values change through event system.
/// </summary>
public class TimeSystemHUD : MonoBehaviour
{
    private Label timeLabel;

    private void Awake()
    {
        InitializeUI();
    }

    private void Start()
    {
        SubscribeToEvents();
        UpdateDisplay();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    // Sets up UI element references
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timeLabel = root.Q<Label>("Time");
    }

    // Subscribes to time system change events
    private void SubscribeToEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += UpdateDisplay;
            TimeSystem.Instance.OnHourChange += UpdateDisplay;
            TimeSystem.Instance.OnMinuteChange += UpdateDisplay;
        }
    }

    // Unsubscribes from time system change events
    private void UnsubscribeFromEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= UpdateDisplay;
            TimeSystem.Instance.OnHourChange -= UpdateDisplay;
            TimeSystem.Instance.OnMinuteChange -= UpdateDisplay;
        }
    }

    // Updates time display with current season, day, and time
    private void UpdateDisplay()
    {
        if (timeLabel != null && TimeSystem.Instance != null)
        {
            string formattedTime = FormatCurrentTime();
            timeLabel.text = $"{TimeSystem.Instance.GetSeason()} Day {TimeSystem.Instance.GetDay()}\n{formattedTime}";
        }
    }
    
    // Formats current time with zero-padded minutes
    private string FormatCurrentTime()
    {
        int hour = TimeSystem.Instance.GetHour();
        int minute = TimeSystem.Instance.GetMinute();
        string minuteStr = minute < 10 ? $"0{minute}" : minute.ToString();
        
        return $"{hour}:{minuteStr}";
    }
}