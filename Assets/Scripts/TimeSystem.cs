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
    
    [Header("Time Settings")]
    public float currentTime = 6f;          // Start at 6:00 AM
    public float timeSpeed = 0.5f;          // 1 real second = 30 in-game minutes
    
    private int currentDay = 1;
    private int hour = 6;
    private int minute = 0;
    private static List<Season> seasons = new() { Season.Spring, Season.Summer, Season.Autumn, Season.Winter };
    private int currentSeasonId = 0;
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
        currentTime += Time.deltaTime * timeSpeed;

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            currentDay++;
            if (currentDay % 2 == 0)
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
