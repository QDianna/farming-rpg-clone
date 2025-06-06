using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Allows players to purchase and unlock expansion land areas.
/// Attach to a tilemap with trigger collider representing purchasable land.
/// </summary>
public class InteractionBuyLand : MonoBehaviour, IInteractable
{
    [SerializeField] private Tilemap expansionTilemap;
    
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
        bool success = player.playerEconomy.CanAfford(100);
        
        if (success)
        {
            player.playerEconomy.SpendMoney(100);
            player.plotlandController.UnlockPlot(expansionTilemap);
            NotificationSystem.ShowNotification("Land purchased! New area unlocked!");
        }
        else
        {
            NotificationSystem.ShowNotification("Cannot purchase this land");
        }
    }
}