using UnityEngine;

/// <summary>
/// Singleton system managing player interactions with objects and terrain.
/// Handles both IInteractable objects and direct plot harvesting functionality.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }
    
    private IInteractable currentInteractable;
    
    private void Awake()
    {
        InitializeSingleton();
    }
    
    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void TryInteract(PlayerController player)
    {
        // Try interactable object first
        if (currentInteractable != null)
        {
            currentInteractable.Interact(player);
            return;
        }

        // Fallback to direct terrain interaction
        TryDirectTerrainInteraction(player);
    }
    
    // Sets up singleton instance
    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Handles direct plot harvesting when no interactable is present
    private void TryDirectTerrainInteraction(PlayerController player)
    {
        Vector3 playerPosition = player.transform.position;
        if (player.plotlandController.CanHarvest(playerPosition))
        {
            player.animator.SetTrigger("Harvest");
            player.plotlandController.HarvestPlot(playerPosition, player);
        }
    }
}