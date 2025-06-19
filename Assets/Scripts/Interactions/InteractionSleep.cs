using UnityEngine;

/// <summary>
/// Sleep interaction that skips time to the next day when activated.
/// Stops current weather when sleeping, then weather system handles the rest.
/// </summary>
public class InteractionSleep : MonoBehaviour, IInteractable
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            NotificationSystem.ShowHelp("Press E to sleep and skip this day!");
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (TimeSystem.Instance == null) 
        {
            Debug.LogError("[InteractionSleep] TimeSystem.Instance is null!");
            return;
        }

        // Check if player can sleep (not between 6 AM and 2 PM)
        if (!TimeSystem.Instance.CanSleep())
        {
            NotificationSystem.ShowHelp("Can't sleep between 6 AM and 2 PM - too much to do during the day!");
            return;
        }

        // Show sleep message
        int currentDay = TimeSystem.Instance.GetDay();
        int nextDay = currentDay + 1;
        
        string sleepMessage = $"Good night! Sleeping until tomorrow morning (Day {nextDay})...";
        
        NotificationSystem.ShowDialogue(sleepMessage, 2f);
        
        // Stop current weather immediately when sleeping
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.StopWeatherOnSleep();
        }
        
        // Skip to next day (will wake up at 6 AM)
        TimeSystem.Instance.SkipDay(player);
        
        Debug.Log($"[InteractionSleep] Player slept from day {currentDay} to day {nextDay}");
    }
}