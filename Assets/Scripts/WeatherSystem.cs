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
/// Generates random weather based on season probabilities and announces changes to other systems.
/// Provides tomorrow's forecast so players can prepare with potions.
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
    private WeatherEvent tomorrowWeather = WeatherEvent.Clear;
    
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
        SetWeather(WeatherEvent.Clear);
        // Predict tomorrow's weather at start
        PredictTomorrowWeather();
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
    
    public WeatherEvent GetTomorrowWeather()
    {
        return tomorrowWeather;
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
    
    // Subscribes to time system day change events
    private void SubscribeToTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += HandleDayChange;
        }
    }
    
    // Unsubscribes from time system events
    private void UnsubscribeFromTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= HandleDayChange;
        }
    }
    
    // Handles day change: apply tomorrow's weather and predict next day
    private void HandleDayChange()
    {
        // Apply tomorrow's weather as today's weather
        SetWeather(tomorrowWeather);
        
        // Predict new tomorrow weather
        PredictTomorrowWeather();
    }
    
    // Predicts tomorrow's weather and shows forecast
    private void PredictTomorrowWeather()
    {
        if (TimeSystem.Instance == null) 
            return;
        
        // First 2 days are always clear to let player learn basics
        int currentDay = TimeSystem.Instance.GetDay();
        if (currentDay <= 3)
        {
            Debug.Log("first 3 days protection");
            tomorrowWeather = WeatherEvent.Clear;
            return;
        }
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        tomorrowWeather = DetermineWeatherForSeason(currentSeason);
        
        // Show forecast if it's not clear weather
        if (tomorrowWeather != WeatherEvent.Clear)
        {
            ShowWeatherForecast(tomorrowWeather);
        }
    }
    
    // Randomly determines weather based on seasonal probabilities
    private WeatherEvent DetermineWeatherForSeason(Season season)
    {
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
    
    // Shows weather forecast notification with context about the illness
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
            NotificationSystem.ShowNotification(forecastMessage);
        }
    }
    
    private string GetStormForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "The air feels unnatural... a violent storm approaches tomorrow. The witch spoke of this corruption. Power Potion might protect your crops from this unnatural fury.";
        }
        else
        {
            return "Something feels terribly wrong with the air... an unnatural storm is brewing for tomorrow. I need to find someone who understands what's happening...";
        }
    }
    
    private string GetFreezeForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "An unnatural cold spreads tomorrow... this isn't normal winter weather. The witch warned of such corruption. Power Potion could shield your plants from this twisted freeze.";
        }
        else
        {
            return "The cold tomorrow feels... wrong. This isn't natural winter weather. I need to learn more about what's causing these strange phenomena...";
        }
    }
    
    private string GetDiseaseForecastMessage(bool hasMetWitch)
    {
        if (hasMetWitch)
        {
            return "I can sense the corruption spreading... the same illness affecting the witch and my spouse will strike the crops tomorrow. Only Heal Potion can cure this blight.";
        }
        else
        {
            return "Something sickly lingers in the air... my crops may be in danger tomorrow. This feels connected to my spouse's mysterious illness somehow...";
        }
    }
    
    // Sets current weather and updates all related systems
    private void SetWeather(WeatherEvent weather)
    {
        currentWeather = weather;
        UpdateVisualEffects();
        AnnounceWeatherEvent(weather);
    }
    
    // Shows notification and triggers weather events with contextual messaging
    private void AnnounceWeatherEvent(WeatherEvent weather)
    {
        string message = GetWeatherMessage(weather);
        
        if (!string.IsNullOrEmpty(message))
        {
            NotificationSystem.ShowNotification(message);
        }
        
        TriggerWeatherEvent(weather);
    }
    
    // Gets notification message for weather event with illness context
    private string GetWeatherMessage(WeatherEvent weather)
    {
        bool hasMetWitch = QuestsSystem.Instance?.HasMetWitch ?? false;
        
        return weather switch
        {
            WeatherEvent.Storm => hasMetWitch ? 
                "The corrupted storm unleashes its fury! Your crops suffer from this unnatural tempest!" :
                "An unnaturally violent storm rages! This destruction feels wrong... what's causing this?",
            WeatherEvent.Freeze => hasMetWitch ? 
                "The twisted cold grips your farm! Plants wither under this corrupted freeze!" :
                "An unnatural freeze spreads across your land! This cold feels... diseased somehow.",
            WeatherEvent.Disease => hasMetWitch ? 
                "The mysterious illness spreads to your crops! The same corruption affecting everything around us!" :
                "A strange blight infects your plants! This looks like the same sickness affecting my spouse...",
            _ => ""
        };
    }
    
    // Triggers appropriate event for weather type
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
    
    // Updates particle effects based on current weather
    private void UpdateVisualEffects()
    {
        DeactivateAllEffects();
        ActivateCurrentWeatherEffect();
    }
    
    // Deactivates all weather particle effects
    private void DeactivateAllEffects()
    {
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
        if (diseaseParticles) diseaseParticles.SetActive(false);
    }
    
    // Activates particle effect for current weather
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