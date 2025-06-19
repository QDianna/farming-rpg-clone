using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Time display UI showing current season, day, year and time with real-time updates.
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

    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        timeLabel = root.Q<Label>("Time");
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
            string yearDay = $"Year {TimeSystem.Instance.GetYear()} Day {TimeSystem.Instance.GetDay()}";
            string seasonDay = $"Day {TimeSystem.Instance.GetDayInSeason()} of {TimeSystem.Instance.GetSeason()}";
            string timeInfo = FormatCurrentTime();
            
            timeLabel.text = $"{yearDay}\n{seasonDay}\n{timeInfo}";
        }
    }
    
    private string FormatCurrentTime()
    {
        int hour = TimeSystem.Instance.GetHour();
        int minute = TimeSystem.Instance.GetMinute();
        string minuteStr = minute < 10 ? $"0{minute}" : minute.ToString();
        
        return $"{hour}:{minuteStr}";
    }
}