using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// PlotState enum for clarity
public enum PlotState
{
    Locked,
    Empty,
    Tilled,
    Planted,
    Grown
}

// Data for a tile in the plot
public class PlotData
{
    public PlotState state;
    public SeedItem seedData;
    public float growthTimer;
}

public class PlotlandController : MonoBehaviour
{
    public Tilemap plotTilemap;         // Tilemap-ul plotland-ului
    public Tilemap cropTilemap;         // Tilemap-ul pentru plante, rendered on top of plotland
    
    public TileBase soilNormalTile;     // tile pamant normal/nearat
    public TileBase soilTilledTile;     // tile pamand arat

    private Dictionary<Vector3Int, PlotData> plotStates = new();
    private void Awake()
    {
        plotTilemap = GetComponent<Tilemap>();
        
        // load all plotland tiles into dictionary
        foreach (Vector3Int pos in plotTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = plotTilemap.GetTile(pos);
            if (tile == soilNormalTile)
            {
                plotStates[pos] = new PlotData { state = PlotState.Empty };
            }
        } 
        
    }

    public bool CanTill(Vector3 worldPosition)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);
        return plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Empty;
    }
    
    public void TillPlot(Vector3 worldPosition)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);
        if (CanTill(worldPosition))
        {
            plotTilemap.SetTile(pos, soilTilledTile);
            plotStates[pos].state = PlotState.Tilled;
        }
    }

    public bool CanPlant(Vector3 worldPosition)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);
        return plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Tilled;
    }


    public void PlantPlot(Vector3 worldPosition, InventoryItem seed, PlayerController player)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);

        if (!CanPlant(worldPosition)) return;

        // Render planted sprite in crop tilemap
        SeedItem seedItem = seed as SeedItem;
        if (seedItem != null && seedItem.plantedTile != null)
        {
            cropTilemap.SetTile(pos, seedItem.plantedTile);
        }
        
        plotStates[pos].state = PlotState.Planted;
        plotStates[pos].seedData = seed as SeedItem;
        plotStates[pos].growthTimer = 0;

        Debug.Log($"Planted {seed.itemName} at {pos}");
        player.inventory.RemoveItem(seed, 1);
    }


    private void Update()
    {
        foreach (var kvp in plotStates)
        {
            Vector3Int pos = kvp.Key;
            PlotData data = kvp.Value;

            if (data.state == PlotState.Planted && data.seedData != null)
            {
                data.growthTimer += Time.deltaTime;
                if (data.growthTimer >= data.seedData.growthTime)
                {
                    data.state = PlotState.Grown;
                    Debug.Log($"CROP READY @ {pos} HARVEST {data.seedData.itemName}");
                    // Update crop tile to grown sprite
                    if (data.seedData.grownTile != null)
                    {
                        cropTilemap.SetTile(pos, data.seedData.grownTile);
                    }
                }
            }
        }
    }
}
