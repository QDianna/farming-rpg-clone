using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Items/SeedItem")]
public class SeedItem : InventoryItem
{
    public float growthTime;
    public List<TileBase> growthStageTiles = new(5);
    public CropItem cropItem;
    
    public override void Use(Vector3 position, PlayerController player)
    {
        if (!player.plotlandController.CanPlant(position))
        {
            Debug.Log("Can't plant here.");
            return;
        }
        
        player.plotlandController.PlantPlot(position, this, player);
        player.inventory.RemoveItem(this, 1);
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
