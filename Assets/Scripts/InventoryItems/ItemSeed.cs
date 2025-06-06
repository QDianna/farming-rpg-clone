using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plantable seed item with multi-stage growth progression and crop output.
/// Handles planting validation and delegates growth management to PlotlandController.
/// </summary>
[CreateAssetMenu(menuName = "Items/ItemSeed")]
public class ItemSeed : InventoryItem
{
    [Header("Growth Properties")]
    public float growthTime;
    public TileBase seedTile;
    public List<TileBase> growthStageTiles = new(4);
    public ItemCrop cropItem;
    
    public override void UseItem(PlayerController player)
    {
        Vector3 worldPos = player.transform.position;

        if (player.plotlandController.CanPlant(worldPos))
        {
            player.animator.SetTrigger("Plant");
            player.plotlandController.PlantPlot(this, worldPos);
            player.inventorySystem.RemoveItem(this, 1);
            NotificationSystem.ShowNotification($"Planted {itemName}");
        }
        else
        {
            NotificationSystem.ShowNotification("Can't plant here!");
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