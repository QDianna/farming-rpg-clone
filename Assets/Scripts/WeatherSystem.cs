using UnityEngine;

/// <summary>
/// Weather event types for seasonal effects.
/// </summary>
public enum WeatherEvent
{
    Clear, BeneficialRain, Storm, Freeze
}

/// <summary>
/// Singleton weather system managing seasonal events, visual effects, and crop growth modifiers.
/// Generates random weather based on season probabilities and announces changes to other systems.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }
    
    [Header("Weather Probabilities")]
    [Range(0f, 1f)] public float springAutumnRainChance = 0.25f;
    [Range(0f, 1f)] public float summerStormChance = 0.15f;
    [Range(0f, 1f)] public float winterFreezeChance = 0.3f;
    
    [Header("Visual Effects")]
    public GameObject rainParticles;
    public GameObject stormParticles;
    public GameObject snowParticles;
    
    private WeatherEvent currentWeather = WeatherEvent.Clear;
    
    public System.Action OnBeneficialRain;
    public System.Action OnStorm;
    public System.Action OnFreeze;
    // public System.Action OnClearWeather;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        SubscribeToTimeSystem();
        SetWeather(WeatherEvent.Clear);
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
            TimeSystem.Instance.OnDayChange += CheckForWeatherEvents;
        }
    }
    
    // Unsubscribes from time system events
    private void UnsubscribeFromTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= CheckForWeatherEvents;
        }
    }
    
    // Evaluates and sets new weather based on current season
    private void CheckForWeatherEvents()
    {
        if (TimeSystem.Instance == null) 
            return;
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        WeatherEvent newWeather = DetermineWeatherForSeason(currentSeason);
        
        SetWeather(newWeather);
    }
    
    // Randomly determines weather based on seasonal probabilities
    private WeatherEvent DetermineWeatherForSeason(Season season)
    {
        float randomValue = Random.Range(0f, 1f);
        
        return season switch
        {
            Season.Spring or Season.Autumn when randomValue <= springAutumnRainChance => WeatherEvent.BeneficialRain,
            Season.Summer when randomValue <= summerStormChance => WeatherEvent.Storm,
            Season.Winter when randomValue <= winterFreezeChance => WeatherEvent.Freeze,
            _ => WeatherEvent.Clear
        };
    }
    
    // Sets current weather and updates all related systems
    private void SetWeather(WeatherEvent weather)
    {
        currentWeather = weather;
        UpdateVisualEffects();
        AnnounceWeatherEvent(weather);
    }
    
    // Shows notification and triggers weather events
    private void AnnounceWeatherEvent(WeatherEvent weather)
    {
        string message = GetWeatherMessage(weather);
        
        if (!string.IsNullOrEmpty(message))
        {
            NotificationSystem.ShowNotification(message);
        }
        
        TriggerWeatherEvent(weather);
    }
    
    // Gets notification message for weather event
    private string GetWeatherMessage(WeatherEvent weather)
    {
        return weather switch
        {
            WeatherEvent.BeneficialRain => "Gentle rain nourishes your crops!",
            WeatherEvent.Storm => "Storm warning! Crops may be damaged!",
            WeatherEvent.Freeze => "Freezing weather slows crop growth!",
            WeatherEvent.Clear => "Clear sunny weather today!",
            _ => ""
        };
    }
    
    // Triggers appropriate event for weather type
    private void TriggerWeatherEvent(WeatherEvent weather)
    {
        switch (weather)
        {
            case WeatherEvent.BeneficialRain: 
                OnBeneficialRain?.Invoke(); 
                break;
            case WeatherEvent.Storm: 
                OnStorm?.Invoke(); 
                break;
            case WeatherEvent.Freeze: 
                OnFreeze?.Invoke(); 
                break;
            case WeatherEvent.Clear: 
                // OnClearWeather?.Invoke(); 
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
        if (rainParticles) rainParticles.SetActive(false);
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
    }
    
    // Activates particle effect for current weather
    private void ActivateCurrentWeatherEffect()
    {
        switch (currentWeather)
        {
            case WeatherEvent.BeneficialRain:
                if (rainParticles) rainParticles.SetActive(true);
                break;
            case WeatherEvent.Storm:
                if (stormParticles) stormParticles.SetActive(true);
                break;
            case WeatherEvent.Freeze:
                if (snowParticles) snowParticles.SetActive(true);
                break;
        }
    }
}