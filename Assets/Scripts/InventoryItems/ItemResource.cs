using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/Resource")]
public class ItemResource : InventoryItem
{
    public override void UseItem(PlayerController player)
    {
        Debug.Log("no direct use for resources");
    }
    
}