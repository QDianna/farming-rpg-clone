using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Items/SeedItem")]
public class SeedItem : InventoryItem
{
    public float growthTime;
    public TileBase plantedTile;
    public TileBase grownTile;
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
}
