using UnityEngine;

[CreateAssetMenu(menuName = "Items/WildPlant")]
public class ItemWildPlant : InventoryItem
{
    [Header("Wild Plant Properties")]
    [SerializeField] private float hungerRestoreAmount;
    [SerializeField] private float energyRestoreAmount;

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