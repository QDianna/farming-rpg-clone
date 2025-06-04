using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Updates health and hunger progress bars based on PlayerStats changes.
/// </summary>
public class PlayerStatsHUD : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;
    
    private ProgressBar healthBar;
    private ProgressBar hungerBar;

    private void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBar = root.Q<ProgressBar>("HealthBar");
        hungerBar = root.Q<ProgressBar>("HungerBar");
        
        if (playerStats != null)
        {
            healthBar.value = playerStats.GetHealth();
            hungerBar.value = playerStats.GetHunger();
        }
    }

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange += UpdateHealthBar;
            playerStats.OnHungerChange += UpdateHungerBar;
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChange -= UpdateHealthBar;
            playerStats.OnHungerChange -= UpdateHungerBar;
        }
    }

    private void UpdateHealthBar(float value)
    {
        if (healthBar != null) healthBar.value = value;
    }

    private void UpdateHungerBar(float value)
    {
        if (hungerBar != null) hungerBar.value = value;
    }
}