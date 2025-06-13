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
            NotificationSystem.ShowHelp("Drank speed potion, you will be running faster today!");
            player.ApplySpeedBuff(speedMultiplier);
            player.inventorySystem.RemoveItem(this, 1);
        }
        else
        {
            NotificationSystem.ShowHelp("Already drank speed potion today.");
        }
    }
    
}