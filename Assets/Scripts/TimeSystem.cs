using System.Collections.Generic;
using UnityEngine;

public enum Season {
    Spring,
    Summer,
    Autumn,
    Winter
}

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }
    
    private static List<Season> seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    
    [Header("Time Settings")]
    // game starts on day 1 of Spring at 6:00 AM
    public int currentSeasonId = 0;
    private int currentDay = 1;
    private float currentTime = 6f;         // current time as float
    
    public float secondToHourRatio = 1f;    // 1 real second = secondToHourRatio in-game hours
    public int daysPerSeason = 4;           // how many days a season has
    private int hour = 6;                   // current hour as int
    private int minute = 0;                 // current minute as int
    
    public event System.Action OnDayChange;
    public event System.Action OnHourChange;
    public event System.Action OnMinuteChange;

    private void Awake()
    {
        // Ensure only one instance exists
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

        // day change
        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            
            // season change
            if (currentDay % daysPerSeason == 0)
                currentSeasonId = (currentSeasonId + 1) % seasons.Count;
            
            OnDayChange?.Invoke();
        }

        if (hour != GetHour())
        {
            hour = GetHour();
            OnHourChange?.Invoke();
        }

        if (minute != GetMinute())
        {
            minute = GetMinute();
            OnMinuteChange?.Invoke();
        }

    }

    public void skipNight()
    {
        if (isNight())
        {
            if (currentTime <= 24f)
                currentDay++;
            
            currentTime = 6f;
            OnDayChange?.Invoke();
            
            Debug.Log("Sleeping through the night...");
        }
        
        Debug.Log("You can only go to sleep between 18:00pm and 06:00am");
    }

    public bool isNight()
    {
        if (currentTime >= 18f || currentTime <= 6f)
            return true;
        return false;
    }
    
    public int GetHour() => Mathf.FloorToInt(currentTime);
    public int GetMinute() => Mathf.FloorToInt((currentTime % 1f) * 60f);
    public int GetDay()
    {
        return currentDay;
    }

    public Season GetSeason()
    {
        return seasons[currentSeasonId];
    }

    public bool isWarmSeason()
    {
        return seasons[currentSeasonId] == Season.Spring || seasons[currentSeasonId] == Season.Summer;
    }

}
