using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Player stats UI display managing health and hunger progress bars.
/// Updates visual indicators in real-time based on PlayerStats changes through event system.
/// </summary>
public class PlayerStatsHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private PlayerStats playerStats;
    
    private ProgressBar healthBar;
    private ProgressBar hungerBar;

    private void Awake()
    {
        InitializeUI();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    // Sets up UI element references and initial values
    private void InitializeUI()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBar = root.Q<ProgressBar>("HealthBar");
        hungerBar = root.Q<ProgressBar>("HungerBar");
        
        if (playerStats != null)
        {
            SetInitialValues();
        }
    }
    
    // Sets initial progress bar values from current stats
    private void SetInitialValues()
    {
        if (healthBar != null)
            healthBar.value = playerStats.GetHealth();
            
        if (hungerBar != null)
            hungerBar.value = playerStats.GetHunger();
    }
    
    // Subscribes to player stats change events
    private void SubscribeToEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange += UpdateHealthBar;
            playerStats.OnHungerChange += UpdateHungerBar;
        }
    }
    
    // Unsubscribes from player stats change events
    private void UnsubscribeFromEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange -= UpdateHealthBar;
            playerStats.OnHungerChange -= UpdateHungerBar;
        }
    }

    // Updates health progress bar display
    private void UpdateHealthBar(float value)
    {
        if (healthBar != null) 
            healthBar.value = value;
    }

    // Updates hunger progress bar display
    private void UpdateHungerBar(float value)
    {
        if (hungerBar != null) 
            hungerBar.value = value;
    }
}