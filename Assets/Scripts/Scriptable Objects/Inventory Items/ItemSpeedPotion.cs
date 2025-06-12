using UnityEngine;

/// <summary>
/// </summary>
[CreateAssetMenu(menuName = "Items/SpeedPotion")]
public class ItemSpeedPotion : InventoryItem
{
    [SerializeField] private float speedMultiplier = 1.5f;
    
    public override void UseItem(PlayerController player)
    {
        if (player.hasSpeedBuff == false)
        {
            NotificationSystem.ShowNotification("Drank speed potion, you will be running faster today!");
            player.ApplySpeedBuff(speedMultiplier);
            player.inventorySystem.RemoveItem(this, 1);
        }
        else
        {
            NotificationSystem.ShowNotification("Already drank speed potion today.");
        }
    }
    
}