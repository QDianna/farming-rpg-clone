using UnityEngine;

/// <summary>
/// Consumable crop item that restores player hunger when eaten.
/// Automatically removes itself from inventory after consumption.
/// </summary>
[CreateAssetMenu(menuName = "Items/Crop")]
public class ItemCrop : InventoryItem
{
    [Header("Consumption Settings")]
    [SerializeField] private float hungerRestoreValue;
    
    public override void UseItem(PlayerController player)
    {
        // Restore hunger and consume item
        if (!player || !player.playerStats) return;
        
        player.playerStats.RestoreHunger(hungerRestoreValue);
        player.inventorySystem.RemoveItem(this, 1);
        NotificationSystem.ShowHelp($"Ate {name} (+{hungerRestoreValue} hunger)");
    }
}