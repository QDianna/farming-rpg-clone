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
/// Singleton time system managing day/night cycles with separate season cycles for weather and plants.
/// Weather seasons (Summer/Winter) last 4-5 days, Plant seasons (Spring/Autumn) last 3 days.
/// </summary>
public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }
    
    [Header("Time Settings")]
    [SerializeField] private float timeProgressionSpeed;
    
    [Header("Season Settings")]
    [SerializeField] private int daysPerSeason;
    [SerializeField] private int currentSeasonIndex;
    
    private static readonly List<Season> Seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    
    private int currentDay = 1;
    private float currentTime = 6f; // Start at 6 AM
    private int cachedHour = 6;
    private int cachedMinute;
    private int daysInCurrentSeason = 0; // Track days spent in current season
    private int currentYear = 1; // Track current year

    public event System.Action OnDayChange;
    public event System.Action<float> OnSleepTimePassed;

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

    public void SkipDay(PlayerController player)
    {
        // Calculate in-game hours skipped when sleeping
        float hoursSkipped = CalculateTimeSkippedDuringSleep();
        
        currentDay++;
        currentTime = 6f;
        daysInCurrentSeason++;
        
        // Check for season change based on current season type
        CheckSeasonChange();
        
        player.playerStats.Sleep();
        
        // Signal growing elements to simulate growth during sleep
        float simulatedTime = hoursSkipped * (300f / 24f);
        OnSleepTimePassed?.Invoke(simulatedTime);
        /*// Convert in-game hours to real-time seconds equivalent
        if (player.plotlandController != null)
        {
            // If 24h in-game = 300 seconds real time, then 1h in-game = 12.5 seconds real time
            float simulatedTime = hoursSkipped * (300f / 24f); // 12.5 seconds per in-game hour
            player.plotlandController.SimulateGrowthDuringSleep(simulatedTime);
        }*/
        
        OnDayChange?.Invoke();
    }
    
    // Calculates how many hours are skipped when sleeping
    private float CalculateTimeSkippedDuringSleep()
    {
        if (currentTime <= 6f)
        {
            // If it's between midnight and 6 AM, skip to 6 AM same day
            return 6f - currentTime;
        }
        else
        {
            // If it's after 6 AM, skip to 6 AM next day
            return (24f - currentTime) + 6f;
        }
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
    public int GetYear() => currentYear;
    public int GetDayInSeason() => daysInCurrentSeason + 1; // +1 because it's 0-based
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
    /// Checks if season should change based on current season's duration
    /// </summary>
    private void CheckSeasonChange()
    {
        if (daysInCurrentSeason >= daysPerSeason)
        {
            AdvanceToNextSeason();
        }
    }
    
    /// <summary>
    /// Advances to the next season and resets day counter
    /// </summary>
    private void AdvanceToNextSeason()
    {
        Season previousSeason = GetSeason();
        currentSeasonIndex = (currentSeasonIndex + 1) % Seasons.Count;
        daysInCurrentSeason = 0;
        
        // Check if we completed a full year (4 seasons)
        if (currentSeasonIndex == 0) // Back to Spring = new year
        {
            currentYear++;
            Debug.Log($"[TimeSystem] New year started: Year {currentYear}");
        }
        
        Season newSeason = GetSeason();
        NotificationSystem.ShowDialogue($"This is the first day of {newSeason}!", 3f);
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
        daysInCurrentSeason++;
        
        // Check for season change
        CheckSeasonChange();
        
        Debug.Log($"[TimeSystem] Day advanced naturally to: {currentDay}");
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