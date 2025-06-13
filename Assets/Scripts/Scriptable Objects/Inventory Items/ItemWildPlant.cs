using UnityEngine;

[CreateAssetMenu(menuName = "Items/WildPlant")]
public class ItemWildPlant : InventoryItem
{
    [Header("Wild Plant Properties")]
    [SerializeField] private float energyRestoreAmount = 20f;
    public override void UseItem(PlayerController player)
    {
        if (!player || !player.playerStats) return;
        
        player.playerStats.RestoreEnergy(energyRestoreAmount);
        player.inventorySystem.RemoveItem(this, 1);
        NotificationSystem.ShowHelp($"Ate {name} (+{energyRestoreAmount} energy)");
    }
}