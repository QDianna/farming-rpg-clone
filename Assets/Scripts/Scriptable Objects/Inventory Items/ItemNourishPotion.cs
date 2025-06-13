using UnityEngine;

/// <summary>
/// Nourish solution item that increases harvest yield for plants.
/// Can only be used on grown plants ready for harvest.
/// Provides bonus crops when harvesting.
/// </summary>
/// 
[CreateAssetMenu(menuName = "Items/NourishPotion")]
public class ItemNourishPotion : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    [SerializeField] private float bonusYieldMultiplier = 1.5f; // +50% yield
    
    public override void UseItem(PlayerController player)
    {
        if (!player.plotlandController.CanHarvest(player.transform.position))
        {
            NotificationSystem.ShowHelp("No ready crops here to nourish");
            return;
        }
        
        player.animator.SetTrigger("Plant");
        
        if (starsEffectPrefab != null)
        {
            Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        // Apply nourish effect to the plot
        player.plotlandController.ApplyNourishEffect(player.transform.position, bonusYieldMultiplier);
        player.inventorySystem.RemoveItem(this, 1);
        
        NotificationSystem.ShowHelp("Applied nourish potion - this crop will yield more when harvested!");
    }
}