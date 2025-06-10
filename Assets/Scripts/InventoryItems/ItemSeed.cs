using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plantable seed item with multi-stage growth progression and crop output.
/// Handles planting validation and delegates growth management to PlotlandController.
/// </summary>
[CreateAssetMenu(menuName = "Items/Seed")]
public class ItemSeed : InventoryItem
{
    [Header("Properties")]
    public int tier = 1;
    public Season season;
    public ItemCrop resultedCrop;
    
    [Header("Growth settings")]
    public float growthTime;
    public List<TileBase> growthStageTiles = new(4);
    public TileBase sickStageTile;
    
    public override void UseItem(PlayerController player)
    {
        if (TimeSystem.Instance.isCurrentSeasonWarm() != TimeSystem.Instance.isWarmSeason(season))
        {
            NotificationSystem.ShowNotification($"Cannot plant {name} in this season!");
            return;
        }
        
        Vector3 worldPos = player.transform.position;

        if (player.plotlandController.CanPlant(worldPos))
        {
            player.animator.SetTrigger("Plant");
            player.plotlandController.PlantPlot(this, worldPos);
            player.inventorySystem.RemoveItem(this, 1);
        }
        else
        {
            NotificationSystem.ShowNotification("You can only plant on the plot land," +
                                                "after you've tilled the ground");
        }
    }

    public TileBase GetStageTile(int stage)
    {
        if (stage >= 0 && stage < growthStageTiles.Count && growthStageTiles[stage] != null)
        {
            return growthStageTiles[stage];
        }
        return null;
    }
}