using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/Crop")]
public class ItemCrop : InventoryItem
{
    [SerializeField] private float hungerRestoreValue;
    public override void UseItem(PlayerController player)
    {
        player.playerStats.RestoreHunger(hungerRestoreValue);
        player.inventorySystem.RemoveItem(this, 1);
        NotificationSystem.ShowNotification($"Ate {name} (+{hungerRestoreValue} hunger)");
    }
}