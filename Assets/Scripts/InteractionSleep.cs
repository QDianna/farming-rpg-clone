using UnityEngine;

/// <summary>
/// Sleep interaction that allows players to skip to the next day via TimeSystem.
/// </summary>
public class InteractionSleep : MonoBehaviour, IInteractable
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (TimeSystem.Instance != null)
        {
            TimeSystem.Instance.skipNight();
            NotificationSystem.ShowNotification("Good night! Skipping to next day...");
        }
        else
        {
            NotificationSystem.ShowNotification("You can only sleep between 18pm and 6am");
        }
    }
}