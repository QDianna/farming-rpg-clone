using UnityEngine;

/// <summary>
/// </summary>
[CreateAssetMenu(menuName = "Items/EndurancePotion")]
public class ItemEndurancePotion : InventoryItem
{
    [SerializeField] private float energyMultiplier = 0.5f;
    
    public override void UseItem(PlayerController player)
    {
        if (player.playerStats.ApplyEnduranceBuff(energyMultiplier))
        {
            player.inventorySystem.RemoveItem(this, 1);
            NotificationSystem.ShowHelp("Drank endurance potion, you will be using less energy for tasks today!");
        }

    }
    
}