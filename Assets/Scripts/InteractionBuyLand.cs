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
    [SerializeField] private Tilemap expansionTilemap;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("Press E to buy this land!");
            InteractionSystem.Instance.SetCurrentInteractable(this);
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Debug.Log("exited interaction");
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        PlotlandController plotlandController = player.plotlandController;
        plotlandController.UnlockPlot(expansionTilemap);
    }
}
