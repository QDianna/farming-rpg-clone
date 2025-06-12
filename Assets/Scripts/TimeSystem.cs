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
/// Handles sleep functionality and provides seasonal weather information.
/// </summary>
public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }
    
    [Header("Time Settings")]
    [SerializeField] private float timeProgressionSpeed = 1f;
    [SerializeField] private int daysPerSeason = 4;
    
    private static readonly List<Season> Seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    
    private int currentSeasonIndex;
    private int currentDay = 1;
    private float currentTime = 6f;
    private int cachedHour = 6;
    private int cachedMinute;

    public event System.Action OnDayChange;
    public event System.Action OnHourChange;
    public event System.Action OnMinuteChange;

    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Update()
    {
        UpdateTime();
        UpdateTimeEvents();
    }

    public void SkipNight()
    {
        if (currentTime <= 24f)
            currentDay++;
        
        if (currentDay % daysPerSeason == 0)
            currentSeasonIndex = (currentSeasonIndex + 1) % Seasons.Count;
        
        currentTime = 6f;
        OnDayChange?.Invoke();
    }

    // Checks if player can sleep during night hours (6pm to 6am)
    public bool CanSleep()
    {
        return currentTime >= 18f || currentTime <= 6f;
    }
    
    // Time getters
    public int GetHour() => Mathf.FloorToInt(currentTime);
    public int GetMinute() => Mathf.FloorToInt((currentTime % 1f) * 60f);
    public int GetDay() => currentDay;
    public Season GetSeason() => Seasons[currentSeasonIndex];
    
    // Checks if current season is warm (spring or summer)
    public bool IsCurrentSeasonWarm()
    {
        var season = Seasons[currentSeasonIndex];
        return season == Season.Spring || season == Season.Summer;
    }

    // Checks if specified season is warm
    public bool IsWarmSeason(Season season)
    {
        return season == Season.Spring || season == Season.Summer;
    }
    
    // Sets up singleton instance
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    // Updates current time and handles day progression
    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeProgressionSpeed;

        if (currentTime >= 24f)
        {
            AdvanceToNextDay();
        }
    }
    
    // Advances to next day and handles season changes
    private void AdvanceToNextDay()
    {
        currentTime = 0f;
        currentDay++;
        
        if (currentDay % daysPerSeason == 0)
            currentSeasonIndex = (currentSeasonIndex + 1) % Seasons.Count;
        
        OnDayChange?.Invoke();
    }
    
    // Triggers events when time values change
    private void UpdateTimeEvents()
    {
        int newHour = GetHour();
        if (cachedHour != newHour)
        {
            cachedHour = newHour;
            OnHourChange?.Invoke();
        }

        int newMinute = GetMinute();
        if (cachedMinute != newMinute)
        {
            cachedMinute = newMinute;
            OnMinuteChange?.Invoke();
        }
    }
}