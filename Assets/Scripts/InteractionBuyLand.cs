using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles the plot land extension when the player interacts with plot land regions
/// that are for sale.
/// Calls PlotlandController's UnlockPlot method for the plot land tilemap that triggered
/// the interaction.
/// </summary>

public class InteractionBuyLand : MonoBehaviour, IInteractable
{
    private Tilemap expansionTilemap;

    private void Start()
    {
        expansionTilemap = GetComponent<Tilemap>();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log("Press E to buy this land!");
            controller.CurrentInteractable = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            Debug.Log("exited interaction");
            controller.CurrentInteractable = null;
        }
    }

    public void Interact(PlayerController player)
    {
        PlotlandController plotlandController = player.plotlandController;
        plotlandController.UnlockPlot(expansionTilemap);

    }
}
