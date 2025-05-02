using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlotlandController : MonoBehaviour
{
    public Tilemap plotTilemap;         // Tilemap-ul plotland-ului
    public TileBase soilNormalTile;     // tile pamant normal/nearat
    public TileBase soilTilledTile;     // tile pamand arat
    public TileBase soilPlantedTile;    // tile pamant cu samanta

    private Dictionary<Vector3Int, string> plantedSeeds = new();  // evidenta semintelor plantate
    private void Awake()
    {
        if (plotTilemap == null)
        {
            plotTilemap = GetComponent<Tilemap>();
        }
    }

    public void TillPlot(Vector3 worldPosition)
    {
        Vector3Int tilePosition = plotTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plotTilemap.GetTile(tilePosition);

        if (currentTile == soilNormalTile)
        {
            plotTilemap.SetTile(tilePosition, soilTilledTile);
        }
    }

    public bool CanPlant(Vector3 worldPosition)
    {
        Vector3Int tilePosition = plotTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plotTilemap.GetTile(tilePosition);

        return currentTile == soilTilledTile;
    }

    public void PlantPlot(Vector3 worldPosition, InventoryItem seed, PlayerController player)
    {
        Vector3Int tilePosition = plotTilemap.WorldToCell(worldPosition);
        TileBase currentTile = plotTilemap.GetTile(tilePosition);
        
        plotTilemap.SetTile(tilePosition, soilPlantedTile);
        
        // TODO - needs a way to keep in mind what plant was planted
        // 🌱 Save what was planted there
        if (!plantedSeeds.ContainsKey(tilePosition))
            plantedSeeds.Add(tilePosition, seed.itemName);
        else
            plantedSeeds[tilePosition] = seed.itemName;

        Debug.Log($"Planted {seed.itemName} at {tilePosition}");
        player.inventory.RemoveItem(seed, 1);
    }
    
}
