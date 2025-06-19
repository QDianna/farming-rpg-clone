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
    [SerializeField] private float baseStormChance;
    [SerializeField] private float baseFreezeChance;
    [SerializeField] private float baseDiseaseChance;
    [SerializeField] private float yearlyIntensityIncrease;
    public float protectionDays;
    
    [Header("Visual Effects")]
    public GameObject stormParticles;
    public GameObject snowParticles;
    public GameObject diseaseParticles;
    
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
    
    public void StopWeatherOnSleep()
    {
        if (currentWeather != WeatherEvent.Clear)
            SetCurrentWeather(WeatherEvent.Clear);
    }
    
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void SubscribeToTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM += GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM += StartForecastedWeather;
        }
    }
    
    private void UnsubscribeFromTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM -= GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM -= StartForecastedWeather;
        }
    }
    
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
    
    // Calculates weather chances based on current year progression
    private WeatherEvent GenerateWeatherForSeason(Season season, int currentDay)
    {
        // First few days are always clear for tutorial
        if (currentDay <= protectionDays)
        {
            Debug.Log($"[Weather] Protection period - Clear weather for tomorrow (Day {currentDay + 1})");
            return WeatherEvent.Clear;
        }
        
        // Get current year from TimeSystem
        int currentYear = TimeSystem.Instance?.GetYear() ?? 1;
        
        // Apply yearly intensity multiplier
        float yearMultiplier = 1f + ((currentYear - 1) * yearlyIntensityIncrease);
        
        Debug.Log($"[Weather] Day {currentDay}, Year {currentYear}, Intensity: {yearMultiplier:F1}x");
        
        // Calculate adjusted chances
        float adjustedStormChance = baseStormChance * yearMultiplier;
        float adjustedFreezeChance = baseFreezeChance * yearMultiplier;
        float adjustedDiseaseChance = baseDiseaseChance * yearMultiplier;
        
        float randomValue = Random.Range(0f, 1f);
        
        // Disease can happen any season
        if (randomValue <= adjustedDiseaseChance)
            return WeatherEvent.Disease;
        
        // Season-specific events
        return season switch
        {
            Season.Summer when randomValue <= adjustedDiseaseChance + adjustedStormChance => WeatherEvent.Storm,
            Season.Winter when randomValue <= adjustedDiseaseChance + adjustedFreezeChance => WeatherEvent.Freeze,
            _ => WeatherEvent.Clear
        };
    }
    
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
        return hasMetWitch
            ? "The air feels wrong... this isn't just a regular storm.\n" +
              "Something powerful is coming tomorrow.\n" +
              "I should prepare a Power Potion to protect the crops."
            : "Something's not right with the air... it feels heavier than any storm I've known.\n" +
              "A dangerous storm might hit tomorrow.\n" +
              "I need to find a way to shield the crops.";
    }

    private string GetFreezeForecastMessage(bool hasMetWitch)
    {
        return hasMetWitch
            ? "This cold... it's not natural. The Witch warned me about such things.\n" +
              "A freezing wave may come tomorrow.\n" +
              "I should prepare a Power Potion to protect the crops."
            : "It's colder than it should be... and it doesn't feel normal.\n" +
              "Something unnatural might hit tomorrow.\n" +
              "I need to find a way to stop the damage.";
    }

    private string GetDiseaseForecastMessage(bool hasMetWitch)
    {
        return hasMetWitch
            ? "The air feels... sick. The Witch said the land itself can be affected.\n" +
              "I do not think my crops will survive this.\n" +
              "I must prepare Heal Potions to help them."
            : "Something feels wrong today... it's more than just the air.\n" +
              "The soil might be affected too.\n" +
              "I need to understand what's happening before it's too late.";
    }

    
    private void SetCurrentWeather(WeatherEvent weather)
    {
        currentWeather = weather;
        UpdateVisualEffects();
        TriggerWeatherEvent(weather);
    }
    
    private void ClearForecast()
    {
        tomorrowsWeather = WeatherEvent.Clear;
        hasForecastForTomorrow = false;
    }
    
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
    
    private void UpdateVisualEffects()
    {
        DeactivateAllEffects();
        ActivateCurrentWeatherEffect();
    }
    
    private void DeactivateAllEffects()
    {
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
        if (diseaseParticles) diseaseParticles.SetActive(false);
    }
    
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