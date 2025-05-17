using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;
using Vector3Int = UnityEngine.Vector3Int;

/// <summary>
/// Defines the state of a tile at a certain time.
/// </summary>
public enum PlotState
{
    Locked,
    Empty,
    Tilled,
    Planted,
    Grown
}


/// <summary>
/// Represents an entry in the plotStates dictionary of the plotland controller.
/// </summary>
public class PlotData
{
    public PlotState state;
    public SeedItem seedData;
    public float growthTimer = 0;
    public int currentGrowthStage = 0;
}


/// <summary>
/// Centralized system for managing the state and behavior of all farming plots in the game.
/// 
/// Responsibilities:
/// - Tracks tile-based plot states (locked, empty, tilled, planted, grown)
/// - Handles tilling, planting, crop growth updates, and harvesting
/// - Manages multiple tilemaps: plotland, crop visuals, and expansion zones
/// 
/// Design considerations:
/// - Uses a dictionary (plotStates) to efficiently track and update individual plot data
/// - Supports expansion of the farming area through UnlockPlot() tied to interactive zones
/// - Decouples visual updates (cropTilemap) from gameplay logic (plotTilemap)
/// - Designed for scalability: adding new crops, zones, or stages requires minimal changes
/// 
/// Crop growth is animated over time using stage-based tiles, and harvests are visually represented
/// by temporary dropped items that move toward the player. Tile-based interactions avoid unnecessary
/// collider overhead, favoring performant grid checks.
/// </summary>
public class PlotlandController : MonoBehaviour
{
    private Tilemap plotTilemap;                            // tilemap plotland
    public Tilemap cropTilemap;                             // tilemap randare plante peste pamant
    // public Tilemap expansion1Tilemap;                       // tilemap zona 1 extindere plotland
    // public Tilemap expansion2Tilemap;                       // tilemap zona 2 extindere plotland
    [SerializeField] private List<Tilemap> expansionTilemaps;

    public TileBase lockedTile;
    public TileBase emptyTile;
    public TileBase tilledTile;                             // tile pamant arat

    private Dictionary<Vector3Int, PlotData> plotStates = new();
    
    
    private void Start()
    {
        plotTilemap = GetComponent<Tilemap>();
        
        // load all tiles from Tilemap_plotland into dictionary and assign Empty state
        foreach (Vector3Int pos in plotTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = plotTilemap.GetTile(pos);
            if (tile == emptyTile)
                plotStates[pos] = new PlotData { state = PlotState.Empty };
        }
        
        // assign Locked state to all tiles from Tilemap_plotland_extension1 and Tilemap_plotland_extension2
        foreach (Tilemap expansionTilemap in expansionTilemaps)
        {
            if (expansionTilemap == null)
                continue;

            foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = expansionTilemap.GetTile(pos);
                if (tile == lockedTile)
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


    public void PlantPlot(InventoryItem seed, Vector3 worldPosition)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);

        // if (!CanPlant(worldPosition)) return;  // already called this in method that calls PlantPlot

        // Render planted sprite in crop tilemap
        SeedItem seedItem = seed as SeedItem;
        
        if (seedItem != null && seedItem.growthStageTiles[0] != null)
        {
            cropTilemap.SetTile(pos, seedItem.growthStageTiles[0]);
        
            plotStates[pos].state = PlotState.Planted;
            plotStates[pos].seedData = seed as SeedItem;
            plotStates[pos].growthTimer = 0;

            Debug.Log($"Planted {seed.itemName} at {pos}");
            return;
        }
        
        Debug.Log("Error - couldn't plant");
    }
    
    public bool CanHarvest(Vector3 worldPosition)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);
        return plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Grown;
    }
    
    public void HarvestPlot(Vector3 worldPosition, PlayerController player)
    {
        Vector3Int pos = plotTilemap.WorldToCell(worldPosition);

        if (!CanHarvest(worldPosition)) return;  // already called in method that calls HarvestPlot

        PlotData data = plotStates[pos];
        CropItem cropItem = data.seedData.cropItem;

        player.inventorySystem.AddItem(data.seedData, Random.Range(1, 3));    // get 1 or 2 seeds back
        player.inventorySystem.AddItem(cropItem, Random.Range(1, 3));         // get 1 or 2 crops from harvest
        
        // display sprite of crop item into the world to mimic harvesting
        cropItem.DisplayCrop(worldPosition, player);
        
        Debug.Log($"Harvested {data.seedData.cropItem} at {pos}");
        
        // reset crop tilemap
        cropTilemap.SetTile(pos, null);
        
        // reset plotland tilemap
        plotTilemap.SetTile(pos, emptyTile);
        data.state = PlotState.Empty;
        data.seedData = null;
        data.growthTimer = 0;
        data.currentGrowthStage = 0;
    }

    // this method is called by the player controller when the player executes an interaction - buy
    public void UnlockPlot(Tilemap expansionTilemap)
    {
        foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = expansionTilemap.GetTile(pos);
            
            if (tile == lockedTile && plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Locked)  // it should be all true
            {
                plotStates[pos].state = PlotState.Empty;
                plotTilemap.SetTile(pos, emptyTile);
                expansionTilemap.SetTile(pos, null);    // Optionally clear from expansion map
            }
        }
        
        Debug.Log($"Unlocked {expansionTilemap.name}");
    }


    private void Update()
    {
        foreach (var kvp in plotStates)
        {
            Vector3Int pos = kvp.Key;
            PlotData data = kvp.Value;

            if (data.state != PlotState.Planted || data.seedData == null)
                continue;
            
            data.growthTimer += Time.deltaTime;

            var growthPercent = Mathf.Clamp01(data.growthTimer / data.seedData.growthTime);
            var maxStage = data.seedData.growthStageTiles.Count - 1;
            var newStage = Mathf.FloorToInt(growthPercent * maxStage);

            if (newStage <= data.currentGrowthStage)
                continue;
                
            TileBase currentTile = data.seedData.GetStageTile(newStage);
            if (currentTile == null)
            {
                Debug.Log("Error updating crop growth");
                return;
            }
                    
            data.currentGrowthStage = newStage;
            // Debug.Log("New stage for " + data.seedData.itemName + " at pos " + pos + " is " + newStage);
                    
            cropTilemap.SetTile(pos, currentTile);
                
            if (newStage != maxStage)
                continue;
            
            // max stage reaached, mark as grown
            data.state = PlotState.Grown;
            Debug.Log("Crop " + data.seedData.itemName + " at pos " + pos + " is fully grown and ready to harvest!");
        }
    }
}
