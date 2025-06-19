using UnityEngine;

[CreateAssetMenu(menuName = "Items/WildPlant")]
public class ItemWildPlant : InventoryItem
{
    [Header("Wild Plant Properties")]
    private float hungerRestoreAmount = 5f;
    private float energyRestoreAmount = 20f;

    public override void UseItem(PlayerController player)
    {
        if (!player || !player.playerStats) return;
        
        player.playerStats.RestoreHunger(hungerRestoreAmount);
        player.playerStats.RestoreEnergy(energyRestoreAmount);
        NotificationSystem.ShowHelp($"Ate {name} " +
                                    $"(+{hungerRestoreAmount} hunger, +{energyRestoreAmount} energy)");
        
        player.inventorySystem.RemoveItem(this, 1);
    }
}