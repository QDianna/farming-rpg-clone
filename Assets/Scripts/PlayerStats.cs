using UnityEngine;
using System;

/// <summary>
/// Player vital statistics system managing hunger, health, and energy with automatic degradation.
/// Handles starvation effects, health loss, and game over conditions with UI event notifications.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("Stat Configuration")]
    [SerializeField] private float hungerLossRate = 5f;
    [SerializeField] private float starvingHealthLossRate = 10f;
    [HideInInspector] public bool hasEnduranceBuff;
    private float energyMultiplier = 1f;

    private float hunger = 100f;
    private float health = 100f;
    private float energy = 100f;
    
    public event Action<float> OnHungerChange;
    public event Action<float> OnHealthChange;
    public event Action<float> OnEnergyChange;

    private void Start()
    {
        // Subscribe to day change event
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange += RemoveEnduranceBuff;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.OnDayChange -= RemoveEnduranceBuff;
        }
    }
    
    private void Update()
    {
        UpdateHungerDegradation();
        UpdateStarvationEffects();
    }

    private void SetHunger(float newHunger)
    {
        hunger = Mathf.Clamp(newHunger, 0, 100);
        OnHungerChange?.Invoke(hunger);
    }

    private void SetHealth(float newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, 100);
        OnHealthChange?.Invoke(health);
        
        if (health <= 0f)
            HandleGameOver();
    }

    public void SetEnergy(float newEnergy)
    {
        energy = Mathf.Clamp(newEnergy, 0, 100);
        OnEnergyChange?.Invoke(energy);
    }
    
    public float GetHunger() => hunger;
    public float GetHealth() => health;
    public float GetEnergy() => energy;

    public void RestoreHunger(float amount)
    {
        SetHunger(hunger + amount);
    }

    public void RestoreEnergy(float amount)
    {
        SetEnergy(energy + amount);
    }
    
    // Applies gradual hunger loss over time
    private void UpdateHungerDegradation()
    {
        float hungerLoss = hungerLossRate / 60f * Time.deltaTime;
        SetHunger(hunger - hungerLoss);
    }
    
    // Applies health loss when starving
    private void UpdateStarvationEffects()
    {
        if (hunger <= 0f)
        {
            float healthLoss = starvingHealthLossRate / 60f * Time.deltaTime;
            SetHealth(health - healthLoss);
        }
    }
    
    // Method to apply endurance buff
    public void ApplyEnduranceBuff(float multiplier)
    {
        energyMultiplier = multiplier;
        hasEnduranceBuff = true;
    
        NotificationSystem.ShowNotification($"Energy consumption reduced by {(1f - multiplier) * 100f:F0}% for the rest of the day!");
    }
    
    // Method to remove endurance buff (called on day change)
    private void RemoveEnduranceBuff()
    {
        if (hasEnduranceBuff)
        {
            energyMultiplier = 1f;
            hasEnduranceBuff = false;
            NotificationSystem.ShowNotification("Endurance boost expired.");
        }
    }
    
    // Method to consume energy with endurance buff consideration
    public void ConsumeEnergy(float amount)
    {
        float actualConsumption = amount * energyMultiplier;
        SetEnergy(energy - actualConsumption);
    }
    
    // Handles game over condition when health reaches zero
    private void HandleGameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0f;
        // TODO: Display Game Over UI
    }
}