using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/ItemWildPlant")]
public class ItemWildPlant : InventoryItem
{
    public override void UseItem(PlayerController player)
    {
        NotificationSystem.ShowNotification($"{this.name} can be used to craft potions!");
    }
}