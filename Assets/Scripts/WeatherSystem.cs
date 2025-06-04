using UnityEngine;

/// <summary>
/// Weather event types for seasonal effects.
/// </summary>
public enum WeatherEvent
{
    Clear, BeneficialRain, Storm, Freeze
}

/// <summary>
/// Singleton weather system managing seasonal events and visual effects.
/// Provides growth modifiers and announces weather changes to other systems.
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
    public System.Action OnClearWeather;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
    
    private void SubscribeToTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += CheckForWeatherEvents;
        }
    }
    
    private void UnsubscribeFromTimeSystem()
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= CheckForWeatherEvents;
        }
    }
    
    private void CheckForWeatherEvents()
    {
        if (TimeSystem.Instance == null) return;
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        WeatherEvent newWeather = DetermineWeatherForSeason(currentSeason);
        
        SetWeather(newWeather);
    }
    
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
    
    private void SetWeather(WeatherEvent weather)
    {
        currentWeather = weather;
        UpdateVisualEffects();
        AnnounceWeatherEvent(weather);
    }
    
    private void AnnounceWeatherEvent(WeatherEvent weather)
    {
        string message = weather switch
        {
            WeatherEvent.BeneficialRain => "Gentle rain nourishes your crops!",
            WeatherEvent.Storm => "Storm warning! Crops may be damaged!",
            WeatherEvent.Freeze => "Freezing weather slows crop growth!",
            WeatherEvent.Clear => "Clear sunny weather today!",
            _ => ""
        };
        
        if (!string.IsNullOrEmpty(message))
        {
            NotificationSystem.ShowNotification(message);
        }
        
        // Invoke events
        switch (weather)
        {
            case WeatherEvent.BeneficialRain: OnBeneficialRain?.Invoke(); break;
            case WeatherEvent.Storm: OnStorm?.Invoke(); break;
            case WeatherEvent.Freeze: OnFreeze?.Invoke(); break;
            case WeatherEvent.Clear: OnClearWeather?.Invoke(); break;
        }
    }
    
    private void UpdateVisualEffects()
    {
        // Deactivate all effects
        if (rainParticles) rainParticles.SetActive(false);
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
        
        // Activate current weather effect
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
    
    public float GetGrowthModifier()
    {
        return currentWeather == WeatherEvent.Freeze ? 0.5f : 1f;
    }
    
    public WeatherEvent GetCurrentWeather()
    {
        return currentWeather;
    }
}