using UnityEngine;

/// <summary>
/// Strength solution item that helps plants survive cold seasons.
/// Can only be used on valid plots during cold weather.
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemStrengthSolution")]
public class StrengthSolutionItem : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    
    public override void UseItem(PlayerController player)
    {
        if (!player.plotlandController.CanAttendPlot(player.transform.position))
        {
            NotificationSystem.ShowNotification("No plants here need attention");
            return;
        }
        
        if (TimeSystem.Instance.isWarmSeason())
        {
            NotificationSystem.ShowNotification("Plants don't need strength in warm season");
            return;
        }
        
        player.animator.SetTrigger("Plant");
        
        if (starsEffectPrefab != null)
        {
            Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        }
        
        player.plotlandController.AttendPlot(player.transform.position);
        player.inventorySystem.RemoveItem(this, 1);
        NotificationSystem.ShowNotification("Applied strength solution to plants");
    }
}