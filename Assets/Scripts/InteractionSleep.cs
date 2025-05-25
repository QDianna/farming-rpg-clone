using UnityEngine;

public class InteractionSleep : MonoBehaviour, IInteractable
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Press E to sleep!");
            player.interactionSystem.SetInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("exited interaction");
            player.interactionSystem.SetInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (TimeSystem.Instance != null)
            TimeSystem.Instance.skipNight();
        else
        {
            Debug.Log("Error - TimeSystem Instance is null");
        }
    }
}
