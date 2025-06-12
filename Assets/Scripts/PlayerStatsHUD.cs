using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Player stats UI display managing health, hunger, and energy progress bars.
/// Updates visual indicators in real-time based on PlayerStats changes through event system.
/// </summary>
public class PlayerStatsHUD : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private PlayerStats playerStats;
    
    private ProgressBar healthBar;
    private ProgressBar hungerBar;
    private ProgressBar energyBar;

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
        energyBar = root.Q<ProgressBar>("EnergyBar");
        
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
            
        if (energyBar != null)
            energyBar.value = playerStats.GetEnergy();
    }
    
    // Subscribes to player stats change events
    private void SubscribeToEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange += UpdateHealthBar;
            playerStats.OnHungerChange += UpdateHungerBar;
            playerStats.OnEnergyChange += UpdateEnergyBar;
        }
    }
    
    // Unsubscribes from player stats change events
    private void UnsubscribeFromEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange -= UpdateHealthBar;
            playerStats.OnHungerChange -= UpdateHungerBar;
            playerStats.OnEnergyChange -= UpdateEnergyBar;
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

    // Updates energy progress bar display
    private void UpdateEnergyBar(float value)
    {
        if (energyBar != null) 
            energyBar.value = value;
    }
}