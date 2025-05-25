using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static TimeSystem Instance { get; private set; }
    
    [Header("Time Settings")]
    public float currentTime = 6f;          // Start at 6:00 AM
    public float timeSpeed = 0.2f;          // 1 real second = 12 in-game minutes
    public int currentDay = 1;
    public int hour = 6;
    public int minute = 0;

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

}
