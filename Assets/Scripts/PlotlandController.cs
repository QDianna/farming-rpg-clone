using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Playables;
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
    public float growthTimer = 0;
    public int stages = 5;          // hardcoded for now?
    public int currentStage = 0;   // starting stage
}

public class PlotlandController : MonoBehaviour
{
    private Tilemap plotTilemap;            // tilemap plotland
    public Tilemap cropTilemap;             // tilemap randare plante peste pamant
    public Tilemap expansion1Tilemap;       // tilemap zona 1 extindere plotland
    public Tilemap expansion2Tilemap;       // tilemap zona 2 extindere plotland

    public TileBase lockedTile;
    public TileBase emptyTile;
    public TileBase tilledTile;             // tile pamant arat

    private Dictionary<Vector3Int, PlotData> plotStates = new();
    private void Start()
    {
        plotTilemap = GetComponent<Tilemap>();
        
        // load all tiles from Tilemap_plotland into dictionary and assign Empty state
        foreach (Vector3Int pos in plotTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = plotTilemap.GetTile(pos);
            
            if (tile == emptyTile)
            {
                plotStates[pos] = new PlotData { state = PlotState.Empty };
            }
        }
        
        // assign Locked state to all tiles from Tilemap_plotland_extension1 and Tilemap_plotland_extension2
        foreach (Vector3Int pos in expansion1Tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = expansion1Tilemap.GetTile(pos);
            
            if (tile == lockedTile)
            {
                plotStates[pos] = new PlotData { state = PlotState.Locked };
            }
        }
        
        foreach (Vector3Int pos in expansion2Tilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = expansion2Tilemap.GetTile(pos);
            
            if (tile == lockedTile)
            {
                plotStates[pos] = new PlotData { state = PlotState.Locked };
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
            plotTilemap.SetTile(pos, tilledTile);
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
        if (seedItem != null && seedItem.stage0 != null)
        {
            cropTilemap.SetTile(pos, seedItem.stage0);
        }
        
        plotStates[pos].state = PlotState.Planted;
        plotStates[pos].seedData = seed as SeedItem;
        plotStates[pos].growthTimer = 0;

        Debug.Log($"Planted {seed.itemName} at {pos}");
        player.inventory.RemoveItem(seed, 1);
    }

    // this method is called by the player controller when the player executes an interaction - buy
    public void UnlockPlot(Tilemap expansion)
    {
        foreach (Vector3Int pos in expansion.cellBounds.allPositionsWithin)
        {
            TileBase tile = expansion.GetTile(pos);
            
            if (tile == lockedTile && plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Locked)  // it should be all true
            {
                plotStates[pos].state = PlotState.Empty;
                plotTilemap.SetTile(pos, emptyTile);
                expansion.SetTile(pos, null);    // Optionally clear from expansion map
            }
        }
        
        Debug.Log($"Unlocked {expansion.name}");
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

                var growthPercent = data.growthTimer / data.seedData.growthTime;
                var newStage = Mathf.FloorToInt(Mathf.Clamp01(growthPercent) * data.stages); // gets stages 0 to 4

                if (newStage > data.currentStage)
                {
                    data.currentStage = newStage;
                    TileBase currentTile = data.seedData.GetStageTile(data.currentStage);
                    
                    cropTilemap.SetTile(pos,currentTile);
                }
            }
        }
    }
}
