using UnityEngine;

/// <summary>
/// Strength solution item that helps plants survive cold seasons.
/// Can only be used on valid plots during cold weather.
/// </summary>

[CreateAssetMenu(menuName = "Items/StrengthPotion")]
public class ItemStrengthPotion : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    
    public override void UseItem(PlayerController player)
    {
        if (!player.plotlandController.CanAttendPlot(player.transform.position) || TimeSystem.Instance.IsCurrentSeasonWarm())
        {
            NotificationSystem.ShowHelp("You need to apply this potion to your crops in the cold time " +
                                                "for them to sprout");
            return;
        }
        
        player.animator.SetTrigger("Plant");
        if (starsEffectPrefab != null)
        {
            Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        player.plotlandController.AttendPlot(player.transform.position);
        player.inventorySystem.RemoveItem(this, 1);
    }
}