using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/PowerPotion")]
public class ItemPowerPotion : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    
    public override void UseItem(PlayerController player)
    {
        player.animator.SetTrigger("Plant"); // Application animation
        
        if (starsEffectPrefab != null)
        {
            Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        // Apply farm-wide protection
        if (player.plotlandController.ApplyFarmProtection())
            player.inventorySystem.RemoveItem(this, 1);
    }
    
}