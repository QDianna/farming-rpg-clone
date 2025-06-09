using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Plot state enumeration for farming system.
/// </summary>
public enum PlotState
{
    Locked, Empty, Tilled, Planted, Grown
}

/// <summary>
/// Data container for individual plot information.
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
/// Manages farming plot states, crop growth, weather integration, and land expansion.
/// Handles tilling, planting, harvesting, and dynamic crop progression.
/// </summary>
public class PlotlandController : MonoBehaviour
{
    [Header("Tilemaps References")]
    // used to divide the plotland into 2 sections, contains the actual farming tiles
    [SerializeField] private List<Tilemap> plotlandTierTilemaps;
    // used to display crops on top of the plotland
    [SerializeField] private Tilemap cropTilemap;
    
    [Header("Tile Assets")]
    [SerializeField] private TileBase lockedTile;
    [SerializeField] private TileBase emptyTile;
    [SerializeField] private TileBase tilledTile;
    
    private Dictionary<Vector3Int, PlotData> plotStates = new();
    
    private void Awake()
    {
        InitializePlotData();
    }
    
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
    
    private void InitializePlotData()
    {
        plotStates.Clear();
        
        foreach (var tilemap in plotlandTierTilemaps)
        {
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                var tile = tilemap.GetTile(pos);
                if (tile != null) // Only process positions that have tiles
                {
                    Debug.Log($"init plot on pos {pos}");
                    
                    var plotData = new PlotData();
                    
                    // Check state from the actual tilemap that displays the tile
                    if (tile == lockedTile)
                        plotData.state = PlotState.Locked;
                    else if (tile == emptyTile)
                        plotData.state = PlotState.Empty;
                    else if (tile == tilledTile)
                        plotData.state = PlotState.Tilled;
                    
                    plotStates[pos] = plotData;
                }
            }
        }
        
        Debug.Log($"Initialized {plotStates.Count} plots");
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
    
    public bool CanTill(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Empty;
    }
    
    public bool CanPlant(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Tilled;
    }
    
    public bool CanHarvest(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) && plotStates[tilePos].state == PlotState.Grown;
    }
    
    public bool CanAttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        return plotStates.ContainsKey(tilePos) &&
               plotStates[tilePos].state == PlotState.Planted &&
               plotStates[tilePos].canStartGrowing == false;
    }
    
    public void TillPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        
        if (!plotStates.ContainsKey(tilePos)) return;
        
        plotStates[tilePos].state = PlotState.Tilled;
        
        // Find which tilemap has this position and update it
        foreach (var tilemap in plotlandTierTilemaps)
        {
            if (tilemap.GetTile(tilePos) != null)
            {
                tilemap.SetTile(tilePos, tilledTile);
                break;
            }
        }
    }
    
    public void PlantPlot(InventoryItem seed, Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        var seedItem = seed as ItemSeed;
        
        if (seedItem == null || !plotStates.ContainsKey(tilePos)) return;
        
        var plotData = plotStates[tilePos];
        plotData.state = PlotState.Planted;
        plotData.seedData = seedItem;
        plotData.currentGrowthStage = 0;
        plotData.canStartGrowing = false;

        // Display crop on crop tilemap
        if (cropTilemap != null && seedItem.growthStageTiles.Count > 0)
        {
            cropTilemap.SetTile(tilePos, seedItem.growthStageTiles[0]);
        }
    }
    
    public void AttendPlot(Vector3 worldPos)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        
        if (!plotStates.ContainsKey(tilePos)) return;
        
        var plotData = plotStates[tilePos];
        plotData.growthTimer = 0;
        plotData.currentGrowthStage = 1;
        plotData.canStartGrowing = true;
        
        // Update crop display
        if (cropTilemap != null && plotData.seedData != null && plotData.seedData.growthStageTiles.Count > 1)
        {
            cropTilemap.SetTile(tilePos, plotData.seedData.growthStageTiles[1]);
        }
    }
    
    public void HarvestPlot(Vector3 worldPos, PlayerController player)
    {
        Vector3Int tilePos = plotlandTierTilemaps[0].WorldToCell(worldPos);
        
        if (!plotStates.ContainsKey(tilePos)) return;
        
        var data = plotStates[tilePos];
        var cropItem = data.seedData.resultedCrop;

        // Give harvest rewards
        player.inventorySystem.AddItem(data.seedData, Random.Range(1, 3));
        player.inventorySystem.AddItem(cropItem, Random.Range(1, 3));
        
        // Show harvest effect
        cropItem.CollectItem(player);
        
        // Reset plot
        ResetPlotToEmpty(tilePos, data);
    }
    
    private void ResetPlotToEmpty(Vector3Int tilePos, PlotData data)
    {
        // Clear crop display
        if (cropTilemap != null)
            cropTilemap.SetTile(tilePos, null);
        
        // Reset plot tile - find which tilemap has this position
        foreach (var tilemap in plotlandTierTilemaps)
        {
            if (tilemap.GetTile(tilePos) != null)
            {
                tilemap.SetTile(tilePos, emptyTile);
                break;
            }
        }
        
        // Reset data
        data.state = PlotState.Empty;
        data.seedData = null;
        data.growthTimer = 0;
        data.currentGrowthStage = 0;
        data.canStartGrowing = false;
    }
    
    private void UpdateCropGrowth()
    {
        float growthModifier = WeatherSystem.Instance?.GetGrowthModifier() ?? 1f;
        
        foreach (var kvp in plotStates)
        {
            var data = kvp.Value;

            if (data.state != PlotState.Planted || data.seedData == null || !data.canStartGrowing)
                continue;

            data.growthTimer += Time.deltaTime * growthModifier;

            var growthPercent = Mathf.Clamp01(data.growthTimer / data.seedData.growthTime);
            var maxStage = data.seedData.growthStageTiles.Count - 1;
            var newStage = Mathf.FloorToInt(growthPercent * maxStage);

            if (newStage <= data.currentGrowthStage) continue;

            var currentTile = data.seedData.GetStageTile(newStage);
            if (currentTile == null) continue;

            data.currentGrowthStage = newStage;
            
            // Update crop display
            if (cropTilemap != null)
                cropTilemap.SetTile(kvp.Key, currentTile);

            if (newStage == maxStage)
            {
                data.state = PlotState.Grown;
                NotificationSystem.ShowNotification($"{data.seedData.name} is ready to harvest!");
            }
        }
    }
    
    private void HandleBeneficialRain()
    {
        foreach (var kvp in plotStates)
        {
            var data = kvp.Value;
        
            if (data.state == PlotState.Planted && data.canStartGrowing && data.seedData != null)
            {
                int maxStage = data.seedData.growthStageTiles.Count - 1;
                int newStage = Mathf.Min(data.currentGrowthStage + 1, maxStage);
            
                if (newStage > data.currentGrowthStage)
                {
                    data.currentGrowthStage = newStage;
                    
                    if (cropTilemap != null)
                        cropTilemap.SetTile(kvp.Key, data.seedData.GetStageTile(newStage));
                
                    if (newStage == maxStage)
                        data.state = PlotState.Grown;
                }
            }
        }
        
        NotificationSystem.ShowNotification("Rain helped your crops grow!");
    }

    private void HandleStorm()
    {
        var plantedPlots = new List<Vector3Int>();
    
        foreach (var kvp in plotStates)
        {
            if (kvp.Value.state == PlotState.Planted || kvp.Value.state == PlotState.Grown)
                plantedPlots.Add(kvp.Key);
        }
    
        if (plantedPlots.Count == 0) return;
    
        int plantsToDestroy = Random.Range(0, Mathf.Min(2, plantedPlots.Count + 1));
    
        for (int i = 0; i < plantsToDestroy; i++)
        {
            int randomIndex = Random.Range(0, plantedPlots.Count);
            Vector3Int plotPos = plantedPlots[randomIndex];
            plantedPlots.RemoveAt(randomIndex);
        
            var plotData = plotStates[plotPos];
            
            plotData.state = PlotState.Tilled;
            plotData.seedData = null;
            plotData.growthTimer = 0;
            plotData.currentGrowthStage = 0;
            plotData.canStartGrowing = false;
        
            // Clear crop
            if (cropTilemap != null)
                cropTilemap.SetTile(plotPos, null);
            
            // Reset plot tile
            foreach (var tilemap in plotlandTierTilemaps)
            {
                if (tilemap.GetTile(plotPos) != null)
                {
                    tilemap.SetTile(plotPos, tilledTile);
                    break;
                }
            }
        }
        
        if (plantsToDestroy > 0)
            NotificationSystem.ShowNotification($"Storm destroyed {plantsToDestroy} crops!");
    }
    
    public void UnlockPlotland(Tilemap tilemap)
    {
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            
            if (tile == lockedTile && plotStates.ContainsKey(pos) && plotStates[pos].state == PlotState.Locked)
            {
                plotStates[pos].state = PlotState.Empty;
                tilemap.SetTile(pos, emptyTile);
            }
        }
    }
}