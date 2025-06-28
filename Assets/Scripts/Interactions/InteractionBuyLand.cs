using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Handles land expansion purchases with tier and cost requirements.
/// Manages player interaction triggers and validates purchase conditions before unlocking tilemap areas.
/// </summary>
public class InteractionBuyLand : MonoBehaviour, IInteractable
{
    [Header("Land Settings")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private int requiredTier = 2;
    [SerializeField] private int purchaseCost = 100;
    
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(this);
            NotificationSystem.ShowHelp($"Press E to buy this land for {purchaseCost}.");
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            InteractionSystem.Instance.SetCurrentInteractable(null);
        }
    }

    public void Interact(PlayerController player)
    {
        if (!CanPurchaseLand(player))
            return;
            
        CompletePurchase(player);
    }

    // Validates tier requirements and player funds
    private bool CanPurchaseLand(PlayerController player)
    {
        if (ResearchSystem.Instance.currentSeedsTier < requiredTier)
        {
            NotificationSystem.ShowHelp($"Unlock Tier {requiredTier} plants first!");
            return false;
        }
        
        if (!player.playerEconomy.CanAfford(purchaseCost))
        {
            NotificationSystem.ShowHelp($"Need {purchaseCost} coins to purchase this land!");
            return false;
        }
        
        return true;
    }

    // Processes the land purchase and unlocks the area
    private void CompletePurchase(PlayerController player)
    {
        player.playerEconomy.SpendMoney(purchaseCost);
        player.plotlandController.UnlockPlotland(targetTilemap);
        NotificationSystem.ShowHelp($"Land purchased for {purchaseCost} coins!");
        
        Destroy(gameObject);
    }
}