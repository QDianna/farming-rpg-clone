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
    [SerializeField] private float timeProgressionSpeed ;
    [SerializeField] private int daysPerSeason;
    [SerializeField] private int currentSeasonIndex;
    
    private static readonly List<Season> Seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    
    private int currentDay = 1;
    private float currentTime = 6f; // Start at 6 AM
    private int cachedHour = 6;
    private int cachedMinute;

    public event System.Action OnDayChange;
    public event System.Action OnHourChange;
    public event System.Action OnMinuteChange;
    public event System.Action On8AM;
    public event System.Action On12PM;

    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Update()
    {
        UpdateTime();
        UpdateTimeEvents();
    }

    /// <summary>
    /// Skips to the next day when player sleeps. Sets time to 6 AM.
    /// </summary>
    public void SkipDay()
    {
        currentDay++;
        currentTime = 6f; // Always wake up at 6 AM
        
        // Check for season change (every daysPerSeason days)
        if (currentDay > 1 && (currentDay - 1) % daysPerSeason == 0)
        {
            currentSeasonIndex = (currentSeasonIndex + 1) % Seasons.Count;
            // Debug.Log($"[TimeSystem] Season changed to: {GetSeason()}");
        }
        
        // Debug.Log($"[TimeSystem] Day skipped to: {currentDay}, Time: {GetHour()}:{GetMinute():00}");
        
        OnDayChange?.Invoke();
    }
    
    /// <summary>
    /// Checks if player can sleep (not between 6 AM and 2 PM to avoid disrupting forecast system)
    /// </summary>
    public bool CanSleep()
    {
        return currentTime < 6f || currentTime >= 14f;
    }
    
    public int GetHour() => Mathf.FloorToInt(currentTime);
    public int GetMinute() => Mathf.FloorToInt((currentTime % 1f) * 60f);
    public int GetDay() => currentDay;
    public Season GetSeason() => Seasons[currentSeasonIndex];
    
    /// <summary>
    /// Checks if current season is warm (spring or summer)
    /// </summary>
    public bool IsCurrentSeasonWarm()
    {
        var season = Seasons[currentSeasonIndex];
        return season == Season.Spring || season == Season.Summer;
    }

    /// <summary>
    /// Checks if specified season is warm
    /// </summary>
    public bool IsWarmSeason(Season season)
    {
        return season == Season.Spring || season == Season.Summer;
    }
    
    /// <summary>
    /// Sets up singleton instance
    /// </summary>
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    /// <summary>
    /// Updates current time and handles day progression
    /// </summary>
    private void UpdateTime()
    {
        currentTime += Time.deltaTime * timeProgressionSpeed;

        if (currentTime >= 24f)
        {
            AdvanceToNextDay();
        }
    }
    
    /// <summary>
    /// Advances to next day naturally (without sleeping) and handles season changes
    /// </summary>
    private void AdvanceToNextDay()
    {
        currentTime = 0f;
        currentDay++;
        
        // Check for season change (every daysPerSeason days)
        if ((currentDay - 1) % daysPerSeason == 0)
        {
            currentSeasonIndex = (currentSeasonIndex + 1) % Seasons.Count;
            // Debug.Log($"[TimeSystem] Season changed to: {GetSeason()}");
        }
        
        // Debug.Log($"[TimeSystem] Day advanced naturally to: {currentDay}");
        OnDayChange?.Invoke();
    }
    
    /// <summary>
    /// Triggers events when time values change
    /// </summary>
    private void UpdateTimeEvents()
    {
        int newHour = GetHour();
        if (cachedHour != newHour)
        {
            cachedHour = newHour;
            OnHourChange?.Invoke();
            
            // Trigger specific hour events
            if (newHour == 8)
                On8AM?.Invoke();
            
            if (newHour == 12)
                On12PM?.Invoke();
        }

        int newMinute = GetMinute();
        if (cachedMinute != newMinute)
        {
            cachedMinute = newMinute;
            OnMinuteChange?.Invoke();
        }
    }
}