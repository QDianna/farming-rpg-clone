using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plot state enumeration for farming system progression.
/// </summary>
public enum PlotState
{
    Locked, Empty, Tilled, Planted, Grown
}

/// <summary>
/// Data container for individual plot information and growth tracking.
/// </summary>
public class PlotData
{
    public PlotState state;
    public ItemSeed seedData;
    public float growthTimer;
    public int currentGrowthStage;
    public bool canStartGrowing;
}

/// <summary>
/// Comprehensive farming system managing plot states, crop growth, and weather integration.
/// Handles tilling, planting, harvesting, land expansion, and dynamic weather effects on crops.
/// </summary>
public class PlotlandController : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private List<Tilemap> plotlandTierTilemaps;
    [SerializeField] private Tilemap cropTilemap;
    
    [Header("Tile Assets")]
    [SerializeField] private TileBase lockedTile;
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase tilledTile;
    
    private readonly Dictionary<Vector3Int, PlotData> plotStates = new();
    
    private void Awake()
    {
        InitializePlotData();
    }
    
    private void Start()
    {
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
    
    // Validation methods for plot actions
    public bool CanTill(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Empty);
    }
    
    public bool CanPlant(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Tilled);
    }
    
    public bool CanHarvest(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return IsValidPlotAction(tilePos, PlotState.Grown);
    }
    
    public bool CanAttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        return plotStates.ContainsKey(tilePos) &&
               plotStates[tilePos].state == PlotState.Planted &&
               !plotStates[tilePos].canStartGrowing;
    }
    
    // Plot action methods
    public void TillPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var state)) 
            return;
        
        state.state = PlotState.Tilled;
        UpdateTilemapTile(tilePos, tilledTile);
    }
    
    public void PlantPlot(InventoryItem seed, Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        var seedItem = seed as ItemSeed;
        
        if (seedItem == null || !plotStates.ContainsKey(tilePos)) 
            return;
        
        SetupPlantedPlot(tilePos, seedItem);
        DisplayCropStage(tilePos, seedItem, 0);
    }
    
    public void AttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;

        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 1;
        plotData.canStartGrowing = true;
        
        DisplayCropStage(tilePos, plotData.seedData, 1);
    }
    
    public void HarvestPlot(Vector3 worldPos, PlayerController player)
    {
        Vector3Int tilePos = GetTilePosition(worldPos);
        
        if (!plotStates.TryGetValue(tilePos, out var plotData)) 
            return;

        GiveHarvestRewards(plotData, player);
        ResetPlotToEmpty(tilePos, plotData);
    }
    
    public void UnlockPlotland(Tilemap tilemap)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            
            if (tile == lockedTile && IsValidPlotAction(pos, PlotState.Locked))
            {
                plotStates[pos].state = PlotState.Empty;
                tilemap.SetTile(pos, emptyTile);
            }
        }
    }
    
    // Scans all tilemaps and initializes plot data based on tile types
    private void InitializePlotData()
    {
        plotStates.Clear();
        
        foreach (var tilemap in plotlandTierTilemaps)
        {
            ScanTilemapForPlots(tilemap);
        }
    }
    
    // Scans individual tilemap and creates plot data entries
    private void ScanTilemapForPlots(Tilemap tilemap)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            if (tile != null)
            {
                var plotData = CreatePlotDataFromTile(tile);
                plotStates[pos] = plotData;
            }
        }
    }
    
    // Creates plot data based on tile type
    private PlotData CreatePlotDataFromTile(TileBase tile)
    {
        var plotData = new PlotData();
        
        if (tile == lockedTile)
            plotData.state = PlotState.Locked;
        else if (tile == emptyTile)
            plotData.state = PlotState.Empty;
        else if (tile == tilledTile)
            plotData.state = PlotState.Tilled;
        
        return plotData;
    }
    
    // Sets up weather event subscriptions
    private void SubscribeToWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain += HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm += HandleStorm;
        }
    }
    
    // Removes weather event subscriptions
    private void UnsubscribeFromWeatherEvents()
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.OnBeneficialRain -= HandleBeneficialRain;
            WeatherSystem.Instance.OnStorm -= HandleStorm;
        }
    }
    
    // Converts world position to tile coordinates
    private Vector3Int GetTilePosition(Vector3 worldPos)
    {
        return plotlandTierTilemaps[0].WorldToCell(worldPos);
    }
    
    // Validates plot action based on current state
    private bool IsValidPlotAction(Vector3Int tilePos, PlotState requiredState)
    {
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == requiredState;
    }
    
    // Updates tile in appropriate tilemap
    private void UpdateTilemapTile(Vector3Int tilePos, TileBase newTile)
    {
        foreach (var tilemap in plotlandTierTilemaps)
        {
            if (tilemap.GetTile(tilePos) != null)
            {
                tilemap.SetTile(tilePos, newTile);
                break;
            }
        }
    }
    
    // Configures plot data for newly planted seed
    private void SetupPlantedPlot(Vector3Int tilePos, ItemSeed seedItem)
    {
        var plotData = plotStates[tilePos];
        plotData.state = PlotState.Planted;
        plotData.seedData = seedItem;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
    }
    
    // Displays crop sprite at specific growth stage
    private void DisplayCropStage(Vector3Int tilePos, ItemSeed seedData, int stage)
    {
        if (cropTilemap != null && seedData != null && stage < seedData.growthStageTiles.Count)
        {
            cropTilemap.SetTile(tilePos, seedData.growthStageTiles[stage]);
        }
    }
    
    // Gives harvest rewards to player
    private void GiveHarvestRewards(PlotData plotData, PlayerController player)
    {
        var cropItem = plotData.seedData.resultedCrop;
        
        player.inventorySystem.AddItem(plotData.seedData, Random.Range(1, 3));
        player.inventorySystem.AddItem(cropItem, Random.Range(1, 3));
        
        cropItem.CollectItem(player);
    }
    
    // Resets plot to empty state after harvest
    private void ResetPlotToEmpty(Vector3Int tilePos, PlotData plotData)
    {
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, null);
        
        UpdateTilemapTile(tilePos, emptyTile);
        
        plotData.state = PlotState.Empty;
        plotData.seedData = null;
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
    }
    
    // Updates crop growth progression for all planted plots
    private void UpdateCropGrowth()
    {
        float growthModifier = WeatherSystem.Instance?.GetGrowthModifier() ?? 1f;
        
        foreach (var kvp in plotStates)
        {
            UpdateIndividualCropGrowth(kvp.Key, kvp.Value, growthModifier);
        }
    }
    
    // Updates growth for single plot
    private void UpdateIndividualCropGrowth(Vector3Int tilePos, PlotData plotData, float growthModifier)
    {
        if (plotData.state != PlotState.Planted || plotData.seedData == null || !plotData.canStartGrowing)
            return;

        plotData.growthTimer += Time.deltaTime * growthModifier;
        
        int newStage = CalculateGrowthStage(plotData);
        if (newStage > plotData.currentGrowthStage)
        {
            AdvanceCropToStage(tilePos, plotData, newStage);
        }
    }
    
    // Calculates current growth stage based on timer
    private int CalculateGrowthStage(PlotData plotData)
    {
        float growthPercent = Mathf.Clamp01(plotData.growthTimer / plotData.seedData.growthTime);
        int maxStage = plotData.seedData.growthStageTiles.Count - 1;
        return Mathf.FloorToInt(growthPercent * maxStage);
    }
    
    // Advances crop to specific growth stage
    private void AdvanceCropToStage(Vector3Int tilePos, PlotData plotData, int newStage)
    {
        var stageTile = plotData.seedData.GetStageTile(newStage);
        if (stageTile == null) 
            return;

        plotData.currentGrowthStage = newStage;
        
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, stageTile);

        int maxStage = plotData.seedData.growthStageTiles.Count - 1;
        if (newStage == maxStage)
            plotData.state = PlotState.Grown;
    }
    
    // Handles beneficial rain weather effect
    private void HandleBeneficialRain()
    {
        foreach (var kvp in plotStates)
        {
            AdvanceCropFromRain(kvp.Key, kvp.Value);
        }
        
        NotificationSystem.ShowNotification("Rain helped your crops grow!");
    }
    
    // Advances individual crop due to rain
    private void AdvanceCropFromRain(Vector3Int tilePos, PlotData plotData)
    {
        if (plotData.state != PlotState.Planted || !plotData.canStartGrowing || plotData.seedData == null)
            return;
        
        int maxStage = plotData.seedData.growthStageTiles.Count - 1;
        int newStage = Mathf.Min(plotData.currentGrowthStage + 1, maxStage);
        
        if (newStage > plotData.currentGrowthStage)
        {
            AdvanceCropToStage(tilePos, plotData, newStage);
        }
    }

    // Handles storm weather effect
    private void HandleStorm()
    {
        var plantedPlots = GetPlantedPlots();
        
        if (plantedPlots.Count == 0) 
            return;
    
        int plantsToDestroy = Random.Range(0, Mathf.Min(2, plantedPlots.Count + 1));
        
        for (int i = 0; i < plantsToDestroy; i++)
        {
            DestroyRandomPlot(plantedPlots);
        }
        
        if (plantsToDestroy > 0)
            NotificationSystem.ShowNotification($"Storm destroyed {plantsToDestroy} crops!");
    }
    
    // Gets list of all planted plot positions
    private List<Vector3Int> GetPlantedPlots()
    {
        var plantedPlots = new List<Vector3Int>();
        
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
                plantedPlots.Add(kvp.Key);
        }
        
        return plantedPlots;
    }
    
    // Destroys random plot from storm damage
    private void DestroyRandomPlot(List<Vector3Int> plantedPlots)
    {
        int randomIndex = Random.Range(0, plantedPlots.Count);
        Vector3Int plotPos = plantedPlots[randomIndex];
        plantedPlots.RemoveAt(randomIndex);
        
        var plotData = plotStates[plotPos];
        ResetPlotFromStorm(plotPos, plotData);
    }
    
    // Resets plot to tilled state after storm damage
    private void ResetPlotFromStorm(Vector3Int plotPos, PlotData plotData)
    {
        plotData.state = PlotState.Tilled;
        plotData.seedData = null;
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;
        
        if (cropTilemap != null)
            cropTilemap.SetTile(plotPos, null);
        
        UpdateTilemapTile(plotPos, tilledTile);
    }
}