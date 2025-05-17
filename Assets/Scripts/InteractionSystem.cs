using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    private IInteractable currentInteractable = null;   // for the IInteractable objects
    
    public void SetInteractable(IInteractable interactable)
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
            player.plotlandController.HarvestPlot(playerPosition, player);
            return;
        }
    }
}
