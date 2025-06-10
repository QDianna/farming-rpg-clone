using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Allows players to purchase and unlock expansion land areas.
/// Attach to a tilemap with trigger collider representing purchasable land.
/// </summary>
public class InteractionBuyLand : MonoBehaviour, IInteractable
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private int requiredTier = 2;
    [SerializeField] private int cost = 100;
    
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
        // Check if player has unlocked the required tier
        if (ResearchSystem.Instance.currentSeedsTier < requiredTier)
        {
            NotificationSystem.ShowNotification($"Unlock Tier {requiredTier} plants first!");
            return;
        }
        
        // Check if player can afford it
        if (!player.playerEconomy.CanAfford(cost))
        {
            NotificationSystem.ShowNotification($"Need {cost} coins to purchase this land!");
            return;
        }
        
        // Purchase the land
        player.playerEconomy.SpendMoney(cost);
        player.plotlandController.UnlockPlotland(tilemap);
        NotificationSystem.ShowNotification($"Land purchased for {cost} coins! You can start planting!");
        
        // Destroy the buy trigger
        Destroy(gameObject);
    }
}