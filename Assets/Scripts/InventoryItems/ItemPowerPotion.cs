using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/PowerPotion")]
public class ItemPowerPotion : InventoryItem
{
    public override void UseItem(PlayerController player)
    {
        Debug.Log("Use power solution");
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}