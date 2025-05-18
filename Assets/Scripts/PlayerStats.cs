using UnityEngine;
using System;

/// <summary>
/// Manages the player's hunger and health, applying gradual hunger loss over time,
/// triggering health loss when starving, and notifying UI through events.
/// Also handles Game Over when health reaches zero.
/// </summary>

public class PlayerStats : MonoBehaviour
{
    public event Action<float> OnHungerChange;
    public event Action<float> OnHealthChange;

    private float hunger = 100f;
    private float health = 100f; 
    private float hungerLossRate = 5f;          // 5 units per minute
    private float hungerHealthLossRate = 10f;   // 10 units per minute
    
    public void SetHunger(float hunger)
    {
        this.hunger = Mathf.Clamp(hunger, 0, 100);
        OnHungerChange?.Invoke(this.hunger);  // anunta ascultatori (UI)
    }
    
    public void SetHealth(float health)
    {
        this.health = Mathf.Clamp(health, 0, 100);
        OnHealthChange?.Invoke(health);
        
        if (health <= 0f)
            HandleGameOver();
    }
    
    public float GetHunger() => hunger;
    public float GetHealth() => health;

    public void RestoreHunger(float amount)
    {
        SetHunger(hunger + amount);
    }
    
    void Update()
    {
        float hungerLoss = hungerLossRate / 60f * Time.deltaTime;
        SetHunger(hunger - hungerLoss);

        if (hunger <= 0f)
        {
            float healthLoss = hungerHealthLossRate / 60f * Time.deltaTime;
            SetHealth(health - healthLoss);
        }
    }
    
    private void HandleGameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0f;  // Oprește timpul (pauzează jocul)
        // TODO: Afișează UI de Game Over
    }
    
    
}
