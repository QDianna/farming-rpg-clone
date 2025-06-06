using UnityEngine;

/// <summary>
/// Crop item that can be consumed to restore hunger and spawns visual drops when harvested.
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemHealingPotion")]
public class ItemHealingPotion : InventoryItem
{
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private float hungerRestoreValue;
    
    public override void UseItem(PlayerController player)
    {
        Debug.Log("Use healing potion");
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}