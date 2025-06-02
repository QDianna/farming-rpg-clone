using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }
    
    private IInteractable currentInteractable = null;   // for the IInteractable objects
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SetCurrentInteractable(IInteractable interactable)
    {
        currentInteractable = interactable;
    }

    public void TryInteract(PlayerController player)
    {
        // IInteractable interaction
        if (currentInteractable != null)
        {
            currentInteractable.Interact(player);
            return;
        }

        // Direct terrain interaction
        Vector3 playerPosition = player.transform.position;
        if (player.plotlandController.CanHarvest(playerPosition))
        {
            player.animator.SetTrigger("Harvest");
            player.plotlandController.HarvestPlot(playerPosition, player);
        }
    }
}