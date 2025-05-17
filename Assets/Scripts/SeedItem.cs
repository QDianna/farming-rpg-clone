using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A ScriptableObject that defines a plantable seed item and its behavior.
/// 
/// Responsibilities:
/// - Defines planting behavior through the overridden Use() method
/// - Stores the growth time and visual tiles for each crop stage
/// - Provides the corresponding CropItem (harvestable output) when the plant is fully grown
/// 
/// Design considerations:
/// - Supports multi-stage growth via a list of TileBase visuals (growthStageTiles)
/// - Delegates planting logic to PlotlandController, maintaining clean separation of concerns
/// - Easily extendable: more growth stages, crop types, or custom logic can be added without altering base systems
/// 
/// This class is part of the modular inventory and farming system,
/// making seeds usable, visual, and integrable into the gameplay loop.
/// </summary>

[CreateAssetMenu(menuName = "Items/SeedItem")]
public class SeedItem : InventoryItem
{
    public float growthTime;
    public List<TileBase> growthStageTiles = new(5);
    public CropItem cropItem;
    
    public override void UseItem(PlayerController player)
    {
        Vector3 position = player.transform.position;
        if (!player.plotlandController.CanPlant(position))
        {
            Debug.Log("Can't plant here.");
            return;
        }
        
        player.plotlandController.PlantPlot(this, position);
        player.inventorySystem.RemoveItem(this, 1);
    }

    public TileBase GetStageTile(int stage)
    {
        if (stage >= growthStageTiles.Count || growthStageTiles[stage] == null)
        {
            Debug.Log("Error - growth stage not defined // no tile assigned");
            return null;
        }
        
        return growthStageTiles[stage];
    }
}
