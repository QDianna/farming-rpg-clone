using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemWildPlant")]
public class ItemWildPlant : InventoryItem
{
    [SerializeField] private GameObject droppedItemPrefab;
    public override void UseItem(PlayerController player)
    {
        NotificationSystem.ShowNotification($"{this.itemName} can be used to craft potions!");
    }

    
}