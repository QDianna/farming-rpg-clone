using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/TheCureBeneath")]
public class ItemTheCureBeneath : InventoryItem
{
    public override void UseItem(PlayerController player)
    {
        Debug.Log("THE CURE BENEATH!!!!");
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}