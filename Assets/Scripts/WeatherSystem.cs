using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WeatherEventType
{
    Clear,
    BeneficialRain,
    Storm,
    Freeze
}

/// <summary>
/// Minimalist weather system that announces seasonal weather events.
/// 
/// Responsibilities:
/// - Determines weather based on current season and random chances
/// - Announces weather events through a clean event system
/// - Controls visual particle effects for different weather types
/// - Provides growth modifiers for temperature-based effects (freeze)
/// 
/// Design considerations:
/// - Event-driven architecture for clean separation of concerns
/// - Other systems (PlotlandController) listen and respond to weather events
/// - Seasonal weather patterns create dynamic farming challenges
/// - Singleton pattern ensures single weather authority
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    #region Singleton
    
    public static WeatherSystem Instance { get; private set; }
    
    #endregion
    
    #region Fields and Properties
    
    [Header("Weather Probability Settings")]
    [Range(0f, 1f)] public float springAutumnRainChance = 0.25f;
    [Range(0f, 1f)] public float summerStormChance = 0.15f;
    [Range(0f, 1f)] public float winterFreezeChance = 0.3f;
    
    [Header("Visual Effects")]
    public GameObject rainParticles;
    public GameObject stormParticles;
    public GameObject snowParticles;
    
    private WeatherEventType currentWeather = WeatherEventType.Clear;
    
    #endregion
    
    #region Events
    
    /// <summary>Events that other systems can subscribe to for weather responses</summary>
    public System.Action OnBeneficialRain;
    public System.Action OnStorm;
    public System.Action OnFreeze;
    public System.Action OnClearWeather;
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    private void Start()
    {
        SubscribeToTimeSystem();
        SetWeather(WeatherEventType.Clear);
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromTimeSystem();
    }
    
    #endregion
    
    #region Initialization
    
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
    
    #endregion
    
    #region Weather Logic
    
    private void CheckForWeatherEvents()
    {
        if (TimeSystem.Instance == null) return;
        
        Season currentSeason = TimeSystem.Instance.GetSeason();
        WeatherEventType newWeather = DetermineWeatherForSeason(currentSeason);
        
        SetWeather(newWeather);
    }
    
    private WeatherEventType DetermineWeatherForSeason(Season season)
    {
        float randomValue = Random.Range(0f, 1f);
        
        switch (season)
        {
            case Season.Spring:
            case Season.Autumn:
                if (randomValue <= springAutumnRainChance)
                    return WeatherEventType.BeneficialRain;
                break;
                
            case Season.Summer:
                if (randomValue <= summerStormChance)
                    return WeatherEventType.Storm;
                break;
                
            case Season.Winter:
                if (randomValue <= winterFreezeChance)
                    return WeatherEventType.Freeze;
                break;
        }
        
        return WeatherEventType.Clear;
    }
    
    private void SetWeather(WeatherEventType weather)
    {
        currentWeather = weather;
        
        UpdateVisualEffects();
        AnnounceWeatherEvent(weather);
    }
    
    private void AnnounceWeatherEvent(WeatherEventType weather)
    {
        switch (weather)
        {
            case WeatherEventType.BeneficialRain:
                Debug.Log("Weather forecast for today: Beneficial rain is nourishing the crops!");
                OnBeneficialRain?.Invoke();
                break;
                
            case WeatherEventType.Storm:
                Debug.Log("Weather forecast for today: A dangerous storm is threatening the crops!");
                OnStorm?.Invoke();
                break;
                
            case WeatherEventType.Freeze:
                Debug.Log("Weather forecast for today: Freezing temperatures slow crop growth!");
                OnFreeze?.Invoke();
                break;
                
            case WeatherEventType.Clear:
                Debug.Log("Weather forecast for today: Sunny clear sky!");
                OnClearWeather?.Invoke();
                break;
        }
    }
    
    #endregion
    
    #region Visual Effects
    
    private void UpdateVisualEffects()
    {
        // Deactivate all particle effects first
        DeactivateAllParticles();
        
        // Activate appropriate effect for current weather
        ActivateWeatherParticles();
    }
    
    private void DeactivateAllParticles()
    {
        if (rainParticles) rainParticles.SetActive(false);
        if (stormParticles) stormParticles.SetActive(false);
        if (snowParticles) snowParticles.SetActive(false);
    }
    
    private void ActivateWeatherParticles()
    {
        switch (currentWeather)
        {
            case WeatherEventType.BeneficialRain:
                if (rainParticles) rainParticles.SetActive(true);
                break;
                
            case WeatherEventType.Storm:
                if (stormParticles) stormParticles.SetActive(true);
                break;
            
            case WeatherEventType.Freeze:
                if (snowParticles) snowParticles.SetActive(true);
                break;
        }
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Returns growth modifier for temperature-based weather effects.
    /// Other weather effects are handled through events.
    /// </summary>
    public float GetGrowthModifier()
    {
        if (currentWeather == WeatherEventType.Freeze)
            return 0.5f;
        
        return 1f;
    }
    
    /// <summary>
    /// Gets the current weather type for external systems.
    /// </summary>
    public WeatherEventType GetCurrentWeather()
    {
        return currentWeather;
    }
    
    #endregion
}