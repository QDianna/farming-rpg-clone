using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Displays current time, day, and season information in the UI.
/// </summary>
public class TimeSystemHUD : MonoBehaviour
{
    private Label timeLabel;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timeLabel = root.Q<Label>("Time");
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

    private void SubscribeToEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += UpdateDisplay;
            TimeSystem.Instance.OnHourChange += UpdateDisplay;
            TimeSystem.Instance.OnMinuteChange += UpdateDisplay;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= UpdateDisplay;
            TimeSystem.Instance.OnHourChange -= UpdateDisplay;
            TimeSystem.Instance.OnMinuteChange -= UpdateDisplay;
        }
    }

    private void UpdateDisplay()
    {
        if (timeLabel != null && TimeSystem.Instance != null)
        {
            int minute = TimeSystem.Instance.GetMinute();
            string minuteStr = minute < 10 ? $"0{minute}" : minute.ToString();
            
            timeLabel.text = $"{TimeSystem.Instance.GetSeason()} Day {TimeSystem.Instance.GetDay()}\n" +
                             $"{TimeSystem.Instance.GetHour()}:{minuteStr}";
        }
    }
}