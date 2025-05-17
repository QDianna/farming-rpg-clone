using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Binds player stats to UI Toolkit progress bars, updating the HUD when hunger or health changes.
/// </summary>

public class PlayerStatsHUD : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    private ProgressBar healthBar;
    private ProgressBar hungerBar;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Link by name from UXML
        healthBar = root.Q<ProgressBar>("HealthBar");
        hungerBar = root.Q<ProgressBar>("HungerBar");

        // Initialize values
        healthBar.value = playerStats.GetHealth();
        hungerBar.value = playerStats.GetHunger();

        // Subscribe to events
        playerStats.OnHealthChange += UpdateHealthBar;
        playerStats.OnHungerChange += UpdateHungerBar;
    }

    private void OnDisable()
    {
        playerStats.OnHealthChange -= UpdateHealthBar;
        playerStats.OnHungerChange -= UpdateHungerBar;
    }

    private void UpdateHealthBar(float value)
    {
        healthBar.value = value;
    }

    private void UpdateHungerBar(float value)
    {
        hungerBar.value = value;
    }
}