using System;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Items/SeedItem")]
public class SeedItem : InventoryItem
{
    public float growthTime;
    public TileBase stage0;
    public TileBase stage1;
    public TileBase stage2;
    public TileBase stage3;
    public TileBase stage4;
    public override void Use(Vector3 position, PlayerController player)
    {
        if (!player.plotlandController.CanPlant(position))
        {
            Debug.Log("Can't plant here.");
            return;
        }
        
        player.plotlandController.PlantPlot(position, this, player);
        // player.inventory.RemoveItem(itemName, 1);
    }

    public TileBase GetStageTile(int stage)
    {
        return stage switch
        {
            0 => stage0,
            1 => stage1,
            2 => stage2,
            3 => stage3,
            _ => stage4
        };
    }
}
