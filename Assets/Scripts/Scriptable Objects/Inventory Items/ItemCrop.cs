using UnityEngine;

/// <summary>
/// Consumable crop item that restores player hunger when eaten.
/// Automatically removes itself from inventory after consumption.
/// </summary>
[CreateAssetMenu(menuName = "Items/Crop")]
public class ItemCrop : InventoryItem
{
    [Header("Consumption Settings")]
    private float hungerRestoreAmount = 20;
    private float energyRestoreAmount = 5;
    
    public override void UseItem(PlayerController player)
    {
        // Restore hunger and consume item
        if (!player || !player.playerStats) return;
        
        player.playerStats.RestoreHunger(hungerRestoreAmount);
        player.playerStats.RestoreEnergy(energyRestoreAmount);
        NotificationSystem.ShowHelp($"Ate {name} " +
                                    $"(+{hungerRestoreAmount} hunger, +{energyRestoreAmount} energy)");
        
        player.inventorySystem.RemoveItem(this, 1);
    }
}