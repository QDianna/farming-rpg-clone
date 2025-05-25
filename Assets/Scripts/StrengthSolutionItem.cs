using UnityEngine;

/// <summary>
/// A specialized InventoryItem representing
/// </summary>

[CreateAssetMenu(menuName = "Items/StrengthSolutionItem")]
public class StrengthSolutionItem : InventoryItem
{
    [SerializeField] private GameObject starsEffectPrefab;
    public override void UseItem(PlayerController player)
    {
        if (player.plotlandController.CanAttendPlot(player.transform.position) == false)
            return;
        
        if (TimeSystem.Instance.isWarmSeason())
        {
            Debug.Log("Plant doesnt need strength in warm season");
            return;
        }
        
        player.animator.SetTrigger("Plant");
        Instantiate(starsEffectPrefab, player.transform.position, Quaternion.identity);
        
        player.plotlandController.AttendPlot(player.transform.position);
        player.inventorySystem.RemoveItem(this, 1);
    }
    
}
