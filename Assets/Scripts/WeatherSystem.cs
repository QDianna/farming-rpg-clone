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
    [SerializeField] private float protectionDays;
    
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
    public System.Action OnWeatherStopped;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM += GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM += StartForecastedWeather;
        }
        
        SetCurrentWeather(WeatherEvent.Clear);
    }
    
    private void Update()
    {
        CheckWeatherEndTime();
    }
    
    private void OnDestroy()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.On12PM -= GenerateTomorrowsForecast;
            TimeSystem.Instance.On8AM -= StartForecastedWeather;
        }
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
    
    public void StopWeatherOnSleep()
    {
        if (currentWeather != WeatherEvent.Clear)
            SetCurrentWeather(WeatherEvent.Clear);
        OnWeatherStopped?.Invoke();
    }
    
    private void CheckWeatherEndTime()
    {
        if (TimeSystem.Instance == null) return;
        
        int currentHour = TimeSystem.Instance.GetHour();
        
        // Stop weather at 8 PM if it's currently active
        if (currentHour >= 20 && currentWeather != WeatherEvent.Clear)
        {
            SetCurrentWeather(WeatherEvent.Clear);
            OnWeatherStopped?.Invoke();
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
  
            SetCurrentWeather(tomorrowsWeather);
            ClearForecast();
        }
    }
    
    // Calculates weather chances based on current year progression
    private WeatherEvent GenerateWeatherForSeason(Season season, int currentDay)
    {
        // First few days are always clear for tutorial
        if (currentDay <= protectionDays)
        {
            return WeatherEvent.Clear;
        }
        
        // Get current year from TimeSystem
        int currentYear = TimeSystem.Instance?.GetYear() ?? 1;
        
        // Apply yearly intensity multiplier
        float yearMultiplier = 1f + ((currentYear - 1) * yearlyIntensityIncrease);
        
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
            Season.Spring when randomValue <= adjustedDiseaseChance + adjustedStormChance * 0.5f => WeatherEvent.Storm,
            Season.Winter when randomValue <= adjustedDiseaseChance + adjustedFreezeChance => WeatherEvent.Freeze,
            Season.Autumn when randomValue <= adjustedDiseaseChance + adjustedFreezeChance * 0.5f => WeatherEvent.Freeze,
            _ => WeatherEvent.Clear
        };
    }
    
    private void ShowWeatherForecast(WeatherEvent weather)
    {
        bool hasMetWitch = QuestsSystem.Instance?.hasMetWitch ?? false;
        
        string forecastMessage = weather switch
        {
            WeatherEvent.Storm => GetStormForecastMessage(hasMetWitch),
            WeatherEvent.Freeze => GetFreezeForecastMessage(hasMetWitch),
            WeatherEvent.Disease => GetDiseaseForecastMessage(hasMetWitch),
            _ => ""
        };
        
        if (!string.IsNullOrEmpty(forecastMessage))
        {
            NotificationSystem.ShowDialogue(forecastMessage, 5f);
        }
    }
    
    private string GetStormForecastMessage(bool hasMetWitch)
    {
        return hasMetWitch
            ? "There’s something wrong in the wind again.\nIt's like the witch said... maybe there’s a potion to protect my crops?"
            : "The sky’s turning strange.\nA storm is coming... doesn't look natural at all. I need to figure out what’s behind it.";
    }

    private string GetFreezeForecastMessage(bool hasMetWitch)
    {
        return hasMetWitch
            ? "That unnatural cold is back.\nShe warned me it’s part of the same curse... I should craft something to protect the crops!"
            : "There’s a sting in the air.\nWinters were never this hard around here... I need answers.";
    }
    
    private string GetDiseaseForecastMessage(bool hasMetWitch)
    {
        return hasMetWitch
            ? "The ground’s changing again.\nIt’s that same sickness... the one hurting him. There must be a potion to heal my crops!"
            : "The soil looks sick.\nIt's like the sickness is spreading past him… into everything around us.";
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