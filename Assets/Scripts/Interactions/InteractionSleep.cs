using UnityEngine;

/// <summary>
/// Sleep interaction that skips time to the next day when activated.
/// Provides time progression functionality for day/night cycle management.
/// </summary>
public class InteractionSleep : MonoBehaviour, IInteractable
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
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
        // Attempts to skip to next day through time system
        if (TimeSystem.Instance != null && TimeSystem.Instance.CanSleep())
        {
            TimeSystem.Instance.SkipNight();
            NotificationSystem.ShowNotification("Good night! Skipping to next day...");
        }
        else
        {
            NotificationSystem.ShowNotification("You can only sleep between 6pm and 6am");
        }
    }
}