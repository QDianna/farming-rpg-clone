using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Enables the player to purchase additional plot land by interacting with expansion zones.
/// When triggered, it calls the PlotlandController to unlock the associated expansion tilemap.
/// This component should be attached to a tilemap representing a locked land area with a trigger collider.
/// Implements the IInteractable interface to support modular interaction logic.
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
