using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemFightPotion")]
public class ItemFightPotion : InventoryItem
{
    public override void UseItem(PlayerController player)
    {
        Debug.Log("Use fight potion");
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}