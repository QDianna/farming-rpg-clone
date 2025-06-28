using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plant-able seed item with seasonal restrictions and multi-stage growth progression.
/// Handles planting validation, season checking, and integrates with PlotlandController for growth management.
/// </summary>
[CreateAssetMenu(menuName = "Items/Seed")]
public class ItemSeed : InventoryItem
{
    [Header("Seed Properties")]
    public int tier;
    public bool warmSeason;
    public ItemCrop resultedCrop;
    
    [Header("Growth Settings")]
    public float growthTime;
    public List<TileBase> growthStageTiles = new(4);
    public TileBase sickStageTile;
    
    public override void UseItem(PlayerController player)
    {
        if (!CanPlantInCurrentSeason())
        {
            NotificationSystem.ShowHelp($"You can only plant {newName} in a " + (warmSeason? "warm" : "cold") + " season!");
            return;
        }
        
        Vector3 plantPosition = player.transform.position;

        if (player.plotlandController.CanPlant(plantPosition))
        {
            PlantSeed(player, plantPosition);
        }
    }

    // Gets tile sprite for specific growth stage
    public TileBase GetStageTile(int stage)
    {
        if (IsValidStage(stage))
        {
            return growthStageTiles[stage];
        }
        return null;
    }
    
    // Checks if seed can be planted in current season
    private bool CanPlantInCurrentSeason()
    {
        return TimeSystem.Instance.IsCurrentSeasonWarm() == warmSeason;
    }
    
    // Plants the seed and triggers planting animation
    private void PlantSeed(PlayerController player, Vector3 position)
    {
        player.animator.SetTrigger("Plant");
        player.plotlandController.PlantPlot(this, position);
        player.inventorySystem.RemoveItem(this, 1);
    }
    
    // Validates growth stage index bounds
    private bool IsValidStage(int stage)
    {
        return stage >= 0 && stage < growthStageTiles.Count && growthStageTiles[stage] != null;
    }
}