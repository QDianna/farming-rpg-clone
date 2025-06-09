using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Season enumeration for weather and farming systems.
/// </summary>
public enum Season
{
    Spring, Summer, Autumn, Winter
}

/// <summary>
/// Singleton time system managing day/night cycles, seasons, and time progression.
/// Supports sleep functionality and seasonal weather effects.
/// </summary>
public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }
    
    [Header("Time Settings")]
    [SerializeField] private float secondToHourRatio = 1f;
    [SerializeField] private int daysPerSeason = 4;
    
    private static readonly List<Season> seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    
    private int currentSeasonId = 0;
    private int currentDay = 1;
    private float currentTime = 6f;
    private int hour = 6;
    private int minute = 0;

    public event System.Action OnDayChange;
    public event System.Action OnHourChange;
    public event System.Action OnMinuteChange;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    private void Update()
    {
        currentTime += Time.deltaTime * secondToHourRatio;

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            
            if (currentDay % daysPerSeason == 0)
                currentSeasonId = (currentSeasonId + 1) % seasons.Count;
            
            OnDayChange?.Invoke();
        }

        int newHour = GetHour();
        if (hour != newHour)
        {
            hour = newHour;
            OnHourChange?.Invoke();
        }

        int newMinute = GetMinute();
        if (minute != newMinute)
        {
            minute = newMinute;
            OnMinuteChange?.Invoke();
        }
    }

    public void skipNight()
    {
        if (!isNight())
        {
            NotificationSystem.ShowNotification("You can only sleep at night (6PM - 6AM)");
            return;
        }
        
        if (currentTime <= 24f)
            currentDay++;
        
        currentTime = 6f;
        OnDayChange?.Invoke();
    }

    public bool isNight()
    {
        return currentTime >= 18f || currentTime <= 6f;
    }
    
    public int GetHour() => Mathf.FloorToInt(currentTime);
    public int GetMinute() => Mathf.FloorToInt((currentTime % 1f) * 60f);
    public int GetDay() => currentDay;
    public Season GetSeason() => seasons[currentSeasonId];
    
    public bool isCurrentSeasonWarm()
    {
        var season = seasons[currentSeasonId];
        return season == Season.Spring || season == Season.Summer;
    }

    public bool isWarmSeason(Season season)
    {
        return season == Season.Spring || season == Season.Summer;
    }
}