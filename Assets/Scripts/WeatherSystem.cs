using UnityEngine;

/// <summary>
/// Weather event types for seasonal effects.
/// </summary>
public enum WeatherEvent
{
    Clear, Storm, Freeze, Disease
}

/// <summary>
/// Singleton weather system managing seasonal events, visual effects, and crop growth modifiers.
/// Weather runs in daily sessions from 8 AM to 8 PM, with forecasts generated at 12 PM for next day.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }
    
    [Header("Weather Probabilities")]
    [Range(0f, 1f)] public float summerStormChance = 0.15f;
    [Range(0f, 1f)] public float winterFreezeChance = 0.3f;
    [Range(0f, 1f)] public float diseaseChance = 0.1f; // Can happen any season
    
    [Header("Visual Effects")]
    public GameObject stormParticles;
    public GameObject snowParticles;
    public GameObject diseaseParticles; // Miasma/fog effect
    
    private WeatherEvent currentWeather = WeatherEvent.Clear;
    private WeatherEvent tomorrowsWeather = WeatherEvent.Clear;
    private bool hasForecastForTomorrow = false;
    
    public System.Action OnStorm;
    public System.Action OnFreeze;
    public System.Action OnDisease;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        SubscribeToTimeSystem();
        SetCurrentWeather(WeatherEvent.Clear);
    }
    
    private void Update()
    {
        CheckWeatherEndTime();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromTimeSystem();
    }

    public float GetGrowthModifier()
    {
        return currentWeather == WeatherEvent.Freeze ? 0.5f : 1f;
    }
    
    public WeatherEvent GetCurrentWeather()
    {
        return currentWeather;
    }
    
    public WeatherEvent GetTomorrowsWeather()
    {
        return tomorrowsWeather;
    }

    public bool HasForecastForTomorrow()
    {
        return hasForecastForTomorrow;
    }
    
    /// <summary>
    /// Called when player sleeps to stop current weather immediately
    /// </summary>
    public void StopWeatherOnSleep()
    {
        if (currentWeather != WeatherEvent.Clear)
            SetCurrentWeather(WeatherEvent.Clear);
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
    /// Subscribes to time system events for forecast and weather application
    /// </summary>
    private void SubscribeToTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM += GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM += StartForecastedWeather;
        }
    }
    
    /// <summary>
    /// Unsubscribes from time system events
    /// </summary>
    private void UnsubscribeFromTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM -= GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM -= StartForecastedWeather;
        }
    }
    
    /// <summary>
    /// Checks if weather should stop at 8 PM (20:00)
    /// </summary>
    private void CheckWeatherEndTime()
    {
        if (TimeSystem.Instance == null) return;
        
        int currentHour = TimeSystem.Instance.GetHour();
        
        // Stop weather at 8 PM if it's currently active
        if (currentHour >= 20 && currentWeather != WeatherEvent.Clear)
        {
            Debug.Log($"[Weather] 8 PM reached - Stopping weather: {currentWeather}");
            SetCurrentWeather(WeatherEvent.Clear);
        }
    }
    
    /// <summary>
    /// Generates weather forecast at 12 PM for tomorrow
    /// </summary>
    private void GenerateTomorrowsForecast()
    {
        if (TimeSystem.Instance == null) return;
        
        int currentDay = TimeSystem.Instance.GetDay();
        
        // Generate tomorrow's weather
        Season currentSeason = TimeSystem.Instance.GetSeason();
        tomorrowsWeather = GenerateWeatherForSeason(currentSeason, currentDay);
        hasForecastForTomorrow = true;
        
        Debug.Log($"[Weather] Forecast: Tomorrow (Day {currentDay + 1}) will have {tomorrowsWeather} weather from 8 AM to 8 PM");
        
        // Show forecast notification if weather isn't clear
        if (tomorrowsWeather != WeatherEvent.Clear)
        {
            ShowWeatherForecast(tomorrowsWeather);
        }
    }
    
    /// <summary>
    /// Starts forecasted weather at 8 AM
    /// </summary>
    private void StartForecastedWeather()
    {
        if (TimeSystem.Instance == null) return;
        
        if (hasForecastForTomorrow)
        {
            int currentDay = TimeSystem.Instance.GetDay();
            Debug.Log($"[Weather] 8 AM Day {currentDay} - Starting forecasted weather: {tomorrowsWeather}");
            
            SetCurrentWeather(tomorrowsWeather);
            ClearForecast();
        }
        else
        {
            Debug.Log("[Weather] 8 AM - No forecast to apply, weather remains clear");
        }
    }
    
    /// <summary>
    /// Generates weather based on season and day
    /// </summary>
    private WeatherEvent GenerateWeatherForSeason(Season season, int currentDay)
    {
        // First 3 days are always clear for tutorial
        if (currentDay <= 2) // Tomorrow will be day 2, 3, or 4
        {
            Debug.Log($"[Weather] Protection period - Clear weather for tomorrow (Day {currentDay + 1})");
            return WeatherEvent.Clear;
        }
        
        float randomValue = Random.Range(0f, 1f);
        
        // Disease can happen any season
        if (randomValue <= diseaseChance)
            return WeatherEvent.Disease;
        
        // Season-specific events
        return season switch
        {
            Season.Summer when randomValue <= diseaseChance + summerStormChance => WeatherEvent.Storm,
            Season.Winter when randomValue <= diseaseChance + winterFreezeChance => WeatherEvent.Freeze,
            _ => WeatherEvent.Clear
        };
    }
    
    /// <summary>
    /// Shows weather forecast notification with preparation advice
    /// </summary>
    private void ShowWeatherForecast(WeatherEvent weather)
    {
        bool hasMetWitch = QuestsSystem.Instance?.HasMetWitch ?? false;
        
        string forecastMessage = weather switch
        {
            WeatherEvent.Storm => GetStormForecastMessage(hasMetWitch),
            WeatherEvent.Freeze => GetFreezeForecastMessage(hasMetWitch),
            WeatherEvent.Disease => GetDiseaseForecastMessage(hasMetWitch),
            _ => ""
        };
        
        if (!string.IsNullOrEmpty(forecastMessage))
        {
            NotificationSystem.ShowDialogue(forecastMessage, 4f);
        }
    }
    
    private string GetStormForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "The air feels wrong... this isn't just a regular storm.\n" +
                   "I should prepare a Power Potion — this might harm the crops.";
        }
        else
        {
            return "Something's not right with the air... it feels heavier than any storm I've known.\n" +
                   "I need to find out what's going on — and how to protect the crops.";
        }
    }
    
    private string GetFreezeForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "This cold... it's not natural. The Witch warned me about such things.\n" +
                   "A Power Potion might help keep the crops safe.";
        }
        else
        {
            return "It's colder than it should be... and it doesn't feel normal.\n" +
                   "I should figure out what's causing this — before it hurts the plants.";
        }
    }
    
    private string GetDiseaseForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "The air feels... sick. The Witch said the land itself can be affected.\n" +
                   "I'll need Heal Potions ready, just in case the crops fall ill.";
        }
        else
        {
            return "Something feels wrong today... it's more than just the air.\n" +
                   "I should look for answers — this might affect the land itself.";
        }
    }
    
    /// <summary>
    /// Sets current weather and updates all related systems
    /// </summary>
    private void SetCurrentWeather(WeatherEvent weather)
    {
        currentWeather = weather;
        UpdateVisualEffects();
        TriggerWeatherEvent(weather);
    }
    
    /// <summary>
    /// Clears the forecast data
    /// </summary>
    private void ClearForecast()
    {
        tomorrowsWeather = WeatherEvent.Clear;
        hasForecastForTomorrow = false;
    }
    
    /// <summary>
    /// Triggers appropriate event for weather type
    /// </summary>
    private void TriggerWeatherEvent(WeatherEvent weather)
    {
        switch (weather)
        {
            case WeatherEvent.Storm: 
                OnStorm?.Invoke(); 
                break;
            case WeatherEvent.Freeze: 
                OnFreeze?.Invoke(); 
                break;
            case WeatherEvent.Disease: 
                OnDisease?.Invoke(); 
                break;
            case WeatherEvent.Clear: 
                // Clear weather - no special effects
                break;
        }
    }
    
    /// <summary>
    /// Updates particle effects based on current weather
    /// </summary>
    private void UpdateVisualEffects()
    {
        DeactivateAllEffects();
        ActivateCurrentWeatherEffect();
    }
    
    /// <summary>
    /// Deactivates all weather particle effects
    /// </summary>
    private void DeactivateAllEffects()
    {
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
        if (diseaseParticles) diseaseParticles.SetActive(false);
    }
    
    /// <summary>
    /// Activates particle effect for current weather
    /// </summary>
    private void ActivateCurrentWeatherEffect()
    {
        switch (currentWeather)
        {
            case WeatherEvent.Storm:
                if (stormParticles) stormParticles.SetActive(true);
                break;
            case WeatherEvent.Freeze:
                if (snowParticles) snowParticles.SetActive(true);
                break;
            case WeatherEvent.Disease:
                if (diseaseParticles) diseaseParticles.SetActive(true);
                break;
        }
    }
}