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
    public float growthTimer;
    public int currentGrowthStage;
    public bool canStartGrowing;
}

/// <summary>
/// Centralized system for managing the state and behavior of all farming plots in the game.
/// 
/// Responsibilities:
/// - Tracks tile-based plot states (locked, empty, tilled, planted, grown)
/// - Handles tilling, planting, crop growth updates, and harvesting
/// - Manages multiple tilemaps: plotland, crop visuals, and expansion zones
/// - Responds to weather events for dynamic crop interactions
/// 
/// Design considerations:
/// - Uses a dictionary (plotStates) to efficiently track and update individual plot data
/// - Supports expansion of the farming area through UnlockPlot() tied to interactive zones
/// - Decouples visual updates (cropTilemap) from gameplay logic (plotTilemap)
/// - Event-driven weather integration for clean separation of concerns
/// </summary>
public class PlotlandController : MonoBehaviour
{
    #region Fields and Properties
    
    [Header("Tilemaps")]
    private Tilemap plotTilemap;                            // Main plotland tilemap
    public Tilemap cropTilemap;                             // Crop visuals rendered above ground
    [SerializeField] private List<Tilemap> expansionTilemaps; // Expansion zone tilemaps
    
    [Header("Tile Assets")]
    public TileBase lockedTile;
    public TileBase emptyTile;
    public TileBase tilledTile;
    
    private Dictionary<Vector3Int, PlotData> plotStates = new();
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializePlotData();
        SubscribeToWeatherEvents();
    }
    
    private void Update()
    {
        UpdateCropGrowth();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromWeatherEvents();
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializePlotData()
    {
        plotTilemap = GetComponent<Tilemap>();
        
        // Load all tiles from main plotland tilemap and assign Empty state
        foreach (Vector3Int pos in plotTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = plotTilemap.GetTile(pos);
            if (tile == emptyTile)
                plotStates[pos] = new PlotData { state = PlotState.Empty };
        }
        
        // Assign Locked state to all tiles from expansion tilemaps
        foreach (Tilemap expansionTilemap in expansionTilemaps)
        {
            if (expansionTilemap == null) continue;

            foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
            {
                TileBase tile = expansionTilemap.GetTile(pos);
                if (tile == lockedTile)
                    plotStates[pos] = new PlotData { state = PlotState.Locked };
            }
        }
    }
    
    private void SubscribeToWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain += HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm += HandleStorm;
        }
    }
    
    private void UnsubscribeFromWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain -= HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm -= HandleStorm;
        }
    }
    
    #endregion
    
    #region Plot State Checks
    
    public bool CanTill(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Empty;
    }
    
    public bool CanPlant(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Tilled;
    }
    
    public bool CanHarvest(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Grown;
    }
    
    public bool CanAttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) &&
               plotStates[tilePos].state == PlotState.Planted &&
               plotStates[tilePos].canStartGrowing == false;
    }
    
    #endregion
    
    #region Plot Actions
    
    public void TillPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        
        plotStates[tilePos].state = PlotState.Tilled;
        plotTilemap.SetTile(tilePos, tilledTile);
        
        Debug.Log($"Tilled the ground at {tilePos}");
    }
    
    public void PlantPlot(InventoryItem seed, Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        SeedItem seedItem = seed as SeedItem;
        
        if (seedItem == null) return;
        
        plotStates[tilePos].state = PlotState.Planted;
        plotStates[tilePos].seedData = seedItem;
        plotStates[tilePos].currentGrowthStage = -1;
        plotStates[tilePos].canStartGrowing = false;

        cropTilemap.SetTile(tilePos, seedItem.seedTile);
        
        Debug.Log($"Planted {seed.itemName} at {tilePos}");
    }
    
    public void AttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);

        plotStates[tilePos].canStartGrowing = true;
        plotStates[tilePos].growthTimer = 0;
        UpdateCropGrowth();
    }
    
    public void HarvestPlot(Vector3 worldPos, PlayerController player)
    {
        Vector3Int tilePos = plotTilemap.WorldToCell(worldPos);
        PlotData data = plotStates[tilePos];
        CropItem cropItem = data.seedData.cropItem;

        // Give items to player
        player.inventorySystem.AddItem(data.seedData, Random.Range(1, 3));    // 1-2 seeds back
        player.inventorySystem.AddItem(cropItem, Random.Range(1, 3));         // 1-2 crops from harvest
        
        // Display harvest animation
        cropItem.DisplayCrop(worldPos, player);
        
        Debug.Log($"Harvested {data.seedData.cropItem} at {tilePos}");
        
        // Reset plot to empty state
        ResetPlotToEmpty(tilePos, data);
    }
    
    private void ResetPlotToEmpty(Vector3Int tilePos, PlotData data)
    {
        cropTilemap.SetTile(tilePos, null);
        plotTilemap.SetTile(tilePos, emptyTile);
        
        data.state = PlotState.Empty;
        data.seedData = null;
        data.growthTimer = 0;
        data.currentGrowthStage = 0;
        data.canStartGrowing = false;
    }
    
    #endregion
    
    #region Crop Growth System
    
    public void UpdateCropGrowth()
    {
        // Get winter growth modifier (0.5x in winter, 1x otherwise)
        float growthModifier = WeatherSystem.Instance != null ? WeatherSystem.Instance.GetGrowthModifier() : 1f;
        
        foreach (var kvp in plotStates)
        {
            Vector3Int pos = kvp.Key;
            PlotData data = kvp.Value;

            if (data.state != PlotState.Planted || data.seedData == null || !data.canStartGrowing)
                continue;

            // Apply growth with weather modifier
            data.growthTimer += Time.deltaTime * growthModifier;

            var growthPercent = Mathf.Clamp01(data.growthTimer / data.seedData.growthTime);
            var maxStage = data.seedData.growthStageTiles.Count - 1;
            var newStage = Mathf.FloorToInt(growthPercent * maxStage);

            if (newStage <= data.currentGrowthStage) continue;

            TileBase currentTile = data.seedData.GetStageTile(newStage);
            if (currentTile == null)
            {
                Debug.LogError("Error updating crop growth - missing tile for stage " + newStage);
                continue;
            }

            // Update growth stage
            data.currentGrowthStage = newStage;
            cropTilemap.SetTile(pos, currentTile);

            // Check if fully grown
            if (newStage == maxStage)
            {
                data.state = PlotState.Grown;
                Debug.Log($"Crop {data.seedData.itemName} at {pos} is fully grown and ready to harvest!");
            }
        }
    }
    
    #endregion
    
    #region Weather Event Handlers
    
    private void HandleBeneficialRain()
    {
        foreach (var kvp in plotStates)
        {
            PlotData plotData = kvp.Value;
        
            if (plotData.state == PlotState.Planted && plotData.canStartGrowing && plotData.seedData != null)
            {
                // Skip one growth stage due to beneficial rain
                int maxStage = plotData.seedData.growthStageTiles.Count - 1;
                int newStage = Mathf.Min(plotData.currentGrowthStage + 1, maxStage);
            
                if (newStage > plotData.currentGrowthStage)
                {
                    plotData.currentGrowthStage = newStage;
                    cropTilemap.SetTile(kvp.Key, plotData.seedData.GetStageTile(newStage));
                
                    if (newStage == maxStage)
                    {
                        plotData.state = PlotState.Grown;
                    }
                }
            }
        }
        
        Debug.Log("Beneficial rain accelerated crop growth!");
    }

    private void HandleStorm()
    {
        var plantedPlots = new List<Vector3Int>();
    
        // Find all planted or grown plots
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
            {
                plantedPlots.Add(kvp.Key);
            }
        }
    
        if (plantedPlots.Count == 0) return;
    
        // Destroy 0-3 random plants
        int plantsToDestroy = Random.Range(0, Mathf.Min(4, plantedPlots.Count + 1));
    
        for (int i = 0; i < plantsToDestroy; i++)
        {
            int randomIndex = Random.Range(0, plantedPlots.Count);
            Vector3Int plotPos = plantedPlots[randomIndex];
            plantedPlots.RemoveAt(randomIndex);
        
            // Destroy the plant (return to tilled state)
            PlotData plotData = plotStates[plotPos];
            plotData.state = PlotState.Tilled;
            plotData.seedData = null;
            plotData.growthTimer = 0;
            plotData.currentGrowthStage = 0;
            plotData.canStartGrowing = false;
        
            cropTilemap.SetTile(plotPos, null);
        }
        
        Debug.Log($"Storm destroyed {plantsToDestroy} crops!");
    }
    
    #endregion
    
    #region Plot Expansion
    
    /// <summary>
    /// Unlocks plots in the specified expansion tilemap, called by player interaction system.
    /// </summary>
    public void UnlockPlot(Tilemap expansionTilemap)
    {
        foreach (Vector3Int pos in expansionTilemap.cellBounds.allPositionsWithin)
        {
            TileBase tile = expansionTilemap.GetTile(pos);
            
            if (tile == lockedTile && plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Locked)
            {
                plotStates[pos].state = PlotState.Empty;
                plotTilemap.SetTile(pos, emptyTile);
                expansionTilemap.SetTile(pos, null);    // Clear from expansion map
            }
        }
        
        Debug.Log($"Unlocked {expansionTilemap.name}");
    }
    
    #endregion
}